using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dreamwork{
public class Stroke : MonoBehaviour
{	

	public LineRenderer lr;
	public List<Vector2> points;
	public sketchpad sref;

    //Stroke object is instantiated when sketchpad.CreateStroke() is called
    public void Init(sketchpad pad)
    {
    	lr = GetComponent<LineRenderer>();
        lr.startColor = Color.black;
        lr.endColor = Color.black;
        sref = pad;
    }

    public void add(Vector2 mousePos){
    	if(!sref.inArea) return;

    	points.Add(mousePos);
    	lr.positionCount = points.Count;
        lr.SetPosition(lr.positionCount - 1, mousePos);

        sref.updateMinMax();
    }
}
}