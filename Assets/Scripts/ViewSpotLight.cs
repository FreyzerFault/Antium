using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ViewSpotLight : MonoBehaviour
{
    public float radius = 1;
    public float targetRadius = 1;
    public float zoomSpeed = 1;
    
    public float CamSizeMargin = 0.5f;

    public bool controlCamera = true;


    public Camera cam;
    private CircleCollider2D circCollider;

    private void Awake()
    {
        circCollider = GetComponent<CircleCollider2D>();
        
        UpdateSize();
    }

    private void Update()
    {
        if (Math.Abs(radius - targetRadius) > 0.00001f) ZoomAnimation(Time.deltaTime);
    }
    
    public void ZoomAnimation(float ts)
    {
        radius = Mathf.Lerp(radius, targetRadius, ts * zoomSpeed);
        
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (controlCamera)
            cam.orthographicSize = radius + CamSizeMargin;
        
        transform.localScale = new Vector3(radius,radius,1);
    }
}
