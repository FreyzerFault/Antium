using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class NestController : MonoBehaviour
{
    public int numAnts = 0;
    public float food = 0;

    public float baseRadius = 10;
    public float baseSize = 5;
    
    public float radiusPerAnt = 5;
    public float sizePerAnt = 1;
    public float birthRatePerFood = 10;
    public float birthFoodCost = 0.1f;
    
    private float Size => sizePerAnt * numAnts + baseSize;
    private float ViewRadius => radiusPerAnt * numAnts + baseRadius;
    private float BirthPeriod => birthRatePerFood / food;

    public CircleCollider2D circleCollider;

    public GameObject hole;
    public ViewSpotLight viewSpotLight;
    public AntController antController;
    public NumberCounter numberCounter;
    public UIController uiController;
    
    private ExplorersController explorersController;
    public int NumAntsExploring => explorersController.explorers.Count;

    private Vector3 targetScale = Vector3.one;

    public bool queenInside = true;

    public GameObject heartsPrefab;
    public GameObject cumbia;

    public event Action OnEnterNest;
    public event Action OnQuitNest;
    public event Action OnAntEnter;

    
    private void Awake()
    {
        explorersController = GetComponent<ExplorersController>();
        
        if (uiController == null)
            uiController = GameObject.FindGameObjectWithTag("UI Controller").GetComponent<UIController>();

        viewSpotLight = transform.parent.GetComponentInChildren<ViewSpotLight>();
        circleCollider = GetComponent<CircleCollider2D>();

        // EVENTS
        OnAntEnter += AddAnt;
        OnAntEnter += UpdateUI;
        OnAntEnter += UpdateSize;

        // Change Game Mode
        OnEnterNest += () => GameManager.Instance.SwitchMode(GameMode.Nest);
        OnQuitNest += () => GameManager.Instance.SwitchMode(GameMode.Ant);

        // Visuals
        OnEnterNest += UpdateSize;
        OnEnterNest += UpdateUI;

        // UI
        OnEnterNest += UpdateUI;
        OnEnterNest += () => uiController.Show();

        OnQuitNest += () => antController.QuitNest(this);

        OnEnterNest += () => queenInside = true;
        OnQuitNest += () => queenInside = false;
    }

    private void Start()
    {
        targetScale = new Vector3(Size, Size, 1);
        StartCoroutine(Folleteo());
        UpdateUI();
    }

    private void Update()
    {
        if (targetScale.magnitude > transform.localScale.magnitude)
            hole.transform.localScale = Vector3.Lerp(hole.transform.localScale, targetScale, Time.deltaTime);
    }
    
    public void EnterNest() => OnEnterNest.Invoke();
    public void QuitNest() => OnQuitNest.Invoke();
    public void EnterAnt() => OnAntEnter.Invoke();

    #region WIN

    private bool WinCondition() => numAnts >= 100;
    private void CheckWinCondition()
    {
        if (WinCondition())
        {
            StartCoroutine(WinAnimation());
            AntController.win1time = true;
        }
        
    }

    private IEnumerator WinAnimation()
    {
        cumbia.SetActive(true);
        
        GameManager.Instance.SwitchMode(GameMode.Pause);
        
        AudioManager.Instance.StopMusic();
        yield return new WaitForSeconds((float) cumbia.GetComponent<VideoPlayer>().length);
        
        viewSpotLight.targetRadius = 0;

        yield return new WaitUntil(() => viewSpotLight.radius < 0.1f);
        
        SceneManager.LoadScene(2);
    }

    #endregion
    
    #region ANTS & FOOD

    public void AddAnt()
    {
        numAnts++;
        numAnts = Mathf.Clamp(numAnts, 0, 999);
        UpdateSize();
        UpdateUI();
        CheckWinCondition();
    }

    public void AddFood()
    {
        food++;
        uiController.UpdateFood(Mathf.FloorToInt(food));
    }
    public void AddFood(int quantity)
    {
        for (int i = 0; i < quantity; i++) AddFood();
    }

    private IEnumerator Folleteo()
    {
        while (!WinCondition())
        {
            yield return new WaitUntil(() => queenInside && food > 0 && numAnts > 0);
            
            food -= birthFoodCost;
            AddAnt();
            Instantiate(heartsPrefab, transform);
            
            yield return new WaitForSeconds(BirthPeriod);
        }
    }

    #endregion

    #region ANIMATIONS

    public void UpdateSize()
    {
        viewSpotLight.targetRadius = ViewRadius;
        targetScale.x = Size;
        targetScale.y = Size;
    }

    #endregion

    #region UI

    public void UpdateUI()
    {
        uiController.UpdateAnts(numAnts);
        uiController.UpdateFood(Mathf.FloorToInt(food));
        uiController.UpdateExplorers(NumAntsExploring);
        numberCounter.UpdateNums(numAnts);
    }

    #endregion
    
    #region INPUT

    // Secondary Action => Quit Nest [Q / B]
    private void OnSecondaryAction(InputValue value)
    {
        if (GameManager.Instance.Mode == GameMode.Nest) 
            QuitNest();
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Food"))
        {
            Destroy(col.gameObject);
            AddFood();
        }
    }
}
