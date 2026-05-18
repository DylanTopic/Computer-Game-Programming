using UnityEngine;

// rotates this object to always face the camera
public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        transform.rotation = cam.transform.rotation;
    }
}