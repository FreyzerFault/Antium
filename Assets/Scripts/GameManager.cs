using UnityEngine;

public enum GameMode {Ant, Nest, Pause}

public class GameManager : Singleton<GameManager>
{
    private GameMode mode;

    public GameMode Mode { get => mode; set => mode = value; }

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
    }

    public void SwitchCamera(Camera camera)
    {
        Camera.main.enabled = false;
        camera.enabled = true;
    }
}
