using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGridManager : MonoBehaviour
{
    public Vector2 chunkSize { get; private set; }
    public Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();
    [SerializeField] private PoissonPlacement poissonPlacement;

    public class Chunk
    {
        public Vector2 worldSpacePosition;
        public Matrix4x4[] transformMatrices;
        public int[] objType;
    }

    private void Start()
    {
        chunkSize = new Vector2(500, 500);
    }


    // Generate grid of chunks given radius around the PlayerPosition
    public void GenerateGrid(Vector2 playerPosition, float radius)
    {
        Vector2 currentChunk = new Vector2(Mathf.Floor(playerPosition.x / chunkSize.x), Mathf.Floor(playerPosition.y / chunkSize.y));

        // Generate chunks around player that are less than a given radius
        for (int x = -30; x < 30; x++) {
            for (int y = -30; y < 30; y++) {
                if (Vector2.Distance(currentChunk, new Vector2(currentChunk.x + x, currentChunk.y + y)) < radius) {
                    Vector2 chunkPos = new Vector2(currentChunk.x + x, currentChunk.y + y);
                    if (!ChunkExists(chunkPos)) {
                        GenerateChunk(chunkPos);
                    }
                } else {
                    Vector2 chunkPos = new Vector2(currentChunk.x + x, currentChunk.y + y);
                    if (ChunkExists(chunkPos)) {
                        UnloadChunk(chunkPos);
                    }
                }
            }
        }

    }

    // Check if chunk has already been generated
    private bool ChunkExists(Vector2 position)
    {
        return chunks.ContainsKey(position);
    }

    // Unload chunk
    private void UnloadChunk(Vector2 position)
    {
        // Garbage handler should remove the chunk
        chunks.Remove(position);
    }

    // Load chunk
    private void GenerateChunk(Vector2 position)
    {
        Chunk chunk = new();
        chunk.worldSpacePosition = position * chunkSize;

        // Generate points and orientations
        chunk.transformMatrices = poissonPlacement.GeneratePoints(chunk.worldSpacePosition.x, chunk.worldSpacePosition.y, chunkSize.x, chunkSize.y);

        // Assign object types randomly
        chunk.objType = new int[chunk.transformMatrices.Length];
        for (int i = 0; i < chunk.transformMatrices.Length; i++)
        {
            chunk.objType[i] = Random.Range(0, 3);
        }

        chunks.Add(position, chunk);
    }

    void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector2, Chunk> chunk in chunks)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(chunk.Value.worldSpacePosition.x, 0, chunk.Value.worldSpacePosition.y), new Vector3(chunkSize.x, 0, chunkSize.y));

            Gizmos.color = Color.blue;
            foreach (Matrix4x4 point in chunk.Value.transformMatrices)
            {
                Gizmos.DrawSphere(point.GetPosition(), 10f);
            }
        }
    }

    
}
