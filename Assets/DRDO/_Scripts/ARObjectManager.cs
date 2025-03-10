using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Michsky.MUIP;

public class ARObjectManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public CustomDropdown objectDropdown; // Michsky Modern UI Dropdown
    public GameObject pointerObject; // Assign Pointer in Inspector
    public Material onSelectedMaterial; // Material for selected object
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject selectedObject = null; // The currently selected object
    private Material originalMaterial; // Stores the previous material before selection

    void Update()
    {
        // Update pointer position based on AR Raycast
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            pointerObject.SetActive(true);
            pointerObject.transform.position = hitPose.position;
            pointerObject.transform.rotation = hitPose.rotation;
        }
        else
        {
            pointerObject.SetActive(false);
        }

        HandleTouchInput();

        // Apply scaling & rotation ONLY if an object is selected
        if (selectedObject != null)
        {
            HandleScalingAndRotation();
        }
    }

    public void PlaceObject()
    {
        if (pointerObject.activeSelf)
        {
            GameObject newObject = null;

            // Michsky UI uses 'selectedItemIndex' instead of TMP Dropdown 'value'
            switch (objectDropdown.selectedItemIndex)
            {
                case 1:
                    newObject = Instantiate(cubePrefab, pointerObject.transform.position, pointerObject.transform.rotation);
                    break;
                case 2:
                    newObject = Instantiate(spherePrefab, pointerObject.transform.position, pointerObject.transform.rotation);
                    break;
            }

            if (newObject != null)
            {
                SelectObject(newObject); // Set new object as selected
            }
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                SelectObject(hit.collider.gameObject);
            }
        }
    }

    private void HandleScalingAndRotation()
    {
        if (selectedObject == null || Input.touchCount < 2) return; // Prevents modifying unselected objects

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // Scaling logic (Pinch Gesture)
        float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float scaleFactor = currentDistance / prevDistance;

        if (!float.IsNaN(scaleFactor) && scaleFactor > 0) // Avoids scaling issues
        {
            selectedObject.transform.localScale *= scaleFactor;
        }

        // Rotation logic (Two-Finger Gesture)
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevAngle = Mathf.Atan2(touch1PrevPos.y - touch2PrevPos.y, touch1PrevPos.x - touch2PrevPos.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(touch1.position.y - touch2.position.y, touch1.position.x - touch2.position.x) * Mathf.Rad2Deg;

        float angleDifference = currentAngle - prevAngle;
        selectedObject.transform.Rotate(Vector3.up, -angleDifference);
    }

    private void SelectObject(GameObject newSelectedObject)
    {
        // Reset material of previously selected object
        if (selectedObject != null && originalMaterial != null)
        {
            Renderer prevRenderer = selectedObject.GetComponent<Renderer>();
            if (prevRenderer != null)
            {
                prevRenderer.material = originalMaterial; // Restore original material
            }
        }

        // Set new selection
        selectedObject = newSelectedObject;

        // Apply selection effect using the provided onSelectedMaterial
        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material; // Store original material
            renderer.material = onSelectedMaterial; // Apply selected material
        }
    }

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            selectedObject = null;
        }
    }
}
