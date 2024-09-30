using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Creates a circle of prefabs that can be rotated and adjusted
/// </summary>

public class CirclePlacementComponent : MonoBehaviour
{
    [Header("Circle of Objects Parameters")]
    [SerializeField] private float m_circleRadius = 1f;
    [SerializeField] private int m_objectQuantity = 10;
    [SerializeField] private float m_rotationOffset = 0;
    [SerializeField] private bool m_rotates = false;
    [SerializeField] private float m_rotationPeriod = 1f;
    [SerializeField] private Vector3 m_circleOffset = Vector3.zero;

    [Header("GameObject References")]
    [SerializeField] private Transform m_targetParent;
    [SerializeField] private GameObject m_circleObjectPrefab;

    private Transform[] m_objects;
    private float m_angleOffset;
    private float m_currentRotationOffset;


    void Start()
    {   
        PopulateCircleArray();
    }

    void Update()
    {
        if (!m_rotates) return;

        // rotate components according to period time
        m_currentRotationOffset += (2 * Mathf.PI * (Time.deltaTime / m_rotationPeriod) ) % (2 * Mathf.PI);
        UpdateCircleArrayPositions();
    }

    // Populate circle array with objects evenly spaced around the circle
    [ContextMenu("PopulateCircleArray")]
    private void PopulateCircleArray() {
        if (m_circleObjectPrefab == null) 
        {
            Debug.LogError("Missing object Prefab to populate circle");
            return;
        }
        if (m_targetParent == null) m_targetParent = this.transform;

        DeleteCircleArrayObjects();

        // Conversion of variables to radians
        m_angleOffset = 2*Mathf.PI/m_objectQuantity;
        m_currentRotationOffset = m_rotationOffset * Mathf.PI / 180;

        float currAngle = m_rotationOffset * Mathf.PI / 180;
        for (int i = 0; i < m_objectQuantity; i++)
        {
            m_objects[i] = Instantiate(
                m_circleObjectPrefab,
                m_targetParent.position + new Vector3( Mathf.Cos(currAngle),  Mathf.Sin(currAngle), 0f) * m_circleRadius,
                Quaternion.identity, 
                m_targetParent
                ).transform;

            currAngle += m_angleOffset;
        }
    }

    [ContextMenu("DeleteObjects")]
    private void DeleteCircleArrayObjects() {
        if (m_objects != null) {
            for (int i = 0; i < m_objects.Length; i++)
            {
                if (m_objects[i] != null)
                    DestroyImmediate(m_objects[i].gameObject);
            }
        }

        m_objects = new Transform[m_objectQuantity];
    }

    // Update circle array positions based on populated values
    private void UpdateCircleArrayPositions() {

        float currAngle = m_currentRotationOffset;

        for (int i = 0; i < m_objectQuantity; i++)
        {
            m_objects[i].localPosition = new Vector3( Mathf.Cos(currAngle),  Mathf.Sin(currAngle), 0f) * m_circleRadius;

            currAngle += m_angleOffset;
        }
    }
}
