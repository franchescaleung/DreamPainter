using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Barracuda;

/*
CODE CHUNKS
	[1] DRAWING FUNCTIONS
		void CreateStroke()
		Texture2D renderDrawing()

	[2] PREDICTION
		string predictObject(Texture2D)

	[3] UTILS
		void ResetDrawing()
		void DrawLine(Texture2D, p1, p2, color)
		void updateMinMax()
		bool inDrawingArea(Vector2)
*/

namespace Dreamwork{
public class sketchpad : MonoBehaviour
{


	[Header("Parameters")]
	public int img_width = 64;
    public int img_height = 64;

	[Header("Main References")]
	public GameManager GM;
    public Camera m_camera;
    public GameObject thoughtBubble;
    private BoxCollider2D thoughtBubbleCollider;

    [Header("Drawing/Prediction Refs")]
    public GameObject strokePrefab;
    public Sprite texture_blank;
    public NNModel modelSource;

    [Header("Drawing States")]
    public Vector2 curr_mousePos;
    public Vector2 max_mousePos, min_mousePos; // used for bounding box on image
    public bool inArea;


    private List<Stroke> strokes;
    private Stroke current_stroke;
	private Color[] texture_blank_pixels;
    private Model runtimeModel;
    

    // Start is called before the first frame update
    void Start()
    {
    	// init
        strokes = new List<Stroke>();
        ResetDrawing();

        //load zeros for binary texture image
        texture_blank_pixels = texture_blank.texture.GetPixels(0, 0, img_width, img_height);

        //rt model for prediction
        runtimeModel = ModelLoader.Load(modelSource);

        //used to position bubble/camera
        thoughtBubbleCollider = thoughtBubble.GetComponent<BoxCollider2D>();

    }


    // Update is called once per frame
    void Update()
    {
        curr_mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
    	inArea = inDrawingArea(curr_mousePos); //check if mouse in drawing area

    	// Left click : new stroke
    	if (Input.GetKeyDown(KeyCode.Mouse0)){
        	CreateStroke();
    	} 

       	// Hold left click: stroke in progress
        else if (Input.GetKey(KeyCode.Mouse0)){
	    	if(current_stroke != null) 
	    		current_stroke.add(curr_mousePos);
	    }

	    // Enter: finish drawing
        else if(Input.GetKeyDown(KeyCode.Return)) 
        {	
        	//render
        	Texture2D img = renderDrawing();

        	//predict
        	string obj_drawn = predictObject(img);
        	Debug.Log("You drew: " + obj_drawn);
			GM.lucyScript.IM.loadItem(obj_drawn);

			//reset & toggle sketchpad off
        	Destroy(img);
        	ResetDrawing();
     		GM.TogglePause();
        	GM.ToggleSketchPad();
        }
    }
    





    /* ------------------------------------------------------------------

	[1] DRAWING FUNCTIONS

    ------------------------------------------------------------------ */

    void CreateStroke(){
    
    	if(!inArea) return;

    	//instantiate
    	GameObject new_stroke = Instantiate(strokePrefab);
    	new_stroke.transform.parent = transform;
    	new_stroke.transform.localPosition = Vector3.back * 0.01f;
    	current_stroke = new_stroke.GetComponent<Stroke>();
    	current_stroke.Init(this);
		strokes.Add(current_stroke);

		//start stroke
    	current_stroke.points = new List<Vector2>();
    	current_stroke.add(curr_mousePos);
    	current_stroke.add(curr_mousePos);

    }


    Texture2D renderDrawing(){
		//convert drawing to texture
		Texture2D img = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);
		
		img.SetPixels( texture_blank_pixels );
		img.Apply();


		// get drawing dimensions
		Vector2 dim = new Vector2(	max_mousePos.x - min_mousePos.x, 
									max_mousePos.y - min_mousePos.y);

	   	float square_dim = Mathf.Max(dim.x, dim.y);

	   	Vector2 offset = new Vector2(	(square_dim - dim.x)/2f, 
	   									(square_dim - dim.y)/2f);

	   	Vector2 scale = new Vector2(	img_width/square_dim, 
	   									img_height/square_dim);

		// draw lines between each scaled point for each stroke
	   	for (int i = 0; i < strokes.Count; i++){
	   		
	       	List<Vector2Int> scaled_pts = new List<Vector2Int>();
	       	float x = (strokes[i].points[0].x - min_mousePos.x + offset.x) * scale.x;
	       	float y = (strokes[i].points[0].y - min_mousePos.y + offset.y) * scale.y;
	       	scaled_pts.Add(new Vector2Int((int)x, (int)y));

	       	for (int j = 1; j < strokes[i].points.Count; j++){
	       		x = (strokes[i].points[j].x - min_mousePos.x + offset.x) * scale.x;
	       		y = (strokes[i].points[j].y - min_mousePos.y + offset.y) * scale.y;
	       		scaled_pts.Add(new Vector2Int((int)x, (int)y));

	       		DrawLine(img, scaled_pts[j-1], scaled_pts[j], Color.white);
	       	}

	    }
		img.Apply();
		//save drawing
		//byte[] bytes = img.EncodeToPNG();
		//File.WriteAllBytes(Application.dataPath + "/test.png", bytes);
		return img;
    }
    





    /* ------------------------------------------------------------------

	[2] PREDICTION

    ------------------------------------------------------------------ */

    string predictObject(Texture2D img){

		Tensor input_tensor = new Tensor(img, 1);

		var worker = WorkerFactory.CreateWorker(runtimeModel);
		worker.Execute(input_tensor);
		Tensor prediction = worker.PeekOutput();


		string obj_pred = class_dict.id2class[prediction.ArgMax()[0]];
		

		prediction.Dispose();
		worker.Dispose();
		input_tensor.Dispose();

		return obj_pred;
    }
    





    /* ------------------------------------------------------------------

    [3] UTILS

    ------------------------------------------------------------------ */

    public void ResetDrawing(){

    	//reset min/max
    	min_mousePos = new Vector2(Mathf.Infinity, Mathf.Infinity);
    	max_mousePos = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

    	//destroy all strokes
    	Stroke temp;
    	for(int i = strokes.Count - 1; i >= 0; i--){
    		temp = strokes[i];
    		strokes.RemoveAt(i);
    		Destroy(temp.gameObject);
    	}
    }

    void DrawLine(Texture2D tex, Vector2Int p1, Vector2Int p2, Color col)
	{
	 	int dy = p2.y - p1.y;
		int dx = p2.x - p1.x;
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

	public void updateMinMax(){
        //update min/max
        min_mousePos = new Vector2(Mathf.Min(min_mousePos.x, curr_mousePos.x),
									Mathf.Min(min_mousePos.y, curr_mousePos.y));
    	max_mousePos = new Vector2(Mathf.Max(max_mousePos.x, curr_mousePos.x),
									Mathf.Max(max_mousePos.y, curr_mousePos.y));
	}

	bool inDrawingArea(Vector2 mousePos){
		return 	(	
					(mousePos.x >= thoughtBubble.transform.position.x + thoughtBubbleCollider.offset.x - (thoughtBubbleCollider.size.x/2f)) && 
					(mousePos.x <= thoughtBubble.transform.position.x + thoughtBubbleCollider.offset.x + (thoughtBubbleCollider.size.x/2f)) &&
					(mousePos.y >= thoughtBubble.transform.position.y + thoughtBubbleCollider.offset.y - (thoughtBubbleCollider.size.y/2f)) && 
					(mousePos.y <= thoughtBubble.transform.position.y + thoughtBubbleCollider.offset.y + (thoughtBubbleCollider.size.y/2f))
				);
    }


}
}
