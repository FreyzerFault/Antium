using System;
using UnityEngine;

public class AntMinion : Ant
{
    private AntController mainAnt;

    public Vector3 positionAssigned;

    private void Awake()
    {
        mainAnt = GameObject.FindGameObjectWithTag("Player").GetComponent<AntController>();
        
        Debug.Log("Assigned: " + positionAssigned);
    }

    private new void Update()
    {
        Vector3 direction = mainAnt.transform.position + mainAnt.transform.rotation * positionAssigned - transform.position;
        if (direction.magnitude > 0.1f)
        {
            targetVelocity = direction.normalized * Mathf.Min(maxSpeed, direction.magnitude);
            targetRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(direction.normalized, Vector3.up, Vector3.back));   
        }
        else
        {
            targetVelocity = Vector3.zero;
            targetRotation = Quaternion.LookRotation(mainAnt.transform.position - transform.position);
        }
        
        base.Update();
    }

    private void OnDrawGizmos()
    {
        Vector3 direction = mainAnt.transform.position + mainAnt.transform.rotation * positionAssigned - transform.position;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + direction, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + direction);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (direction).normalized * Mathf.Min(maxSpeed, direction.magnitude));
    }
}
