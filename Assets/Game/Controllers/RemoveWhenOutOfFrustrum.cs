using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveWhenOutOfFrustrum : MonoBehaviour
{
    private float m_counter = 0;

    private void Update()
    {
        m_counter += 1 * Time.deltaTime;
        if (m_counter > 5)
            Destroy(gameObject);
    }
    private void OnBecameInvisible()
    {
        //Destroy(gameObject);  //Not Working
        //Debug.Log(gameObject.name + " has left the camera frustum."); 
    }
}
