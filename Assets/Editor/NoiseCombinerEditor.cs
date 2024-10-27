using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseCombiner))]
public class NoiseCombinerEditor : Editor
{
    public string filePath = "";
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NoiseCombiner c = (NoiseCombiner)target;

        GUILayout.Space(10);

        GUILayout.Label("File path (or drag and drop folder)");
        Rect textFieldRect = EditorGUILayout.GetControlRect();
        filePath = EditorGUI.TextField(textFieldRect, filePath);

        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (textFieldRect.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            filePath = AssetDatabase.GetAssetPath(draggedObject);
                        }
                    }
                    Event.current.Use();
                }
                break;
        }


        if (GUILayout.Button("Save to file"))
        {
            Texture3D texture = c.Combine();
            AssetDatabase.CreateAsset(texture, filePath + "/texture.asset");
        }
    }
}
