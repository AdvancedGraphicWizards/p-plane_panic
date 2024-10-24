using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem.Utilities;

public class SquashAlongVectorComponent : MonoBehaviour
{
    public Vector3 squashDirection = new Vector3(1, 1, 0); // Arbitrary squash vector
    public float squashFactor = 0.5f; // Factor to scale along the direction
    float stretchFactor = 0f; // Factor to scale along the direction

    private GameObject m_scaleParent;
    private GameObject m_rotationGrandparent;

    private Vector3 previousPos;

    void Start()
    {
        m_scaleParent = new GameObject($"scaleParent<{transform.name}>");
        m_rotationGrandparent = new GameObject($"rotationGrandparent<{transform.name}>");

        // set scaleparents forward direction to squashdirection
        m_scaleParent.transform.forward = squashDirection;
        m_scaleParent.transform.localPosition = transform.localPosition;

        // set rotationgrandparents rotation and position to the objects original position and rotation
        m_rotationGrandparent.transform.rotation = transform.rotation;
        m_rotationGrandparent.transform.localPosition = m_scaleParent.transform.position;

        // Set parent hiearchy
        transform.parent = m_scaleParent.transform;
        m_scaleParent.transform.parent = m_rotationGrandparent.transform;
    }

    private void Update()
    {
        Vector3 movementVector = transform.position - previousPos;
        float stretchFactorTarget = Vector3.Magnitude(movementVector)*3f;
        stretchFactor = Mathf.Max(Mathf.Lerp(stretchFactor, stretchFactorTarget, Time.deltaTime),1);
        StretchAlongVector(movementVector, stretchFactor, true);
        previousPos = transform.position;
    }

    public void StretchAlongVector(Vector3 direction, float factor, bool maintain_volume)
    {
        if (Vector3.zero == direction) direction = Vector3.one;
        Vector3 scaleVector;
        m_scaleParent.transform.forward = direction;
        transform.rotation = m_rotationGrandparent.transform.rotation;

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
