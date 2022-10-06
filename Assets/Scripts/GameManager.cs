using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum GameMode {Ant, Nest, Pause}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GameMode mode;
    [SerializeField]
    private GameMode prevMode;

    public GameMode Mode { get => mode; private set => mode = value; }
    public GameMode PrevMode { get => prevMode; private set => prevMode = value; }

    public void SwitchMode(GameMode newMode)
    {
        // Al salir de un Modo
        switch (mode)
        {
            case GameMode.Ant:
                // TODO
                break;
            case GameMode.Nest:
                // TODO
                break;
            case GameMode.Pause:
                // TODO
                break;
        }
        
        // Al entrar en ese modo
        switch (newMode)
        {
            case GameMode.Ant:
                // TODO
                break;
            case GameMode.Nest:
                // TODO
                break;
            case GameMode.Pause:
                // TODO
                break;
        }

        prevMode = mode;
        mode = newMode;
    }

    public void SwitchToPreviousMode() => SwitchMode(prevMode);

    public void SwitchCamera(Camera camera)
    {
        Camera.main.enabled = false;
        camera.enabled = true;
    }

    public void ResetGame() => SceneManager.LoadScene(1);

    public void Quit () 
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
		        Application.Quit();
        #endif
    }
}
