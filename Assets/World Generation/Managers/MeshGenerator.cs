using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    // Generates a new mesh - SOURCE: https://gist.github.com/runewake2/b382ecd3abc3a32b096d08cc69c541fb, modified
    public static Mesh GenerateMesh(float vertexDistance, int vertexCount) {

        // Mesh data
        Mesh mesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        // Vertices, normals and uv generation
        for (int x = 0; x < vertexCount; x++) {
            for (int z = 0; z < vertexCount; z++) {
                float xCord = vertexDistance * x - vertexDistance * (vertexCount - 1) * 0.5f;
                float zCord = vertexDistance * z - vertexDistance * (vertexCount - 1) * 0.5f;
                float yCord = 0;
                
                vertices.Add(new Vector3(xCord, yCord, zCord));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)vertexCount, z / (float)vertexCount));
            }
        }

        // Triangle generation
        var triangles = new List<int>();
        for (int i = 0; i < vertexCount * vertexCount - vertexCount; ++i) {
            if ((i + 1) % vertexCount == 0) {
                continue;
            }
            triangles.AddRange(new List<int>() {
                i + 1 + vertexCount, i + vertexCount, i,
                i, i + 1, i + vertexCount + 1
            });
        }

        // Move data to mesh
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        return mesh;
    }

    public static Mesh GenerateTriangleMesh(float vertexDistance, int vertexCount, float rotation) {
        
        // Mesh data
        Mesh mesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        // Vertices, normals and uv generation
        for (int x = 0; x < vertexCount; x++) {
            for (int z = 0; z < vertexCount - x; z++) {
                float xCord = vertexDistance * x - vertexDistance * (vertexCount - 1) * 0.5f;
                float zCord = vertexDistance * z - vertexDistance * (vertexCount - 1) * 0.5f;
                float yCord = 0;
                
                // Add vertices, normals and uvs
                vertices.Add(Quaternion.Euler(0, rotation - 45f, 0) * new Vector3(xCord, yCord, zCord));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)vertexCount, z / (float)vertexCount));
            }
        }

        // Triangle generation
        var triangles = new List<int>();
        int i = 0;
        for (int x = 0; x < vertexCount - 1; x++) {
            for (int z = 0; z < vertexCount - 2 - x; z++) {
                triangles.AddRange(new List<int>() {
                    i, i + 1, i + (vertexCount - x),
                    i + (vertexCount - x), i + 1, i + (vertexCount - x) + 1
                });
                i++;
            }
            triangles.AddRange(new List<int>() {
                i, i + 1, i + (vertexCount - x)
            });
            i+=2;
        }

        // Move data to mesh
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        return mesh;
    }
}
