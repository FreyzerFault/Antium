using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ant : MonoBehaviour
{
    public float viewRadius = 10;
    public float maxSpeed = 10;
    public float acceleration = 10;
    public float rotationSpeed = 10;
    public float maxHeadAngle = 20;
    public bool CarryFood = false;

    public float Speed = 0;

    protected Rigidbody2D rb;
    
    public float targetSpeed = 0;
    public Quaternion targetRotation = Quaternion.identity;
    
    public Vector3 targetPosition = Vector3.zero;

    public Transform foodGrabPos;
    public Transform foodTarget;

    public GameObject body;
    public GameObject head;
    public Animator animator;
    private static readonly int SpeedID = Animator.StringToHash("speed");

    protected BoxCollider2D bodyCollider;
    protected CircleCollider2D viewCollider;

    public GameObject foodPrefab;

    protected static AntController player = null;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (player == null && p != null)
            player = p.GetComponent<AntController>();

        viewCollider = GetComponentInChildren<CircleCollider2D>();
        if (viewCollider != null)
            viewCollider.radius = viewRadius;
        
        bodyCollider = GetComponent<BoxCollider2D>();

        targetPosition = transform.position;
    }

    protected virtual void Update()
    {
        // Si ha visto comida, va a por ella si no lleva ninguna
        if (!CarryFood && foodTarget != null) GoToFood(foodTarget);

        // Smooth SPEED depend on Distance to target
        Speed = Mathf.Lerp(Speed, targetSpeed, Time.deltaTime * acceleration);
        Speed = Speed < 0.1f ? 0 : Speed;

        HandleMovement();
        HandleAnimations();
    }


    #region MOVEMENT

    private void HandleMovement()
    {
        // Smoth ROTATION
        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed *
            Mathf.Clamp01(Speed));
        
        // POSITION update
        rb.velocity = body.transform.up * Speed;
        rb.angularVelocity = 0;
        //transform.position += body.transform.up * (Time.deltaTime * Speed);
    }

    protected void GoTo(Vector3 position)
    {
        Vector3 dir = position - transform.position;
        targetSpeed = Mathf.Clamp(dir.magnitude, 0, maxSpeed);
        targetRotation = Quaternion.LookRotation(Vector3.forward, dir);
    }

    protected void GoToFood(Transform food) => GoTo(food.position);

    protected IEnumerator Wander(float wanderStrength = 1)
    {
        float seed = Random.Range(-9999, 9999);
        
        while (true)
        {
            // Random Rotation in [-90,90]
            targetRotation = Quaternion.Euler(0,0,body.transform.rotation.eulerAngles.z + Random.value * 180 - 90);
            
            // Random Position at a distance [maxSpeed/2, maxSpeed + maxSpeed/2]
            float randSpeed = Mathf.PerlinNoise(seed + Time.time, 0);
            targetSpeed = randSpeed * maxSpeed / 2 + maxSpeed / 2;

            yield return new WaitForSeconds(0.5f);
        }
    }
    
    #endregion

    #region FOOD

    protected virtual void PickupFood(Transform food)
    {
        if (CarryFood) return;
        
        CarryFood = true;
        
        // Place the food in the mouth and destroy the original
        Destroy(food.gameObject);
        GameObject foodObj = Instantiate(foodPrefab, foodGrabPos.position, foodGrabPos.rotation);
        foodObj.transform.SetParent(foodGrabPos);

        // Disable the food's collider
        foodObj.GetComponent<Collider2D>().enabled = false;
        foodTarget = null;
    }
    
    protected virtual void SetFoodTarget(Transform food) => foodTarget = food;

    #endregion

    #region ANIMATIONS
    
    private void HandleAnimations()
    {
        // Animation change speed
        animator.SetFloat(SpeedID, Speed);

        // HEAD ROTATION
        // Rota la cabeza hacia donde quiere girar pero limitado por el angulo maximo
        float angle = Vector3.SignedAngle(body.transform.up, targetRotation * Vector3.up, Vector3.forward);
        angle = Mathf.Clamp(angle, -maxHeadAngle, maxHeadAngle);
        head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, Quaternion.Euler(0,0, angle), Time.deltaTime * rotationSpeed);
    }
    
    public virtual void EnterNest(NestController nest) => 
        StartCoroutine(EnterNestAnimation(nest, true, nest.EnterAnt));
    public virtual void QuitNest(NestController nest) => 
        StartCoroutine(QuitNestAnimation(nest));
    
    protected IEnumerator EnterNestAnimation(NestController nest, bool destroyOnEnter = true, Action onEnd = null)
    {
        // Collider DISABLE
        bodyCollider.enabled = false;

        float ts = 0.01f;
        while (transform.localScale.magnitude > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, nest.transform.position, ts);
            body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.Euler(90, 0, body.transform.rotation.eulerAngles.z), ts * 5);
            transform.localScale -= Vector3.one * ts;
            yield return new WaitForSeconds(ts);
        }
        
        
        if (CarryFood)
        {
            nest.AddFood();
            Destroy(foodGrabPos.GetChild(0).gameObject);
            CarryFood = false;
        }
        
        if (destroyOnEnter)
            Destroy(gameObject);
        
        onEnd?.Invoke();
    }
    
    protected IEnumerator QuitNestAnimation(NestController nest, Action onEnd = null)
    {
        // Collider DISABLE
        bodyCollider.enabled = false;

        transform.position = nest.transform.position;
        Speed = 0;
        targetPosition = transform.position + Vector3.up * maxSpeed;
        targetSpeed = maxSpeed;
        
        float randAngle = Random.value * 360 - 180;
        body.transform.rotation = Quaternion.Euler(90, 0, randAngle);
        targetRotation = Quaternion.Euler(0, 0, randAngle);
        
        float ts = 0.01f;
        while (transform.localScale.x <= 1f)
        {
            transform.localScale += Vector3.one * (ts * 10);
            yield return new WaitForSeconds(ts);
        }

        transform.localScale = Vector3.one;

        yield return new WaitUntil( () => !bodyCollider.IsTouching(nest.circleCollider));
        yield return new WaitForSeconds(1f);
        
        bodyCollider.enabled = true;
        
        onEnd?.Invoke();
    }

    #endregion

    #region COLLIDERS

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Food"))
        {
            // Coge comida al contacto
            if (col.IsTouching(bodyCollider) && col.transform == foodTarget){
                PickupFood(col.transform);
            }
            // Marca la comida como objetivo si esta a la vista
            else if (viewCollider != null && col.IsTouching(viewCollider) && foodTarget == null)
            {
                SetFoodTarget(col.transform);
            }
        }
        
        if (col.CompareTag("Nest") && col.IsTouching(bodyCollider))
        {
            Debug.Log("I can view: " + col.name);
            // Entra en el Hormiguero al contacto
            if (col.IsTouching(bodyCollider))
            {
                NestController nest = col.GetComponent<NestController>();
                EnterNest(nest);
            }
            // Marca el hormiguero como objetivo si esta a la vista
            else if (CarryFood && viewCollider != null && col.IsTouching(viewCollider))
            {
                Transform nest = col.transform;
                GoTo(nest.position);
            }
        }
    }

    protected void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Wall"))
        {
            Vector3 oppositeDir = col.contacts[0].normal;
            body.transform.rotation = Quaternion.LookRotation(Vector3.forward, oppositeDir);//Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + 180);
            targetRotation = body.transform.rotation;
        }
    }

    #endregion


    #region GIZMO
    
    protected void OnDrawGizmos()
    {
        Vector3 direction = targetRotation * Vector3.up;
        
        // Target:
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + direction * targetSpeed, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + direction * targetSpeed);
        
        // Speed
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + direction * Speed);
    }

    #endregion
}
