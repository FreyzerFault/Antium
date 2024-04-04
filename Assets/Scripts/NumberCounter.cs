using System.Linq;
using UnityEngine;

public class NumberCounter : MonoBehaviour
{
    [SerializeField] private int number;
    private SpriteRenderer[] numberRenderers;

    private const float NumberSpacing = .08f;
    
    private void Awake()
    {
        numberRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // Update the Number Sprites to show the num digits
    public void UpdateNums(int newNum)
    {
        number = newNum;
        var numS = number.ToString();
        var numWithPadding = number.ToString("000");
        
        // Hide ALL
        foreach (SpriteRenderer numberRenderer in numberRenderers) 
            numberRenderer.enabled = false;
        
        // Show only the necessary digits. Starting at the LAST digit [2] -> [1] -> [0]
        for (var i = numberRenderers.Length - 1; i >= numberRenderers.Length - numS.Length; i--) 
            numberRenderers[i].enabled = true;

        var sprites = numWithPadding.Select(digit => Resources.Load<Sprite>($"Sprites/Symbols/Numbers/{digit}")).ToArray();
        
        switch (numS.Length)
        {
            case 1:
                numberRenderers[2].sprite = sprites[2];
                numberRenderers[2].transform.localPosition = Vector3.zero;
                break;
            case 2:
                numberRenderers[1].sprite = sprites[1];
                numberRenderers[2].sprite = sprites[2];
                numberRenderers[1].transform.localPosition = Vector3.left * NumberSpacing;
                numberRenderers[2].transform.localPosition = Vector3.right * NumberSpacing;
                break;
            case 3:
                numberRenderers[0].sprite = sprites[0];
                numberRenderers[1].sprite = sprites[1];
                numberRenderers[2].sprite = sprites[2];
                numberRenderers[0].transform.localPosition = Vector3.left * (NumberSpacing * 2);
                numberRenderers[1].transform.localPosition = Vector3.zero;
                numberRenderers[2].transform.localPosition = Vector3.right * (NumberSpacing * 2);
                break;
        }
    }
}
