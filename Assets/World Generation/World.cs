using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour {

    // World settings
    [SerializeField] private WorldSettingsSO m_settings;
    public static WorldSettingsSO settings;

    // Generates LOD based on distance from main camera
    private Camera mainCamera;

    // QuadTree root nodes
    private BinaryTreeNode[] roots = new BinaryTreeNode[4];

    // Start is called before the first frame update
    void Start() {

        // Assign callback for settings change
        m_settings.OnSettingsChange += ReloadWorld;
    }

    // Update is called once per frame
    void Update() {
        mainCamera = Camera.main;

        // Update LOD based on distance from main camera
        Queue<BinaryTreeNode> queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            float distance = Vector3.Distance(mainCamera.transform.position, node.position);

            // Split 
            if (node.children == null && distance < settings.vertexDistance * settings.vertexCount * Mathf.Pow(Mathf.Sqrt(2), node.LODExp)) {//Mathf.Pow(2, node.LODExp)) {
                node.Split();

            // Merge
            } else if (node.children != null && distance > settings.vertexDistance * settings.vertexCount * Mathf.Pow(Mathf.Sqrt(2), node.LODExp + 1)) {// * Mathf.Pow(2, node.LODExp + 1)) {
                node.MarkMerge();
            }

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }

        // Merge marked nodes
        queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            if (node.LODExp != settings.levelsOfDetail) {
                node.Merge();
            }

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }

        // Update chunks
        queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            node.GenerateChunk();

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }
    }
    
    // Returns height for position x, z based on array of octaves and perlin noise
    public static float GetHeight(float x, float z) {
        float height = 0;
        // foreach (Octave octave in settings.octaves) {
        //     if (octave.enabled) {
        //         height += (Mathf.PerlinNoise(x * octave.scale + settings.seedX, z * octave.scale + settings.seedZ) - 0.5f) * 2f * octave.amplitude;
        //     }
        // }
        height = TerrainAtPosition(x, z).x;

        return height;
    }

    // Returns normal for position x, z based on array of octaves and perlin noise
    public static Vector3 GetNormal(float x, float y, float z) {
        return HeightmapComponent.GetNormalAtPosition(x, y, z, settings.vertexDistance);
    }

    // Returns height for position x, z based on heightmapComponent function
    private static Vector4 TerrainAtPosition(float x, float y) {
        return new Vector4(HeightmapComponent.HeightAtPosition(x,y),0,0,0);
    }
    
    // Reloads the world with new settings on settings change
    public void ReloadWorld() {

        // Remove old world
        foreach (BinaryTreeNode root in roots) {
            if (root != null) {
                root.Merge();
                ChunkManager.RemoveChunk(root);
                BinaryTreeNode.nodes.Remove(root.nodeID.node);
            }
        }

        // Assign new settings and create new world
        settings = m_settings;
        roots[0] = new BinaryTreeNode(
            new NeighborIDs { left = 0, right = 0, edge = 6, node = 4 }, 
            transform.position, 
            0, 
            settings.levelsOfDetail
        );
        // roots[1] = new BinaryTreeNode(
        //     new NeighborIDs { left = 0, right = 0, edge = 0, node = 5 }, 
        //     transform.position, 
        //     90, 
        //     settings.levelsOfDetail
        // );
        roots[2] = new BinaryTreeNode(
            new NeighborIDs { left = 0, right = 0, edge = 4, node = 6 }, 
            transform.position, 
            180, 
            settings.levelsOfDetail
        );
        // roots[3] = new BinaryTreeNode(
        //     new NeighborIDs { left = 0, right = 0, edge = 0, node = 7 }, 
        //     transform.position, 
        //     270, 
        //     settings.levelsOfDetail
        // );
    }

    // Validate settings on change
    private void OnValidate() {
        if (m_settings == null) {
            Debug.LogError("WorldSettingsSO not assigned in World");
            return;
        }

        ReloadWorld();
    }

    // Unsubscribe from event on destroy
    private void OnDestroy() {
        m_settings.OnSettingsChange -= ReloadWorld;
    }
}