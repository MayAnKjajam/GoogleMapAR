
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectSpawner : MonoBehaviour
{

    public ReadCSV data;
    public GameObject obj,Observer,Missile,Airdefence,ArmouredVehiche,Infantaryvehicle;
    public int index;
    public List<string> UniqueID;
    public List<string> UniqueEntityName;
    public TextMeshProUGUI DebugText;
    public Transform ParentSpawner;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {

        data = GameObject.Find("DataManager").GetComponent<ReadCSV>();
        FillUniqueID();
        index = UniqueID.Count;
        StartCoroutine(PlaceObject(0));
        DebugText.text = data.gameObject.name+"/n";
    }
    public void FillUniqueID()
    {
        for(int i = 0; i < data.ID.Count; i++ ) 
        {
            bool Alreadypresent=false;
            int counter = 0;
            while (counter < UniqueID.Count)
            {
                if (data.ID[i] == UniqueID[counter])
                {
                    Alreadypresent = true;
                }
                counter++;
            }
            if (!Alreadypresent)
            {
                UniqueID.Add(data.ID[i]);
                UniqueEntityName.Add(data.EntityName[i]);
                DebugText.text = DebugText.text + data.ID[i] + "/n";
            }
        } 
    }
    IEnumerator PlaceObject(int i)
    {
       
        if (i < index)
        {
            
            GameObject g=null;
            yield return new WaitForSeconds(0.1f);
            if (UniqueEntityName[i] == "Observer")
            {
                g = GameObject.Instantiate(Observer, ParentSpawner);
                g.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f),0f, Random.Range(-0.4f, 0.4f));
            }
            if (UniqueEntityName[i] == "Armoured Vehicle")
            {
                g = GameObject.Instantiate(ArmouredVehiche, ParentSpawner);
                g.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
            }
            if (UniqueEntityName[i] == "Missile")
            {
                g = GameObject.Instantiate(Missile, ParentSpawner);
                g.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
            }
            if (UniqueEntityName[i] == "Infantry Fighting Vehicle")
            {
                g = GameObject.Instantiate(Infantaryvehicle, ParentSpawner);
                g.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
            }
            if (UniqueEntityName[i] == "Air Defence")
            {
                g = GameObject.Instantiate(Airdefence, ParentSpawner);
                g.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
            }
            
            if (g != null)
            {
                g.GetComponent<ObjectInformation>().ObjectID = UniqueID[i];
            }//g.GetComponent<ArcGISLocationComponent>().Longitude = float.Parse(data.Longitude[i]);
             //g.GetComponent<ArcGISLocationComponent>().Latitude = float.Parse(data.Latitude[i]);

            DebugText.text = DebugText.text+"\n"+g.name;
            i++;
            StartCoroutine(PlaceObject(i));
        }
        yield return new WaitForSeconds(0.1f);
    }

}