using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelReference : MonoBehaviour
{
    public Button PanelInOut;
    public Animator Panel;
    public TextMeshProUGUI ID, Lat, Log, Head, Pitch, Roll ,Entity;
    GameObject PreviousObj;
    public TMP_Dropdown Dropdown;
    public GameObject ActiveCamera,HeliCamera,UIHolder, WarningMessage, SpawnerParent;
    public Slider VisiblitySlider;

    // Start is called before the first frame update
    void Start()
    {
        PanelInOut.onClick.AddListener(PanelInOutFun);
        Dropdown.onValueChanged.AddListener(delegate { ChangeView(); });
    }

    void PanelInOutFun()
    {
        Panel.SetTrigger("InfoPanel");
    }

    public void ReleaseSelectedObject(GameObject CurrentObj)
    {
        if (PreviousObj == null)
        {
            PreviousObj = CurrentObj;        
        }
        else
        {
            
            PreviousObj.GetComponent<ObjectInformation>().ReleaseObject();
            if (PreviousObj == CurrentObj)
            {
                ID.text = "NA";
                Lat.text = "NA";
                Log.text = "NA";
                Roll.text = "NA";
                Entity.text = "NA";
                Head.text = "NA";
                Pitch.text = "NA";
                PreviousObj = null;
                if (Dropdown.value != 1)
                {
                    ActiveCamera = null;
                }
            }
            else { PreviousObj = CurrentObj; }
        }
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    

    public void ChangeView()
    {
        if (Dropdown.value == 1)
        {
            if (ActiveCamera != null)
            {
                ActiveCamera.SetActive(true);
                HeliCamera.SetActive(false);
                UIHolder.SetActive(false);
                for (int i = 0; i < SpawnerParent.transform.childCount; i++)
                {
                    SpawnerParent.transform.GetChild(i).GetComponent<CapsuleCollider>().enabled = false;
                }
                RenderSettings.fogStartDistance = -0.8f;
                RenderSettings.fogEndDistance = 0.7f;
                VisiblitySlider.minValue = -0.8f;
                VisiblitySlider.maxValue = 0.69f;
                VisiblitySlider.value = VisiblitySlider.minValue;
            }
            else
            {
                StartCoroutine(ShowWarning());
                Dropdown.value = 0;
            }
        }
        if (Dropdown.value == 2)
        {
            if (ActiveCamera != null)
            {
                if (ActiveCamera.activeInHierarchy)
                {
                    ActiveCamera.SetActive(false);
                }
            }
            for (int i = 0; i < SpawnerParent.transform.childCount; i++)
            {
                SpawnerParent.transform.GetChild(i).GetComponent<CapsuleCollider>().enabled = true;
            }
            RenderSettings.fogStartDistance = 0.1f;
            RenderSettings.fogEndDistance = 1f;
            VisiblitySlider.minValue = 0.1f;
            VisiblitySlider.maxValue = 0.9f;
            VisiblitySlider.value = VisiblitySlider.minValue;
            ActiveCamera = null;
            HeliCamera.SetActive(true);
            UIHolder.SetActive(true);
        }
    }

    IEnumerator ShowWarning()
    {
        WarningMessage.SetActive(true);
        yield return new WaitForSeconds(3f);
        WarningMessage.SetActive(false);
    }
}
