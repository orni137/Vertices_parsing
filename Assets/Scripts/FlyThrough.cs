/*
---------------------- Simplest Camera Control ----------------------
-- FlyThrough.cs
-------------------------------------------------------------------
*/

using UnityEngine;

public class FlyThrough : MonoBehaviour {

    public float lookSpeed = 4.0f;
    public float moveSpeed = 2.0f;

    float rotationX = 0.0f;
    float rotationY = 0.0f;
    Quaternion newRot = Quaternion.identity;
    Vector3 newPos = Vector3.zero;

    public void addShift (Vector3 shift) {
        transform.position += shift;
        newPos += shift;
    }

    void Start () {
        newRot = transform.localRotation;
        newPos = transform.position;
        rotationX = transform.localRotation.eulerAngles.y;
        rotationY = -transform.localRotation.eulerAngles.x;
        if (rotationY > 90.0f) {
            rotationY -= 360.0f;
        }
    }

    void LateUpdate ()  {
        if (Input.GetMouseButton (1)) {
            rotationX += Input.GetAxis ("Mouse X") * lookSpeed;
            rotationY += Input.GetAxis ("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp (rotationY, -90, 90);
        }
        newRot = Quaternion.AngleAxis(rotationX, Vector3.up) * Quaternion.AngleAxis(rotationY, Vector3.left);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, newRot, 0.1f);

        float mul = 1.0f;
        if (Input.GetKey(KeyCode.LeftShift))
            mul = 5.0f;
            
        float up = 0.0f;
        if (Input.GetKey (KeyCode.E)) up += 1.0f;
        if (Input.GetKey (KeyCode.Q)) up -= 1.0f;

        newPos += transform.forward * moveSpeed * mul * Input.GetAxis ("Vertical") +
        transform.right * moveSpeed * mul * Input.GetAxis ("Horizontal") +
        transform.up * moveSpeed * mul * up;
        
        transform.position = 0.9f*transform.position + 0.1f*newPos;
    }

}
