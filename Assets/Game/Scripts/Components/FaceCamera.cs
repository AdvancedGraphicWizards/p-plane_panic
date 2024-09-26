using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void Update()
    {
        // Update to always face the camera
        transform.LookAt(Camera.main.transform);
    }
}
