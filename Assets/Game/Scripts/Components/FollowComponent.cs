using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowComponent : MonoBehaviour
{
    [SerializeField] private Transform m_followTransform;
    [SerializeField] private string m_followTransformName;


    void Start()
    {
        TryFindTargetTransform();
    }

    private void TryFindTargetTransform(){
        if (m_followTransform == null) {
            if (GameObject.Find(m_followTransformName) != null) {
                m_followTransform = GameObject.Find(m_followTransformName).transform;
            }
        }
    }

    void Update()
    {
        TryFindTargetTransform();

        if (m_followTransform != null)
            transform.position = m_followTransform.position;
        if (m_followTransform != null)
            transform.rotation = m_followTransform.rotation;
    }
}
