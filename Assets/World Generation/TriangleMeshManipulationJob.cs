using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public struct TriangleMeshManipulationIJob : IJobParallelFor {

    // Mesh variables
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;

    // Triangle mesh variables
    public float vertexDistance;
    public int vertexCount;
    public float rotation;
    public Vector3 position;

    // Execute job for each vertex
    public void Execute(int index) {

        // Calculate x and z coordinates
        int length = vertexCount;
        int x = 0, z = 0;
        int i = 0;
        while (i + length <= index) {
            x++;
            i += length;
            length--;
        }
        z = index - i;

        // Calculate vertex position
        Vector3 vertex = Quaternion.Euler(0, rotation - 45f, 0) * new Vector3(
            vertexDistance * x - vertexDistance * (vertexCount - 1) * 0.5f,
            0,
            vertexDistance * z - vertexDistance * (vertexCount - 1) * 0.5f
        );
        vertex.y = World.GetHeight(vertex.x + position.x, vertex.z + position.z);
        normals[index] = World.GetNormal(vertex.x + position.x, vertex.y, vertex.z + position.z);

        // Set vertices, normals and uvs
        vertices[index] = vertex;
        uvs[index] = new Vector2(x / (float)vertexCount, z / (float)vertexCount);
    }
}

public class TriangleMeshManipulationJob : MonoBehaviour {

    // Job and handle
    private TriangleMeshManipulationIJob job;
    public JobHandle handle;

    // Vertex manipulation variables
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;

    public void Initialize(Mesh mesh) {
        vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
        normals = new NativeArray<Vector3>(mesh.normals, Allocator.Persistent);
        uvs = new NativeArray<Vector2>(mesh.uv, Allocator.Persistent);
    }

    public void UpdateMesh(float vertexDistance, int vertexCount, float rotation) {

        // Create job
        job = new TriangleMeshManipulationIJob() {
            vertices = vertices,
            normals = normals,
            uvs = uvs,
            vertexDistance = vertexDistance,
            vertexCount = vertexCount,
            rotation = rotation,
            position = transform.position
        };

        // Schedule job
        handle = job.Schedule(vertices.Length, 1);
        handle.Complete();

        // Set mesh data
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.SetVertices(vertices);
        //mesh.RecalculateTangents();
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();

        // Update collider
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void OnDestroy() {
        vertices.Dispose();
        normals.Dispose();
        uvs.Dispose();
    }
}