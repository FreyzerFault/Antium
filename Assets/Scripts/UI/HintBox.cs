using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintBox : MonoBehaviour
{
    public Image imageKeyboard;
    public Image imageGamepad;
    public TMP_Text text;
    
    public Animator animator;
    private static readonly int ShowID = Animator.StringToHash("Show");

    public Hint[] hints;
    
    public bool IsShown => animator.GetBool(ShowID);

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowHint(int hintIndex, int seconds = 5)
    {
        if (hintIndex < hints.Length)
            StartCoroutine(ShowHintRoutine(hints[hintIndex].spritePC, hints[hintIndex].spriteGampepad, hints[hintIndex].hintText, seconds));
    }
    
    public void ShowHint(Sprite spriteKeyboard, Sprite spriteGamepad, string hint, float seconds) => StartCoroutine(ShowHintRoutine(spriteKeyboard, spriteGamepad, hint, seconds));

    private IEnumerator ShowHintRoutine(Sprite spriteKeyboard, Sprite spriteGamepad, string hint, float seconds)
    {
        if (IsShown)
        {
            yield return new WaitUntil(() => !IsShown);
            yield return new WaitForSeconds(1);
        }
        
        animator.SetBool(ShowID, true);
        
        text.text = hint;
        imageKeyboard.enabled = spriteKeyboard != null;
        imageGamepad.enabled = spriteGamepad != null;
        imageKeyboard.sprite = spriteKeyboard;
        imageGamepad.sprite = spriteGamepad;

        yield return new WaitForSeconds(seconds);
        
        animator.SetBool(ShowID, false);
    }
}
