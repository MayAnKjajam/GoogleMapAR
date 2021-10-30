using UnityEngine;
using System.Collections.Generic;
using MEC;
using TerraLand;

public class InfiniteTerrainOffline : MonoBehaviour
{
    public static RuntimeOffline runTime;
    private FloatingOriginAdvanced floatingOriginAdvanced;

    //private int GridWidth;
    //private int GridHeight;
	private static GameObject player;
	
	private int _gridWidth;
	private int _gridHeight;
    public static Terrain[,] _grid;
    private static Terrain[,] newGrid;
	//private GameObject _playerObject;
    private Terrain playerTerrain;

    static float centerOffset;

    public static bool northDetected = false;
    public static bool southDetected = false;
    public static bool eastDetected = false;
    public static bool westDetected = false;
    public static List<string> northTerrains;
    public static List<string> southTerrains;
    public static List<string> eastTerrains;
    public static List<string> westTerrains;
    public static List<Terrain> northTerrainsList;
    public static List<Terrain> southTerrainsList;
    public static List<Terrain> eastTerrainsList;
    public static List<Terrain> westTerrainsList;
    public static List<string> northTerrainsNeighbor;
    public static List<string> southTerrainsNeighbor;
    public static List<string> eastTerrainsNeighbor;
    public static List<string> westTerrainsNeighbor;

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

    private static int northCounter = 0;
    private static int southCounter = 0;
    private static int eastCounter = 0;
    private static int westCounter = 0;

    public static bool reachedMostNORTH = false;
    public static bool reachedMostSOUTH = false;
    public static bool reachedMostEAST = false;
    public static bool reachedMostWEST = false;

    public static Vector3 playerPosition;
    public static float areaLength;

    public static float boundsEdge;
    private static GameObject boundingBox;
    private bool boundsDebug = false;
    private Material debugMaterial;

    public static string excludedTerrainNORTH = "";
    public static string excludedTerrainSOUTH = "";
    public static string excludedTerrainEAST = "";
    public static string excludedTerrainWEST = "";

    private static float tileResolution;

    private static int counter;
    //private static float terrainsHeight;
    private static int xOffset;
    private static int yOffset;
    private static int newX;
    private static int newY;
    private static Terrain middle;

    //private static int startingTileRow = 0;
    //private static int startingTileColumn = 0;

    private static Collider boundsCollider;
    private static Renderer boundsRenderer;

    // Camera's Y angle on hybrid directions
    // North East Combo = 44.99
    // East North Combo = 45.01
    // East South Combo = 134.99
    // South East Combo = 135.01
    // South West Combo = 224.99
    // West South Combo = 225.01
    // West North Combo = 314.99
    // North West Combo = 315.01

    void Start ()
    {
        Initialize();
    }

    public void Initialize ()
    {
        player = Camera.main.gameObject;

        if(Camera.main.GetComponent<FloatingOriginAdvanced>() != null)
            floatingOriginAdvanced = Camera.main.GetComponent<FloatingOriginAdvanced>();

        _gridWidth = runTime.activeTilesGrid;
        _gridHeight = runTime.activeTilesGrid;
        _grid = new Terrain[_gridWidth, _gridHeight];
        counter = 0;

        if(transform.childCount == _gridWidth * _gridHeight)
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

        tileResolution = _grid[0, 0].terrainData.size.x;
        centerOffset = tileResolution / 2f;
        areaLength = RuntimeOffline.exaggeratedWorldSize * 1000;
        boundsEdge = (tileResolution * runTime.activeTilesGrid) + 0;

        northTerrains = new List<string>();
        southTerrains = new List<string>();
        eastTerrains = new List<string>();
        westTerrains = new List<string>();

        boundingBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundingBox.name = "Bounding Box";
        boundingBox.transform.position = new Vector3(RuntimeOffline.worldPositionOffsetX, 0, RuntimeOffline.worldPositionOffsetY);
        boundingBox.transform.localScale = new Vector3(areaLength - boundsEdge, 100000, areaLength - boundsEdge);
        boundsCollider = boundingBox.GetComponent<Collider>();
        boundsCollider.enabled = true;
        boundsCollider.isTrigger = true;
        
        boundsRenderer = boundingBox.GetComponent<Renderer>();

        if (boundsDebug)
            boundsRenderer.enabled = true;
        else
            boundsRenderer.enabled = false;

        debugMaterial = Resources.Load("TerraUnity/Debug/Debug Material") as Material;

        if (debugMaterial != null)
            boundsRenderer.sharedMaterial = debugMaterial;
    }

	void Update ()
	{
        ManageTerrains();
	}

    private void ManageTerrains ()
    {
        //if(northTerrains.Count <= _gridWidth) isOneStepNorth = true; else isOneStepNorth = false;
        //if(southTerrains.Count <= _gridWidth) isOneStepSouth = true; else isOneStepSouth = false;
        //if(eastTerrains.Count <= _gridWidth) isOneStepEast = true; else isOneStepEast = false;
        //if(westTerrains.Count <= _gridWidth) isOneStepWest = true; else isOneStepWest = false;

        playerPosition = player.transform.position;

        if (!boundsCollider.bounds.Contains(playerPosition))
        {
            float northPoint = boundingBox.transform.position.z + (boundingBox.transform.localScale.z / 2f);
            float southPoint = boundingBox.transform.position.z - (boundingBox.transform.localScale.z / 2f);
            float eastPoint = boundingBox.transform.position.x + (boundingBox.transform.localScale.x / 2f);
            float westPoint = boundingBox.transform.position.x - (boundingBox.transform.localScale.x / 2f);

            if (playerPosition.x > eastPoint)
                playerPosition = new Vector3(Mathf.Clamp(playerPosition.x, westPoint, eastPoint) , playerPosition.y, playerPosition.z);

            if (playerPosition.x < westPoint)
                playerPosition = new Vector3(Mathf.Clamp(playerPosition.x, westPoint, eastPoint), playerPosition.y, playerPosition.z);

            if (playerPosition.z > northPoint)
                playerPosition = new Vector3(playerPosition.x, playerPosition.y, Mathf.Clamp(playerPosition.z, southPoint, northPoint));

            if (playerPosition.z < southPoint)
                playerPosition = new Vector3(playerPosition.x, playerPosition.y, Mathf.Clamp(playerPosition.z, southPoint, northPoint));
        }

        DebugBounds();

        playerTerrain = null;
        xOffset = 0;
        yOffset = 0;

        northDetected = false;
        southDetected = false;
        eastDetected = false;
        westDetected = false;

        //foreach(Terrain t in TerraLandRuntimeOffline.terrainsInProgress)
        //t.drawHeightmap = false;

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                if(
                    (playerPosition.x >= _grid[x, y].transform.position.x + centerOffset) &&
                    (playerPosition.x <= (_grid[x, y].transform.position.x + tileResolution) + centerOffset) &&
                    (playerPosition.z >= _grid[x, y].transform.position.z - centerOffset) &&
                    (playerPosition.z <= (_grid[x, y].transform.position.z + tileResolution) - centerOffset)
                )
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

        if(TerraLandRuntimeOffline.worldIsGenerated && playerTerrain != _grid[(_gridWidth - 1) / 2, (_gridHeight - 1) / 2])
        {
            newGrid = new Terrain[_gridWidth, _gridHeight];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    newX = x + xOffset;

                    // Moving EAST
                    if (newX < 0)
                    {
                        newX = _gridWidth - 1;

                        if(!reachedMostEAST)
                            Timing.RunCoroutine(GenerateEAST(x, y, false));
                    }

                    //Moving WEST
                    else if (newX > (_gridWidth - 1))
                    {
                        newX = 0;

                        if(!reachedMostWEST)
                            Timing.RunCoroutine(GenerateWEST(x, y, false));
                    }

                    newY = y + yOffset;

                    //Moving SOUTH
                    if (newY < 0)
                    {
                        newY = _gridHeight - 1;

                        if(!reachedMostSOUTH)
                            Timing.RunCoroutine(GenerateSOUTH(x, y, false));
                    }

                    //Moving NORTH
                    else if (newY > (_gridHeight - 1))
                    {
                        newY = 0;

                        if(!reachedMostNORTH)
                            Timing.RunCoroutine(GenerateNORTH(x, y, false));
                    }

                    newGrid[newX, newY] = _grid[x, y];
                }
            }

            UpdatePositions();
        }
    }

    private IEnumerator<float> GenerateNORTH (int x, int y, bool retry)
    {
        northCounter++;

        if(inProgressEast || inProgressWest || inProgressSouth)
        {
            if(!retry && northCounter == 1)
            {
                if(inProgressWest)
                    excludedTerrainWEST = _grid[0, _gridWidth - 1].name;
                else if(inProgressEast)
                    excludedTerrainEAST = _grid[0, _gridWidth - 1].name;

                for (int i = 0; i < _gridWidth; i++)
                {
                    _grid[i, _gridWidth - 1].drawHeightmap = false;
                    _grid[i, _gridWidth - 1].transform.position = new Vector3
                        (
                        _grid[i, _gridWidth - 1].transform.position.x,
                        _grid[i, _gridWidth - 1].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
                        _grid[i, _gridWidth - 1].transform.position.z
                        );
                } 
            }

            if(northCounter == _gridWidth)
            {
                yield return Timing.WaitForSeconds(1f);
                northCounter = 0;

                for (int i = 0; i < _gridWidth; i++)
                    Timing.RunCoroutine(GenerateNORTH(i, 0, true));
            }

            yield break;
        }

        if(northCounter == 1)
        {
            if (inProgressNorth || inProgressSouth)
                TerraLandRuntimeOffline.FinalizeTiles(TerraLandRuntimeOffline.direction);

            inProgressNorth = true;
            inProgressSouth = false;
            reachedMostSOUTH = false;

            TerraLandRuntimeOffline.direction = "North";

            RuntimeOffline.padStartX--;
            RuntimeOffline.padEndX++;

            northTerrainsNeighbor = new List<string>();
            northTerrainsList = new List<Terrain>();
        }

        _grid[x, y].drawHeightmap = false;
        _grid[x, y].transform.position = new Vector3
            (
            _grid[x, y].transform.position.x,
            _grid[x, y].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
            _grid[x, y].transform.position.z
            );

        northTerrains.Add(_grid[x, y].name);
        northTerrainsNeighbor.Add(_grid[x, y].name);
        northTerrainsList.Add(_grid[x, y]);

        if (northCounter == _gridWidth)
        {
            //StopAllCoroutines();
            TerraLandRuntimeOffline.GetTerrainBounds();
            runTime.SendNewTiles(northTerrainsList);
            northCounter = 0;
        }
    }

    private IEnumerator<float> GenerateSOUTH (int x, int y, bool retry)
    {
        southCounter++;

        if(inProgressEast || inProgressWest || inProgressNorth)
        {
            if(!retry && southCounter == 1)
            {
                if(inProgressWest)
                    excludedTerrainWEST = _grid[0, 0].name;
                else if(inProgressEast)
                    excludedTerrainEAST = _grid[0, 0].name;

                for (int i = 0; i < _gridWidth; i++)
                {
                    _grid[i, 0].drawHeightmap = false;
                    _grid[i, 0].transform.position = new Vector3
                        (
                        _grid[i, 0].transform.position.x,
                        _grid[i, 0].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
                        _grid[i, 0].transform.position.z
                        );
                }
            }

            if(southCounter == _gridWidth)
            {
                yield return Timing.WaitForSeconds(1f);
                southCounter = 0;

                for (int i = 0; i < _gridWidth; i++)
                    Timing.RunCoroutine(GenerateSOUTH(i, _gridWidth - 1, true));
            }

            yield break;
        }

        if(southCounter == 1)
        {
            if (inProgressNorth || inProgressSouth)
                TerraLandRuntimeOffline.FinalizeTiles(TerraLandRuntimeOffline.direction);

            inProgressSouth = true;
            inProgressNorth = false;
            reachedMostNORTH = false;

            TerraLandRuntimeOffline.direction = "South";

            RuntimeOffline.padStartX++;
            RuntimeOffline.padEndX--;

            southTerrainsNeighbor = new List<string>();
            southTerrainsList = new List<Terrain>();
        }

        _grid[x, y].drawHeightmap = false;
        _grid[x, y].transform.position = new Vector3
            (
            _grid[x, y].transform.position.x,
            _grid[x, y].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
            _grid[x, y].transform.position.z
            );

        southTerrains.Add(_grid[x, y].name);
        southTerrainsNeighbor.Add(_grid[x, y].name);
        southTerrainsList.Add(_grid[x, y]);

        if (southCounter == _gridWidth)
        {
            //StopAllCoroutines();
            TerraLandRuntimeOffline.GetTerrainBounds();
            runTime.SendNewTiles(southTerrainsList);
            southCounter = 0;
        } 
    }

    private IEnumerator<float> GenerateEAST (int x, int y, bool retry)
    {
        eastCounter++;

        if(inProgressNorth || inProgressSouth || inProgressWest)
        {
            if(!retry && eastCounter == 1)
            {
                if(inProgressNorth)
                    excludedTerrainNORTH = _grid[0, 0].name;
                else if(inProgressSouth)
                    excludedTerrainSOUTH = _grid[0, 0].name;

                for (int i = 0; i < _gridWidth; i++)
                {
                    _grid[0, i].drawHeightmap = false;
                    _grid[0, i].transform.position = new Vector3
                        (
                        _grid[0, i].transform.position.x,
                        _grid[0, i].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
                        _grid[0, i].transform.position.z
                        );
                }
            }

            if(eastCounter == _gridWidth)
            {
                yield return Timing.WaitForSeconds(1f);
                eastCounter = 0;

                for (int i = 0; i < _gridWidth; i++)
                    Timing.RunCoroutine(GenerateEAST(_gridWidth - 1, i, true));
            }

            yield break;
        }

        if(eastCounter == 1)
        {
            if (inProgressEast || inProgressWest)
                TerraLandRuntimeOffline.FinalizeTiles(TerraLandRuntimeOffline.direction);

            inProgressEast = true;
            inProgressWest = false;
            reachedMostWEST = false;

            TerraLandRuntimeOffline.direction = "East";

            RuntimeOffline.padStartY++;
            RuntimeOffline.padEndY--;

            eastTerrainsNeighbor = new List<string>();
            eastTerrainsList = new List<Terrain>();
        }

        _grid[x, y].drawHeightmap = false;
        _grid[x, y].transform.position = new Vector3
            (
            _grid[x, y].transform.position.x,
            _grid[x, y].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
            _grid[x, y].transform.position.z
            );

        eastTerrains.Add(_grid[x, y].name);
        eastTerrainsNeighbor.Add(_grid[x, y].name);
        eastTerrainsList.Add(_grid[x, y]);

        if (eastCounter == _gridWidth)
        {
            //StopAllCoroutines();
            TerraLandRuntimeOffline.GetTerrainBounds();
            runTime.SendNewTiles(eastTerrainsList);
            eastCounter = 0;
        } 
    }

    private IEnumerator<float> GenerateWEST (int x, int y, bool retry)
    {
        westCounter++;

        if (inProgressNorth || inProgressSouth || inProgressEast)
        {
            if(!retry && westCounter == 1)
            {
                if(inProgressNorth)
                    excludedTerrainNORTH = _grid[_gridWidth - 1, _gridWidth - 1].name;
                else if(inProgressSouth)
                    excludedTerrainSOUTH = _grid[_gridWidth - 1, _gridWidth - 1].name;

                for (int i = 0; i < _gridWidth; i++)
                {
                    _grid[_gridWidth - 1, i].drawHeightmap = false;
                    _grid[_gridWidth - 1, i].transform.position = new Vector3
                        (
                        _grid[_gridWidth - 1, i].transform.position.x,
                        _grid[_gridWidth - 1, i].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
                        _grid[_gridWidth - 1, i].transform.position.z
                        );
                }
            }

            if(westCounter == _gridWidth)
            {
                yield return Timing.WaitForSeconds(1f);
                westCounter = 0;

                for (int i = 0; i < _gridWidth; i++)
                    Timing.RunCoroutine(GenerateWEST(0, i, true));
            }

            yield break;
        }

        if(westCounter == 1)
        {
            if (inProgressEast || inProgressWest)
                TerraLandRuntimeOffline.FinalizeTiles(TerraLandRuntimeOffline.direction);

            inProgressWest = true;
            inProgressEast = false;
            reachedMostEAST = false;

            TerraLandRuntimeOffline.direction = "West";

            RuntimeOffline.padStartY--;
            RuntimeOffline.padEndY++;

            westTerrainsNeighbor = new List<string>();
            westTerrainsList = new List<Terrain>();
        }

        _grid[x, y].drawHeightmap = false;
        _grid[x, y].transform.position = new Vector3
            (
            _grid[x, y].transform.position.x,
            _grid[x, y].transform.position.y - RuntimeOffline.hiddenTerrainsBelowUnits,
            _grid[x, y].transform.position.z
            );

        westTerrains.Add(_grid[x, y].name);
        westTerrainsNeighbor.Add(_grid[x, y].name);
        westTerrainsList.Add(_grid[x, y]);

        if (westCounter == _gridWidth)
        {
            //StopAllCoroutines();
            TerraLandRuntimeOffline.GetTerrainBounds();
            runTime.SendNewTiles(westTerrainsList);
            westCounter = 0;
        }
    }
	
	private void UpdatePositions ()
	{
        _grid = newGrid;
        middle = _grid[((_gridWidth - 1) / 2), (_gridHeight - 1) / 2];

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {       
                if (!(x.Equals((_gridWidth - 1) / 2) && y.Equals((_gridHeight - 1) / 2)))
                {
                    xOffset = ((_gridWidth - 1) / 2) - x;
                    yOffset = ((_gridHeight - 1) / 2) - y;

                    _grid[x, y].transform.position = new Vector3
                    (
                        middle.transform.position.x - (tileResolution * xOffset),
                        _grid[x, y].transform.position.y,
                        middle.transform.position.z + (tileResolution * yOffset)
                    );
                }
            }
        }
		
        northDetected = false;
        southDetected = false;
        eastDetected = false;
        westDetected = false;

        TerraLandRuntimeOffline.stitchingTerrainsList.Clear();

        for (int y = 0; y < runTime.activeTilesGrid; y++)
            for (int x = 0; x < runTime.activeTilesGrid; x++)
                TerraLandRuntimeOffline.stitchingTerrainsList.Add(_grid[x, y]);
    }

    private void DebugBounds ()
    {
        if (boundsDebug)
        {
            boundsRenderer.enabled = true;

            if (floatingOriginAdvanced != null && floatingOriginAdvanced.enabled && floatingOriginAdvanced.gameObject.activeSelf && floatingOriginAdvanced.originChanged)
                print("Origin is changed!");
        }
        else
            boundsRenderer.enabled = false;
    }
}

