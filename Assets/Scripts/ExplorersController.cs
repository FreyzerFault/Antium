using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ExplorersController))]
public class ExplorersController : MonoBehaviour
{
    public AntExplorer explorerPrefab;
    public Transform explorersStack;
    public List<AntExplorer> explorers = new List<AntExplorer>();

    private NestController nest;

    private void Awake()
    {
        nest = GetComponent<NestController>();

        if (explorersStack == null)
        {
            GameObject obj = new GameObject("[Explorers]");
            explorersStack = obj.transform;
        }
    }

    public void SendExplorer()
    {
        if (nest.numAnts > 0)
        {
            nest.numAnts--;
            
            AntExplorer explorer = Instantiate(explorerPrefab, transform.position, Quaternion.identity, explorersStack);
            explorers.Add(explorer);
            
            explorer.QuitNest(nest);
            explorer.nest = nest;

            nest.UpdateSize();
            nest.UpdateUI();
        }
        else
            Debug.Log("No quedan Hormigas que mandar a Explorar");
    }

    #region INPUT
    
    // Ternary Action => Send Explorer [E / X]
    private void OnTernaryAction(InputValue value)
    {
        if (GameManager.Instance.Mode == GameMode.Nest && nest.queenInside)
        {
            SendExplorer();
            nest.UpdateUI();
        }
    }

    #endregion
}
