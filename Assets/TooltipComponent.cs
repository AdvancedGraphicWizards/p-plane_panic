using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TooltipComponent : MonoBehaviour
{
    public FloatEvent CompleteEvent;
    public int requiredCompleteEvents;
    private int currentCompleteEvents = 0;
    public string targetObjectName;
    public GameObject planeObject;

    private void Update() {
        if (targetObjectName != null) {
             if (GameObject.Find(targetObjectName) != null)
            {
                transform.SetParent(planeObject.transform);
                transform.position = GameObject.Find(targetObjectName).transform.position;
            }
        }
    }

    private void OnEnable()
    {
        CompleteEvent.Subscribe(EventCallback);
    }

    private void OnDisable()
    {
        CompleteEvent.Unsubscribe(EventCallback);
    }

    private void EventCallback(float a)
    {
        currentCompleteEvents++;
        if (currentCompleteEvents >= requiredCompleteEvents) {
            gameObject.SetActive(false);
        }
    }
}
