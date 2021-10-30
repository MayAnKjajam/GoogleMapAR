/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2016
    
    http://terraunity.com
    info@terraunity.com
    
    This script is written for Unity 3D Engine.
    Unity 3D Version: Unity 5.x
    
    
    INFO: This script moves the Directional Light as the Sun in your scene to the right position corresponding to the real-world location of the light source.
    This is to ensure that Image Effects like "Sun Shafts" work as expected and apply effects properly. The Sun position will be updated during run-time & also in the Editor.

    Written by: Amir Badamchi
    
*/


using UnityEngine;

[ExecuteInEditMode]
public class SunPosition : MonoBehaviour
{
    public GameObject player;
    public float distance = 10000f;
    public bool mainLight = true;
    public Light sun;

    void Start ()
    {
        //player = Camera.main.gameObject;

        if(mainLight)
            sun = transform.GetComponent<Light>();

        SetSunPosition();
    }
	
	void Update ()
    {
        SetSunPosition();
    }

    private void SetSunPosition ()
    {
        if(!mainLight)
            transform.rotation = sun.transform.rotation;

        transform.position = player.transform.position + (-transform.forward * distance);
    }
}

