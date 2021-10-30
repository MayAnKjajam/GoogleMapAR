/// <summary>
/// This is the precise Geo-Location algorithm for objects in Unity based on Mercator projection.
/// It keeps all the calculations including vectors on double precision and finally coverts them
/// to floats when falls back to Unity transforms.
/// </summary>

using UnityEngine;
using System;
using TerraLand.Utils;

[ExecuteInEditMode]
public class LatLon2UnityMercator : MonoBehaviour
{
    public double areaTop;
    public double areaBottom;
    public double areaLeft;
    public double areaRight;

    public double areaWidth;
    public double areaLength;

    public bool forceMoveToLatLon = true;
    public double destinationLat;
    public double destinationLon;

    public bool lockToCenter = false;
    public double scaleFactor = 1;
    public bool worldIsCentered = true;

    private double LAT;
    private double LON;
    private Vector3d worldPosition;

    //private double currentLatitude;
    //private double currentLongitude;

    void Update ()
    {
        if (forceMoveToLatLon)
            PerformGeoLocation();
    }

    public void PerformGeoLocation ()
    {
        double yMaxTop = AreaBounds.LatitudeToMercator(areaTop);
        double xMinLeft = AreaBounds.LongitudeToMercator(areaLeft);
        double yMinBottom = AreaBounds.LatitudeToMercator(areaBottom);
        double xMaxRight = AreaBounds.LongitudeToMercator(areaRight);
        double latSize = Math.Abs(yMaxTop - yMinBottom);
        double lonSize = Math.Abs(xMinLeft - xMaxRight);
        double worldSizeX = areaWidth * scaleFactor;
        double worldSizeY = areaLength * scaleFactor;
        double originLatitude = (areaTop + areaBottom) / 2d;
        double originLongitude = (areaLeft + areaRight) / 2d;
        //double driftFactor = AreaBounds.MercatorLatitudeDriftFactor(originLatitude);

        if (lockToCenter)
        {
            LAT = AreaBounds.LatitudeToMercator(originLatitude);
            LON = AreaBounds.LongitudeToMercator(originLongitude);
        }
        else
        {
            LAT = AreaBounds.LatitudeToMercator(destinationLat);
            LON = AreaBounds.LongitudeToMercator(destinationLon);
        }

        double[] latlonDeltaNormalized = AreaBounds.GetNormalizedDelta(LAT, LON, yMaxTop, xMinLeft, latSize, lonSize);
        Vector2d worldPositionXZ = AreaBounds.GetWorldPositionFromTile(latlonDeltaNormalized[0], latlonDeltaNormalized[1], worldSizeY, worldSizeX);

        if(worldIsCentered)
        {
            Vector3d worldPositionTemp = new Vector3d(worldPositionXZ.x + worldSizeY / 2, 0, worldPositionXZ.y - worldSizeX / 2);
            double[] latlonDeltaNormalizedCenter = AreaBounds.GetNormalizedDelta(AreaBounds.LatitudeToMercator(originLatitude), AreaBounds.LongitudeToMercator(originLongitude), yMaxTop, xMinLeft, latSize, lonSize);
            Vector2d worldPositioncenter = AreaBounds.GetWorldPositionFromTile(latlonDeltaNormalizedCenter[0], latlonDeltaNormalizedCenter[0], worldSizeY, worldSizeX);
            double offsetZ = worldPositioncenter.y - (worldSizeX / 2);
            worldPosition = new Vector3d(worldPositionTemp.x, 0, worldPositionTemp.z - offsetZ);
        }
        else
            worldPosition = new Vector3d(worldPositionXZ.x + worldSizeY / 2, 0, worldPositionXZ.y - worldSizeX / 2);

        transform.position = (Vector3)worldPosition;
    }
}

