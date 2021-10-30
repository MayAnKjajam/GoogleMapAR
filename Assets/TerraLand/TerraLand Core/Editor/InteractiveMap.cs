using UnityEngine;
using UnityEditor;
using System;

namespace TerraLand
{
    public class InteractiveMap : EditorWindow
    {
        public static void Init()
        {
            InteractiveMap window = (InteractiveMap)EditorWindow.GetWindow(typeof(InteractiveMap));
            window.position = new Rect(5, 135, 1600, 670);
        }


        Vector2 mouse_move;
        Event key;
        Vector2 offset_map;
        Vector2 offset_map1;
        Vector2 offset_map2;
        Vector2 offset_map3;
        Vector2 offset_map4;

        Vector2 offset;
        UnityEngine.Color[] pixels;
        Vector2 offset2;
        float time1;

        bool zooming = false;
        double zoom;
        double zoom1;
        double zoom2;
        double zoom3;
        double zoom4;
        double zoom_step;
        double zoom1_step;
        double zoom2_step;
        double zoom3_step;
        double zoom4_step;
        double zoom_pos;
        double zoom_pos1;
        double zoom_pos2;
        double zoom_pos3;
        double zoom_pos4;

        bool request1;
        bool request2;
        bool request3;
        bool request4;

        bool request_load1_1;
        bool request_load1_2;
        bool request_load1_3;
        bool request_load1_4;
        bool request_load2_1;
        bool request_load2_2;
        bool request_load2_3;
        bool request_load2_4;
        bool request_load3;
        bool request_load4;

        bool animate = false;
        latlong_class latlong_animate;
        latlong_class latlong_mouse;

        bool map_scrolling = false;
        float save_global_time;
        public bool focus = false;
        Vector2 scrollPos;
        private float mouse_sensivity = 2f;

        int image_width;
        int image_height;

        double minLatitude = -85.05112878;
        double maxLatitude = 85.05112878;
        double pi = 3.14159265358979323846264338327950288419716939937510;

        private WWW myExt1_1;
        private WWW myExt1_2;
        private WWW myExt1_3;
        private WWW myExt1_4;
        private WWW myExt2_1;
        private WWW myExt2_2;
        private WWW myExt2_3;
        private WWW myExt2_4;
        private WWW myExt3;
        private WWW myExt4;

        private float save_global_timer = 5f;

        bool map_load1_1 = false;
        bool map_load1_2 = false;
        bool map_load1_3 = false;
        bool map_load1_4 = false;
        bool map_load2_1 = false;
        bool map_load2_2 = false;
        bool map_load2_3 = false;
        bool map_load2_4 = false;
        bool map_load3 = false;
        bool map_load4 = false;

        public static int map_zoom = 17;
        int map_zoom_old;

        Texture2D map0;
        Texture2D map1_1;
        Texture2D map1_2;
        Texture2D map1_3;
        Texture2D map1_4;
        Texture2D map2_1;
        Texture2D map2_2;
        Texture2D map2_3;
        Texture2D map2_4;
        Texture2D map3;
        Texture2D map4;

        public enum mapSourceEnum
        {
            google,
            bing,
            mapbox,
            mapquest,
            yandex,
            openstreetmap
        }
        public static mapSourceEnum mapSource = mapSourceEnum.google;

        public enum mapTypeGoogleEnum
        {
            roadmap,
            terrain,
            satellite,
            hybrid
        }
        public static mapTypeGoogleEnum mapTypeGoogle = mapTypeGoogleEnum.hybrid;

        public enum mapTypeBingEnum
        {
            Aerial,
            AerialWithLabels,
            Road
        }
        public static mapTypeBingEnum mapTypeBing = mapTypeBingEnum.AerialWithLabels;

        public enum mapTypeMapBoxEnum
        {
            Streets,
            StreetsBasic,
            StreetsSatellite,
            Light,
            Dark,
            Satellite,
            Wheatpaste,
            Comic,
            Outdoors,
            RunBikeHike,
            Pencil,
            Pirates,
            Emerald,
            HighContrast
        }
        public static mapTypeMapBoxEnum mapTypeMapBox = mapTypeMapBoxEnum.Emerald;
        string mapboxType;

        public enum mapTypeMapQuestEnum
        {
            Map,
            Satellite,
            Hybrid
        }
        public static mapTypeMapQuestEnum mapTypeMapQuest = mapTypeMapQuestEnum.Map;
        string mapquestType;

        public enum mapTypeYandexEnum
        {
            Map,
            Satellite,
            Geo,
            Traffic,
            MapGeo,
            MapTraffic,
            MapGeoTraffic,
            SatelliteGeo,
            SatelliteTraffic,
            SatelliteGeoTraffic
        }
        public static mapTypeYandexEnum mapTypeYandex = mapTypeYandexEnum.Map;
        string yandexType;

        latlong_class map_latlong = new latlong_class();
        public static latlong_class map_latlong_center = new latlong_class();
        latlong_class centerCoords = new latlong_class();

        private int tileSize = 400;
        int cropSize = 32;
        string url;
        string bingKey = "AkVbFBwbHyzgZH_12ZGeqWVk0hnpUt-XxEtZqzP4ZuEBOJuMqPbDckEH3EJBOiIr";
        string googleKey = "AIzaSyCb__ygqWRPGXz-kXG0dIVa5077IhVbWBg";
        string mapboxKey = "pk.eyJ1IjoidGVycmF1bml0eSIsImEiOiJjaW5mc2JndDkwMDhudmhseHBjY3RzZ2sxIn0.1Ww4CBximhSucJQ_PMwKmg";
        string mapquestKey = "8f0uA1tzcCOyhAqeTq5k8U9cGeQ4twSM";

        Rect areaRect;
        Vector2 topLeft;
        Vector2 bottomRight;
        latlong_class topLeftCoords = new latlong_class();
        latlong_class bottomRightCoords = new latlong_class();
        Texture2D centerCross;
        Material mat;
        string mouseLocation;
        float loadedTiles = 0;
        bool checkedTile1, checkedTile2, checkedTile3, checkedTile4, checkedTile5, checkedTile6, checkedTile7, checkedTile8, checkedTile9, checkedTile10 = false;
        public static bool updateArea = true;
        public static bool showArea = true;
        public static bool showCross = true;

        public static int requestIndex = 0;


        void OnEnable()
        {
            SetCrossMarker();
            RequestMap();
        }

        void OnInspectorUpdate()
        {
            if (focus)
                this.Repaint();
        }

        void OnFocus()
        {
            focus = true;
        }

        void OnLostFocus()
        {
            focus = false;
        }

        void SetCrossMarker()
        {
            centerCross = Resources.Load("TerraUnity/Downloader/CenterCross") as Texture2D;
            mat = (Material)Resources.Load("TerraUnity/Downloader/CrossMat");
        }

        void OnGUI()
        {
            key = Event.current;

            if (key.type == EventType.KeyDown)
                RequestMap();

            latlong_mouse = pixel_to_latlong(new Vector2(key.mousePosition.x - (position.width / 2) + offset_map.x, key.mousePosition.y - (position.height / 2) - offset_map.y), map_latlong_center, zoom);

            if (map4 && map_zoom_old > 3)
            {
                image_width = (int)((zoom_pos4 + 1) * map4.width * 4 * 2);
                image_height = (int)((zoom_pos4 + 1) * map4.height * 4 * 2);

                if (image_width < position.width * 12)
                    EditorGUI.DrawPreviewTexture(new Rect(((-offset_map4.x) - ((float)zoom_pos4 * ((map4.width * 2 * 2) + offset_map4.x)) - (tileSize * 3)) - (tileSize - position.width / 2), ((offset_map4.y) - ((float)zoom_pos4 * ((tileSize * 2 * 2) - offset_map4.y)) - ((tileSize / 2) * 7)) - ((tileSize / 2) - position.height / 2), (float)image_width, (float)image_height), map4);
            }

            if (map3 && map_zoom_old > 2)
            {
                if ((map_zoom > map_zoom_old + 2 && map_load4) && !map_load3) { }
                else
                {
                    image_width = (int)((zoom_pos3 + 1) * map3.width * 4);
                    image_height = (int)((zoom_pos3 + 1) * map3.height * 4);

                    if (image_width < position.width * 12)
                        EditorGUI.DrawPreviewTexture(new Rect(((-offset_map3.x) - ((float)zoom_pos3 * ((map3.width * 2) + offset_map3.x)) - tileSize) - (tileSize - position.width / 2), ((offset_map3.y) - ((float)zoom_pos3 * ((tileSize * 2) - offset_map3.y)) - ((tileSize / 2) * 3)) - ((tileSize / 2) - position.height / 2), (float)image_width, (float)image_height), map3);
                }
            }

            if (map0)
            {
                if ((map_zoom > map_zoom_old + 2 && map_load3) || (map_zoom > map_zoom_old + 3 && map_load4)) { }
                else
                {
                    image_width = (int)(map0.width + ((float)zoom_pos1 * map0.width));
                    image_height = (int)(map0.height + ((float)zoom_pos1 * map0.height));

                    Rect rect = new Rect
                        (
                            (-offset_map1.x) - ((float)zoom_pos1 * (800 + offset_map1.x)) - (800 - position.width / 2),
                            (offset_map1.y) - ((float)zoom_pos1 * ((800 / 2) - offset_map1.y)) - ((800 / 2) - position.height / 2),
                            (float)image_width,
                            (float)image_height
                        );

                    EditorGUI.DrawPreviewTexture(rect, map0);
                }
            }

            if (showArea)
            {
                if (requestIndex == 0)
                {
                    topLeftCoords = new latlong_class(Double.Parse(TerrainGenerator.top), Double.Parse(TerrainGenerator.left));
                    bottomRightCoords = new latlong_class(Double.Parse(TerrainGenerator.bottom), Double.Parse(TerrainGenerator.right));

                    if (updateArea)
                        centerCoords = new latlong_class(Double.Parse(TerrainGenerator.latitudeUser), Double.Parse(TerrainGenerator.longitudeUser));
                    else
                        centerCoords = pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);
                }
                else if (requestIndex == 1)
                {
                    topLeftCoords = new latlong_class(Double.Parse(TerraLand_Terrain.top), Double.Parse(TerraLand_Terrain.left));
                    bottomRightCoords = new latlong_class(Double.Parse(TerraLand_Terrain.bottom), Double.Parse(TerraLand_Terrain.right));

                    centerCoords = pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);
                }

                topLeft = latlong_to_pixel(topLeftCoords, centerCoords, zoom, new Vector2(position.width, position.height));
                bottomRight = latlong_to_pixel(bottomRightCoords, centerCoords, zoom, new Vector2(position.width, position.height));

                areaRect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);

                EditorGUI.DrawRect(areaRect, new UnityEngine.Color(0f, 0.75f, 0f, 0.2f));
            }

            if (showCross)
                EditorGUI.DrawPreviewTexture(new Rect((Screen.width / 2) - 12, (Screen.height / 2) - 22, 24, 24), centerCross, mat);

            GUI.color = UnityEngine.Color.gray;

            mouseLocation = Math.Round(latlong_mouse.latitude, 9, MidpointRounding.AwayFromZero) +
            ", " +
            Math.Round(latlong_mouse.longitude, 9, MidpointRounding.AwayFromZero);

            string mapProvider = mapSource.ToString().ToUpper();

            EditorGUI.HelpBox(new Rect(5, Screen.height - 40, 365, 16), mapProvider + "    Zoom: " + map_zoom + "    " + mouseLocation, MessageType.None);

            if (requestIndex == 0)
            {
                EditorGUI.HelpBox
                (
                    new Rect(Screen.width - 210, Screen.height - 40, 200, 16),
                    "AREA SIZE: " + TerrainGenerator.areaSizeLon.ToString("0.000") + " x " + TerrainGenerator.areaSizeLat.ToString("0.000") + " KM",
                    MessageType.None
                );
            }
            else if (requestIndex == 1)
            {
                EditorGUI.HelpBox
                (
                    new Rect(Screen.width - 210, Screen.height - 40, 200, 16),
                    "AREA SIZE: " + TerraLand_Terrain.areaSizeLon.ToString("0.000") + " x " + TerraLand_Terrain.areaSizeLat.ToString("0.000") + " KM",
                    MessageType.None
                );
            }

            GUI.color = UnityEngine.Color.black;

            if (map_load1_1 && map_load1_2 && map_load1_3 && map_load1_4 && map_load2_1 && map_load2_2 && map_load2_3 && map_load2_4 && map_load3 && map_load4)
            {
                loadedTiles = 0;
                checkedTile1 = checkedTile2 = checkedTile3 = checkedTile4 = checkedTile5 = checkedTile6 = checkedTile7 = checkedTile8 = checkedTile9 = checkedTile10 = false;
            }
            else
            {
                if (!checkedTile1 && map_load1_1) { loadedTiles++; checkedTile1 = true; }
                if (!checkedTile2 && map_load1_2) { loadedTiles++; checkedTile2 = true; }
                if (!checkedTile3 && map_load1_3) { loadedTiles++; checkedTile3 = true; }
                if (!checkedTile4 && map_load1_4) { loadedTiles++; checkedTile4 = true; }
                if (!checkedTile5 && map_load2_1) { loadedTiles++; checkedTile5 = true; }
                if (!checkedTile6 && map_load2_2) { loadedTiles++; checkedTile6 = true; }
                if (!checkedTile7 && map_load2_3) { loadedTiles++; checkedTile7 = true; }
                if (!checkedTile8 && map_load2_4) { loadedTiles++; checkedTile8 = true; }
                if (!checkedTile9 && map_load3) { loadedTiles++; checkedTile9 = true; }
                if (!checkedTile10 && map_load4) { loadedTiles++; checkedTile10 = true; }
            }

            float progressTiles = Mathf.InverseLerp(0f, 9f, loadedTiles) * (Screen.width - 10);

            if (loadedTiles != 0)
                EditorGUI.HelpBox(new Rect(5, 5, progressTiles, 10), "", MessageType.None);

            GUI.color = UnityEngine.Color.white;

            zoom = Math.Log((zoom_pos1 + 1), 2.0) + (double)map_zoom_old;
            mouse_move = key.delta;

            if (key.button == 0 && key.clickCount == 2)
            {
                if (key.mousePosition.y > 20)
                {
                    stop_download();
                    latlong_animate = latlong_mouse;
                    animate = true;
                }
            }

            if (key.button == 0)
            {
                if (key.type == EventType.MouseDown)
                {
                    map_scrolling = true;
                }

                else if (key.type == EventType.MouseUp)
                {
                    map_scrolling = false;
                }

                if (key.type == EventType.MouseDrag)
                {
                    if (map_scrolling && key.mousePosition.y > 0)
                    {
                        animate = false;
                        move_center(new Vector2(-mouse_move.x / mouse_sensivity, mouse_move.y / mouse_sensivity), true);
                    }
                }
            }

            if (key.type == EventType.ScrollWheel)
            {
                bool zoom_change = false;

                if (key.delta.y > 0)
                {
                    if (map_zoom > 1)
                    {
                        if (zoom1 > 0)
                        {
                            zoom1 = (zoom1 - 1) / 2;

                            if (zoom1 < 1)
                                zoom1 = 0;
                        }
                        else if (zoom1 < 0)
                        {
                            zoom1_step /= 2;
                            zoom1 += zoom1_step;
                        }
                        else
                        {
                            zoom1 = -0.5f;
                            zoom1_step = -0.5f;
                        }

                        if (zoom2 > 0)
                        {
                            zoom2 = (zoom2 - 1) / 2;

                            if (zoom2 < 1)
                                zoom2 = 0;
                        }
                        else if (zoom2 < 0)
                        {
                            zoom2_step /= 2;
                            zoom2 += zoom2_step;
                        }
                        else
                        {
                            zoom2 = -0.5f;
                            zoom2_step = -0.5f;
                        }

                        if (zoom3 > 0) { zoom3 = (zoom3 - 1) / 2; if (zoom3 < 1) { zoom3 = 0; } }
                        else if (zoom3 < 0) { zoom3_step /= 2; zoom3 += zoom3_step; } else { zoom3 = -0.5; zoom3_step = -0.5; }

                        if (zoom4 > 0) { zoom4 = (zoom4 - 1) / 2; if (zoom4 < 1) { zoom4 = 0; } }
                        else if (zoom4 < 0) { zoom4_step /= 2; zoom4 += zoom4_step; } else { zoom4 = -0.5; zoom4_step = -0.5; }

                        convert_center();
                        --map_zoom;
                        zoom_change = true;
                        RequestMap_timer();
                    }
                }
                else
                {
                    if (map_zoom < 19)
                    {
                        if (zoom1 < 0) { zoom1 -= zoom1_step; zoom1_step *= 2; if (zoom1 > -0.5) { zoom1 = 0; } }
                        else if (zoom1 > 0)
                        {
                            zoom1 = (zoom1 * 2) + 1;
                        }
                        else { zoom1 = 1; }

                        if (zoom2 < 0) { zoom2 -= zoom2_step; zoom2_step *= 2; if (zoom2 > -0.5) { zoom2 = 0; } }
                        else if (zoom2 > 0)
                        {
                            zoom2 = (zoom2 * 2) + 1;
                        }
                        else { zoom2 = 1; }

                        if (zoom3 < 0) { zoom3 -= zoom3_step; zoom3_step *= 2; if (zoom3 > -0.5) { zoom3 = 0; } }
                        else if (zoom3 > 0)
                        {
                            zoom3 = (zoom3 * 2) + 1;
                        }
                        else { zoom3 = 1; }

                        if (zoom4 < 0) { zoom4 -= zoom4_step; zoom4_step *= 2; if (zoom4 > -0.5) { zoom4 = 0; } }
                        else if (zoom4 > 0)
                        {
                            zoom4 = (zoom4 * 2) + 1;
                        }
                        else { zoom4 = 1; }

                        convert_center();
                        ++map_zoom;

                        zoom_change = true;
                        RequestMap_timer();
                    }
                }

                if (zoom_change)
                {
                    stop_download();
                    time1 = Time.realtimeSinceStartup;
                    zooming = true;
                }
            }
        }

        bool move_to_latlong(latlong_class latlong, float speed)
        {
            latlong_class latlong_center = pixel_to_latlong(Vector2.zero, map_latlong_center, zoom);

            Vector2 pixel = latlong_to_pixel(latlong, latlong_center, zoom, new Vector2(position.width, position.height));

            float delta_x = (pixel.x - (position.width / 2)) - offset_map.x;
            float delta_y = -((pixel.y - (position.height / 2)) + offset_map.y);

            if (Mathf.Abs(delta_x) < 0.01f && Mathf.Abs(delta_y) < 0.01f)
            {
                map_latlong_center = latlong;
                offset_map = Vector2.zero;

                RequestMap();
                this.Repaint();
                return true;
            }

            delta_x /= (250 / speed);
            delta_y /= (250 / speed);

            move_center(new Vector2(delta_x, delta_y), false);

            return false;
        }

        void move_center(Vector2 offset2, bool map)
        {
            offset = offset2;
            offset_map += offset;

            if (zoom_pos1 != 0)
                offset_map1 += offset / (float)(zoom_pos1 + 1);
            else
                offset_map1 += offset;

            if (zoom_pos2 != 0)
                offset_map2 += offset / (float)(zoom_pos2 + 1);
            else
                offset_map2 += offset;

            if (zoom_pos3 != 0)
                offset_map3 += offset / (float)(zoom_pos3 + 1);
            else
                offset_map3 += offset;

            if (zoom_pos4 != 0)
                offset_map4 += offset / (float)(zoom_pos4 + 1);
            else
                offset_map4 += offset;

            if (map)
            {
                stop_download();
                RequestMap_timer();
            }

            this.Repaint();
        }

        void convert_center()
        {
            map_latlong_center = pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);
            offset_map = Vector2.zero;
        }

        void RequestMap_timer()
        {
            time1 = Time.realtimeSinceStartup;

            request1 = true;
            request2 = true;
            request3 = true;
            request4 = true;

            this.Repaint();
        }

        public void RequestMap()
        {
            RequestMap1_1();
            RequestMap1_2();
            RequestMap1_3();
            RequestMap1_4();
            RequestMap2_1();
            RequestMap2_2();
            RequestMap2_3();
            RequestMap2_4();
            RequestMap3();
            RequestMap4();

            this.Repaint();
        }

        void RequestMap1_1()
        {
            stop_download_map1_1();
            request_load1_1 = true;
            map_load1_1 = false;

            map_latlong = pixel_to_latlong(new Vector2(-((tileSize * 2) - (tileSize / 2)), -(tileSize / 2)), map_latlong_center, map_zoom); // 1-1

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt1_1 != null)
                myExt1_1.Dispose();

            myExt1_1 = new WWW(url);
        }

        void RequestMap1_2()
        {
            stop_download_map1_2();
            request_load1_2 = true;
            map_load1_2 = false;

            map_latlong = pixel_to_latlong(new Vector2(-(tileSize / 2), -(tileSize / 2)), map_latlong_center, map_zoom); // 1-2

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt1_2 != null)
                myExt1_2.Dispose();

            myExt1_2 = new WWW(url);
        }

        void RequestMap1_3()
        {
            stop_download_map1_3();
            request_load1_3 = true;
            map_load1_3 = false;

            map_latlong = pixel_to_latlong(new Vector2((tileSize / 2), -(tileSize / 2)), map_latlong_center, map_zoom); // 1-3

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt1_3 != null)
                myExt1_3.Dispose();

            myExt1_3 = new WWW(url);
        }

        void RequestMap1_4()
        {
            stop_download_map1_4();
            request_load1_4 = true;
            map_load1_4 = false;

            map_latlong = pixel_to_latlong(new Vector2(((tileSize * 2) - (tileSize / 2)), -(tileSize / 2)), map_latlong_center, map_zoom); // 1-4

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt1_4 != null)
                myExt1_4.Dispose();

            myExt1_4 = new WWW(url);
        }

        void RequestMap2_1()
        {
            stop_download_map2_1();
            request_load2_1 = true;
            map_load2_1 = false;

            map_latlong = pixel_to_latlong(new Vector2(-((tileSize * 2) - (tileSize / 2)), (tileSize - (cropSize * 2)) / 2), map_latlong_center, map_zoom); // 2-1

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt2_1 != null)
                myExt2_1.Dispose();

            myExt2_1 = new WWW(url);
        }

        void RequestMap2_2()
        {
            stop_download_map2_2();
            request_load2_2 = true;
            map_load2_2 = false;

            map_latlong = pixel_to_latlong(new Vector2(-(tileSize / 2), (tileSize - (cropSize * 2)) / 2), map_latlong_center, map_zoom); // 2-2

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt2_2 != null)
                myExt2_2.Dispose();

            myExt2_2 = new WWW(url);
        }

        void RequestMap2_3()
        {
            stop_download_map2_3();
            request_load2_3 = true;
            map_load2_3 = false;

            map_latlong = pixel_to_latlong(new Vector2((tileSize / 2), (tileSize - (cropSize * 2)) / 2), map_latlong_center, map_zoom); // 2-3

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt2_3 != null)
                myExt2_3.Dispose();

            myExt2_3 = new WWW(url);
        }

        void RequestMap2_4()
        {
            stop_download_map2_4();
            request_load2_4 = true;
            map_load2_4 = false;

            map_latlong = pixel_to_latlong(new Vector2(((tileSize * 2) - (tileSize / 2)), (tileSize - (cropSize * 2)) / 2), map_latlong_center, map_zoom); // 2-4

            url = RequestTileURL(mapSource, map_latlong.latitude, map_latlong.longitude, map_zoom, tileSize);

            if (myExt2_4 != null)
                myExt2_4.Dispose();

            myExt2_4 = new WWW(url);
        }

        void RequestMap3()
        {
            if (map_zoom > 2)
            {
                stop_download_map3();
                request_load3 = true;
                map_load3 = false;

                url = RequestTileURL(mapSource, map_latlong_center.latitude, map_latlong_center.longitude, (map_zoom - 2), tileSize);

                if (myExt3 != null)
                    myExt3.Dispose();

                myExt3 = new WWW(url);
            }
        }

        void RequestMap4()
        {
            if (map_zoom > 3)
            {
                stop_download_map4();
                request_load4 = true;
                map_load4 = false;

                url = RequestTileURL(mapSource, map_latlong_center.latitude, map_latlong_center.longitude, (map_zoom - 3), tileSize);

                if (myExt4 != null)
                    myExt4.Dispose();

                myExt4 = new WWW(url);
            }
        }

        private string RequestTileURL(mapSourceEnum mapSource, double lat, double lon, int zoom, int resolution)
        {
            if (mapSource.ToString().Equals("google"))
            {
                string tileURL = "http://maps.googleapis.com/maps/api/staticmap?center=" +
                        lat +
                        "," +
                        lon +
                        "&zoom=" +
                        zoom +
                        "&scale=false" +
                        "&size=" +
                        resolution + "x" + resolution +
                        "&maptype=" +
                        mapTypeGoogle.ToString() +
                        "&key=" +
                        googleKey +
                        "&format=" +
                        "jpg" +
                        "&visual_refresh=" +
                        "true";

                return tileURL;
            }
            else if (mapSource.ToString().Equals("bing"))
            {
                string tileURL = "http://dev.virtualearth.net/REST/v1/Imagery/Map/" +
                        mapTypeBing.ToString() +
                        "/" +
                        lat +
                        "," +
                        lon +
                        "/" +
                        zoom +
                        "?&mapSize=" +
                        resolution + "," + resolution +
                        "&key=" +
                        bingKey;

                return tileURL;
            }
            else if (mapSource.ToString().Equals("mapbox"))
            {
                SetMapBoxType();

                string tileURL = "https://api.mapbox.com/v4/mapbox." +
                    mapboxType +
                    "/" +
                    lon +
                    "," +
                    lat +
                    "," +
                    zoom +
                    "/" +
                    resolution + "x" + resolution +
                    ".jpg70" +
                    "?access_token=" +
                    mapboxKey;

                return tileURL;
            }
            else if (mapSource.ToString().Equals("mapquest"))
            {
                SetMapQuestType();

                string tileURL = "http://www.mapquestapi.com/staticmap/v4/getplacemap" +
                    "?key=" +
                    mapquestKey +
                    "&location=" +
                    lat +
                    "," +
                    lon +
                    "&size=" +
                    resolution + "," + resolution +
                    "&type=" +
                    mapquestType +
                    "&zoom=" +
                    zoom +
                    "&imagetype=" +
                    "jpg" +
                    "&scalebar=" +
                    "false";

                return tileURL;
            }

            else if (mapSource.ToString().Equals("yandex"))
            {
                SetYandexType();

                string tileURL = "http://static-maps.yandex.ru/1.x/?lang=en-US&" +
                    "ll=" +
                    lon +
                    "," +
                    lat +
                    "&z=" +
                    zoom +
                    "&l=" +
                    yandexType +
                    "&size=" +
                    resolution + "," + resolution;

                return tileURL;
            }

            else if (mapSource.ToString().Equals("openstreetmap"))
            {
                string tileURL = "http://open.mapquestapi.com/staticmap/v4/getmap" +
                    "?key=" +
                    mapquestKey +
                    "&size=" +
                    resolution + "," + resolution +
                    "&zoom=" +
                    zoom +
                    "&center=" +
                    lat +
                    "," +
                    lon +
                    "&scalebar=" +
                    "false";

                return tileURL;
            }

            return null;
        }

        private void SetMapBoxType()
        {
            if (mapTypeMapBox.ToString().Equals("Streets"))
                mapboxType = "streets";
            else if (mapTypeMapBox.ToString().Equals("StreetsBasic"))
                mapboxType = "streets-basic";
            else if (mapTypeMapBox.ToString().Equals("StreetsSatellite"))
                mapboxType = "streets-satellite";
            else if (mapTypeMapBox.ToString().Equals("Light"))
                mapboxType = "light";
            else if (mapTypeMapBox.ToString().Equals("Dark"))
                mapboxType = "dark";
            else if (mapTypeMapBox.ToString().Equals("Satellite"))
                mapboxType = "satellite";
            else if (mapTypeMapBox.ToString().Equals("Wheatpaste"))
                mapboxType = "wheatpaste";
            else if (mapTypeMapBox.ToString().Equals("Comic"))
                mapboxType = "comic";
            else if (mapTypeMapBox.ToString().Equals("Outdoors"))
                mapboxType = "outdoors";
            else if (mapTypeMapBox.ToString().Equals("RunBikeHike"))
                mapboxType = "run-bike-hike";
            else if (mapTypeMapBox.ToString().Equals("Pencil"))
                mapboxType = "pencil";
            else if (mapTypeMapBox.ToString().Equals("Pirates"))
                mapboxType = "pirates";
            else if (mapTypeMapBox.ToString().Equals("Emerald"))
                mapboxType = "emerald";
            else if (mapTypeMapBox.ToString().Equals("HighContrast"))
                mapboxType = "high-contrast";
        }

        private void SetMapQuestType()
        {
            if (mapTypeMapQuest.ToString().Equals("Map"))
                mapquestType = "map";
            else if (mapTypeMapQuest.ToString().Equals("Satellite"))
                mapquestType = "sat";
            else if (mapTypeMapQuest.ToString().Equals("Hybrid"))
                mapquestType = "hyb";
        }

        private void SetYandexType()
        {
            if (mapTypeYandex.ToString().Equals("Map"))
                yandexType = "map";
            else if (mapTypeYandex.ToString().Equals("Satellite"))
                yandexType = "sat";
            else if (mapTypeYandex.ToString().Equals("Geo"))
                yandexType = "skl";
            else if (mapTypeYandex.ToString().Equals("Traffic"))
                yandexType = "trf";

            else if (mapTypeYandex.ToString().Equals("MapGeo"))
                yandexType = "map,skl";
            else if (mapTypeYandex.ToString().Equals("MapTraffic"))
                yandexType = "map,trf";
            else if (mapTypeYandex.ToString().Equals("MapGeoTraffic"))
                yandexType = "map,skl,trf";

            else if (mapTypeYandex.ToString().Equals("SatelliteGeo"))
                yandexType = "sat,skl";
            else if (mapTypeYandex.ToString().Equals("SatelliteTraffic"))
                yandexType = "sat,trf";
            else if (mapTypeYandex.ToString().Equals("SatelliteGeoTraffic"))
                yandexType = "map,skl,trf";
        }


        void stop_download()
        {
            stop_download_map1_1();
            stop_download_map1_2();
            stop_download_map1_3();
            stop_download_map1_4();
            stop_download_map2_1();
            stop_download_map2_2();
            stop_download_map2_3();
            stop_download_map2_4();
            stop_download_map3();
            stop_download_map4();
        }

        void stop_download_map1_1()
        {
            if (request_load1_1)
            {
                map_load1_1 = false;

                if (myExt1_1 != null)
                {
                    myExt1_1.Dispose();
                    myExt1_1 = null;
                }
            }
            request_load1_1 = false;
        }

        void stop_download_map1_2()
        {
            if (request_load1_2)
            {
                map_load1_2 = false;

                if (myExt1_2 != null)
                {
                    myExt1_2.Dispose();
                    myExt1_2 = null;
                }
            }
            request_load1_2 = false;
        }

        void stop_download_map1_3()
        {
            if (request_load1_3)
            {
                map_load1_3 = false;

                if (myExt1_3 != null)
                {
                    myExt1_3.Dispose();
                    myExt1_3 = null;
                }
            }
            request_load1_3 = false;
        }

        void stop_download_map1_4()
        {
            if (request_load1_4)
            {
                map_load1_4 = false;

                if (myExt1_4 != null)
                {
                    myExt1_4.Dispose();
                    myExt1_4 = null;
                }
            }
            request_load1_4 = false;
        }

        void stop_download_map2_1()
        {
            if (request_load2_1)
            {
                map_load2_1 = false;

                if (myExt2_1 != null)
                {
                    myExt2_1.Dispose();
                    myExt2_1 = null;
                }
            }
            request_load2_1 = false;
        }

        void stop_download_map2_2()
        {
            if (request_load2_2)
            {
                map_load2_2 = false;

                if (myExt2_2 != null)
                {
                    myExt2_2.Dispose();
                    myExt2_2 = null;
                }
            }
            request_load2_2 = false;
        }

        void stop_download_map2_3()
        {
            if (request_load2_3)
            {
                map_load2_3 = false;

                if (myExt2_3 != null)
                {
                    myExt2_3.Dispose();
                    myExt2_3 = null;
                }
            }
            request_load2_3 = false;
        }

        void stop_download_map2_4()
        {
            if (request_load2_4)
            {
                map_load2_4 = false;

                if (myExt2_4 != null)
                {
                    myExt2_4.Dispose();
                    myExt2_4 = null;
                }
            }
            request_load2_4 = false;
        }

        void stop_download_map3()
        {
            if (request_load3)
            {
                map_load3 = false;

                if (myExt3 != null)
                {
                    myExt3.Dispose();
                    myExt3 = null;
                }
            }
            request_load3 = false;
        }

        void stop_download_map4()
        {
            if (request_load4)
            {
                map_load4 = false;

                if (myExt4 != null)
                {
                    myExt4.Dispose();
                    myExt4 = null;
                }
            }
            request_load4 = false;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                this.Close();
                return;
            }

            if (Time.realtimeSinceStartup > save_global_time + (save_global_timer * 60))
                save_global_time = Time.realtimeSinceStartup;

            if (zooming)
            {
                zoom_pos = Mathf.Lerp((float)zoom_pos, (float)zoom1, 0.1f);

                zoom_pos1 = Mathf.Lerp((float)zoom_pos1, (float)zoom1, 0.1f);
                zoom_pos2 = Mathf.Lerp((float)zoom_pos2, (float)zoom2, 0.1f);

                zoom_pos3 = Mathf.Lerp((float)zoom_pos3, (float)zoom3, 0.1f);
                zoom_pos4 = Mathf.Lerp((float)zoom_pos4, (float)zoom4, 0.1f);

                if (Mathf.Abs((float)zoom_pos1 - (float)zoom1) < 0.001f)
                {
                    zoom_pos = zoom1;
                    zoom_pos1 = zoom1;
                    zoom_pos2 = zoom2;
                    zoom_pos3 = zoom3;
                    zoom_pos4 = zoom4;
                    zooming = false;
                }

                this.Repaint();
            }

            if (animate)
            {
                if (move_to_latlong(latlong_animate, 45))
                    animate = false;
            }

            if (request1 && Time.realtimeSinceStartup - time1 > 1.5f)
            {
                request1 = false;
                convert_center();

                RequestMap1_1();
                RequestMap2_1();
                RequestMap1_3();
                RequestMap2_3();
            }
            if (request2 && Time.realtimeSinceStartup - time1 > 1.7f)
            {
                request2 = false;
                convert_center();

                RequestMap1_2();
                RequestMap2_2();
                RequestMap1_4();
                RequestMap2_4();
            }

            if (request3 && Time.realtimeSinceStartup - time1 > 1.9f)
            {
                request3 = false;
                convert_center();

                RequestMap3();
            }
            if (request4 && Time.realtimeSinceStartup - time1 > 2.1f)
            {
                request4 = false;
                convert_center();

                RequestMap4();
            }

            if (myExt1_1 != null)
            {
                if (myExt1_1.isDone && !map_load1_1)
                {
                    map_load1_1 = true;

                    if (!map1_1)
                    {
                        map1_1 = new Texture2D(tileSize, tileSize);
                        map1_1.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt1_1.LoadImageIntoTexture(map1_1);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map1_1.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(0, tileSize - cropSize, tileSize, (tileSize - cropSize), pixels);
                }
            }

            if (myExt1_2 != null)
            {
                if (myExt1_2.isDone && !map_load1_2)
                {
                    if (!map1_2)
                    {
                        map1_2 = new Texture2D(tileSize, tileSize);
                        map1_2.wrapMode = TextureWrapMode.Clamp;
                    }

                    offset_map2 = Vector2.zero;
                    zoom2 = 0;
                    zoom_pos2 = 0;

                    this.Repaint();

                    map_load1_2 = true;

                    myExt1_2.LoadImageIntoTexture(map1_2);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map1_2.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize, tileSize - cropSize, tileSize, (tileSize - cropSize), pixels);
                }
            }

            if (myExt1_3 != null)
            {
                if (myExt1_3.isDone && !map_load1_3)
                {
                    map_load1_3 = true;

                    if (!map1_3)
                    {
                        map1_3 = new Texture2D(tileSize, tileSize);
                        map1_3.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt1_3.LoadImageIntoTexture(map1_3);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map1_3.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize * 2, tileSize - cropSize, tileSize, (tileSize - cropSize), pixels);
                }
            }

            if (myExt1_4 != null)
            {
                if (myExt1_4.isDone && !map_load1_4)
                {
                    map_load1_4 = true;

                    if (!map1_4)
                    {
                        map1_4 = new Texture2D(tileSize, tileSize);
                        map1_4.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt1_4.LoadImageIntoTexture(map1_4);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map1_4.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize * 3, tileSize - cropSize, tileSize, (tileSize - cropSize), pixels);
                }
            }

            if (myExt2_1 != null)
            {
                if (myExt2_1.isDone && !map_load2_1)
                {
                    map_load2_1 = true;

                    if (!map2_1)
                    {
                        map2_1 = new Texture2D(tileSize, tileSize);
                        map2_1.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt2_1.LoadImageIntoTexture(map2_1);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map2_1.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(0, 0, tileSize, tileSize - cropSize, pixels);
                }
            }

            if (myExt2_2 != null)
            {
                if (myExt2_2.isDone && !map_load2_2)
                {
                    if (!map2_2)
                    {
                        map2_2 = new Texture2D(tileSize, tileSize);
                        map2_2.wrapMode = TextureWrapMode.Clamp;
                    }

                    offset_map2 = Vector2.zero;
                    zoom2 = 0;
                    zoom_pos2 = 0;

                    this.Repaint();

                    map_load2_2 = true;

                    myExt2_2.LoadImageIntoTexture(map2_2);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map2_2.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize, 0, tileSize, tileSize - cropSize, pixels);
                }
            }

            if (myExt2_3 != null)
            {
                if (myExt2_3.isDone && !map_load2_3)
                {
                    map_load2_3 = true;

                    if (!map2_3)
                    {
                        map2_3 = new Texture2D(tileSize, tileSize);
                        map2_3.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt2_3.LoadImageIntoTexture(map2_3);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map2_3.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize * 2, 0, tileSize, tileSize - cropSize, pixels);
                }
            }

            if (myExt2_4 != null)
            {
                if (myExt2_4.isDone && !map_load2_4)
                {
                    map_load2_4 = true;

                    if (!map2_4)
                    {
                        map2_4 = new Texture2D(tileSize, tileSize);
                        map2_4.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt2_4.LoadImageIntoTexture(map2_4);

                    if (!map0)
                        map0 = new Texture2D(1600, (800 - (cropSize * 2)));

                    pixels = map2_4.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));
                    map0.SetPixels(tileSize * 3, 0, tileSize, tileSize - cropSize, pixels);
                }
            }

            if (map_load1_1 && map_load1_2 && map_load1_3 && map_load1_4 && map_load2_1 && map_load2_2 && map_load2_3 && map_load2_4)
            {
                map0.Apply();

                map_zoom_old = map_zoom;

                offset_map1 = Vector2.zero;
                zoom1 = 0;
                zoom_pos1 = 0;

                this.Repaint();
            }

            if (myExt3 != null)
            {
                if (myExt3.isDone && !map_load3)
                {
                    if (!map3)
                    {
                        map3 = new Texture2D(tileSize, (tileSize - cropSize));
                        map3.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt3.LoadImageIntoTexture(map3);
                    map_load3 = true;

                    if (map3.width == tileSize && map3.height == tileSize)
                    {
                        pixels = map3.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));

                        map3.Resize(tileSize, (tileSize - cropSize));
                        map3.SetPixels(0, 0, tileSize, (tileSize - cropSize), pixels);
                        map3.Apply();
                    }

                    zoom3 = 0;
                    zoom_pos3 = 0;
                    offset_map3 = Vector2.zero;
                    this.Repaint();
                }
            }

            if (myExt4 != null)
            {
                if (myExt4.isDone && !map_load4)
                {
                    if (!map4)
                    {
                        map4 = new Texture2D(tileSize, (tileSize - cropSize));
                        map4.wrapMode = TextureWrapMode.Clamp;
                    }

                    myExt4.LoadImageIntoTexture(map4);
                    map_load4 = true;

                    if (map4.width == tileSize && map4.height == tileSize)
                    {
                        pixels = map4.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));

                        map4.Resize(tileSize, (tileSize - cropSize));
                        map4.SetPixels(0, 0, tileSize, (tileSize - cropSize), pixels);
                        map4.Apply();
                    }

                    offset_map4 = Vector2.zero;
                    zoom_pos4 = 0;
                    zoom4 = 0;
                    this.Repaint();
                }
            }
        }

        public class latlong_class
        {
            //public double latitude = 35.6892;
            //public double longitude = 51.3890;

            public double latitude;
            public double longitude;

            public latlong_class()
            {

            }

            public latlong_class(double latitude1, double longitude1)
            {
                latitude = latitude1;
                longitude = longitude1;
            }

            void reset()
            {
                latitude = 0;
                longitude = 0;
            }
        }

        private class map_pixel_class
        {
            public double x;
            public double y;

            void reset()
            {
                x = 0;
                y = 0;
            }
        }

        latlong_class clip_latlong(latlong_class latlong)
        {
            if (latlong.latitude > maxLatitude)
                latlong.latitude -= (maxLatitude * 2);
            else if (latlong.latitude < minLatitude)
                latlong.latitude += (maxLatitude * 2);

            if (latlong.longitude > 180)
                latlong.longitude -= 360;
            else if (latlong.longitude < -180)
                latlong.longitude += 360;

            return latlong;
        }

        map_pixel_class clip_pixel(map_pixel_class map_pixel, double zoom)
        {
            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            if (map_pixel.x > mapSize - 1)
                map_pixel.x -= mapSize - 1;
            else if (map_pixel.x < 0)
                map_pixel.x = mapSize - 1 - map_pixel.x;

            if (map_pixel.y > mapSize - 1)
                map_pixel.y -= mapSize - 1;
            else if (map_pixel.y < 0)
                map_pixel.y = mapSize - 1 - map_pixel.y;

            return map_pixel;
        }

        Vector2 latlong_to_pixel(latlong_class latlong, latlong_class latlong_center, double zoom, Vector2 screen_resolution)
        {
            latlong = clip_latlong(latlong);
            latlong_center = clip_latlong(latlong_center);

            double x = (latlong.longitude + 180.0) / 360.0;
            double sinLatitude = Math.Sin(latlong.latitude * pi / 180.0);
            double y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            Vector2 pixel = new Vector2((float)x, (float)y);

            x = (latlong_center.longitude + 180.0) / 360.0;
            sinLatitude = Math.Sin(latlong_center.latitude * pi / 180.0);
            y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            Vector2 pixel2 = new Vector2((float)x, (float)y);
            Vector2 pixel3 = pixel - pixel2;

            pixel3.x *= (float)(256.0 * Math.Pow(2.0, zoom));
            pixel3.y *= (float)(256.0 * Math.Pow(2.0, zoom));

            pixel3 += new Vector2(screen_resolution.x / 2f, screen_resolution.y / 2f);

            return pixel3;
        }

        map_pixel_class latlong_to_pixel2(latlong_class latlong, double zoom)
        {
            latlong = clip_latlong(latlong);

            double x = (latlong.longitude + 180.0) / 360.0;
            double sinLatitude = Math.Sin(latlong.latitude * pi / 180.0);
            double y = 0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * pi);

            x *= 256.0 * Math.Pow(2.0, zoom);
            y *= 256.0 * Math.Pow(2.0, zoom);

            map_pixel_class map_pixel = new map_pixel_class();

            map_pixel.x = x;
            map_pixel.y = y;

            return map_pixel;
        }

        latlong_class pixel_to_latlong2(map_pixel_class map_pixel, double zoom)
        {
            map_pixel = clip_pixel(map_pixel, zoom);

            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            double x = (map_pixel.x / mapSize) - 0.5;
            double y = 0.5 - (map_pixel.y / mapSize);

            latlong_class latlong = new latlong_class();

            latlong.latitude = 90.0 - 360.0 * Math.Atan(Math.Exp(-y * 2.0 * pi)) / pi;
            latlong.longitude = 360.0 * x;

            return latlong;
        }

        latlong_class pixel_to_latlong(Vector2 offset, latlong_class latlong_center, double zoom)
        {
            double mapSize = 256.0 * Math.Pow(2.0, zoom);

            map_pixel_class map_pixel_center = latlong_to_pixel2(latlong_center, zoom);
            map_pixel_class map_pixel = new map_pixel_class();

            map_pixel.x = map_pixel_center.x + offset.x;
            map_pixel.y = map_pixel_center.y + offset.y;

            double x = (map_pixel.x / mapSize) - 0.5;
            double y = 0.5 - (map_pixel.y / mapSize);

            latlong_class latlong = new latlong_class();

            latlong.latitude = 90.0 - 360.0 * Math.Atan(Math.Exp(-y * 2.0 * pi)) / pi;
            latlong.longitude = 360.0 * x;

            latlong = clip_latlong(latlong);
            return latlong;
        }
    }
}

