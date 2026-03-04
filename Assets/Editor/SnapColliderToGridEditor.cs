using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BoxCollider2D))]
public class SnapColliderToGridEditor : Editor
{
    const float gridSize = 0.32f;
    static readonly Vector3 gridOffset = new Vector3(0.23f, -2.75f, 0f);
    Editor defaultEditor;

    void OnEnable() => defaultEditor = CreateEditor(targets, System.Type.GetType("UnityEditor.BoxCollider2DEditor, UnityEditor"));

    void OnDisable()
    {
        if (defaultEditor != null)
            DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI()
    {
        // keep at the top of the inspector : )
        if (GUILayout.Button("Snap To Grid"))
        {
            foreach (BoxCollider2D col in targets)
            {
                SnapCollider(col);
            }
        }
        
        EditorGUILayout.Space();

        if (defaultEditor != null)
            defaultEditor.OnInspectorGUI();
    }

    void SnapCollider(BoxCollider2D col)
    {
        Transform t = col.transform;

        // snap position to center of entered grid cell(s)
        Vector3 pos = t.position - gridOffset;
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
        t.position = pos + gridOffset;

        // snap size to nearest grid multiple
        Vector2 size = col.size;
        size.x = Mathf.Round(size.x / gridSize) * gridSize;
        size.y = Mathf.Round(size.y / gridSize) * gridSize;
        col.size = size;

        EditorUtility.SetDirty(col);  // save

        // undo
        Undo.RegisterCompleteObjectUndo(col, "Snap collider to grid");
    }
}