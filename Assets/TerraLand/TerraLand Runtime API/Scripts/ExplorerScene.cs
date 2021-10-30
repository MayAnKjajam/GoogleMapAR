/*
	_____  _____  _____  _____  ______
	    |  _____ |      |      |  ___|
	    |  _____ |      |      |     |
	
     U       N       I       T      Y
                                         
	
	TerraUnity Co. - Earth Simulation Tools
	May 2012
	
	http://terraunity.com
	info@terraunity.com
	
	This script is written for Unity 3D Engine.
	
	
	
	HOW TO USE:   This script will manage a "Free Camera" style movement specifically for navigating on terrains.
	
	Just Attach this script to your main camera in the scene. There must be at least one terrain available in the
	scene for the "ExplorerScene" script to work properly.
	
	Change or set various options in the script to fit your needs in the scene.
	
	Controls
	
	Navigation:   Arrow Keys/WASD
	Move Up:   "E"
	Move Down:   "Q"
	Eye View:   Left Mouse Button
	Drag:   Middle Mouse Button (Scroll)
	Zoom In/Out:   Mouse Scroll
	Fly To Point Of Interest:   "1"
	Exit Fly Mode:   "Esc"
	
	
	License: You can freely use this script in your Commercial or Non-Commercial projects.

    Written by: Amir Badamchi
    
*/

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody), typeof (SphereCollider), typeof (MouseLookCustom))]
public class ExplorerScene : MonoBehaviour
{
	public Terrain terrain;
	public bool speedChange = true;
	public float navigationSpeed = 1f;
	public float dragSpeed = 1f;
	public bool fOV = true;
	public float fOVSpeed = 1f;
    public float heightAboveSurface = 5.0f;
	public bool fogControl = true;
	public float fogMaximumHeight = 1000.0f;
	public float fogMinimumHeight = 200.0f;
	public bool jungleAmbient = false;
	public float extraFogFactor = 1.5f;
	public Light darkerSun;
	public float remainingSunLightPercent = 75.0f;
	public bool zFighting = false;
	public Vector3 pointOfInterest = new Vector3(0.0f, 300.0f, 0.0f);
	public bool rotateAround = true;
	
	private float speedFactorAll;
	private float navigationSpeedF;
	private float dragSpeedF;
	private float fogDenseInitial;
	private float fogDenseHeight;
	private float fogDenseNormalized;
	
	private float sunLightInitial;
	private float ambientZone1;
	private float ambientZone2;
	private float remainingLight;
	
	private float terrainWidth;
	private float maxHeight;
	private float speedFactor;
	private float dragSpeedFactor;
	private float heightPos;
		
    private Texture2D logo;
	
	private float camView;
	private bool moving = false;

    bool isInit = false;
	
	void Start ()
    {
		transform.GetComponent<Rigidbody>().mass = 1f;
		transform.GetComponent<Rigidbody>().drag = Mathf.Infinity;
		transform.GetComponent<Rigidbody>().angularDrag = Mathf.Infinity;
		transform.GetComponent<Rigidbody>().useGravity = false;
		transform.GetComponent<Rigidbody>().isKinematic = false;
		transform.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		transform.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
		
		if (!terrain)
			terrain = Terrain.activeTerrain;
		
		GameObject cam = transform.gameObject;
		SphereCollider sphereCollider = cam.GetComponent<SphereCollider>();
		
		// If you go through surfaces, just increase this number until it is detected by Collision Detection System
        sphereCollider.radius = heightAboveSurface;
	
        if (terrain)
        {
		    speedFactorAll = Mathf.InverseLerp(0f, 4096f, ((terrain.terrainData.size.x + terrain.terrainData.size.z) / 2f));
            terrainWidth = terrain.terrainData.size.x;
            maxHeight = terrainWidth / 3f;
        }
		
        navigationSpeedF = speedFactorAll * 150f * navigationSpeed;
		dragSpeedF = speedFactorAll * 20f * dragSpeed;
	
		speedFactor = navigationSpeedF;
		dragSpeedFactor = dragSpeedF;
		
		pointOfInterest = transform.position;
	
		if (zFighting)
			GetComponent<Camera>().nearClipPlane = maxHeight * 0.01f;
        
		if (GetComponent<Camera>().farClipPlane < maxHeight * 8f)
			GetComponent<Camera>().farClipPlane = maxHeight * 8f;
		
		if (!speedChange)
		
		fogDenseInitial = RenderSettings.fogDensity;
		
		if (darkerSun)
			sunLightInitial = darkerSun.GetComponent<Light>().intensity;
		
		camView = GetComponent<Camera>().fieldOfView;

        logo = Resources.Load("TerraUnity/Images/Logo/Logo") as Texture2D;
	}

	void Update ()
    {
        if (terrain == null)
            terrain = Terrain.activeTerrain;
        else
        {
            if(!isInit)
            {
                speedFactorAll = Mathf.InverseLerp(0f, 4096f, ((terrain.terrainData.size.x + terrain.terrainData.size.z) / 2f));
                terrainWidth = terrain.terrainData.size.x;
                maxHeight = terrainWidth / 3f;
                navigationSpeedF = speedFactorAll * 150f * navigationSpeed;
                dragSpeedF = speedFactorAll * 20f * dragSpeed;
                speedFactor = navigationSpeedF;
                dragSpeedFactor = dragSpeedF;

                isInit = true;
            }

            heightPos = transform.position.y - terrain.SampleHeight(transform.position) - terrain.transform.position.y;

            transform.Translate(new Vector3(Input.GetAxis("Horizontal") * 10.0f * navigationSpeed * Time.deltaTime, 0f, 0f));
            transform.Translate(new Vector3(0f, 0f, Input.GetAxis("Vertical") * 10.0f * navigationSpeed * Time.deltaTime));

            if (Input.GetKey("e"))
                transform.Translate(Vector3.up * 2.0f * navigationSpeed * Time.deltaTime);

            if (Input.GetKey("q"))
                transform.Translate(Vector3.down * 2.0f * navigationSpeed * Time.deltaTime);

            // POI
            if (Input.GetKey("1"))
                //originalRotation = transform.localRotation;
                moving = true;
            
            if (Input.GetKey("escape"))
                moving = false;

            if (moving)
            {
                transform.position = Vector3.Lerp(transform.position, pointOfInterest, Time.deltaTime / 3f);

                var targetRotation = Quaternion.LookRotation(pointOfInterest - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

                if (rotateAround) {
                    transform.Translate(new Vector3(10f * navigationSpeed * Time.deltaTime, 0f, 0f));
                    transform.Translate(new Vector3(0f, navigationSpeed * Time.deltaTime, 0f));
                }

                if ((Mathf.Abs(transform.position.x - pointOfInterest.x) < 2f)&&(Mathf.Abs(transform.position.z - pointOfInterest.z) < 2f))
                    moving = false;
            }

            // Fog & Sun Settings
            if (fogControl)
            {
                fogDenseHeight = Mathf.InverseLerp(fogMaximumHeight, fogMinimumHeight, transform.position.y);
                fogDenseNormalized = fogDenseHeight * fogDenseInitial;
                RenderSettings.fogDensity = fogDenseNormalized;
            }

            if (transform.position.y < fogMinimumHeight)
            {   
                if (jungleAmbient)
                {
                    ambientZone1 = Mathf.InverseLerp(fogMinimumHeight, terrain.transform.position.y + terrain.SampleHeight(transform.position), transform.position.y);
                    RenderSettings.fogDensity = fogDenseInitial + (fogDenseInitial * (ambientZone1 * extraFogFactor));
                }

                if (darkerSun)
                {
                    ambientZone2 = Mathf.InverseLerp(terrain.transform.position.y + terrain.SampleHeight(transform.position), fogMinimumHeight, transform.position.y);
                    remainingLight = (sunLightInitial * (remainingSunLightPercent / 100.0f));
                    darkerSun.GetComponent<Light>().intensity = sunLightInitial * ambientZone2;

                    if (darkerSun.GetComponent<Light>().intensity < remainingLight) {
                        darkerSun.GetComponent<Light>().intensity = remainingLight;
                    }
                }
            }

            //--------------------------------------------------------------------------------------

            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                transform.position = transform.position + Camera.main.transform.forward * navigationSpeed * 3f;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                transform.position = transform.position - Camera.main.transform.forward * navigationSpeed * 3f;
            }

            //--------------------------------------------------------------------------------------

            if (Input.GetMouseButton(2))
            {
                transform.Translate(-new Vector3(Input.GetAxis("Mouse X")* dragSpeed * 10f, Input.GetAxis("Mouse Y")* dragSpeed * 10f, 0f) );
            }

            //--------------------------------------------------------------------------------------

//            // Terrain Bounds
//
//            if (heightPos < 0.5f)
//                heightPos = 0.5f;
//
//            // Y Bounds 
//            if (transform.position.y > (terrain.transform.position.y + terrainWidth / 1f))
//                transform.position = new Vector3 (transform.position.x, (terrain.transform.position.y + terrainWidth / 1f), transform.position.z);
//
//            if (transform.position.y < terrain.transform.position.y + transform.gameObject.GetComponent<Camera>().nearClipPlane + 0.01f)
//                transform.position = new Vector3 (transform.position.x, terrain.transform.position.y + transform.gameObject.GetComponent<Camera>().nearClipPlane + 0.01f, transform.position.z);
//
//            // X & Z Bounds
//            if (transform.position.x > terrain.transform.position.x + terrain.terrainData.size.x)
//                transform.position = new Vector3 (terrain.transform.position.x + terrain.terrainData.size.x, transform.position.y, transform.position.z);
//
//            if (transform.position.x < terrain.transform.position.x)
//                transform.position = new Vector3 (terrain.transform.position.x, transform.position.y, transform.position.z);
//
//            if (transform.position.z > terrain.transform.position.z + terrain.terrainData.size.z)
//                transform.position = new Vector3 (transform.position.x, transform.position.y, terrain.transform.position.z + terrain.terrainData.size.z);
//
//            if (transform.position.z < terrain.transform.position.z)
//                transform.position = new Vector3 (transform.position.x, transform.position.y, terrain.transform.position.z);


            //--------------------------------------------------------------------------------------

            // Here Are Different Height Layers To Set Camera Speed

            if (heightPos >= maxHeight * 0.5f)
                dragSpeed = dragSpeedFactor;

            if (heightPos >= maxHeight) {
                if (speedChange){
                    navigationSpeed = speedFactor;
                }
            }
            if ((heightPos <= maxHeight * 0.95f)&&(heightPos >= maxHeight * 0.9f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.975f;
                }
            }
            if ((heightPos <= maxHeight * 0.9f)&&(heightPos >= maxHeight * 0.85f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.95f;
                }
            }
            if ((heightPos <= maxHeight * 0.85f)&&(heightPos >= maxHeight * 0.8f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.925f;
                }
            }
            if ((heightPos <= maxHeight * 0.8f)&&(heightPos >= maxHeight * 0.75f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.9f;
                }
            }
            if ((heightPos <= maxHeight * 0.75f)&&(heightPos >= maxHeight * 0.7f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.875f;
                }
            }
            if ((heightPos <= maxHeight * 0.7f)&&(heightPos >= maxHeight * 0.65f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.85f;
                }
            }
            if ((heightPos <= maxHeight * 0.65f)&&(heightPos >= maxHeight * 0.6f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.825f;
                }
            }
            if ((heightPos <= maxHeight * 0.6f)&&(heightPos >= maxHeight * 0.55f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.8f;
                }
            }
            if ((heightPos <= maxHeight * 0.55f)&&(heightPos >= maxHeight * 0.5f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.775f;
                }
            }

            if ((heightPos <= maxHeight * 0.5f)&&(heightPos >= maxHeight * 0.475f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.75f;
                    dragSpeed = dragSpeedFactor;
                }
            }
            if ((heightPos <= maxHeight * 0.475f)&&(heightPos >= maxHeight * 0.45f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.725f;
                    dragSpeed = dragSpeedFactor * 0.9f;
                }
            }
            if ((heightPos <= maxHeight * 0.45f)&&(heightPos >= maxHeight * 0.425f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.7f;
                    dragSpeed = dragSpeedFactor * 0.8f;
                }
            }
            if ((heightPos <= maxHeight * 0.425f)&&(heightPos >= maxHeight * 0.4f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.65f;
                    dragSpeed = dragSpeedFactor * 0.7f;
                }
            }
            if ((heightPos <= maxHeight * 0.4f)&&(heightPos >= maxHeight * 0.375f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.6f;
                    dragSpeed = dragSpeedFactor * 0.6f;
                }
            }
            if ((heightPos <= maxHeight * 0.375f)&&(heightPos >= maxHeight * 0.35f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.55f;
                    dragSpeed = dragSpeedFactor * 0.5f;
                }
            }
            if ((heightPos <= maxHeight * 0.35f)&&(heightPos >= maxHeight * 0.325f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.5f;
                    dragSpeed = dragSpeedFactor * 0.4f;
                }
            }
            if ((heightPos <= maxHeight * 0.325f)&&(heightPos >= maxHeight * 0.3f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.4f;
                    dragSpeed = dragSpeedFactor * 0.3f;
                }
            }
            if ((heightPos <= maxHeight * 0.3f)&&(heightPos >= maxHeight * 0.2f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.275f;
                    dragSpeed = dragSpeedFactor * 0.2f;
                }
                if (fOV) {
                    GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, camView, Time.deltaTime * fOVSpeed);
                }
                if (zFighting) {
                    GetComponent<Camera>().nearClipPlane = maxHeight * 0.01f;
                }
            }
            if ((heightPos <= maxHeight * 0.2f)&&(heightPos >= maxHeight * 0.1f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.15f;
                    dragSpeed = dragSpeedFactor * 0.1f;
                }
                if (fOV) {
                    GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, camView / 1.1f, Time.deltaTime * fOVSpeed);
                }
                if (zFighting) {
                    GetComponent<Camera>().nearClipPlane = maxHeight * 0.005f;
                }
            }
            if ((heightPos <= maxHeight * 0.1f)&&(heightPos >= maxHeight * 0.01f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.075f;
                    dragSpeed = dragSpeedFactor * 0.02f;
                }
                if (fOV) {
                    GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, camView / 1.1f / 1.1f, Time.deltaTime * fOVSpeed);
                }
                if (zFighting) {
                    GetComponent<Camera>().nearClipPlane = maxHeight * 0.003f;
                }
            }
            if ((heightPos <= maxHeight * 0.01f)&&(heightPos >= maxHeight * 0.00275f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.02f;
                    dragSpeed = dragSpeedFactor * 0.01f;
                }
                if (fOV) {
                    GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, camView / 1.1f / 1.1f / 1.1f, Time.deltaTime * fOVSpeed);
                }
                if (zFighting) {
                    GetComponent<Camera>().nearClipPlane = maxHeight * 0.002f;
                }
            }
            if ((heightPos <= maxHeight * 0.00275f)&&(heightPos >= 0f)) {
                if (speedChange){
                    navigationSpeed = speedFactor * 0.0075f;
                    dragSpeed = dragSpeedFactor * 0.0075f;
                }
                if (fOV) {
                    GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, camView / 1.1f / 1.1f / 1.1f / 1.1f, Time.deltaTime * fOVSpeed);
                }
                if (zFighting) {
                    GetComponent<Camera>().nearClipPlane = 0.225f;
                }
            }

            //--------------------------------------------------------------------------------------

            Cursor.visible = false;
        }
	}
	
	void OnGUI ()
    {
		GUI.backgroundColor = new Color(0,0,0,0);
		GUI.Box (new Rect(Screen.width - 175, Screen.height - 69, 170, 64), logo);
	}
}

