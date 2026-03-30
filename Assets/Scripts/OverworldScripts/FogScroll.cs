using UnityEngine;
using UnityEngine.UI;


public class FogScroll : MonoBehaviour
{
    public float scrollMovementMultiplier = 0.02f;  // how much fog moves relative to camera
    public float driftSpeed = 0.01f; 

    private RawImage img;  // have to use raw img vs. img so that UV can be manipulated for scrolling effect
    private Vector2 offset;
    private Vector3 lastCameraPos;
    private Transform cam;

    private void Start()
    {
        img = GetComponent<RawImage>();
        lastCameraPos = Camera.main.transform.position;
        cam = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 camPos = cam.position;
        Vector3 delta = camPos - lastCameraPos;

        // move fog opposite to camera movement
        offset.x -= delta.x * scrollMovementMultiplier;
        offset.y -= delta.y * scrollMovementMultiplier;
        offset.x += driftSpeed * Time.deltaTime;  // drifting effect

        img.uvRect = new Rect(offset, Vector2.one);  // update UV offset for scrolling

        lastCameraPos = camPos;
    }
}