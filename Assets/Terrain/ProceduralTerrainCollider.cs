using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ProceduralTerrainCollider : MonoBehaviour {

    [Tooltip("The targets to generate the collider around")]
    public GameObject[] targets;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        // Loop through each target
        foreach (GameObject target in targets) {

            // Get the bounds of the targets
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            for (int n = 1; n < renderers.Length; n++) {
                bounds.Encapsulate(renderers[n].bounds);
            }

            // Create the vertices
            Vector3[] vertices = new Vector3[] {
                new Vector3(bounds.min.x, 0, bounds.min.z),
                new Vector3(bounds.min.x, 0, bounds.max.z),
                new Vector3(bounds.max.x, 0, bounds.min.z),
                new Vector3(bounds.max.x, 0, bounds.max.z)
            };
            for (int n = 0; n < vertices.Length; n++) {
                vertices[n].y = WorldGeneration.SampleHeight(vertices[n].x, vertices[n].z);
            }

            // Create the triangles
            int[] triangles = new int[] {
                0, 1, 2,
                2, 1, 3
            };

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Set the mesh
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
