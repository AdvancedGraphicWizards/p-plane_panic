using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlaneCrashController : MonoBehaviour
{
    public VoidEvent Event;
    public UnityEvent Response;
    [SerializeField] private string terrainTag = "";

    private void Update()
    {
        if (transform.position.y < -10) CallResponse();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(terrainTag)){
            CallResponse();
        }
    }

    void OnEnable()
    {
        Event.Subscribe(CallResponse);
    }

    void OnDisable()
    {
        Event.Unsubscribe(CallResponse);
    }

    private void CallResponse()
    {
        Response.Invoke();
    }
    public void TriggerLoseEvent()
    {
        CallResponse();
        return;
    }
}