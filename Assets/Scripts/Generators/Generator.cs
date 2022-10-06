using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform stackParent;

    public List<GameObject> items = new List<GameObject>();

    // Numero de items generados por Cluster
    public int minCluster = 3;
    public int maxCluster = 8;
    
    // Espacio de margen donde no se generan items
    public float spawnMargin = 5;
    protected Vector3 SpawnMargin => new Vector3(spawnMargin, spawnMargin, 0);
    
    // True => CIRCLE Dist ; False => SQUARE Dist
    public bool circleDistribution = true;
    
    // Mapa de Clusters por Region
    protected Dictionary<Vector2Int, Transform> clusterParents = new Dictionary<Vector2Int, Transform>();

    public Transform GetCluster(Vector2Int region) => clusterParents[region];

    public WorldGenerator worldGenerator;

    protected event Action<GameObject> OnSpawn = delegate(GameObject o) {  };

    protected void Awake()
    {
        worldGenerator = GameObject.FindGameObjectWithTag("WorldGenerator").GetComponent<WorldGenerator>();
        
        OnSpawn += item => items.Add(item);
        
        //ClearAllItems();
    }

    // Spawn in a position
    public GameObject SpawnItem(Vector3 position)
    {
        GameObject item = Instantiate(itemPrefab, position, Quaternion.identity, stackParent);
        OnSpawn.Invoke(item);
        return item;
    }

    public GameObject[] SpawnCluster(Vector2Int region, int numItems = -1)
    {
        if (circleDistribution)
            return SpawnClusterInRadius(region, numItems);
        
        return SpawnClusterInSquare(region, numItems);
    }
    
    // in SQUARE
    public GameObject[] SpawnClusterInSquare(Vector3 minPos, Vector3 maxPos, int numItems = -1)
    {
        if (numItems == -1) numItems = Mathf.RoundToInt(Random.value * (maxCluster - minCluster) + minCluster);
        
        GameObject[] spawned = new GameObject[numItems];
        for (int i = 0; i < numItems; i++)
        {
            Vector3 pos = new Vector3(
                Random.value * (maxPos.x - minPos.x) + minPos.x,
                Random.value * (maxPos.y - minPos.y) + minPos.y, 
                0
                );
            
            spawned[i] = SpawnItem(pos);
        }

        return spawned;
    }
    
    // in RADIUS
    public GameObject[] SpawnClusterInRadius(Vector3 center, float radius, int numItems = -1)
    {
        if (numItems == -1) numItems = Mathf.RoundToInt(Random.value * (maxCluster - minCluster) + minCluster);
        
        GameObject[] spawned = new GameObject[numItems];
        for (int i = 0; i < numItems; i++)
        {
            Vector3 pos = center + (Vector3) Random.insideUnitCircle * radius;
            spawned[i] = SpawnItem(pos);
        }
        return spawned;
    }
    
    
    // in SQUARE in REGION
    private GameObject[] SpawnClusterInSquare(Vector2Int region, int numItems = -1)
    {
        if (numItems == -1) numItems = Mathf.RoundToInt(Random.value * (maxCluster - minCluster) + minCluster);
        
        Vector3 minPos = worldGenerator.GetRegionPos(region) + SpawnMargin;
        Vector3 maxPos = worldGenerator.GetRegionEnd(region) - SpawnMargin;
        
        GameObject[] spawned = SpawnClusterInSquare(minPos, maxPos, numItems);

        // Assign a Cluster Parent to each item
        Transform parent = GetClusterParent(region);
        
        foreach (GameObject obj in spawned) obj.transform.parent = parent;

        return spawned;
    }

    // in RADIUS in REGION
    private GameObject[] SpawnClusterInRadius(Vector2Int region, int numItems = -1)
    {
        if (numItems == -1) numItems = Mathf.RoundToInt(Random.value * (maxCluster - minCluster) + minCluster);
        
        Vector3 center = worldGenerator.GetRegionCenter(region);

        float maxRadius = Mathf.Max(worldGenerator.regionSize - spawnMargin, worldGenerator.regionSize / 4f) / 2;
        float minRadius = Mathf.Max(maxRadius - worldGenerator.regionSize / 8f, 0.1f) / 2;
        float radius = Random.value * (maxRadius - minRadius) + minRadius;
        
        GameObject[] spawned = SpawnClusterInRadius(center, radius, numItems);

        // Assign a Cluster Parent to each item
        Transform parent = GetClusterParent(region);
        
        foreach (GameObject obj in spawned) obj.transform.parent = parent;

        return spawned;
    }
    
    protected Transform GetClusterParent(Vector2Int region)
    {
        if (clusterParents.ContainsKey(region)) return clusterParents[region];
        
        Transform parent = new GameObject("Cluster " + region).transform;
        parent.position = worldGenerator.GetRegionCenter(region);
        parent.parent = stackParent;
        
        clusterParents.Add(region, parent);

        return parent;
    }

    public Transform[] GetClusters() => clusterParents.Values.ToArray();
    public Vector2Int[] GetRegions() => clusterParents.Keys.ToArray();

    public void RespawnCluster(Vector2Int region)
    {
        int numItemsToSpawn = Mathf.RoundToInt(Random.value * 2);
        int itemsInCluster = clusterParents[region].childCount;
        
        // Si ya hay el numero maximo no respawnea nada
        if (numItemsToSpawn + itemsInCluster > maxCluster) return;
        
        if (circleDistribution) SpawnClusterInRadius(region, numItemsToSpawn);
        else SpawnClusterInSquare(region, numItemsToSpawn);
    }

    public void ClearAllItems()
    {
        foreach (Transform cluster in clusterParents.Values)
        {
            while (cluster.childCount > 0)
            {
                DestroyImmediate(cluster.GetChild(0).gameObject);
            }
        }
        clusterParents.Clear();
        
        while (stackParent.childCount > 0)
        {
            DestroyImmediate(stackParent.GetChild(0).gameObject);
        }
        items.Clear();
    }
}
