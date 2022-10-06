using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[CreateAssetMenu(fileName = "Hint ", menuName = "Hints/Create Hint")]
public class Hint : ScriptableObject
{
    public string hintText;
    public Sprite spritePC;
    public Sprite spriteGampepad;
}
