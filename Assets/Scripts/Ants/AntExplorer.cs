using System.Collections;
using System.Text;
using UnityEngine;

public class AntExplorer : Ant
{
    private float seed;
    public float wanderStrength = 1;

    public NestController nest;
    
    private static WorldGenerator world;
    private static Transform[] foodRegions;
    
    protected new void Awake()
    {
        base.Awake();

        StartCoroutine(Wander(wanderStrength));

        if (world == null)
            world = GameObject.FindGameObjectWithTag("WorldGenerator").GetComponent<WorldGenerator>();
        
        // Busca las regiones con comida
        if (foodRegions == null || foodRegions.Length == 0 || foodRegions[0] == null)
            foodRegions = world.foodGenerator.GetClusters();

        StartCoroutine(SearchFood(20));
    }

    protected override void Update()
    {
        // Si tiene comida va al nido, sino busca la mas cercana
        if (CarryFood) GoTo(nest.transform.position);
        else if (foodTarget == null) foodTarget = GetNearestFood();

        base.Update();
    }

    private Transform GetNearestFood()
    {
        // Search the Near region with food
        Transform nearestRegion = null;
        float minDist = float.MaxValue;
        foreach (Transform foodRegion in foodRegions)
        {
            // Siempre que haya comida
            if (foodRegion.childCount > 0)
            {
                float dist = Vector3.Distance(transform.position, foodRegion.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestRegion = foodRegion;
                }
            }
        }
            
        // Choose Random Food from Region
        Transform nearestFood = null; 
        if (nearestRegion != null)
        {
            CircleCollider2D[] food = nearestRegion.GetComponentsInChildren<CircleCollider2D>();
            nearestFood = food[Mathf.FloorToInt(Random.value * food.Length)].transform;
        }

        return nearestFood;
    }
    
    private IEnumerator SearchFood(float frecuency)
    {
        while (true)
        {
            yield return new WaitUntil(() => !CarryFood && foodTarget == null);
            
            
            yield return new WaitForSeconds(frecuency);
        }
    }

    public override void EnterNest(NestController nest)
    {
        // Entra pero al final sale del hormiguero cuando acaba la animacion
        StartCoroutine(EnterNestAnimation(nest, false, () => QuitNest(nest)));
    }
}
