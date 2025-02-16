#if UNITY_EDITOR
    using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// En editor sin ejecutar: % en pantalla < minScreenPercent -> lowLODobj
/// Ejecutándose: distancia a camara > maxDistanceToCamera -> lowLODobj
/// </summary>
[ExecuteAlways]
public class DynamicLODobj : MonoBehaviour
{
    public float minScreenPercent = 0.01f;
    public float cameraMargin = 10;

    private Camera _cam;

    [FormerlySerializedAs("high LOD Object"), SerializeField] private GameObject baseLODobj;
    [FormerlySerializedAs("low LOD Object"), SerializeField] private GameObject lowLODobj;
        
    [Space]
    
    public Bounds objBounds;
    
    void Awake()
    {
        UpdateBounds();
        UpdateCamera();
    }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        bool useLowLOD =
            Application.isPlaying && OutOfCamera()
            || !Application.isPlaying && ScreenPercent() < minScreenPercent;

        if (useLowLOD)
        {
            baseLODobj.SetActive(false);
            lowLODobj.SetActive(true);
        }
        else
        {
            baseLODobj.SetActive(true);
            lowLODobj.SetActive(false);
        }
    }

    private void UpdateBounds()
    {
        if (GetComponent<Collider2D>() != null)
            objBounds = GetComponent<Collider2D>().bounds;
        else if (GetComponent<Renderer>() != null)
            objBounds = GetComponent<Renderer>().bounds;
        // If can't use Collider2D or Renderer, implement another way
    }

    private void UpdateCamera() => _cam = Camera.main;

    private bool OutOfCamera()
    {
        Vector2 objPos = new Vector2(transform.position.x, transform.position.y);
        float height = 2f * _cam.orthographicSize;
        float width = height * _cam.aspect;
        Bounds camBounds = new Bounds(_cam.transform.position, new Vector3(width, height, 1000));
        camBounds.min -= new Vector3(cameraMargin, cameraMargin, 0);
        camBounds.max += new Vector3(cameraMargin, cameraMargin, 0);
        
        return !camBounds.Contains(objPos);
    }
    

    // % de la pantalla ocupado (relativo entre las diagonales del objeto y la pantalla)
    private float ScreenPercent()
    {
        Vector2 screenMin = _cam.WorldToScreenPoint(objBounds.min);
        Vector2 screenMax = _cam.WorldToScreenPoint(objBounds.max);
        float objDiagSize = Vector2.Distance(screenMin, screenMax);
        float sceneSize = Mathf.Sqrt(Screen.width^2 + Screen.height^2);
        
#if UNITY_EDITOR
        sceneSize = SceneView.lastActiveSceneView.size;
        objDiagSize = Vector3.Distance(objBounds.min, objBounds.max);
#endif
        
        float sizePercentOnScreen = objDiagSize / sceneSize;
        return sizePercentOnScreen;
    }

}
