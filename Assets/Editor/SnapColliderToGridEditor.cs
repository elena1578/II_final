using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BoxCollider2D))]
public class SnapColliderToGridEditor : Editor
{
    const float gridSize = 0.32f;
    static readonly Vector3 gridOffset = new Vector3(0.23f, -2.75f, 0f);

    public override void OnInspectorGUI()
    {
        BoxCollider2D col = (BoxCollider2D)target;
        
        // keep at the top of the inspector : )
        // EditorGUILayout.LabelField("Aligns collider to center of grid defined in GridMovementController", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Snap To Grid"))
        {
            SnapCollider(col);
        }
        
        base.OnInspectorGUI();
    }

    void SnapCollider(BoxCollider2D col)
    {
        Transform t = col.transform;

        // snap position to nearest grid point
        Vector3 pos = t.position;
        pos.x = Mathf.Round((pos.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        pos.y = Mathf.Round((pos.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
        t.position = pos;

        // snap size to nearest grid multiple
        Vector2 size = col.size;
        size.x = Mathf.Round(size.x / gridSize) * gridSize;
        size.y = Mathf.Round(size.y / gridSize) * gridSize;
        col.size = size;

        EditorUtility.SetDirty(col);
    }
}