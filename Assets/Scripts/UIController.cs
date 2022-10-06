using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text ants;
    public Text food;
    public Text explorers;

    private void Awake()
    {
        ants.text = "0";
        food.text = "0";
        explorers.text = "0";
    }

    private void Start()
    {
        Hide();
    }

    public void UpdateAnts(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        ants.text = value.ToString();
    }
    
    public void UpdateFood(int value)
    {
        food.text = value.ToString();
    }
    
    public void UpdateExplorers(int value)
    {
        explorers.text = value.ToString();
    }

    public void Hide() => transform.GetChild(0).gameObject.SetActive(false);
    public void Show() => transform.GetChild(0).gameObject.SetActive(true);
}
