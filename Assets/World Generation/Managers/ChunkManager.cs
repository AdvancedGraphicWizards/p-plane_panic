using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class to create and disable chunk-GameObjects based on QuadTreeNodes
public static class ChunkManager {

    // List of all chunks
    private static Dictionary<int, GameObject> chunks = new Dictionary<int, GameObject>();

    // Queue for chunk-GameObjects to re-use
    private static Queue<GameObject> recycleQueue = new Queue<GameObject>();

    // Unloads chunk and adds it to recycleQueue
    public static void RemoveChunk(BinaryTreeNode node) {
        if (node.chunk == null) { return; }

        // Disable GameObject
        node.chunk.gameObject.SetActive(false);
        chunks.Remove(node.nodeID.node);

        // Enqueue GameObject to be recycled and remove reference
        recycleQueue.Enqueue(node.chunk.gameObject);
        node.chunk = null;
    }

    // Unloads chunk and adds it to recycleQueue
    public static void RemoveChunk(QuadTreeNode node) {
        if (node.chunk == null) { return; }

        // Disable GameObject
        node.chunk.gameObject.SetActive(false);
        chunks.Remove(node.ID);

        // Enqueue GameObject to be recycled and remove reference
        recycleQueue.Enqueue(node.chunk.gameObject);
        node.chunk = null;
    }

    // Returns GameObject ready to be used as chunk
    public static GameObject GetChunkObject(BinaryTreeNode node) {

        // Get chunk if possible, else create new chunk
        if (!recycleQueue.TryDequeue(out GameObject chunk)) {
            chunk = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(TriangleMeshManipulationJob), typeof(MeshCollider));
            chunk.GetComponent<MeshRenderer>().material = World.settings.material;

            // Generate mesh based on LOD
            Mesh mesh = MeshGenerator.GenerateTriangleMesh(
                World.settings.vertexDistance * Mathf.Pow(Mathf.Sqrt(2), node.LODExp),
                World.settings.vertexCount, 
                node.rotation
            );
            chunk.GetComponent<MeshFilter>().mesh = mesh;
            chunk.GetComponent<TriangleMeshManipulationJob>().Initialize(mesh);

            // Set parent to World GameObject if it exists (TEMP)
            if (GameObject.Find("World") != null) {
                chunk.transform.parent = GameObject.Find("World").transform;
            }
        }
        
        // Update chunk GameObject
        chunk.name = "Chunk " + node.nodeID.node;
        chunk.transform.position = node.position;

        // Update mesh based on LOD
        chunk.GetComponent<TriangleMeshManipulationJob>().UpdateMesh(
            World.settings.vertexDistance * Mathf.Pow(Mathf.Sqrt(2), node.LODExp),
            World.settings.vertexCount,
            node.rotation
        );

        // Add to chunks dictionary
        chunks.Add(node.nodeID.node, chunk);
        chunk.SetActive(true);

        // Return new chunk
        return chunk;
    }

    // Returns GameObject ready to be used as chunk
    public static GameObject GetChunkObject(QuadTreeNode node) {
        GameObject chunk;

        // Get chunk if possible, else create new chunk
        if (!recycleQueue.TryDequeue(out chunk)) {
            chunk = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer),typeof(MeshCollider));
            Mesh genereatedMesh = MeshGenerator.GenerateMesh(World.settings.vertexDistance * Mathf.Pow(2, node.LODExp), World.settings.vertexCount);
            chunk.GetComponent<MeshFilter>().mesh = genereatedMesh;
            chunk.GetComponent<MeshCollider>().sharedMesh = genereatedMesh;

            // Set parent to World GameObject if it exists (TEMP)
            if (GameObject.Find("World") != null) {
                chunk.transform.parent = GameObject.Find("World").transform;
            }
        }
        
        // Create new chunk GameObject
        chunk.name = "Chunk " + node.ID;
        chunk.transform.position = node.position;
        chunk.GetComponent<MeshFilter>().mesh = MeshGenerator.GenerateMesh(World.settings.vertexDistance * Mathf.Pow(2, node.LODExp), World.settings.vertexCount);
        chunk.GetComponent<MeshRenderer>().material = World.settings.material;

        // Set height based on position
        Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i].y = World.GetHeight(vertices[i].x + node.position.x, vertices[i].z + node.position.z);
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Add to chunks dictionary
        chunks.Add(node.ID, chunk);
        chunk.SetActive(true);

        // Return new chunk
        return chunk;
    }
}