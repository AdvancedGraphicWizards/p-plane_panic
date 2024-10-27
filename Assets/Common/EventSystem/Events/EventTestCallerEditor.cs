using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EventTestCaller))]
public class EventTestCallerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        EventTestCaller caller = (EventTestCaller)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Call event")) {
            caller.Call();
        }
    }
}
