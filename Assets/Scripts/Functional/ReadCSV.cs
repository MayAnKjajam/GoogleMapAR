using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ReadCSV : MonoBehaviour
{
    public List<string> ID;
    public List<string> Description;
    public List<string> EntityName;
    public List<string> Longitude;
    public List<string> Latitude;
    public List<string> Heading;
    public List<string> Roll;
    public List<string> Altitude;


    // Start is called before the first frame update
    void Awake()
    {
        ReadcsvFile();
    }
    void ReadcsvFile()
    {
        //StreamReader Streamrd = new StreamReader(Path.Combine(Application.streamingAssetsPath, "EDD_Lakhanpur.csv"));
        TextAsset txt = (TextAsset)Resources.Load("EDD_Lakhanpur", typeof(TextAsset));
        string filecontent = txt.text;
        byte[] byteArray = Encoding.UTF8.GetBytes(filecontent);
        MemoryStream stream = new MemoryStream(byteArray);
        StreamReader Streamrd = new StreamReader(stream);
        //Debug.Log(txt.text);
        // System.IO.StreamReader Streamrd = new System.IO.StreamReader(Application.dataPath + "/Resources/EDD_Lakhanpur.csv");
        //string filecontent = txt.Text;
        bool end = false;
        //string[] lines = System.IO.File.ReadAllLines(filecontent);
        while (!end)
        {
            string data = Streamrd.ReadLine();
            if (data == null)
            {
                end = true;
                break;
            }
            var data_val = data.Split(',');

          //  Debug.Log(data_val[0] + "||" + data_val[1] + "||" + data_val[2] + "||" + data_val[3] + "||" + data_val[4] + "||" + data_val[5] + "||" + data_val[6] + "||" + data_val[7] + "||" + data_val[8]);
            EntityName.Add(data_val[1]);
            Description.Add(data_val[2]);
            ID.Add(data_val[3]); 
            Latitude.Add(data_val[4]);
            Longitude.Add(data_val[5]);
            Roll.Add(data_val[6]);
            Altitude.Add(data_val[7]);
            Heading.Add(data_val[8]);
        }

    }

}
