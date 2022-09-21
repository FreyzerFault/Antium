using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class AntController : Ant
{
    public int foodPickup = 0;

    public GameObject viewLight; // Light circle for vision
    public Camera camera; // Camera following the ant
    
    
    // MINIONS
    public GameObject minionPrefab; // Prefab to Instance
    public GameObject minionStack; // Parent of all minions
    public List<AntMinion> minions; // Container for all minions


    // Events
    public event Action OnFoodPickup;

    private void Awake()
    {
        camera = Camera.main;
        
        
        // EVENTS
        OnFoodPickup += () => foodPickup++;
    }

    private new void Update()
    {
        base.Update();
        
        if (viewLight != null)
        {
            viewLight.transform.localScale = new Vector3(viewRadius, viewRadius, 1);
        }
        
        // Camera may follow Player
        camera.transform.position = transform.position + Vector3.back * 10;
    }



    private void AddMinion(Ant ant)
    {
        // Set a Random Position behind the main Ant to follow it
        Vector3 minionPos = GetRandomPositionBehind();
        int iterations = 0;
        while (!minions.TrueForAll((otherMinion) => Vector3.Distance(minionPos, otherMinion.positionAssigned) > distanceBetweenMinions)
               && iterations < 100)
        {
            minionPos = GetRandomPositionBehind();
            iterations++;
        }

        if (iterations >= 100)
        {
            Debug.Log("Max minions reached. Can't load more");
            return;
        }
        
        // Create the Minion Object
        GameObject minionObj = Instantiate(minionPrefab, ant.transform.position, ant.transform.rotation, minionStack.transform);
        minionObj.name = "Minion " + minions.Count + 1;
        
        // Add to the Minions list
        AntMinion minion = minionObj.GetComponent<AntMinion>();
        minions.Add(minion);

        
        minion.positionAssigned = minionPos;
    }

    private const float minionsBoundX = 4;
    private const float minionsBoundY = 4;
    private const float distanceBetweenMinions = 1.5f;
    private const float minMinionDistanceToPlayer = 3;

    private Vector3 GetRandomPositionBehind()
    {
        Vector3 randomPos = Vector3.down * (Random.value * minionsBoundY - minionsBoundY / 2) + Vector3.right * (Random.value * minionsBoundX - minionsBoundX / 2);
        float side = Random.value;
        switch (side)
        {
            // BACK
            case < .2f:
                Debug.Log("BACK");
                randomPos += Vector3.down * minionsBoundY / 2 + Vector3.down * minMinionDistanceToPlayer;
                break;
            // BACK-RIGHT
            case < .4f:
                Debug.Log("RIGHT");
                randomPos += Vector3.right * minionsBoundX / 2 + Vector3.right * minMinionDistanceToPlayer
                    + Vector3.down * minionsBoundY / 2 + Vector3.down * minMinionDistanceToPlayer;
                break;
            // BACK-LEFT
            case < .6f:
                Debug.Log("RIGHT");
                randomPos += Vector3.left * minionsBoundX / 2 + Vector3.left * minMinionDistanceToPlayer
                    + Vector3.down * minionsBoundY / 2 + Vector3.down * minMinionDistanceToPlayer;
                break;
            // RIGHT
            case < .8f:
                Debug.Log("RIGHT");
                randomPos += Vector3.right * minionsBoundX / 2 + Vector3.right * minMinionDistanceToPlayer;
                break;
            // LEFT
            default:
                Debug.Log("LEFT");
                randomPos += Vector3.left * minionsBoundY / 2 + Vector3.left * minMinionDistanceToPlayer;
                break;
        }

        return randomPos;
    }
        

    public float MaxMinions =>
        (minionsBoundX - distanceBetweenMinions) / distanceBetweenMinions * (minionsBoundY - distanceBetweenMinions) / distanceBetweenMinions * 5;
    public int LeaveAntsInNest()
    {
        int numMinions = minions.Count;
        foreach (AntMinion minion in minions)
        {
            Destroy(minion);
        }
        return numMinions;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ant"))
        {
            AddMinion(collision.collider.GetComponent<Ant>());
        }
    }


    #region INPUTS

    private Vector3 currentRotVel = Vector3.zero;

    private void OnMove(InputValue value)
    {
        targetVelocity = value.Get<Vector2>() * maxSpeed;
        
        // target ROTATION
        if (targetVelocity.magnitude != 0)
        {
            float zRot = Vector2.SignedAngle(Vector2.up, targetVelocity);

            targetRotation = Quaternion.Euler(0, 0, zRot);
        }
    }

    private void OnMainAction(InputValue value)
    {
        
    }

    private void OnPause(InputValue value)
    {
        
    }

    #endregion
    
}
