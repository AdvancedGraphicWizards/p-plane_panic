using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "EventTestCaller", menuName = "ScriptableObject/EventTestCaller")]
public class EventTestCaller : ScriptableObject {

    [SerializeField] private UnityEvent e;

    public void Call() {
        Debug.Log("HELLO");
        e?.Invoke();
    }
}
