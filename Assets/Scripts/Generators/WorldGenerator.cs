using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    public enum RegionType { StartPoint, Food, AntSpawn, Wall, Empty }
    private const int NumTypes = 5;
    
    
    public WallGenerator wallGenerator;
    public FoodGenerator foodGenerator;
    public AntsGenerator antGenerator;

    public LineRenderer worldBorder;
    public SpriteRenderer ground;
    
    private GameObject player;
    
    public Transform emptyParent;
    public Transform startParent;
    
    public Bounds worldBounds = new Bounds(Vector3.zero, new Vector3(200, 200, 0));
    public Vector2 WorldSize => worldBounds.size;

    public Vector2Int NumRegions => 
        new Vector2Int(Mathf.FloorToInt(WorldSize.x / regionSize), Mathf.FloorToInt(WorldSize.y / regionSize));

    public Vector2Int CenterRegion => new Vector2Int(Mathf.FloorToInt(NumRegions.x / 2f), Mathf.FloorToInt(NumRegions.y / 2f));
    public Vector3 WorldCenter => GetRegionCenter(CenterRegion);
    

    public int regionSize = 10;
    
    public bool generateWorldOnStartup = true;
    public float respawnTime = 60;
    
    public float[] spawnRates = new float[NumTypes - 1];

    private RegionType[][] regionMap;
    
    private void Awake()
    {
        wallGenerator = GetComponent<WallGenerator>();
        foodGenerator = GetComponent<FoodGenerator>();
        antGenerator = GetComponent<AntsGenerator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }
    
    private void Start()
    {
        if (!generateWorldOnStartup) return;
        
        Clear();
        SetupBorder();
        SetupRegions();
        SpawnPlayer();
        SpawnAll();
        StartCoroutine(RespawnRoutine(respawnTime));
    }



    # region RegionOperations

    public RegionType GetRegionType(Vector2Int region) => regionMap[region.y][region.x];

    public Vector3 GetRegionPos(Vector2Int region) =>
        new Vector3(region.x * regionSize - WorldSize.x / 2, region.y * regionSize - WorldSize.y / 2, 0);

    public Vector3 GetRegionEnd(Vector2Int region) => GetRegionPos(region) + new Vector3(regionSize, regionSize, 0);

    public Vector3 GetRegionCenter(Vector2Int region) =>
        GetRegionPos(region) + new Vector3(regionSize / 2f, regionSize / 2f, 0);

    public Vector2Int GetRegion(Vector3 pos) => 
        new Vector2Int(Mathf.FloorToInt(pos.x / regionSize), Mathf.FloorToInt(pos.y / regionSize)) + NumRegions / 2;

    public GameObject[] GetRegionItems(Vector2Int region)
    {
        switch (GetRegionType(region))
        {
            case RegionType.Empty:
            case RegionType.StartPoint:
                return Array.Empty<GameObject>();
            case RegionType.Food:
                return foodGenerator.GetCluster(region).GetComponentsInChildren<GameObject>();
            case RegionType.AntSpawn:
                return antGenerator.GetCluster(region).GetComponentsInChildren<GameObject>();
            case RegionType.Wall:
                return wallGenerator.GetCluster(region).GetComponentsInChildren<GameObject>();
        }
        return Array.Empty<GameObject>();
    }
    
    // Action per each region
    private void ForEachRegion(Action<Vector2Int> action) 
    {
        for (int y = 0; y < NumRegions.y; y++)
        for (int x = 0; x < NumRegions.x; x++)
        {
            action(new Vector2Int(x,y));
        }
    }

    #endregion

    #region SETUP

    // WORLD BORDER
    public void SetupBorder()
    {
        float w = WorldSize.x / 2, h = WorldSize.y / 2;
        var borderPositions3D = new Vector3[] { new (w, h, 0), new (-w, h, 0), new (-w, -h, 0), new(w, -h, 0) };
        var borderPositions2D = new Vector2[] { new (w, h), new (-w, h), new (-w, -h), new(w, -h), new (w, h) };
        worldBorder.SetPositions(borderPositions3D);
        worldBorder.loop = true;

        var edgeC = worldBorder.GetComponent<EdgeCollider2D>();
        edgeC.edgeRadius = worldBorder.widthMultiplier / 2;
        edgeC.points = borderPositions2D;

        ground.size = WorldSize;
    }


    // Spawn Player in center of World
    public void SpawnPlayer()
    {
        player.transform.SetPositionAndRotation(WorldCenter, Quaternion.identity);
        player.gameObject.SetActive(true);
    }

    // Initialize Regions and assign RegionType
    public void SetupRegions()
    {
        regionMap = new RegionType[NumRegions.y][];
        for (int y = 0; y < NumRegions.y; y++) regionMap[y] = new RegionType[NumRegions.x];
        
        ForEachRegion(region =>
            regionMap[region.y][region.x] =
                Vector2Int.Distance(region, CenterRegion) <= 1
                    ? RegionType.StartPoint
                    : GetRandomRegionType(spawnRates));
    }

    // Spawn All Items
    public void SpawnAll()
    {
        ForEachRegion(SpawnItems);
    }

    #endregion

    #region Spawn

    // Spawn Items in the Region given
    public void SpawnItems(Vector2Int region)
    {
        Vector3 regionPos = GetRegionPos(region);
        Vector3 regionEndPos = regionPos + new Vector3(regionSize, regionSize, 0);
        Vector3 regionCenter = regionPos + new Vector3(regionSize / 2f, regionSize / 2f, 0);
        
        if (!worldBounds.Contains(regionPos))
            Debug.Log("Spawning Items out of World Bounds. Region: " + region + " | Position: " + regionPos);

        
        switch (regionMap[region.y][region.x])
        {
            case RegionType.Empty:
                GameObject emptyRegion = new GameObject("Empty " + region);
                emptyRegion.transform.parent = emptyParent;
                break;
            case RegionType.StartPoint:
                GameObject startRegion = new GameObject("Start " + region);
                startRegion.transform.parent = startParent;
                break;
            case RegionType.Food:
                foodGenerator.SpawnCluster(region);
                break;
            case RegionType.AntSpawn:
                antGenerator.SpawnCluster(region);
                break;
            case RegionType.Wall:
                wallGenerator.SpawnCluster(region);
                break;
        }
    }

    public void RespawnItems(Vector2Int region)
    {
        RegionType type = GetRegionType(region);
        if (type == RegionType.Wall || type == RegionType.StartPoint || type == RegionType.Empty)
            return;
        
        // Solo respawnean la Comida y las Hormigas perdidas
        if (Vector3.Distance(GetRegionCenter(region), player.transform.position) < regionSize * 3)
            return;
        if (type == RegionType.Food)
            foodGenerator.RespawnCluster(region);
        else if (type == RegionType.AntSpawn)
            antGenerator.RespawnCluster(region);
    }
    
    

    // Por cada tick, respawneamos los items de todas las regiones
    private IEnumerator RespawnRoutine(float respawnTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            ForEachRegion(RespawnItems);
        }
    }

    #endregion
    
    
    

    

    public void Clear()
    {
        wallGenerator.ClearAllItems();
        foodGenerator.ClearAllItems();
        antGenerator.ClearAllItems();
        while (emptyParent.childCount > 0) DestroyImmediate(emptyParent.GetChild(0).gameObject);
        while (startParent.childCount > 0) DestroyImmediate(startParent.GetChild(0).gameObject);
    }
    
    private static RegionType GetRandomRegionType(float[] spawnRates)
    {
        float ratesSum = 0;
        foreach (float rate in spawnRates) ratesSum += rate;
        if (ratesSum != 1) Debug.Log("Spawn rates do not add up to 1");

        float rand = Random.value;
                
        if (rand < spawnRates[0]) return RegionType.Food;
        
        rand -= spawnRates[0];
        
        if (rand < spawnRates[1]) return RegionType.AntSpawn;
        
        rand -= spawnRates[1];
        
        if (rand < spawnRates[2]) return RegionType.Wall;
        
        return RegionType.Empty;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.magenta;
        // ForEachRegion(region => Gizmos.DrawWireCube(GetRegionCenter(region), Vector3.one * regionSize));
    }
}
