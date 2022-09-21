using UnityEngine;

public class Ant : MonoBehaviour
{
    public float viewRadius = 10;
    public float maxSpeed = 10;
    public float rotationSpeed = 10;
    public float maxHeadAngle = 20;

    protected float speed = 0;
    
    protected bool haveFood = false;
    
    
    public Vector2 targetVelocity = Vector2.zero;
    public Quaternion targetRotation = Quaternion.identity;
    
    
    public GameObject head;
    public Animator animator;
    private static readonly int SpeedID = Animator.StringToHash("speed");


    protected void Update()
    {
        // Smooth SPEED
        speed = Mathf.Lerp(speed, targetVelocity.magnitude, Time.deltaTime * 10);
        speed = Mathf.Max(speed, 0.001f);
        
        HandleMovement();
        HandleAnimations();
    }

    protected void HandleMovement()
    {
        // POSITION
        transform.position += transform.up * (Time.deltaTime * speed);

        // Smoth ROTATION
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed *
            Mathf.Clamp01(speed));
    }

    private void HandleAnimations()
    {
        // Animation change speed
        animator.SetFloat(SpeedID, speed);

        // HEAD ROTATION
        // Rota la cabeza hacia donde quiere girar pero limitado por el angulo maximo
        float angle = Vector3.SignedAngle(transform.up, targetRotation * Vector3.up, Vector3.back);
        angle = Mathf.Clamp(angle, -maxHeadAngle, maxHeadAngle);
        head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, Quaternion.Euler(0,0, -angle), Time.deltaTime * rotationSpeed);
    }
}
