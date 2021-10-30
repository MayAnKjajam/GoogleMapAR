using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementController : MonoBehaviour
{
    public GameObject ObjectToSpawn;
    public GameObject PlacementIndicator;
    public Camera raycastCamera;
    public GameObject objectInstance;
    public Transform target;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private bool ValidPose;

    // Start is called before the first frame update
    private void Start()
    {
        RenderSettings.fog = false;
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (objectInstance == null && ValidPose && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                PlaceARObejct();
            }
        }
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    public void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        ValidPose = hits.Count > 0;
        if (ValidPose)
        {
            PlacementPose = hits[0].pose;
        }
    }

    public void UpdatePlacementIndicator()
    {
        if (objectInstance == null && ValidPose)
        {
            PlacementIndicator.SetActive(true);
            PlacementIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else
        {
            PlacementIndicator.SetActive(false);
        }
    }

    private void PlaceARObejct()
    {
        objectInstance = Instantiate(ObjectToSpawn, target);
        objectInstance.transform.SetPositionAndRotation(new Vector3(PlacementPose.position.x-0.5f, PlacementPose.position.y, PlacementPose.position.z - 0.5f), PlacementPose.rotation);
        //ObjectToSpawn.SetActive(true);
        //objectInstance = ObjectToSpawn;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}