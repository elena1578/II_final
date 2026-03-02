using UnityEngine;
using UnityEditor;


[InitializeOnLoad]  // ensure static constructor automatically runs (so tool is always active in editor)
public static class PlaceTreeOverlayTool
{
    private static string spritePath = "Assets/Assets/MapDecorations/spr_tree_tuft.png";
    private static string parentName = "TreeOverlays";

    private static bool aHeld = false;

    static PlaceTreeOverlayTool() => SceneView.duringSceneGui += OnSceneGUI;

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // track a key state
        // keydown = key pressed [this frame], keyup = key released [this frame]
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.A)
        {
            aHeld = true;
        }
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.A)
        {
            aHeld = false;
        }

        // shift + A + left click
        if (aHeld && e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            PlaceTreeAtMouse(e);
            e.Use();
        }
    }

    private static void PlaceTreeAtMouse(Event e)
    {
       // convert mouse pos -> world pos by using ray origin as placement point
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 worldPos = ray.origin;
        worldPos.z = 0f;

        // find sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError("[PlaceTreeOverlayTool] Sprite not found at path: " + spritePath);
            return;
        }

        // create tree GameObject and parent under "TreeOverlays"
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
            parent = new GameObject(parentName);

        GameObject tree = new GameObject("TreeOverlay");
        tree.transform.position = worldPos;
        tree.transform.SetParent(parent.transform);

        // add sr and set sorting layer to "Tree"
        SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Tree";

        // allow undo
        Undo.RegisterCreatedObjectUndo(tree, "Place tree overlay");
    }
}