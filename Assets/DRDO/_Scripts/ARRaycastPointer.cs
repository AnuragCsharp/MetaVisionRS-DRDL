using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARRaycastPointer : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject pointerObject; // Assign the Pointer in the Inspector
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
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
    }
}
