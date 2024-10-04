using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode {
    
    // Node ID
    public int ID;
    public static Dictionary<int, QuadTreeNode> nodes = new Dictionary<int, QuadTreeNode>();

    /** Children partition order:
     *    ---------
     *    | 1 | 2 |
     *    ---------
     *    | 3 | 4 |
     *    ---------
    **/
    public QuadTreeNode[] children;

    // Exponent of level of detail
    public int LODExp;

    // Chunk object and position in case chunk is null
    public GameObject chunk;
    public Vector3 position;

    // Constructor with nodeId
    public QuadTreeNode(int ID, Vector3 position, int LODExp) {
        this.ID = ID;
        this.position = position;
        this.LODExp = LODExp;
    }

    // Splits the node into 4 children
    public void Split() {
        if (children != null || LODExp == 0) return;

        // Create children
        children = new QuadTreeNode[4];
        for (int i = 0; i < 4; i++) {
            children[i] = new QuadTreeNode(
                ID * 4 + (i + 1), 
                position + new Vector3(i / 2 - 0.5f, 0, i % 2 - 0.5f) * (World.settings.vertexCount - 1) * World.settings.vertexDistance * (1 << LODExp - 1),
                LODExp - 1
            );
            nodes.Add(children[i].ID, children[i]);
            children[i].Generate();
        }

        // Remove chunk if it exists
        if (chunk != null) {
            ChunkManager.RemoveChunk(this);
        }
        nodes.Remove(ID);
    }

    // Merges the children into the parent node
    public void Merge() {
        if (children == null) return;
        Queue<QuadTreeNode> queue = new Queue<QuadTreeNode>(children);
        while (queue.Count > 0) {
            QuadTreeNode node = queue.Dequeue();
            if (node.children != null) {
                queue.Enqueue(node.children[0]);
                queue.Enqueue(node.children[1]);
                queue.Enqueue(node.children[2]);
                queue.Enqueue(node.children[3]);
            } else {
                ChunkManager.RemoveChunk(node);
            }
            nodes.Remove(node.ID);
        }
        children = null;

        // Generate chunk
        Generate();
        nodes.Add(ID, this);
    }

    // Generates the GameObject for the chunk
    public void Generate() {
        if (chunk != null) return;
        chunk = ChunkManager.GetChunkObject(this);
    }
}
