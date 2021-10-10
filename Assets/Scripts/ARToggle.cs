using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARToggle : MonoBehaviour
{

    public GameObject ARCamera;
    public GameObject NormalCamera;
    public Toggle ARMode;
    public GameObject Joystick;
    public TMP_Dropdown Dropdown;
    // Start is called before the first frame update
    void Start()
    {
        ARMode.onValueChanged.AddListener(delegate { ModeToggle(); });
        ARCamera = GameObject.Find("AR Camera");
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ModeToggle()
    {
        if (ARMode.isOn)
        {
            Dropdown.interactable = false;
            Dropdown.value = 0;
            NormalCamera.SetActive(false);
            ARCamera.SetActive(true);
            Joystick.SetActive(false);
        }
        else
        {
            Dropdown.interactable = true;
            NormalCamera.SetActive(true);
            ARCamera.SetActive(false);
            Joystick.SetActive(true);
        }
    }
}
