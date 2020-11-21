using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera2D : MonoBehaviour
{

    public Transform followTransform;
    public float smoothSpeed = 0.5f;
    private Camera mainCam;
    private Vector3 smoothPos;
    

    private void Start()
    {
        mainCam = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
    	Vector3 targetPos = new Vector3(followTransform.position.x, followTransform.position.y, this.transform.position.z);
        smoothPos = Vector3.Lerp(this.transform.position, targetPos, smoothSpeed);
        this.transform.position = smoothPos;
        
        
    }
}