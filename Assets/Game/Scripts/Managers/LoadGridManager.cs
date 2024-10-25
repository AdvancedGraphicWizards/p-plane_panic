using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGridManager : MonoBehaviour
{
    private Vector2 chunkSize;
    private Vector2 currentPlayerView;
    private Vector2 currentPlayerPos;
    private Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();
    private PoissonPlacement poissonPlacement;

    private class Chunk
    {
        public Vector2 worldSpacePosition;
        public Vector3[] points;
        public Quaternion[] orientations;
        public int[] objType;
    }

    private void Start()
    {
        chunkSize = new Vector2(10, 10);
    }



    // Generate grid of chunks given radius around the PlayerPosition
    private void GenerateGrid(Vector2 playerPosition, Vector2 radius)
    {
        currentPlayerPos = playerPosition;
        Vector2 currentChunk = new Vector2(Mathf.Floor(playerPosition.x / chunkSize.x), Mathf.Floor(playerPosition.y / chunkSize.y));

        // Generate chunks around player that are less than a given radius
        for (int x = -30; x < 30; x++) {
            for (int y = -30; y < 30; y++) {
                if (Vector2.Distance(currentChunk, new Vector2(currentChunk.x + x, currentChunk.y + y)) < 15) {
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
        (chunk.points, chunk.orientations) = poissonPlacement.GeneratePoints(chunk.worldSpacePosition.x, chunk.worldSpacePosition.y);

        // Assign object types randomly
        chunk.objType = new int[chunk.points.Length];
        for (int i = 0; i < chunk.points.Length; i++)
        {
            chunk.objType[i] = Random.Range(0, 3);
        }

        chunks.Add(position, chunk);
    }

    
}
