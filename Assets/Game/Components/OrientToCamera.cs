using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientToCamera : MonoBehaviour
{
    private Transform targetCamera;
  
    private void OnEnable() {
        targetCamera = Camera.main.transform;
    }

    private void LateUpdate() {
        transform.LookAt(targetCamera);
        transform.RotateAround(transform.position, transform.up, 180f);
    }
}