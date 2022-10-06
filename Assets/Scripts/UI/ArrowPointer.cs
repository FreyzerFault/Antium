using System;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    private Transform icon;

    public Transform target;
    public float radius = 10;

    private void Awake()
    {
        icon = transform.GetChild(0);
    }

    private void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, (target.position - transform.position).normalized);
            transform.position = transform.parent.position +
                                 (target.position - transform.parent.position).normalized * radius;
        }
        
        // ICON sin rotar
        icon.transform.rotation = Quaternion.identity;
    }

}
