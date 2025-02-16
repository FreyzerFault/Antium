using System.Collections;
using UnityEngine;

public class AntWanderer : Ant
{
    public float minDistUpdate = 10;
    
    private AntController playerAnt;
    private IEnumerator wanderCoroutine;
    
    [SerializeField]
    private bool farFromPlayer = true;

    protected override void Awake()
    {
        base.Awake();
        
        playerAnt = FindFirstObjectByType<AntController>();

        StartCoroutine(Wander());
    }
    
    protected override void Update()
    {
        farFromPlayer = Vector3.Distance(player.transform.position, transform.position) > minDistUpdate;

        if (farFromPlayer)
        {
            targetSpeed = 0;
            targetRotation = Quaternion.identity;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            Speed = maxSpeed / 2;
            return;
        }

        base.Update();
    }
    private new void OnCollisionEnter2D(Collision2D col)
    {
        base.OnCollisionEnter2D(col);
        
        if (col.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
