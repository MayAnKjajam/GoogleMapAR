using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectInformation : MonoBehaviour
{
    public GameObject CockPitCamera;
    public GameObject canvas;
    public GameObject InfoPanel;
    public string ObjectID;
    public bool isActive = false;
    public List<string> Longitude;
    public List<string> Latitude;
    public List<string> Description;
    public List<string> Heading;
    public List<string> Roll;
    public List<string> Altitude;
    public Material Selected;
    public Material Normal;
    public MeshRenderer[] Object;
    bool Highlighted;
 
    // Start is called before the first frame update
    void Start()
    {
        InfoPanel = GameObject.Find("Info Panel");
        isActive = false;
        GetInfo();
    }

    public void GetInfo()
    {
        ReadCSV g = GameObject.Find("DataManager").GetComponent<ReadCSV>();
        for (int i = 0; i < g.ID.Count; i++)
        {
            if (ObjectID == g.ID[i])
            {
                Longitude.Add(g.Longitude[i]);
                Latitude.Add(g.Latitude[i]);
                Altitude.Add(g.Altitude[i]);
                Description.Add(g.Description[i]);
                Heading.Add(g.Heading[i]);
                Roll.Add(g.Roll[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Highlighted)
        { InfoPanel.GetComponent<InfoPanelReference>().ActiveCamera = CockPitCamera; }
    }

    private void OnMouseDown()
    {
        if (!Highlighted)
        {
            AnimatorClipInfo[] m_CurrentClipInfo;
            m_CurrentClipInfo = InfoPanel.GetComponent<InfoPanelReference>().Panel.GetCurrentAnimatorClipInfo(0);
            if (m_CurrentClipInfo[0].clip.name != "InfoPanelOut")
            {
                InfoPanel.GetComponent<InfoPanelReference>().Panel.SetTrigger("InfoPanel");
            }
            InfoPanel.GetComponent<InfoPanelReference>().ID.text = ObjectID;
            InfoPanel.GetComponent<InfoPanelReference>().Lat.text = Latitude[0];
            InfoPanel.GetComponent<InfoPanelReference>().Log.text = Longitude[0];
            InfoPanel.GetComponent<InfoPanelReference>().Roll.text = Roll[0];
            InfoPanel.GetComponent<InfoPanelReference>().Head.text = Heading[0];
            InfoPanel.GetComponent<InfoPanelReference>().Pitch.text = Altitude[0];
            InfoPanel.GetComponent<InfoPanelReference>().Entity.text = Description[0];

            SelectObject();
            InfoPanel.GetComponent<InfoPanelReference>().ReleaseSelectedObject(this.gameObject);
        }
        else
        {
            AnimatorClipInfo[] m_CurrentClipInfo;
            m_CurrentClipInfo = InfoPanel.GetComponent<InfoPanelReference>().Panel.GetCurrentAnimatorClipInfo(0);
            if (m_CurrentClipInfo[0].clip.name == "InfoPanelOut")
            {
                InfoPanel.GetComponent<InfoPanelReference>().Panel.SetTrigger("InfoPanel");
            }
            InfoPanel.GetComponent<InfoPanelReference>().ReleaseSelectedObject(this.gameObject);
        }
    }

    public void SelectObject()
    {
        if (!Highlighted)
        {
            foreach (MeshRenderer MR in Object)
            {
                Material[] mat = MR.materials;
                mat[0] = Selected;
                MR.materials = mat;
            }
            Highlighted = true;
            InfoPanel.GetComponent<InfoPanelReference>().ActiveCamera = CockPitCamera;
        }
    }

    public void ReleaseObject()
    {
        foreach (MeshRenderer MR in Object)
        {
            Material[] mat = MR.materials;
            mat[0] = Normal;
            MR.materials = mat;
        }
        Highlighted = false;
    }

}