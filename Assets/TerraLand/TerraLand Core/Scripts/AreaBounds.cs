using System;
using TerraLand.Utils;

public class AreaBounds
{
    // Earth's radius, sphere
    const double earthRadiusEquatorial = 6378137;
    const double earthRadiusPolar = 6356752.3142;

    public static void MetricsToBBox (double latitide, double longitude, double areaSizeLat, double areaSizeLon, out double top, out double left, out double bottom, out double right)
    {
        // Offsets in meters
        double dn = (areaSizeLat / 2d) * 1000d;
        double de = (areaSizeLon / 2d) * 1000d;
        
        // Coordinate offsets in radians
        double dLat = dn / earthRadiusEquatorial;
        double dLon = de / (earthRadiusEquatorial * Math.Cos(Math.PI * latitide / 180));
        
        top = latitide + dLat * 180d / Math.PI; // Top
        left = longitude - dLon * 180d / Math.PI; // Left
        bottom = latitide - dLat * 180d / Math.PI; // Bottom
        right = longitude + dLon * 180d / Math.PI; // Right
    }

    public static void MetricsToBBox (double latitide, double longitude, double areaSizeLat, double areaSizeLon, out string top, out string left, out string bottom, out string right)
    {
        // Offsets in meters
        double dn = (areaSizeLat / 2d) * 1000d;
        double de = (areaSizeLon / 2d) * 1000d;

        // Coordinate offsets in radians
        double dLat = dn / earthRadiusEquatorial;
        double dLon = de / (earthRadiusEquatorial * Math.Cos(Math.PI * latitide / 180));

        top = (latitide + dLat * 180d / Math.PI).ToString(); // Top
        left = (longitude - dLon * 180d / Math.PI).ToString(); // Left
        bottom = (latitide - dLat * 180d / Math.PI).ToString(); // Bottom
        right = (longitude + dLon * 180d / Math.PI).ToString(); // Right
    }

    public static void FindPointAtDistanceFrom (double Latitude, double Longitude, double initialBearingRadians, double distanceKilometres, out double lat, out double lon)
    {
        double ereRad = earthRadiusEquatorial / 1000d;
        double erpRad = earthRadiusPolar / 1000d;
        double radiusEarthKilometres = erpRad + (90 - Math.Abs(Latitude)) / 90 * (ereRad - erpRad);

        //double radiusEarthKilometres = 6371.01;
        //double radiusEarthKilometres = earthRadius / 1000d;

        double distRatio = distanceKilometres / radiusEarthKilometres;
        double distRatioSine = Math.Sin(distRatio);
        double distRatioCosine = Math.Cos(distRatio);

        double startLatRad = DegreesToRadians(Latitude);
        double startLonRad = DegreesToRadians(Longitude);

        double startLatCos = Math.Cos(startLatRad);
        double startLatSin = Math.Sin(startLatRad);

        double endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

        double endLonRads = startLonRad
            + Math.Atan2(
                Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
                distRatioCosine - startLatSin * Math.Sin(endLatRads));

        lat = RadiansToDegrees(endLatRads);
        lon = RadiansToDegrees(endLonRads);
    }

    public static double DegreesToRadians (double degrees)
    {
        const double degToRadFactor = Math.PI / 180;
        return degrees * degToRadFactor;
    }

    public static double RadiansToDegrees (double radians)
    {
        const double radToDegFactor = 180 / Math.PI;
        return radians * radToDegFactor;
    }

    public static double LatitudeToMercator (double lat)
    {
        return Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) / (Math.PI / 180.0) * 20037508.34 / 180.0;
    }

    public static double LongitudeToMercator (double lon)
    {
        return lon * 20037508.34 / 180.0;
    }

    public static double[] GetNormalizedDelta (double lat, double lon, double worldTop, double worldLeft, double latSize, double lonSize)
    {
        double worldBottom = worldTop - latSize;
        double worldRight = worldLeft + lonSize;
        double latMultiplier = (worldTop - lat) / (worldTop - worldBottom);
        double lonMultiplier = (worldRight - lon) / (worldRight - worldLeft);

        return new double[] { latMultiplier, lonMultiplier };
    }

    public static Vector2d GetWorldPositionFromTile (double latDelta, double lonDelta, double worldSizeLat, double worldSizeLon)
    {
        return new Vector2d(-worldSizeLon * lonDelta, worldSizeLat * (1 - latDelta));
    }

    public static double MercatorLatitudeDriftFactor (double latitude)
    {
        const double c2 = 0.00001120378;
        double latRadians = DegreesToRadians(latitude);
        return (1 + c2 * (Math.Cos(2 * latRadians) - 1)) / Math.Cos(latRadians);
    }
}

