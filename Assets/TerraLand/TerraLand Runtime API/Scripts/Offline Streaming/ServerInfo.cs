using UnityEngine;
using System.IO;

public class ServerInfo : MonoBehaviour
{
    public static void GetServerCoords
        (
            string serverPath,
            out string location,
            out string serverInfoPath,
            out string globalHeightmapPath,
            out string globalHeightmapPath2,
            out string globalSatelliteImagePath,
            out string globalSatelliteImagePath2,
            out double top,
            out double left,
            out double bottom,
            out double right,
            out double latExtent,
            out double lonExtent,
            out float areaSize
        )
    {
        serverInfoPath = Path.GetFullPath(serverPath) + "/Info/";
        globalHeightmapPath = serverInfoPath + "GlobalHeightmap.tif";
        globalSatelliteImagePath = serverInfoPath + "GlobalSatelliteImage.jpg";
        globalHeightmapPath2 = serverInfoPath + "GlobalHeightmap2.tif";
        globalSatelliteImagePath2 = serverInfoPath + "GlobalSatelliteImage2.jpg";
        string infoFilePath = serverInfoPath + "Terrain Info.tlps";
        string text = File.ReadAllText(infoFilePath);
        string[] lines = text.Split('\n');
        string[] lineInfo = new string[2];
        top = left = bottom = right = latExtent = lonExtent = 0d;
        areaSize = 0f;
        location = "Area";

        foreach (string line in lines)
        {
            lineInfo = line.Split(' ');

            if (lineInfo[0].StartsWith("Address"))
            {
                location = "";

                for (int i = 1; i < lineInfo.Length; i++)
                {
                    location += lineInfo[i];

                    if (i != lineInfo.Length - 1)
                        location += " ";
                }
            }
            else if (lineInfo[0].StartsWith("LatExtents"))
                areaSize = float.Parse(lineInfo[1]);
            else if (lineInfo[0].StartsWith("ArbitraryTop"))
                top = double.Parse(lineInfo[1]);
            else if (lineInfo[0].StartsWith("ArbitraryLeft"))
                left = double.Parse(lineInfo[1]);
            else if (lineInfo[0].StartsWith("ArbitraryBottom"))
                bottom = double.Parse(lineInfo[1]);
            else if (lineInfo[0].StartsWith("ArbitraryRight"))
                right = double.Parse(lineInfo[1]);
        }

        latExtent = top - bottom;
        lonExtent = right - left;
    }
}

