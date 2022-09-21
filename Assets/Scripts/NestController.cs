using System;
using UnityEngine;

public class NestController : MonoBehaviour
{
    public int numAnts = 0;
    public int food = 0;
    public int numAntsExploring = 0;
    private NestNumbers nestNumbers;

    public AntController antController;
    public UIController uiController;

    public Camera camera;
    
    public event Action OnEnterNest;
    public event Action OnQuitNest;
    
    private void Awake()
    {
        nestNumbers = GetComponentInChildren<NestNumbers>();

        // Add Ants following you and Food picked up
        OnEnterNest += () => AddAnts(antController.LeaveAntsInNest());
        OnEnterNest += () => AddFood(antController.foodPickup);
        
        // Change Game Mode
        OnEnterNest += () => GameManager.Instance.SwitchMode(GameMode.Nest);
        OnQuitNest += () => GameManager.Instance.SwitchMode(GameMode.Ant);

        // Switch Cameras
        OnEnterNest += () => GameManager.Instance.SwitchCamera(camera);
        OnQuitNest += () => GameManager.Instance.SwitchCamera(antController.camera);
    }
    
    
    public void AddAnts(int n)
    {
        numAnts += n;
        numAnts = Mathf.Clamp(numAnts, 0, 999);
        nestNumbers.UpdateNums(numAnts);
        uiController.UpdateAnts(numAnts);
    }

    public void AddFood(int quantity)
    {
        food += quantity;
        uiController.UpdateFood(quantity);
    }

    public void sendExplorer(int numExplorers)
    {
        numAntsExploring += numExplorers;
        uiController.UpdateExplorers(numAntsExploring);
    }
}
