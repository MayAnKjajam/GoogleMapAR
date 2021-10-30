using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BitMiracle.LibTiff.Classic;
using UnityEngine.Networking;

public class OfflineStreaming : MonoBehaviour
{
    public bool showCoordinates = true;
    private static double[] coordinates;
    private static double[] coordinatesTLBR;

    public static RuntimeOffline runtime;
    private static FloatingOriginAdvanced floatingOrigin;
    private Vector3 playerPosition;

    public static void Initialize ()
    {
        if (runtime.player.GetComponent<FloatingOriginAdvanced>() != null)
            floatingOrigin = runtime.player.GetComponent<FloatingOriginAdvanced>();

        if (PlayerPrefs.HasKey("TileTLBRCoords"))
        {
            coordinates = new double[2];
            coordinatesTLBR = new double[4];
            List<string> coords = PlayerPrefs.GetString("TileTLBRCoords").Split(',').ToList<string>();
            coordinatesTLBR[0] = double.Parse(coords[0]); // Top
            coordinatesTLBR[1] = double.Parse(coords[1]); // Left
            coordinatesTLBR[2] = double.Parse(coords[2]); // Bottom
            coordinatesTLBR[3] = double.Parse(coords[3]); // Right
        }  
    }

    private void Update ()
    {
        if (floatingOrigin != null)
            playerPosition = floatingOrigin.absolutePosition;
        else
            playerPosition = runtime.player.transform.position;

        if(showCoordinates && coordinatesTLBR != null)
            coordinates = RuntimeLatLon.GetLatLon(playerPosition, coordinatesTLBR[2], coordinatesTLBR[3], 1);
            

    }
}

