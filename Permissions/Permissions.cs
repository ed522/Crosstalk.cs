using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Crosstalk;


/// <summary>
/// The tree representing the permissions state, enabling cascading permissions.
/// <br/><br/>
/// 
/// <b>Cascading Permissions</b>
/// Consider a program with the following permissions:
/// <code>
/// 
/// </summary>
public class PermissionTree
{
    private readonly List<Permission> permissions;
    private readonly TrieNode rootNode;
    /// <summary>
    /// Creates a new <c>PermissionTree</c> representing the array of permissions.
    /// <br/>
    /// All permissions with a <c>null</c> value (unasserted permissions) are ignored, all else are set accordingly.
    /// </summary>
    /// <param name="permissions"></param>
    public PermissionTree(Permission[] permissions)
    {

        // The root permission cannot be unasserted
        if (permissions.Any(p => p.Components.Length == 0 && p.Value == null))
            throw new ArgumentException("The root permission must not be unasserted (have a null Value)", $"permissions[{permissions.Length}].Value");

        this.permissions = [.. permissions.Where(p => p.Value != null)];
        // construct the trie
        // The root node is always false so that any unasserted permission is denied.
        // The permissions model used is an allowlist-type model where permissions are granted selectively.
        // (This can be overriden by a permission with no components, but an unasserted root is not allowed.)
        TrieNode root = new([], false);
        // Assign each permission (where they are asserted)
        foreach (Permission permission in permissions.Where(p => p.Value != null))
        {
            AddSetPermission(permission, root);
        }
        // Store the root for further use.
        this.rootNode = root;
    }

    private static bool? AddSetPermission(Permission permission, TrieNode rootNode)
    {
        if (permission.Value == null) throw new ArgumentException("Permission value cannot be null when setting a permission directly.", "permission.Value");
        TrieNode currentNode = rootNode;
        for (int i = 0; i < permission.Components.Length; i++)
        {
            // The component we are using
            string component = permission.Components[i];
            // Does the current node already contain this key?
            // If so, rebase on that.
            if (currentNode.Children != null && currentNode.Children.TryGetValue(component, out TrieNode? existingChild))
                currentNode = existingChild;
            // If not, add it and rebase on that.
            else
            {
                TrieNode newChild = new([], null);
                currentNode.Children.Add(component, newChild);
                currentNode = newChild;
            }

        }
        // Now assign the value correctly
        bool? lastValue = currentNode.Value;
        currentNode.Value = permission.Value;
        return lastValue;
    }

    /// <summary>
    /// Set the specified permission to its value.
    /// This method creates tree paths if they do not exist, and prunes unnecessary branches if the value is null.
    /// <br/>
    /// Note that this method does not travel the permissions tree. This will return <c>null</c> where a specific
    /// tree position is not asserted, even if permissions up the tree are (and thus where a querying method would return true/false).
    /// </summary>
    /// <param name="permission">The permission and value to set.</param>
    /// <returns>The previous value of the permission, or null if not asserted.</returns>
    public bool? SetPermission(Permission permission)
    {
        // First, check if we are allocating or deallocating this permission
        if (permission.Value != null)
        {
            // This is a permission to allocate, add it here
            this.permissions.Add(permission);
            return AddSetPermission(permission, rootNode);
        }

        // We are deallocating it.

        // Make sure that this permission exists first
        // Permissions to deallocate do not follow cascade rules; thus it needs to explicitly be
        // set to be able to deallocate it
        int matchIndex = this.permissions.FindIndex(p => p.Components == permission.Components && p.Value != null);
        // terminal
        if (matchIndex == -1) return null;

        // otherwise, continue on
        this.permissions.RemoveAt(matchIndex);
        // Next, go down the tree to find the bottom-most node ...
        // ... but we need to keep track of the upwards nodes as well
        List<TrieNode> nodes = [];
        TrieNode lastNode = rootNode;
        nodes.Add(rootNode);

        // go down the tree
        for (int i = 0; i < permission.Components.Length; i++)
        {
            string component = permission.Components[i];
            try
            {
                TrieNode newNode = lastNode.Children[component];
                nodes.Add(newNode);
                lastNode = newNode;
            }
            catch (KeyNotFoundException)
            {
                // We're removing a permission that doesn't exist.
                // Stop here (this permission was not asserted)
                return null;
            }
        }
        bool? oldValue = lastNode.Value;
        lastNode.Value = null;

        // Now we can deallocate everything that carries no information.
        // Go over everything in reverse order, and check if any children have no value and no children.
        // That is the signifier that it is invalid, thus it can be removed.
        // Keep doing that up the chain until it fails (meaning that the chain stops there)

        // (we specifically start at the next node up to skip the end node)
        for (int i = nodes.Count - 2; i >= 0; i--)
        {
            TrieNode currentNode = nodes[i];
            bool haveRemovedItem = false;
            // search the children
            foreach (var child in currentNode.Children)
            {
                // Check if it's invalid; if so, remove it.
                if (child.Value.Value == null && child.Value.Children.Count == 0)
                {
                    currentNode.Children.Remove(child.Key);
                    haveRemovedItem = true;
                }
            }
            // Now if we haven't removed anything, then there's nothing left to do
            if (!haveRemovedItem) return oldValue;
        }

        // failsafe
        return oldValue;

    }
    /// <summary>
    /// Checks if this tree contains the following permission.
    /// <br/>
    /// This method follows the cascade rules: if a full permission is not contained, it will look
    /// upwards in the tree until it finds a match.
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    public bool HasPermission(Permission permission)
    {
        // before a tree search we can do a quick check
        Permission? potentialMatch = this.permissions.Find(p => p.Components == permission.Components && p.Value != null);
        if (potentialMatch != default(Permission))
            return (bool) potentialMatch.Value!;

        // First, go as deep as possible into the tree, until there are no more nodes.
        List<TrieNode> nodes = [];
        TrieNode currentNode = rootNode;
        foreach (string component in permission.Components)
        {
            // If true, the node is contained and we can keep going
            if (currentNode.Children.TryGetValue(component, out TrieNode? newNode))
            {
                currentNode = newNode;
                nodes.Add(newNode);
            }
            // Otherwise, we can't go any deeper and have to move on to tree traversal
            else break;
        }
        // Next, traverse the tree.
        // Go through the list in reverse looking for an asserted permission.
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            bool? value = nodes[i].Value;
            if (value != null) return (bool) value!;
        }
        // ^ fell through
        // This means the root node was unasserted, which is an illegal state.
        throw new UnreachableException("Root permission node has no asserted permission");

    }

    /// <summary>
    /// A node of the permissions trie. Represents part of the permission tree.
    /// <br/>
    /// Terminality is implied by the length of Children. If Children is of length 0 and Value is null, this node is invalid and may be pruned.
    /// </summary>
    /// <param name="Children">The children of this node, or an empty dictionary if there are none.</param>
    /// <param name="Value">The value of this node: true if explicitly allowed, false if denied, and null if not asserted.</param>
    private sealed class TrieNode(Dictionary<string, TrieNode> Children, bool? Value)
    {
        public Dictionary<string, TrieNode> Children { get; set; } = Children;
        public bool? Value { get; set; } = Value;
    }
}

/// <summary>
/// Represents a specific permission alongside its value.
/// </summary>
/// <param name="Components">All components of this permission</param>
/// <param name="Value">The value, if one exists; a <c>null</c> value represents a permission that is left to further-up nodes to decide.</param>
public class Permission(string[] Components, bool? Value)
{
    public required string[] Components { get; init; } = Components;
    public bool? Value { get; set; } = Value;
}

