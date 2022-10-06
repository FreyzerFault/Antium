using System.Collections;
using UnityEngine;

public class AntMinion : Ant
{
    private AntController mainAnt;

    public Vector3 positionAssigned;

    public bool enterNest = false;

    private new void Awake()
    {
        base.Awake();
        mainAnt = GameObject.FindGameObjectWithTag("Player").GetComponent<AntController>();
    }

    private new void Update()
    {
        maxSpeed = mainAnt.maxSpeed;

        var dir = mainAnt.transform.position + mainAnt.body.transform.rotation * positionAssigned - transform.position;
        
        targetSpeed = dir.magnitude;
        targetRotation = Quaternion.LookRotation(Vector3.forward, dir.normalized);

        base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Nest") && !enterNest)
            return;
        
        base.OnTriggerEnter2D(col);
    }
}
