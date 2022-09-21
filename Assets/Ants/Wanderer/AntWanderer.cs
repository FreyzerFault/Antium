using UnityEngine;

public class AntWanderer : Ant
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
