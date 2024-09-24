using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlaneCrashController : MonoBehaviour
{
    public VoidEvent Event;
    public UnityEvent Response;

    private void Update()
    {
        if (transform.position.y < 0) CallResponse();
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
