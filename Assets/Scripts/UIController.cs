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
    
    public void UpdateAnts(int value)
    {
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
}
