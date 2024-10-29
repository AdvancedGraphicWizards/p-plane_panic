using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(WaterPlaneManager))]
public class WaterPlaneManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaterPlaneManager waterManager = (WaterPlaneManager)target;

        if (GUILayout.Button("Reinitialise Grid"))
        {
            waterManager.DeleteGrid();
            waterManager.InitialiseGrid();
        }
    }
}
