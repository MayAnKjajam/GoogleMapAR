// As requested here: https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3391209
// Here is the script to get Lat/Lon coordinates from camera position & vice versa in runtime.

using UnityEngine;
using System;

public class RuntimeLatLon : MonoBehaviour
{
    private static bool showDestination = false;
    private static double playerLat;
    private static double playerLon;
    private static Vector3 worldPosFromCoords;
    private static double earthRadius = 6378137;

    public static double[] GetLatLon
        (
            Vector3 realWorldPosition,
            double latitude,
            double longitude,
            float unitMeters = 1 // unitMeters value specifies each engine unit's metric size defaulted to 1 => 1 unit = 1 meter
        )
    {
        double offsetLat = ((realWorldPosition.z * unitMeters) / earthRadius) * 180 / Math.PI;
        playerLat = latitude + offsetLat; // Moving NORTH/SOUTH
        double offsetLon = ((realWorldPosition.x * unitMeters) / (earthRadius * Math.Cos(Math.PI * playerLat / 180))) * 180 / Math.PI;
        playerLon = longitude + offsetLon; // Moving EAST/WEST
        double[] playerLatLon = new double[2];
        playerLatLon[0] = playerLat;
        playerLatLon[1] = playerLon;

        return playerLatLon;
    }

    public static Vector3 GetDestinationWorldPosition
        (
            double latitude,
            double longitude,
            double destinationLatitude,
            double destinationLongitude,
            float destinationHeight,
            float unitMeters = 1 // unitMeters value specifies each engine unit's metric size defaulted to 1 => 1 unit = 1 meter
        )
    {
        showDestination = true;
        double destinationOffsetLat = destinationLatitude - latitude;
        double destinationOffsetLon = destinationLongitude - longitude;
        double worldPosZ = (destinationOffsetLat * (Math.PI / 180) * earthRadius) * unitMeters;
        double worldPosX = ((destinationOffsetLon * (earthRadius * Math.Cos(Math.PI * destinationLatitude / 180))) * Math.PI / 180) * unitMeters;
        worldPosFromCoords = new Vector3((float)worldPosX, destinationHeight, (float)worldPosZ);

        return worldPosFromCoords;
    }

    void OnGUI ()
    {
        GUI.backgroundColor = new UnityEngine.Color(0.3f, 0.3f, 0.3f, 0.3f);
        GUI.Box(new Rect(10, Screen.height - 35, 220, 22), "Lat: " + playerLat.ToString("0.000000") + "   Lon: " + playerLon.ToString("0.000000"));

        if (showDestination)
            GUI.Box(new Rect(250, Screen.height - 35, 420, 22), "Destination Point   X: " + worldPosFromCoords.x.ToString("0.000") + "   Y: " + worldPosFromCoords.y.ToString("0.000") + "   Z: " + worldPosFromCoords.z.ToString("0.000"));
    }
}

