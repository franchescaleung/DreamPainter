using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dreamwork{
public class GameManager : MonoBehaviour
{
    [Header("Parameters")]
    public Vector2 camOffsetFromThoughtBubble = new Vector2(0f,2f);

	[Header("Lucy")]
	public GameObject lucyGO;
	public PlayerLogic lucyScript;
    public sketchpad sp;

	[Header("Tilemaps")]
	public Grid grid;
	public Tilemap terrainTM;
	public Tilemap actionAreaTM;

	[Header("Layers Refs")]
	public LayerMask terrainLayer;
    public LayerMask actionAreaLayer;


    [Header("Other Refs")]
    public int level_num = 1;
    public LevelsData levelsData;
    public SmoothCamera2D cam;


    [Header("Game States")]
    public bool onPause = false;
    public bool showSketchPad = false;

    [Header("Background")]
    public SpriteRenderer darkBG;
    public SpriteRenderer lightBG;

    // Start is called before the first frame update
    void Start()
    {
        levelsData = GetComponent<LevelsData>();
        levelsData.GM = this;
    }

    void Update()
    {
        bool pressed_tab = PlayerInput.sketchpad;
        if(pressed_tab){
            TogglePause();
            ToggleSketchPad();
        }
    }

    public void TogglePause(){

        //RESUME
        if(onPause){ 
            onPause = false;
            Time.timeScale = 1;
        } 

        //PAUSE
        else { 
            onPause = true;
            Time.timeScale = 0;
        }
    }   

    public void ToggleSketchPad(){
        // DISABLE
        if(showSketchPad){
            showSketchPad = false;
            sp.gameObject.SetActive(false);

            sp.ResetDrawing();
            cam.followTransform = lucyGO.transform;

        }

        // ACTIVATE
        else{
            showSketchPad = true;
            sp.gameObject.SetActive(true);
            sp.transform.position = new Vector3(lucyGO.transform.position.x + camOffsetFromThoughtBubble.x,
                                            lucyGO.transform.position.y + camOffsetFromThoughtBubble.y,
                                            sp.transform.position.z);
            cam.followTransform = sp.transform;
            
        }
    }

    public string activateArea(string action_area_name, string itemName)
    {
        string response = levelsData.getResponse(level_num, action_area_name, itemName);
        // Debug.Log(response);
        return response;
    }
    
    public void removeActionArea(int x1,int y1, int x2, int y2){
        // requirements: x1 <= x2, y1 <= y2
        for(int x = x1; x <= x2; x++){
            for(int y = y1; y <= y2; y++){
                actionAreaTM.SetTile(new Vector3Int(x,y,0), null);
            }
        }
            
    }

public IEnumerator fadeOut(SpriteRenderer MyRenderer, float duration)
{
    float counter = 0;
    //Get current color
    Color spriteColor = MyRenderer.material.color;

    while (counter < duration)
    {
        counter += Time.deltaTime;
        //Fade from 1 to 0
        float alpha = Mathf.Lerp(1, 0, counter / duration);
        Debug.Log(alpha);
        //Change alpha only
        Color c = MyRenderer.color;
        c.a = alpha;
        MyRenderer.color = c;
        //Wait for a frame
        yield return null;
    }
}

}
}