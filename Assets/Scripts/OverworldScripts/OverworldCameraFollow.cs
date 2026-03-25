using UnityEngine;
using UnityEngine.SceneManagement;


public class OverworldCameraFollow : MonoBehaviour
{
    private Transform target;  // player
    private float camHalfHeight;
    private float camHalfWidth;

    private float minX, maxX, minY, maxY;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SetupCameraBounds();

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        FindPlayer();
        SetupCameraBounds();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("[CameraFollow] Player not found");
    }

    private void SetupCameraBounds()
    {
        GameObject boundsObj = GameObject.Find("CameraBounds");
        if (boundsObj == null)
        {
            Debug.LogWarning("[CameraFollow] CameraBounds object not found in scene.");
            return;
        }

        BoxCollider2D box = boundsObj.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning("[CameraFollow] No BoxCollider2D found on CameraBounds object.");
            return;
        }

        minX = box.bounds.min.x;
        maxX = box.bounds.max.x;
        minY = box.bounds.min.y;
        maxY = box.bounds.max.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x;
        float targetY = target.position.y;

        float clampedX = Mathf.Clamp(targetX, minX + camHalfWidth, maxX - camHalfWidth);
        float clampedY = Mathf.Clamp(targetY, minY + camHalfHeight, maxY - camHalfHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}