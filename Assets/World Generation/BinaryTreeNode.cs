using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NeighborIDs {
    public int left;
    public int right;
    public int edge;
    public int node;
}

public class BinaryTreeNode {

    // Active node IDs
    public NeighborIDs nodeID;
    public static Dictionary<int, BinaryTreeNode> nodes = new Dictionary<int, BinaryTreeNode>();
    public bool merge = false;

    // Children, 
    public BinaryTreeNode[] children;

    // Exponent of level of detail
    public int LODExp;

    // Chunk object and position in case chunk is null
    public GameObject chunk;
    public Vector3 position;
    public float rotation;

    // Constructor with nodeId
    public BinaryTreeNode(NeighborIDs nodeID, Vector3 position, float rotation, int LODExp) {
        this.nodeID = nodeID;
        this.position = position;
        this.rotation = rotation;
        this.LODExp = LODExp;
    }

    // Splits the node into 4 children
    public void Split() {
        if (children != null || LODExp == 0) return;

        // Create children
        children = new BinaryTreeNode[2];
        for (int i = 0; i < 2; i++) {
            float rotation = this.rotation + 180f + 45f * -(2*i-1) * (2*(LODExp % 2)-1);
            children[i] = new BinaryTreeNode(
                SplitNodeIDs(nodeID, i),
                position + Quaternion.Euler(0, rotation, 0) * Vector3.forward * (World.settings.vertexCount - 1) * World.settings.vertexDistance * Mathf.Pow(Mathf.Sqrt(2), LODExp - 2),
                rotation, 
                LODExp - 1
            );
            nodes.Add(children[i].nodeID.node, children[i]);
        }

        // Longest Edge Bisection rule
        if (BinaryTreeNode.nodes.TryGetValue(nodeID.edge, out BinaryTreeNode neighbor)) {
            neighbor.Split();
        } else if (BinaryTreeNode.nodes.TryGetValue(nodeID.edge >> 1, out BinaryTreeNode neighborParent)) {
            neighborParent.Split();
            BinaryTreeNode.nodes.TryGetValue(nodeID.edge, out neighbor);
            neighbor.Split();
        }

        // Remove chunk
        if (chunk != null) {
            ChunkManager.RemoveChunk(this);
        }
    }

    public void MarkMerge() {
        if (children == null) return;

        // Check if merge can cause t-junction
        for (int i = 0; i < 2; i++) {
            if (nodes.ContainsKey(children[i].nodeID.edge << 1)) {
                return;
            }
        }

        // Mark for merge
        merge = true;
        for (int i = 0; i < 2; i++) {
            children[i].MarkMerge();
        }
    }

    // Merges the children into the parent node
    public void Merge(bool first = true) {
        if (children == null || !merge) return;
        
        // Check if edge neighbor wants to merge
        if (first) {
            nodes.TryGetValue(nodeID.edge, out BinaryTreeNode neighbor);
            if (neighbor != null && neighbor.merge) {
                neighbor.Merge(false);
            } else if (neighbor != null) {
                merge = false;
                return;
            }
        }

        // Remove children
        for (int i = 0; i < 2; i++) {
            children[i].Merge(false);
            if (children[i].chunk != null) {
                ChunkManager.RemoveChunk(children[i]);
            }
            nodes.Remove(children[i].nodeID.node);
        }
        children = null;
        merge = false;
    }

    /** ReconstructTree -- Reconstructs the tree from the root node
      *
      * This function reconstructs missing nodes in the tree by creating missing nodes from NeighborIDs.
      * Missing nodes are such nodes that are required for Longest Edge Bisection rule but are not present in the tree.
      */
    public void ReconstructTree() {

        // Reconstruct tree
        Queue<BinaryTreeNode> queue = new Queue<BinaryTreeNode>(new BinaryTreeNode[] { nodes[1] });
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();

            // Generate missing nodes
            if (node.children == null) {
                node.GenerateNodeFromID(node.nodeID.left >> 1);
                node.GenerateNodeFromID(node.nodeID.right >> 1);
                node.GenerateNodeFromID(node.nodeID.edge >> 1);
            }

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }
    }

    /** UpdateNeighborIDs -- Updates the neighbors of the node
      *
      * This function propagates the NeighborIDs of updated parent nodes their children.
      */
    public void UpdateNeighborIDs() {
        if (children == null) return;

        // Update children
        for (int i = 0; i < 2; i++) {
            children[i].nodeID = SplitNodeIDs(nodeID, i);
            children[i].UpdateNeighborIDs();
        }
    }

    /** GenerateChunk -- Generates a chunk object for the node
      *
      * This function generates a chunk object for the node if it does not exist.
      * Only call when the whole tree is updated to avoid unnecessary chunk generation.
      */
    public void GenerateChunk() {
        if (chunk != null || children != null) return;
        chunk = ChunkManager.GetChunkObject(this);
    }

    /** GenerateNodeFromID -- Generates a node and all its parents from a given ID
      * 
      * This function splits all parent nodes according to Longest Edge Bisection rule
      * until the node with the given ID is reached.
      */
    public void GenerateNodeFromID(int nodeID) {
        if (nodeID > 1 << World.settings.levelsOfDetail || nodes.ContainsKey(nodeID)) return;
        
        // Generate parent nodes by splitting until nodeID is reached
        NeighborIDs parentID = new NeighborIDs { left = 0, right = 0, edge = 0, node = 1 };
        for (int bitID = Mathf.FloorToInt(Mathf.Log(nodeID, 2)); bitID > 0; bitID--) {
            NeighborIDs nextID = SplitNodeIDs(parentID, (nodeID >> bitID) & 1);
            if (!BinaryTreeNode.nodes.ContainsKey(nextID.node)) {
                BinaryTreeNode.nodes.TryGetValue(parentID.node, out BinaryTreeNode parent);
                parent.Split();
            }
            parentID = nextID;
        }
        return;
    }

    /** SplitNodeIDs -- Updates the IDs of neighbors after one LEB split
      *
      * This code applies the following rules:
      * Split left:
      * LeftID  = 2 * NodeID + 1
      * RightID = 2 * EdgeID + 1
      * EdgeID  = 2 * RightID + 1
      * NodeID  = 2 * NodeID
      *
      * Split right:
      * LeftID  = 2 * EdgeID
      * RightID = 2 * NodeID
      * EdgeID  = 2 * LeftID
      * NodeID  = 2 * NodeID + 1
      *
      *            Edge
      *    -------------------
      *     \       |       /
      *      \  2n  | 2n+1 /
      * Left  \     |     /  Right
      *        \    |    /
      *         \   |   /
      *          \  |  /
      *           \ | /
      *            \|/
      */
    public NeighborIDs SplitNodeIDs(NeighborIDs nodeID, int splitBit) {
        int bitRight = nodeID.right == 0 ? 0 : 1, bitEdge = nodeID.edge == 0 ? 0 : 1;
        if (splitBit == 0) {
            return new NeighborIDs {
                left = nodeID.node << 1 | 1,
                right = nodeID.edge << 1 | bitEdge,
                edge = nodeID.right << 1 | bitRight,
                node = nodeID.node << 1
            };
        } else {
            return new NeighborIDs {
                left = nodeID.edge << 1,
                right = nodeID.node << 1,
                edge = nodeID.left << 1,
                node = nodeID.node << 1 | 1
            };
        }
    }
}
