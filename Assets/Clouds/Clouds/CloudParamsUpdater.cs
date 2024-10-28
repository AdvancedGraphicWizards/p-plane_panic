using System.Collections.Generic;
using UnityEngine;

public class CloudParamsUpdater : MonoBehaviour
{
    [SerializeField] private CloudParams options;

    private List<Shape> shapes = new();

    void Start() {
        options.UploadInternal();

        foreach (Transform t in transform) {
            t.gameObject.SetActive(false);
        }
        UpdateShapes();
    }


    void Update() {
        options.UploadInternal();

        UpdateShapes();   
    }

    void UpdateShapes() {
        shapes.Clear();
        // Separate clouds
        foreach (Transform cloud in transform) {
            // Spheres per cloud
            foreach (Transform sphere in cloud) {
                shapes.Add(new Shape(sphere.position, sphere.localScale));
            }
        }
        options.UploadShape(shapes);
    }
}