using System.Collections.Generic;
using UnityEngine;
using Michsky.MUIP; // Import Modern UI Pack
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class AnimatedARObjectSelection : MonoBehaviour
{
    public GameObject placementMarker; // Pointer object (AR Raycast Pointer)
    public List<GameObject> animatedPrefabs; // Assign animated 3D Prefabs in Inspector
    public GameObject listViewPanel; // Assign ListView Panel (Modern UI)
    public ButtonManager hamburgerButton; // Modern UI Pack ButtonManager
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject selectedObject = null; // The currently selected object
    private AnimatedObjectController animatedObjectController; // Reference to animation manager

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        animatedObjectController = GetComponent<AnimatedObjectController>(); // Ensure we have an animation controller

        // Ensure ListViewPanel is disabled initially
        listViewPanel.SetActive(false);

        //// ✅ Using `OnClick()` instead of `clickEvent`
        //hamburgerButton.GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    listViewPanel.SetActive(!listViewPanel.activeSelf);
        //});

        //// Assign click event to all buttons inside ListView dynamically
        //Transform listContainer = listViewPanel.transform.Find("Scroll Area/List");
        //if (listContainer != null)
        //{
        //    foreach (Transform item in listContainer)
        //    {
        //        Button button = item.GetComponent<Button>(); // Get Modern UI Button
        //        if (button != null)
        //        {
        //            int index = item.GetSiblingIndex(); // Get index of button
        //            button.onClick.AddListener(() => PlaceAnimatedObject(index));
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.LogError("❌ List container not found! Make sure 'Scroll Area/List' exists in your ListView UI.");
        //}
    }

    public void AssignMenuListeners()
    {
        hamburgerButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            listViewPanel.SetActive(!listViewPanel.activeSelf);
        });

        // Assign click event to all buttons inside ListView dynamically
        Transform listContainer = listViewPanel.transform.Find("Scroll Area/List");
        if (listContainer != null)
        {
            foreach (Transform item in listContainer)
            {
                Button button = item.GetComponent<Button>(); // Get Modern UI Button
                if (button != null)
                {
                    int index = item.GetSiblingIndex(); // Get index of button
                    button.onClick.AddListener(() => PlaceAnimatedObject(index));
                }
            }
        }
        else
        {
            Debug.LogError("❌ List container not found! Make sure 'Scroll Area/List' exists in your ListView UI.");
        }
    }

    void Update()
    {
        // Update Pointer Position (Ensure it moves with AR Raycast)
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            placementMarker.SetActive(true);
            placementMarker.transform.position = hitPose.position;
            placementMarker.transform.rotation = hitPose.rotation;
        }
        else
        {
            placementMarker.SetActive(false);
        }

        // Allow Only Selected Object to Rotate & Scale
        if (selectedObject != null)
        {
            HandleScalingAndRotation();
        }
    }

    void PlaceAnimatedObject(int index)
    {
        if (placementMarker == null || animatedPrefabs[index] == null)
            return;

        // Spawn animated object at pointer position
        GameObject newObject = Instantiate(animatedPrefabs[index], placementMarker.transform.position, placementMarker.transform.rotation);

        // ✅ Ensure animation is applied
        animatedObjectController.AssignAnimationClip(newObject);

        // ✅ Ensure the new object is properly assigned to `selectedObject`
        selectedObject = newObject;

        // 🔹 Debug Log to show selected animated object name in Console
        Debug.Log(" Selected Animated 3D Object: " + animatedPrefabs[index].name);
    }

    private void HandleScalingAndRotation()
    {
        if (selectedObject == null || Input.touchCount < 2) return;

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // ✅ Improved Scaling (Pinch Gesture)
        float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float scaleFactor = currentDistance / prevDistance;

        if (!float.IsNaN(scaleFactor) && scaleFactor > 0 && Mathf.Abs(scaleFactor - 1) < 0.3f) // ✅ Prevent extreme scaling
        {
            selectedObject.transform.localScale *= scaleFactor;
        }

        // ✅ Improved Rotation (Two-Finger Gesture)
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevAngle = Mathf.Atan2(touch1PrevPos.y - touch2PrevPos.y, touch1PrevPos.x - touch2PrevPos.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(touch1.position.y - touch2.position.y, touch1.position.x - touch2.position.x) * Mathf.Rad2Deg;

        float angleDifference = currentAngle - prevAngle;
        selectedObject.transform.Rotate(Vector3.up, -angleDifference, Space.World);
    }
}
