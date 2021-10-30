using UnityEngine;
using System.Collections.Generic;
using MEC;

namespace TerraLand
{
    public class InfiniteTerrain : MonoBehaviour
    {
        private int GridWidth;
        private int GridHeight;
        private GameObject PlayerObject;

        private int _gridWidth;
        private int _gridHeight;
        public static Terrain[,] _grid;
        private GameObject _playerObject;

        private Runtime runTime;

        static float centerOffset;
        int chunks;

        public static bool northDetected = false;
        public static bool southDetected = false;
        public static bool eastDetected = false;
        public static bool westDetected = false;
        public static List<string> northTerrains;
        public static List<string> southTerrains;
        public static List<string> eastTerrains;
        public static List<string> westTerrains;
        public static List<Terrain> northTerrainsNeighbor;
        public static List<Terrain> southTerrainsNeighbor;
        public static List<Terrain> eastTerrainsNeighbor;
        public static List<Terrain> westTerrainsNeighbor;


        public static List<int> northIndexes;
        public static int northIndex;
        public static int southIndex;
        public static int eastIndex;
        public static int westIndex;


        public static List<int> northIndexImagery;
        public static List<int> southIndexImagery;
        public static List<int> eastIndexImagery;
        public static List<int> westIndexImagery;

        public static bool inProgressNorth;
        public static bool inProgressSouth;
        public static bool inProgressEast;
        public static bool inProgressWest;
        public static bool hybridNorth;
        public static bool hybridSouth;
        public static bool hybridEast;
        public static bool hybridWest;
        public static bool isOneStepNorth;
        public static bool isOneStepSouth;
        public static bool isOneStepEast;
        public static bool isOneStepWest;


        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            PlayerObject = Camera.main.gameObject;

            runTime = GameObject.Find("World Generator").GetComponent<Runtime>();
            _gridWidth = (int)runTime.terrainGridSize;
            _gridHeight = (int)runTime.terrainGridSize;
            _grid = new Terrain[_gridWidth, _gridHeight];
            chunks = (_gridWidth * _gridHeight);

            int counter = 0;

            if (transform.childCount == _gridWidth * _gridHeight)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    for (int y = 0; y < _gridHeight; y++)
                    {
                        _grid[y, x] = transform.GetChild(counter).GetComponent<Terrain>();
                        counter++;
                    }
                }
            }

            centerOffset = _grid[0, 0].terrainData.size.x / 2f;

            northTerrains = new List<string>();
            southTerrains = new List<string>();
            eastTerrains = new List<string>();
            westTerrains = new List<string>();

            northIndex = 0;
            southIndex = chunks - _gridWidth;
            eastIndex = _gridWidth - 1;
            westIndex = 0;

            northIndexes = new List<int>();
        }

        void Update()
        {
            if (northTerrains.Count > 0) inProgressNorth = true; else inProgressNorth = false;
            if (southTerrains.Count > 0) inProgressSouth = true; else inProgressSouth = false;
            if (eastTerrains.Count > 0) inProgressEast = true; else inProgressEast = false;
            if (westTerrains.Count > 0) inProgressWest = true; else inProgressWest = false;

            if (northTerrains.Count <= _gridWidth) isOneStepNorth = true; else isOneStepNorth = false;
            if (southTerrains.Count <= _gridWidth) isOneStepSouth = true; else isOneStepSouth = false;
            if (eastTerrains.Count <= _gridWidth) isOneStepEast = true; else isOneStepEast = false;
            if (westTerrains.Count <= _gridWidth) isOneStepWest = true; else isOneStepWest = false;

            Vector3 playerPosition = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, PlayerObject.transform.position.z);
            Terrain playerTerrain = null;
            int xOffset = 0;
            int yOffset = 0;

            northDetected = false;
            southDetected = false;
            eastDetected = false;
            westDetected = false;

            //foreach(Terrain t in TerraLand.TerraLandRuntime.terrainsInProgress)
            //t.drawHeightmap = false;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if ((playerPosition.x >= _grid[x, y].transform.position.x + centerOffset) &&
                        (playerPosition.x <= (_grid[x, y].transform.position.x + _grid[x, y].terrainData.size.x) + centerOffset) &&
                        (playerPosition.z >= _grid[x, y].transform.position.z - centerOffset) &&
                        (playerPosition.z <= (_grid[x, y].transform.position.z + _grid[x, y].terrainData.size.z) - centerOffset))
                    {
                        playerTerrain = _grid[x, y];
                        xOffset = ((_gridWidth - 1) / 2) - x;
                        yOffset = ((_gridHeight - 1) / 2) - y;
                        break;
                    }
                }

                if (playerTerrain != null)
                    break;
            }

            if (TerraLand.TerraLandRuntime.worldIsGenerated && playerTerrain != _grid[(_gridWidth - 1) / 2, (_gridHeight - 1) / 2])
            {
                Terrain[,] newGrid = new Terrain[_gridWidth, _gridHeight];

                for (int x = 0; x < _gridWidth; x++)
                {
                    for (int y = 0; y < _gridHeight; y++)
                    {
                        int newX = x + xOffset;

                        // Moving EAST
                        if (newX < 0)
                        {
                            newX = _gridWidth - 1;
                            _grid[x, y].drawHeightmap = false;
                            Timing.RunCoroutine(GenerateEAST(x, y));
                        }

                        //Moving WEST
                        else if (newX > (_gridWidth - 1))
                        {
                            newX = 0;
                            _grid[x, y].drawHeightmap = false;
                            Timing.RunCoroutine(GenerateWEST(x, y));
                        }

                        int newY = y + yOffset;

                        //Moving SOUTH
                        if (newY < 0)
                        {
                            newY = _gridHeight - 1;
                            _grid[x, y].drawHeightmap = false;
                            Timing.RunCoroutine(GenerateSOUTH(x, y));
                        }

                        //Moving NORTH
                        else if (newY > (_gridHeight - 1))
                        {
                            newY = 0;
                            _grid[x, y].drawHeightmap = false;
                            Timing.RunCoroutine(GenerateNORTH(x, y));
                        }

                        newGrid[newX, newY] = _grid[x, y];
                    }
                }

                _grid = newGrid;
                UpdatePositions();
            }
        }

        private IEnumerator<float> GenerateNORTH(int x, int y)
        {
            //        northIndex = int.Parse(_grid[x, y].name.Split(new char[] {' '})[0]) - 1;
            //        northIndexes.Add(int.Parse(_grid[x, y].name.Split(new char[] {' '})[0]) - 1);
            //
            //        //if(!northDetected)
            //        //{
            //        TerraLand.TerraLandRuntime.GetTerrainBoundsNORTH(northIndex);
            //            //northDetected = true;
            //        //}
            //
            //
            //
            //        print(_grid[x, y].name);
            //        //print(northIndex);
            //
            //        //yield return Timing.WaitForSeconds(1);
            //
            //        //TerraLand.TerraLandRuntime.GetDynamicTerrainNORTH(northIndex);



            if (northTerrains.Count == 0)
                northIndexImagery = new List<int>();

            northTerrains.Add(_grid[x, y].name);
            northIndexImagery.Add(int.Parse(_grid[x, y].name.Split(new char[] { ' ' })[0]) - 1);

            if (!northDetected)
            {
                TerraLand.TerraLandRuntime.GetTerrainBoundsNORTH();
                northDetected = true;
            }




            yield return 0;
        }

        private IEnumerator<float> GenerateSOUTH(int x, int y)
        {
            if (southTerrains.Count == 0)
                southIndexImagery = new List<int>();

            southTerrains.Add(_grid[x, y].name);
            southIndexImagery.Add(int.Parse(_grid[x, y].name.Split(new char[] { ' ' })[0]) - 1);

            if (!southDetected)
            {
                TerraLand.TerraLandRuntime.GetTerrainBoundsSOUTH();
                southDetected = true;
            }

            yield return 0;
        }

        private IEnumerator<float> GenerateEAST(int x, int y)
        {
            if (eastTerrains.Count == 0)
                eastIndexImagery = new List<int>();

            eastTerrains.Add(_grid[x, y].name);
            eastIndexImagery.Add(int.Parse(_grid[x, y].name.Split(new char[] { ' ' })[0]) - 1);

            if (!eastDetected)
            {
                TerraLand.TerraLandRuntime.GetTerrainBoundsEAST();
                eastDetected = true;
            }

            yield return 0;
        }

        private IEnumerator<float> GenerateWEST(int x, int y)
        {
            if (westTerrains.Count == 0)
                westIndexImagery = new List<int>();

            westTerrains.Add(_grid[x, y].name);
            westIndexImagery.Add(int.Parse(_grid[x, y].name.Split(new char[] { ' ' })[0]) - 1);

            if (!westDetected)
            {
                TerraLand.TerraLandRuntime.GetTerrainBoundsWEST();
                westDetected = true;
            }

            yield return 0;
        }

        private void UpdatePositions()
        {
            Terrain middle = _grid[(_gridWidth - 1) / 2, (_gridHeight - 1) / 2];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (!(x.Equals((_gridWidth - 1) / 2) && y.Equals((_gridHeight - 1) / 2)))
                    {
                        int xOffset = ((_gridWidth - 1) / 2) - x;
                        int yOffset = ((_gridHeight - 1) / 2) - y;

                        _grid[x, y].transform.position = new Vector3
                        (
                            middle.transform.position.x - (middle.terrainData.size.x * xOffset),
                            middle.transform.position.y,
                            middle.transform.position.z + (middle.terrainData.size.z * yOffset)
                        );
                    }
                }
            }


            northDetected = false;
            southDetected = false;
            eastDetected = false;
            westDetected = false;



            //		for (int x = 0; x < _gridWidth; x++)
            //        {
            //			for (int y = 0; y < _gridHeight; y++)
            //			{
            //				Terrain left = (x == 0) ? null : _grid[x - 1, y];
            //				Terrain top = (y == 0) ? null : _grid[x, y - 1];
            //				Terrain right = (x == (_gridWidth - 1)) ? null : _grid[x + 1, y];
            //				Terrain bottom = (y == (_gridHeight - 1)) ? null : _grid[x, y + 1];
            //
            //				_grid[x, y].SetNeighbors(left, top, right, bottom);
            //			}
            //        }
        }

        private void Extras()
        {
            /*
            if(!delayed)
                bottomLeft = _grid[0, _gridWidth - 1];
            else
                bottomLeft = _grid[0, 0];

            northIndexTerrain = int.Parse(bottomLeft.name.Split(new char[] {' '})[0]) - 1;

            if(!delayed)
                topLeft = _grid[0, 0];
            else
                topLeft = _grid[0, _gridWidth - 1];

            southIndexTerrain = int.Parse(topLeft.name.Split(new char[] {' '})[0]) - 1;

            if(!delayed)
                topRight = _grid[0, 0];
            else
                topRight = _grid[_gridHeight - 1, 0];

            eastIndexTerrain = int.Parse(topRight.name.Split(new char[] {' '})[0]) - 1;

            if(!delayed)
                bottomRight = _grid[_gridHeight - 1, 0];
            else
                bottomRight = _grid[0, 0];

            westIndexTerrain = int.Parse(bottomRight.name.Split(new char[] {' '})[0]) - 1;
            */
        }
    }
}

