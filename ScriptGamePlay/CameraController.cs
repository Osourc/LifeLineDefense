using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {

    public BoxCollider2D boundaryCollider;

    Vector3 touchStart;
    public float zoomOutMin = 1;
    public float zoomOutMax = 4;
    private bool dragStartedInUI = false;


    void Update () 
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (IsTouchingUI()) {
                dragStartedInUI = true;
                return;
            } else {
                dragStartedInUI = false;
                touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if(dragStartedInUI) {
            return;
        }

        if(Input.touchCount == 2){
            Debug.Log("Starting Zoom");
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            zoom(difference * 0.01f);
        }
        else if(Input.GetMouseButton(0))
        {
            //Debug.Log("Panning");
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = Camera.main.transform.position + direction;

            Bounds bounds = boundaryCollider.bounds;
            newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x, bounds.max.x);
            newPosition.y = Mathf.Clamp(newPosition.y, bounds.min.y, bounds.max.y);

            Camera.main.transform.position = newPosition;
        }

        zoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    void zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }

    // Helper function to check if a touch or click is over a UI element
    private bool IsTouchingUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        return false;
    }
}
