using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class SquashAlongVectorComponent : MonoBehaviour
{
    public Vector3 squashDirection = new Vector3(1, 1, 0); // Arbitrary squash vector
    public float squashFactor = 0.5f; // Factor to scale along the direction

    private GameObject m_scaleParent;
    private GameObject m_rotationGrandparent;

    void Start()
    {
        m_scaleParent = new GameObject($"scaleParent<{transform.name}>");
        m_rotationGrandparent = new GameObject($"rotationGrandparent<{transform.name}>");

        // set scaleparents forward direction to squashdirection
        m_scaleParent.transform.forward = squashDirection;

        // set rotationgrandparents rotation and position to the objects original position and rotation
        m_rotationGrandparent.transform.rotation = transform.localRotation;
        m_rotationGrandparent.transform.localPosition = transform.localPosition;

        // Set parent hiearchy
        transform.parent = m_scaleParent.transform;
        m_scaleParent.transform.parent = m_rotationGrandparent.transform;

        StretchAlongVector(Vector3.one, 2, false);
    }

    public void StretchAlongVector(Vector3 direction, float factor, bool maintain_volume)
    {
        Vector3 scaleVector;

        if (maintain_volume) {
            float inverseFactor = 1/Mathf.Sqrt(1 + factor);
            scaleVector = new Vector3(1*inverseFactor,1*inverseFactor,1*factor);
        }
        else {
            scaleVector = new Vector3(1,1,1*factor);
        }
        m_scaleParent.transform.localScale = new Vector3(1,1,1*factor);
    }
}
