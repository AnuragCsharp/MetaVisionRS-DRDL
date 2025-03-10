using System.Collections.Generic;
using UnityEngine;
using Michsky.MUIP; // Import Modern UI Pack
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class ARObjectSelection : MonoBehaviour
{
    public GameObject placementMarker; // Pointer object (AR Raycast Pointer)
    public List<GameObject> objectPrefabs; // Assign all 3D Prefabs in Inspector
    public GameObject animatedPrefab; // ✅ Animated GameObject with Animator
    public GameObject listViewPanel; // Assign ListView Panel
    public ButtonManager hamburgerButton; // Modern UI Pack ButtonManager
    public ButtonManager deleteButton; // ✅ Delete Selected Object
    public ButtonManager animatedDemoButton; // ✅ Spawn Animated Object
    private List<GameObject> spawnedObjects = new List<GameObject>(); // ✅ List to track all spawned objects
    private GameObject selectedObject = null;
    private GameObject animatedObject = null; // ✅ Reference to animated object
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();

        // Ensure ListViewPanel is disabled initially
        listViewPanel.SetActive(false);

        // ✅ Using `OnClick()`
        hamburgerButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            listViewPanel.SetActive(!listViewPanel.activeSelf);
        });

        // ✅ Assign ListView Button Listeners
        Transform listContainer = listViewPanel.transform.Find("Scroll Area/List");
        if (listContainer != null)
        {
            foreach (Transform item in listContainer)
            {
                Button button = item.GetComponent<Button>();
                if (button != null)
                {
                    int index = item.GetSiblingIndex();
                    button.onClick.AddListener(() => PlaceObject(index));
                }
            }
        }
        else
        {
            Debug.LogError("❌ List container not found! Make sure 'Scroll Area/List' exists in your ListView UI.");
        }

        // ✅ Assign Delete & Animated Model Buttons
        deleteButton.onClick.AddListener(DeleteSelectedObject);
        animatedDemoButton.onClick.AddListener(SpawnAnimatedModel);
    }

    void Update()
    {
        // Update Pointer Position for Placement
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

        // Allow Selected Object or Animated Object to Scale & Rotate
        if (selectedObject != null || animatedObject != null)
        {
            HandleScalingAndRotation();
        }
    }

    void PlaceObject(int index)
    {
        if (placementMarker == null || objectPrefabs[index] == null)
            return;

        // Spawn object at pointer position
        GameObject newObject = Instantiate(objectPrefabs[index], placementMarker.transform.position, placementMarker.transform.rotation);

        // ✅ Add object to list for tracking
        spawnedObjects.Add(newObject);

        // ✅ Ensure the new object is properly assigned to `selectedObject`
        selectedObject = newObject;

        Debug.Log("✅ Selected 3D Object: " + objectPrefabs[index].name);
    }

    private void HandleScalingAndRotation()
    {
        GameObject activeObject = selectedObject != null ? selectedObject : animatedObject;

        if (activeObject == null || Input.touchCount < 2) return;

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // ✅ Pinch to Scale
        float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float scaleFactor = currentDistance / prevDistance;

        if (!float.IsNaN(scaleFactor) && scaleFactor > 0 && Mathf.Abs(scaleFactor - 1) < 0.3f)
        {
            activeObject.transform.localScale *= scaleFactor;
        }

        // ✅ Rotate Object
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevAngle = Mathf.Atan2(touch1PrevPos.y - touch2PrevPos.y, touch1PrevPos.x - touch2PrevPos.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(touch1.position.y - touch2.position.y, touch1.position.x - touch2.position.x) * Mathf.Rad2Deg;

        float angleDifference = currentAngle - prevAngle;
        activeObject.transform.Rotate(Vector3.up, -angleDifference, Space.World);
    }

    // ✅ Function to Delete Selected or Animated Object
    private void DeleteSelectedObject()
    {
        // ✅ Delete all spawned objects
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear(); // ✅ Clear list

        // ✅ Delete animated object if it exists
        if (animatedObject != null)
        {
            Destroy(animatedObject);
            animatedObject = null;
        }

        Debug.Log("✅ All Objects Deleted!");
    }

    // ✅ Function to Spawn Animated Model
    private void SpawnAnimatedModel()
    {
        if (placementMarker == null || animatedPrefab == null)
            return;

        // ✅ Destroy Previous Animated Object if Exists
        if (animatedObject != null)
        {
            Destroy(animatedObject);
        }

        // ✅ Instantiate Animated Model
        animatedObject = Instantiate(animatedPrefab, placementMarker.transform.position, placementMarker.transform.rotation);

        // ✅ Play Animation Automatically
        Animator animator = animatedObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("AnimationName"); // Replace with actual animation name
        }

        Debug.Log("✅ Animated Model Spawned & Playing Animation");
    }
}
