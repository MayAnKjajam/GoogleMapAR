using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PlacementController simpleRaycast;
    public GameObject target;
   
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
    }

    
  
    public void ResetAll()
    {
        if (simpleRaycast.objectInstance != null)
        {
            Destroy(target.transform.GetChild(0).gameObject);
            simpleRaycast.objectInstance = null;
        }
    }

   
   
}