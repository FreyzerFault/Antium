using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public TMP_Text textUI;
    public Animator animator;
    
    public List<String> dialogue = new();
    public List<int> fontSizes = new();
    public int index;

    public bool finished = false;
    private static readonly int ShowID = Animator.StringToHash("Show");

    public event Action OnNext;
    public event Action OnEnd;
    
    public bool IsShown => animator.GetBool(ShowID);
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetBool(ShowID, false);
        
        textUI.text = dialogue[0];
        textUI.fontSize = fontSizes[0];
        
        if (dialogue.Count <= 1) finished = true;
    }

    public void ShowText(float seconds, int i = -1)
    {
        // Si es -1 no cambia el texto
        if (i > -1)
            SkipToText(i);
        
        StartCoroutine(ShowRoutine(seconds));
    }

    private IEnumerator ShowRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        Hide();
    }

    public void Next()
    {
        if (finished)
        {
            Hide();
            OnEnd?.Invoke();
            return;
        }
        
        OnNext?.Invoke();

        SetText(index + 1);
    }

    public void SkipToText(int i) => SetText(i);

    private void SetText(int i)
    {
        index = i;
        
        textUI.text = dialogue[i];
        textUI.fontSize = fontSizes[i];
        
        if (i >= dialogue.Count - 1) finished = true;
        
        Show();
    }
    
    public void Show()
    {
        if (animator != null)
            animator.SetBool(ShowID, true);
    }
    public void Hide()
    {
        if (animator != null)
            animator.SetBool(ShowID, false);
    }
}
