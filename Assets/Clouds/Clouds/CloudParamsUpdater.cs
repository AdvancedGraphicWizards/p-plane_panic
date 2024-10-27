
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
        foreach (Transform t in transform) {
            shapes.Add(new Shape(t.position, t.localScale));
        }
        options.UploadShape(shapes);
    }
}