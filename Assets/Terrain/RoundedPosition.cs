using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundedPosition : MonoBehaviour {

    [Tooltip("The target transform to round the position to.")]
    [SerializeField] private Transform targetPosition;

    [Tooltip("The size of the step to round the position to.")]
    [SerializeField] private float stepSize = 8f;

    // Start is called before the first frame update
    void Start() {
        Debug.Assert(stepSize > 0, "Step size must be greater than 0.", this);
        Debug.Assert(targetPosition != null, "Target position must be set.", this);
    }

    // Update is called once per frame
    void Update() {

        // Round the position to the nearest step size.
        transform.position = new Vector3(
            Mathf.Round(targetPosition.position.x / stepSize) * stepSize,
            0,
            Mathf.Round(targetPosition.position.z / stepSize) * stepSize
        );
    }
}
