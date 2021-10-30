using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;

[Serializable]
public class GeoCodeData
{
    public GeoCodeDataResourcesSet[] resourceSets;
}

[Serializable]
public class GeoCodeDataResourcesSet
{
    public GeoCodeDataResource[] resources;
}

[Serializable]
public class GeoCodeDataResource
{
    public GeoCodeDataPoint point;
    public GeoCodeDataAddress address;
}

[Serializable]
public class GeoCodeDataPoint
{
    public string[] coordinates;
}

[Serializable]
public class GeoCodeDataAddress
{
    public string addressLine;
    public string adminDistrict;
    public string adminDistrict2;
    public string countryRegion;
    public string formattedAddress;
    public string locality;
    public string postalCode;
}

public class GeoCoder : MonoBehaviour
{
	public static bool recognized = true;
	public static List<string> foundLocations;
    private static string APIKey = "iAoMF7rTEVmoF5UnEPCF~Ih3S85nSTpPy7mgX5HTnvQ~Aj5FK0mJDmkkCs15fgCCvxj2A3UOoRUmJBHSNma6Sv2zxQQdRXmWFhvy0f3SvJE1";

    public static List<Vector2> AddressToLatLong(string address)
    {
        string url = "http://dev.virtualearth.net/REST/v1/Locations/" + address + "?o=json" + "&key=" + APIKey;

        string detectedLatitude = "";
		string detectedLongitude = "";
        string formattedAddress = "";
		List<Vector2> coordinates = new List<Vector2>();
		foundLocations = new List<string>();

        try
        {
            using (WebClient client = new WebClient())
            {
                string response = client.DownloadString(url);
                GeoCodeData geoCodeData = JsonUtility.FromJson<GeoCodeData>(response);

                if (geoCodeData.resourceSets.Length == 0)
                    throw new Exception("Not Found!");

                for(int i = 0; i < geoCodeData.resourceSets[0].resources.Length; i++)
                {
                    detectedLatitude = geoCodeData.resourceSets[0].resources[i].point.coordinates[0];
                    detectedLongitude = geoCodeData.resourceSets[0].resources[i].point.coordinates[1];
                    formattedAddress = geoCodeData.resourceSets[0].resources[i].address.formattedAddress;

                    coordinates.Add(new Vector2(float.Parse(detectedLatitude), float.Parse(detectedLongitude)));
                    foundLocations.Add(formattedAddress);
                }
            }
        }
		catch (Exception e)
        {
            print(e.Message);
        }
        finally
        {
			if(coordinates.Count > 0)
				recognized = true;
			else
				recognized = false;
        }
		
		return coordinates;
    }
}

