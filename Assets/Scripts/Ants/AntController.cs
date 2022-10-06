using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class AntController : Ant
{
    public int foodPickup = 0;
    public float baseSpeed = 10;
    private float ViewRadius => radiusPerMinion * NumMinions + viewRadius;
    private float MaxSpeed => speedPerMinion * NumMinions + maxSpeed;

    // Light circle for vision
    public ViewSpotLight viewSpotLight;
    
    // UI Arrows
    public ArrowPointer arrowPointerNest;
    public ArrowPointer arrowPointerAnt;
    
    
    // MINIONS
    public GameObject minionPrefab; // Prefab to Instance
    public GameObject minionStack; // Parent of all minions
    public List<AntMinion> minions; // Container for all minions

    private int NumMinions => minions.Count;
    public float radiusPerMinion = 1;
    public float speedPerMinion = 1;
    public int minMinionsToBuildNest = 4;

    
    // NEST
    public GameObject nestPrefab;
    public NestController nestController;
    
    // Lost Ants
    private WorldGenerator worldGenerator;
    private AntWanderer[] LostAnts => worldGenerator.antGenerator.stackParent.GetComponentsInChildren<AntWanderer>();
    
    
    // Hint Box
    public HintBox hintBox;

    public static bool win1time = false;
    public GameObject crown;

    public event Action OnMinionPickup;

    private new void Awake()
    {
        base.Awake();

        viewSpotLight = GetComponentInChildren<ViewSpotLight>();

        worldGenerator = GameObject.FindGameObjectWithTag("WorldGenerator").GetComponent<WorldGenerator>();
        
        OnMinionPickup += () =>
        {
            if (nestController == null && NumMinions == minMinionsToBuildNest) hintBox.ShowHint(0);
        };

        crown.SetActive(win1time);
    }

    private void Start()
    {
        viewSpotLight.radius = 0;
    }

    private new void Update()
    {
        base.Update();
        
        viewSpotLight.cam.transform.up = Vector3.up;
        UpdateViewSpotLight();
        UpdateArrows();
    }
    
    private void UpdateViewSpotLight()
    {
        viewSpotLight.targetRadius = ViewRadius;
        viewCollider.radius = ViewRadius;
    }

    #region ARROWS

    private void UpdateArrows()
    {
        bool blink = Mathf.FloorToInt(Time.time) % 2 == 0;
        
        if (blink)
        {
            if (nestController != null)
            {
                // Aparece solo a cierta DISTANCIA
                arrowPointerNest.gameObject.SetActive(
                    Vector3.Distance(transform.position, nestController.transform.position) > ViewRadius + 3
                    );
                arrowPointerNest.target = nestController.transform;
                arrowPointerNest.radius = ViewRadius + 1;
            }


            float minDistance = float.MaxValue;
            Transform nearestAnt = null;
            foreach (AntWanderer ant in LostAnts)
            {
                float distance = Vector3.Distance(ant.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestAnt = ant.transform;
                }
            }

            if (nearestAnt != null)
            {
                arrowPointerAnt.gameObject.SetActive(minDistance > ViewRadius + 3);
                arrowPointerAnt.target = nearestAnt;
                arrowPointerAnt.radius = ViewRadius + 1;
            }
        }
        else
        {
            arrowPointerNest.gameObject.SetActive(false);
            arrowPointerAnt.gameObject.SetActive(false);
        }
    }

    #endregion
    

    #region MINIONS

    private void AddMinion(Ant ant)
    {
        // Set a Random Position behind the main Ant to follow it
        Vector3 minionPos = GetRandomPositionBehind();
        
        // Avoid positions near another ant or the main ant
        int iterations = 0;
        while (!minions.TrueForAll((otherMinion) => Vector3.Distance(minionPos, otherMinion.positionAssigned) > DistanceBetweenMinions)
               && iterations < 100)
        {
            minionPos = GetRandomPositionBehind();
            iterations++;
        }

        // If can't find a position, don't spawn minion, set a counter
        if (iterations >= 100)
        {
            Debug.Log("Max minions reached. Can't load more");
            
            // TODO set counter for excess minions
            
            return;
        }
        
        // Create the Minion Object
        GameObject minionObj = Instantiate(minionPrefab, ant.transform.position, ant.body.transform.rotation, minionStack.transform);
        minionObj.name = "Minion " + minions.Count + 1;
        
        // Add to the Minions list
        AntMinion minion = minionObj.GetComponent<AntMinion>();
        minions.Add(minion);
        
        // Food
        if (ant.CarryFood)
        {
            ant.foodGrabPos.GetChild(0).parent = minion.foodGrabPos;
            minion.CarryFood = ant.CarryFood;
        }

        minion.positionAssigned = minionPos;
        
        OnMinionPickup.Invoke();
    }

    private const float MinionsBoundX = 4;
    private const float MinionsBoundY = 4;
    private const float DistanceBetweenMinions = 1.5f;
    private const float MinMinionDistanceToPlayer = 3;

    private Vector3 GetRandomPositionBehind()
    {
        Vector3 randomPos = Vector3.down * (Random.value * MinionsBoundY - MinionsBoundY / 2) + Vector3.right * (Random.value * MinionsBoundX - MinionsBoundX / 2);
        float side = Random.value;
        switch (side)
        {
            // BACK
            case < .2f:
                randomPos += Vector3.down * MinionsBoundY / 2 + Vector3.down * MinMinionDistanceToPlayer;
                break;
            // BACK-RIGHT
            case < .4f:
                randomPos += Vector3.right * MinionsBoundX / 2 + Vector3.right * MinMinionDistanceToPlayer
                                                               + Vector3.down * MinionsBoundY / 2 + Vector3.down * MinMinionDistanceToPlayer;
                break;
            // BACK-LEFT
            case < .6f:
                randomPos += Vector3.left * MinionsBoundX / 2 + Vector3.left * MinMinionDistanceToPlayer
                                                              + Vector3.down * MinionsBoundY / 2 + Vector3.down * MinMinionDistanceToPlayer;
                break;
            // RIGHT
            case < .8f:
                randomPos += Vector3.right * MinionsBoundX / 2 + Vector3.right * MinMinionDistanceToPlayer;
                break;
            // LEFT
            default:
                randomPos += Vector3.left * MinionsBoundY / 2 + Vector3.left * MinMinionDistanceToPlayer;
                break;
        }

        return randomPos;
    }
    
    
    public int LeaveMinionsInNest()
    {
        int numMinions = minions.Count;
        
        foreach (AntMinion minion in minions)
        {
            minion.positionAssigned = Vector3.zero;
            minion.enterNest = true;
        }
        
        minions.Clear();
        
        return numMinions;
    }

    #endregion

    #region FOOD


    protected override void SetFoodTarget(Transform food)
    {
        StartCoroutine(SendMinionToFood(food));
    }

    protected override void PickupFood(Transform food)
    {
        if (!CarryFood)
        {
            foodPickup++;
            base.PickupFood(food);
            return;
        }
        // Si ya lleva comida, lo coge uno de los minions    
        StartCoroutine(SendMinionToFood(food));
    }

    private Dictionary<Transform, AntMinion> minionsSendedToFood = new Dictionary<Transform, AntMinion>();
    private IEnumerator SendMinionToFood(Transform food)
    {
        if (minionsSendedToFood.Count == NumMinions ||  minionsSendedToFood.ContainsKey(food))
            yield break;
        
        AntMinion minionSended = null;
        foreach (AntMinion minion in minions)
        {
            if (!minion.CarryFood && !minionsSendedToFood.ContainsValue(minion))
            {
                minionsSendedToFood.Add(food, minion);
                minionSended = minion;
                break;
            }
        }
        if (minionSended == null)
            yield break;

        minionSended.foodTarget = food;

        // Espera hasta que el minion coge la comida
        bool haveYourFood = foodGrabPos.transform.childCount > 0 && foodGrabPos.transform.GetChild(0) == food;
        while (!minionSended.CarryFood && !haveYourFood)
            yield return new WaitForEndOfFrame();

        minionSended.foodTarget = null;
        minionsSendedToFood.Remove(food);
        foodPickup++;
        yield return null;
    }

    #endregion

    #region NEST
    
    private void CreateNest()
    {
        if (minions.Count >= 4)
        {
            GameObject nest = Instantiate(nestPrefab, transform.position, Quaternion.identity);
            nest.name = "Nest";

            nestController = nest.GetComponentInChildren<NestController>();
            nestController.antController = this;
            EnterNest(nestController);
        }
        else
            Debug.Log("Not enough ants to create a nest: " + minions.Count + " ants");
    }
    
    #endregion


    #region ANIMATIONS

    // Before Entering Nest Animation
    public override void EnterNest(NestController nest)
    {
        // Add Ants & Food to Nest
        LeaveMinionsInNest();
        
        // Change View Light
        viewSpotLight.gameObject.SetActive(false);
        nestController.viewSpotLight.gameObject.SetActive(true);
        nestController.viewSpotLight.radius = viewSpotLight.radius;
        
        // Change Cam
        GameManager.Instance.SwitchCamera(nestController.viewSpotLight.cam);
        
        // Animation
        StartCoroutine(base.EnterNestAnimation(nest, false, OnEnterNest));
    }

    // After Entering Nest Animation
    private int timesEntered = 0;
    private void OnEnterNest()
    {
        // Cambia de Modo
        GameManager.Instance.SwitchMode(GameMode.Nest);
        
        // Quit Nest Hint
        if (timesEntered == 0) 
            hintBox.ShowHint(1);
        // Seach Food Hint
        else if (timesEntered == 1)
            hintBox.ShowHint(2);
        
        // Send Explorers Hint
        if (nestController.numAnts > 6)
            hintBox.ShowHint(3);
        
        timesEntered++;
        
        nestController.EnterNest();
    }
    
    
    // Before Quitting Nest Animation
    public override void QuitNest(NestController nest)
    {
        // Cambia de Modo
        GameManager.Instance.SwitchMode(GameMode.Ant);

        // Set View Light to Player
        viewSpotLight.targetRadius = viewRadius;

        StartCoroutine(base.QuitNestAnimation(nest, OnQuitNest));
    }

    // After Quitting Nest Animation
    private void OnQuitNest()
    {
        // Change View Light
        viewSpotLight.gameObject.SetActive(true);
        nestController.viewSpotLight.gameObject.SetActive(false);
        viewSpotLight.radius = nestController.viewSpotLight.radius;
        
        // Change Cam
        GameManager.Instance.SwitchCamera(viewSpotLight.cam);
    }

    #endregion
    

    #region COLLISIONS

    private new void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        
        // ADD Ant to MINIONS
        if (collision.collider.CompareTag("Wanderer"))
        {
            AddMinion(collision.collider.GetComponent<Ant>());
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        // Enter NEST
        if (other.CompareTag("Nest") && other.IsTouching(bodyCollider))
        {
            NestController nest = other.GetComponent<NestController>();
            EnterNest(nest);
        }
        
        // Pick Food
        if (other.CompareTag("Food") && other.IsTouching(bodyCollider) && !minionsSendedToFood.ContainsKey(other.transform))
        {
            PickupFood(other.transform);
        }
    }

    #endregion
    

    #region INPUTS

    // MOVE [Vector2]
    private void OnMove(InputValue value)
    {
        if (GameManager.Instance.Mode != GameMode.Ant)
        {
            targetSpeed = 0;
            return;
        }
        
        Vector2 inputDir = value.Get<Vector2>();
        targetSpeed = inputDir.magnitude * MaxSpeed;

        // target ROTATION
        if (targetSpeed != 0)
        {
            targetRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(inputDir.x, inputDir.y, 0));
        }
    }

    // Main Action => Accept Dialogue [Space / A]
    // private void OnMainAction(InputValue value)
    // {
    //     if (dialogueBox.IsShown)
    //         dialogueBox.Next();
    // }
    
    // Ternary => Create Nest [E / X]
    private void OnTernaryAction(InputValue value)
    {
        if (GameManager.Instance.Mode == GameMode.Ant && nestController == null)
            CreateNest();
    }

    // PAUSE
    private MenuToggle menuToggle;
    public void OnPause(InputValue value)
    {
        if (menuToggle == null) 
            menuToggle = GameObject.FindGameObjectWithTag("MenuToggle").GetComponent<MenuToggle>();

        menuToggle.Toggle();
    }

    #endregion
    
}
