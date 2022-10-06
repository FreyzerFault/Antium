using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class WallGenerator : Generator
{
    public float wallSize = 5;

    protected new void Awake()
    {
        base.Awake();

        // Cada muro spawneado Randomiza su forma
        OnSpawn += wall => RandomizeWallShape(wall.GetComponent<SpriteShapeController>(), 10);
        
        // Y lo escala a la medida deseada
        OnSpawn += wall => wall.transform.localScale = new Vector3(wallSize, wallSize, 0);
    }
    
    
    private void RandomizeWallShape(SpriteShapeController shape, int complexity = 5)
    {
        Spline spline = shape.spline;

        spline.Clear();

        float randAngle = 0;
        float angleStepMin = 360f / complexity / 2;
        float angleStepMax = angleStepMin * 3;

        float centerDistMin = 1;
        float centerDistMax = 5;

        // POINTS
        for (int i = 0; i < complexity; i++)
        {
            float angleStep = Random.value * (angleStepMax - angleStepMin) + angleStepMin;
            randAngle += angleStep;
            float randDistToCenter = Random.value * (centerDistMax - centerDistMin) + centerDistMin;
            Vector3 pointPos = Quaternion.Euler(0, 0, randAngle) * Vector3.up * randDistToCenter;
            spline.InsertPointAt(i, pointPos);
        }

        // TANGENTS
        for (int i = 0; i < complexity; i++)
        {
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            float distToNext =
                Vector3.Distance(spline.GetPosition(i), spline.GetPosition((i + 1) % complexity));
            float distToPrev = Vector3.Distance(spline.GetPosition(i),
                spline.GetPosition((i == 0) ? complexity - 1 : i - 1));

            float lTangentLength = Random.value * distToPrev / 2 + 0.5f;
            float rTangentLength = Random.value * distToNext / 2 + 0.5f;

            Vector3 lTangent = Quaternion.Euler(0, 0, -90) * spline.GetPosition(i).normalized * lTangentLength;
            Vector3 rTangent = Quaternion.Euler(0, 0, 90) * spline.GetPosition(i).normalized * rTangentLength;

            spline.SetLeftTangent(i, lTangent);
            spline.SetRightTangent(i, rTangent);
        }
    }
}
