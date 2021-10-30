using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Security;

namespace TerraLand
{
    public class Tile : MonoBehaviour
    {
        //private const string elevationBaseSeverURL = "https://elevation.arcgis.com/arcgis/services/WorldElevation/Terrain/ImageServer?token=";
        private const string elevationBaseSeverURL = "https://elevation.arcgis.com/arcgis/services/WorldElevation/TopoBathy/ImageServer?token=";
        private const string tokenBaseURL = "https://www.arcgis.com/sharing/rest/oauth2/token/authorize?client_id=n0dpgUwqazrQTyXZ&client_secret=3d4867add8ee47b6ac0c498198995298&grant_type=client_credentials&expiration=20160";

        public static void LoadHeightmapFromESRIServer(string fileName, double top, double left, double bottom, double right, int resolution)
        {
            TerraLandWorldElevation.TopoBathy_ImageServer mapserviceElevation = new TerraLandWorldElevation.TopoBathy_ImageServer();
            mapserviceElevation.Timeout = 5000000;
            string serverURL = elevationBaseSeverURL + GenerateTokenElevation();
            mapserviceElevation.Url = serverURL;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = resolution;
            geoImgDesc.Width = resolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = DDToWebMercatorLon(left);
            extentElevation.YMin = DDToWebMercatorLat(bottom);
            extentElevation.XMax = DDToWebMercatorLon(right);
            extentElevation.YMax = DDToWebMercatorLat(top);
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;

            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
            File.WriteAllBytes(fileName, result.ImageData);
        }

        public static float[,] LoadHeightmapFromLocalServer(string fileName)
        {
            HeightData.LoadHeightDataFromESRITif(fileName);
            return HeightData.heightData;
        }

        public static void LoadSatelliteMapFromESRIServer(string fileName, double top, double left, double bottom, double right, int resolution)
        {
            TerraLandWorldImagery.World_Imagery_MapServer mapserviceImagery = new TerraLandWorldImagery.World_Imagery_MapServer();
            mapserviceImagery.Timeout = 5000000;
            TerraLandWorldImagery.TileImageInfo tileImageInfo = mapserviceImagery.GetTileImageInfo(mapserviceImagery.GetDefaultMapName());
            tileImageInfo.CompressionQuality = 100;

            TerraLandWorldImagery.MapServerInfo mapinfo = mapserviceImagery.GetServerInfo(mapserviceImagery.GetDefaultMapName());
            TerraLandWorldImagery.MapDescription mapdesc = mapinfo.DefaultMapDescription;

            double xMinLeft = left * 20037508.34 / 180.0;
            double yMaxTop = Math.Log(Math.Tan((90.0 + top) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMaxTop = yMaxTop * 20037508.34 / 180.0;

            double xMaxRight = right * 20037508.34 / 180.0;
            double yMinBottom = Math.Log(Math.Tan((90.0 + bottom) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMinBottom = yMinBottom * 20037508.34 / 180.0;

            TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
            extent.XMin = xMinLeft;
            extent.YMin = yMinBottom;
            extent.XMax = xMaxRight;
            extent.YMax = yMaxTop;
            mapdesc.MapArea.Extent = extent;

            TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
            imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
            imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

            TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
            imgdisp.ImageHeight = resolution;
            imgdisp.ImageWidth = resolution;

            imgdisp.ImageDPI = 72;

            TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
            imgdesc.ImageDisplay = imgdisp;
            imgdesc.ImageType = imgtype;

            TerraLandWorldImagery.MapImage mapimg = mapserviceImagery.ExportMapImage(mapdesc, imgdesc);
            File.WriteAllBytes(fileName, mapimg.ImageData);
        }

        public static Texture2D LoadSatelliteMapFromLocalServer(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            Texture2D image = new Texture2D(1, 1);
            image.LoadImage(bytes);
            return image;
        }

        private static string GenerateTokenElevation()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(tokenBaseURL);

            req.KeepAlive = false;
            req.ProtocolVersion = HttpVersion.Version10;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            try
            {
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                return str.Replace("{\"access_token\":\"", "").Replace("\",\"expires_in\":1209600}", "");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            return null;
        }

        private static double DDToWebMercatorLat(double mercatorY_lat)
        {
            double num = mercatorY_lat * 0.017453292519943295;
            double result = 3189068.5 * Math.Log((1.0 + Math.Sin(num)) / (1.0 - Math.Sin(num)));
            return result;
        }

        private static double DDToWebMercatorLon(double mercatorX_lon)
        {
            double num = mercatorX_lon * 0.017453292519943295;
            double result = 6378137.0 * num;
            return result;
        }
    }
}

