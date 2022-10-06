using System;
using UnityEngine;

public class NumberCounter : MonoBehaviour
{
    private int number;
    private SpriteRenderer[] numbers;

    private const float NumberSpacing = .08f;
    
    private void Awake()
    {
        numbers = GetComponentsInChildren<SpriteRenderer>();
    }


    // Update the Number Sprites to show the num over the nest
    public void UpdateNums(int newNum)
    {
        number = newNum;
        String numS = number.ToString();
        switch (numS.Length)
        {
            case 1:
                numbers[0].enabled = false;
                numbers[1].enabled = false;
                numbers[2].enabled = true;
                numbers[2].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[0]);
                numbers[2].transform.localPosition = Vector3.zero;
                break;
            case 2:
                numbers[0].enabled = false;
                numbers[1].enabled = true;
                numbers[2].enabled = true;
                numbers[1].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[0]);
                numbers[2].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[1]);
                numbers[1].transform.localPosition = Vector3.left * NumberSpacing;
                numbers[2].transform.localPosition = Vector3.right * NumberSpacing;
                break;
            case 3:
                numbers[0].enabled = true;
                numbers[1].enabled = true;
                numbers[2].enabled = true;
                numbers[0].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[0]);
                numbers[1].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[1]);
                numbers[2].sprite = Resources.Load<Sprite>("Sprites/Symbols/Numbers/" + numS[2]);
                numbers[0].transform.localPosition = Vector3.left * (NumberSpacing * 2);
                numbers[1].transform.localPosition = Vector3.zero;
                numbers[2].transform.localPosition = Vector3.right * (NumberSpacing * 2);
                break;
            default:
                break;
        }
    }
}
