using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Barracuda;

public class drawtut : MonoBehaviour
{
/*

    void Awake(){
    }

    private void Update()
    {
        Drawing();
    }
    

    void Drawing() 
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateBrush();
            
           
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            PointToMousePos();
        }
        else if(Input.GetKeyDown(KeyCode.Mouse1)){

	       	float width = max_xy[0]- min_xy[0];
	       	float height = max_xy[1] - min_xy[1]; 
	       	float square_dim = Mathf.Max(width,height);
	       	float x_offset = (square_dim - width)/2f;
	       	float y_offset = (square_dim - height)/2f;

	       	float x_padding = img_width*0.1f;
	       	float y_padding = img_height*0.1f;

	       	for (int bc = 0; bc <= brush_count; bc++){
		       	List<Vector2> scaled_pts = new List<Vector2>();
		       	scaled_pts.Add(
		       					new Vector2(
		       						((m_Points[bc][0].x - min_xy[0] + x_offset)*img_width*0.8f/square_dim) + x_padding, 
		       						((m_Points[bc][0].y - min_xy[1] + y_offset)*img_height*0.8f/square_dim) + y_padding)
		       				);

		       	for (int i = 1; i < m_Points[bc].Count; i++){
		       		scaled_pts.Add(
		       					new Vector2(
		       						((m_Points[bc][i].x - min_xy[0] + x_offset)*img_width*0.8f/square_dim) + x_padding, 
		       						((m_Points[bc][i].y - min_xy[1] + y_offset)*img_height*0.8f/square_dim) + y_padding)
		       					);
		       		DrawLine(img, scaled_pts[i-1], scaled_pts[i], Color.white);

		       	}
		       	Destroy(brushInstances[bc]);
		    }
	       	img.Apply();
	 		
	       	Tensor input_tensor = new Tensor(img, 1);

	       	Tensor temp_tensor = new Tensor(1,28,28,1);
	       	float max_px = -1;
	       	float min_px = 999;
	       	for(int w = 0; w < 28; w++){
	       		for(int h = 0; h < 28; h++){
	       			min_px = Mathf.Min(input_tensor[0,w,h,0], min_px);
	       		}
	       	}

	       	for(int w = 0; w < 28; w++){
	       		for(int h = 0; h < 28; h++){
	       			temp_tensor[0,w,h,0] = input_tensor[0,w,h,0] - min_px;
	       		}
	       	}

	       	Tensor temp_tensor2 = new Tensor(1,28,28,1);

	       	for(int w = 0; w < 28; w++){
	       		for(int h = 0; h < 28; h++){
	       			max_px = Mathf.Max(temp_tensor[0,w,h,0], max_px);
	       		}
	       	}

	       	for(int w = 0; w < 28; w++){
	       		for(int h = 0; h < 28; h++){
	       			temp_tensor2[0,w,h,0] = temp_tensor[0,w,h,0] / max_px;
	       		}
	       	}
	       	var worker = WorkerFactory.CreateWorker(runtimeModel);
	       	worker.Execute(temp_tensor2);
	       	Tensor prediction = worker.PeekOutput();

	       	Debug.Log(pred_num + ":" + class_dict[prediction.ArgMax()[0]]);
	       	pred_num ++;

	        
	        Destroy(img);
			// byte[] bytes = img.EncodeToPNG();
	  		// File.WriteAllBytes(Application.dataPath + "/2.png", bytes);

	        ResetDrawing();
	        prediction.Dispose();
	        worker.Dispose();
	        input_tensor.Dispose();
	        temp_tensor.Dispose();
	        temp_tensor2.Dispose();
        }
        else 
        {
            currentLineRenderer = null;
        }
    }

    void CreateBrush() 
    {

        brush_count ++;
        brushInstances.Add(Instantiate(brush));
        currentLineRenderer = brushInstances[brush_count].GetComponent<LineRenderer>();
        currentLineRenderer.startColor = Color.black;
        currentLineRenderer.endColor = Color.black;
        m_EdgeCollider2D = brushInstances[brush_count].AddComponent<EdgeCollider2D> ();
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        m_Points.Add(new List<Vector2>());
    }

    void AddAPoint(Vector2 pointPos) 
    {
    	if ( !m_Points[brush_count].Contains ( pointPos ) )
		{	
			min_xy[0] = Mathf.Min(min_xy[0], pointPos[0]);
			min_xy[1] = Mathf.Min(min_xy[1], pointPos[1]);
			max_xy[0] = Mathf.Max(max_xy[0], pointPos[0]);
			max_xy[1] = Mathf.Max(max_xy[1], pointPos[1]);

			m_Points[brush_count].Add ( pointPos );
			currentLineRenderer.positionCount = m_Points[brush_count].Count;
	        int positionIndex = currentLineRenderer.positionCount - 1;
	        currentLineRenderer.SetPosition(positionIndex, pointPos);
	        m_EdgeCollider2D.points = m_Points[brush_count].ToArray ();
	    }
    }

    void PointToMousePos() 
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        if (lastPos != mousePos) 
        {
            AddAPoint(mousePos);
            lastPos = mousePos;
        }
    }




    void DrawLine(Texture2D tex, Vector2Int p1, Vector2Int p2, Color col)
	{
	 	int dy = (int)(y1-y0);
		int dx = (int)(x1-x0);
	 	int stepx, stepy;
	 
		if (dy < 0) { dy = -dy;	stepy = -1; }
		else { stepy = 1; }
		if (dx < 0) { dx = -dx; stepx = -1; }
		else { stepx = 1; }
		dy <<= 1; dx <<= 1;
	 
		float fraction = 0;
	 
		tex.SetPixel(p1.x, p1.y, col);
		if (dx > dy) {
			fraction = dy - (dx >> 1);
			while (Mathf.Abs(p1.x - p2.x) > 1) {
				if (fraction >= 0) {
					p1.y += stepy;
					fraction -= dx;
				}
				p1.x += stepx;
				fraction += dy;
				tex.SetPixel(p1.x, p1.y, col);
			}
		}
		else {
			fraction = dx - (dy >> 1);
			while (Mathf.Abs(p1.y - p2.y) > 1) {
				if (fraction >= 0) {
					p1.x += stepx;
					fraction -= dy;
				}
				p1.y += stepy;
				fraction += dx;
				tex.SetPixel(p1.x, p1.y, col);
			}
		}
	}
*/
}
