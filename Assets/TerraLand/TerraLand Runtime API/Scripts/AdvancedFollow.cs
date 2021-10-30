/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2018
    
    http://terraunity.com
    info@terraunity.com
    
    This script is written for Unity 3D Engine.
    Unity 3D Version: Unity 2017.2 & up
    
    
    
    HOW TO USE: This is the modified version of the original "SmoothFollow" script by Unity. Attach the script to any desired cameras in the scene.
    Type terrains layer name into the "Terrain Layer Name" field in order to detect underneath terrains.

    
    Added Features:
        .Activate/Bypass height damping
        .Detect underneath terrain if existing and avoid going through its surface

    Written by: Amir Badamchi
    
*/


using UnityEngine;

public class AdvancedFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 10.0f;
    public float rotationDamping;

    //public string terrainLayerName = "Terrain";
    public LayerMask terrainLayer;

    public bool heightDamp = false;
    public float heightDamping;

    private Terrain terrain;
    private RaycastHit hit;
    private float nearClipDistance;

    // The height we want the camera to be above terrain
    public float focusedHeight = 1.0f;
    public float minimumHeight = 0.5f;


    private void Start ()
    {
        transform.position = target.position;

        // All main terrains in the scene are in the 8th layer named "Terrain"
        //terrainLayer = 1 << LayerMask.NameToLayer(terrainLayerName);

        nearClipDistance = transform.GetComponent<Camera>().nearClipPlane;
    }

    void LateUpdate()
    {
        if (!target)
            return;

        float wantedRotationAngle = target.eulerAngles.y;
        float currentRotationAngle = transform.eulerAngles.y;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Detect active terrain below object
        Ray ray = new Ray (transform.position, Vector3.down);

        if (Physics.Raycast (ray, out hit, Mathf.Infinity, terrainLayer))
            terrain = hit.transform.gameObject.GetComponent<Terrain>();

        if(heightDamp)
        {
            float currentHeight = transform.position.y;

            if(terrain != null)
            {
                float terrainHeight = terrain.SampleHeight(transform.position);
                float terrainHeightCam = terrainHeight + (nearClipDistance + minimumHeight);
                float heightAbove = terrainHeight + focusedHeight;

                // Check camera height & avoid going below terrain
                if(currentHeight >= terrainHeightCam)
                    currentHeight = Mathf.Lerp(currentHeight, heightAbove, heightDamping * Time.deltaTime);
                else
                    currentHeight = Mathf.Lerp(currentHeight, terrainHeightCam, 1000f * Time.deltaTime);

                transform.position = new Vector3(target.position.x, currentHeight, target.position.z);
            }
            else
            {
                currentHeight = Mathf.Lerp(currentHeight, target.position.y + focusedHeight, heightDamping * Time.deltaTime);
                transform.position = new Vector3(target.position.x, currentHeight, target.position.z);
            }
        }
        else
            transform.position = target.position;

        transform.position -= currentRotation * Vector3.forward * distance;
        transform.LookAt(target);
    }
}

