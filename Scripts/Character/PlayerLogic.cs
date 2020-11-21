using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;



/*
CODE CHUNKS
    [1] MOVEMENT
        TogglePhysics() // RB: kinematic <--> dynamic
        updateMovement() // default movement
        void updateMovementVehicle(string) // hot air balloon

    [2] ANIMATION
        void updateAnimationState()

    [3] ITEM FUNCTIONS
        void useCurrentItem() // when E-pressed

    [4] COLLISIONS
        void checkRaycasts() // terrain & ladder
        OnTriggerStay2D & OnTriggerExit2D //for Action Areas
*/


namespace Dreamwork{

public class PlayerLogic : MonoBehaviour
{
    [Header("Player States")]
    public string movement_mode = "default"; // or "hot air balloon"
    public bool physicsOn = true;
    private float lastTimeGrounded;
    private GameObject curr_item;
    private string current_action_area = "";
    private bool inTerrain = false;
    private bool ladderAbove = false;
    private bool ladderBelow = false;
    private float ladderCenter;
    

    [Header("Parameters")]
	public float move_speed;
	public float jump_speed;
    public float crouch_speed_multiplier;
	public float fallMultiplier = 2.5f; 
	public float lowJumpMultiplier = 2f;
	public float rememberGroundedFor = 0.1f;
    public float hot_air_balloon_speed = 3f;

	[Header("Character Colliders")]
	public Vector2 boxColliderOffset = new Vector2(0f, -0.02f);
	public Vector2 boxColliderSize = new Vector2(0.6f, 1.37f);
    public Vector2 crouchBoxColliderOffset = new Vector2(0f,-0.23f);
    public Vector2 crouchBoxColliderSize = new Vector2(0.6f,0.9f);
    public Vector2 hotAirBalloonColliderOffset = new Vector2(0f,0f);
    public Vector2 hotAirBalloonColliderSize = new Vector2(0.622f,2.65f);


    [Header("Layers Reference")]
	public LayerMask terrainLayer;
    public LayerMask actionAreaLayer;
    public LayerMask ladderLayer;


    [Header("Character Status")]
	public bool isGrounded = false;
    public bool isCrouching = false;
    public bool isSqueezed = false;
    public bool climbingLadder = false;
    public bool canBalloon = false;
    

    [Header("Animation")]
    public AnimatorController anim_controller_default;
    public AnimatorController anim_controller_vehicle;
    private Animator anim;

    
    [Header("References")]
    public ItemsManager IM;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D bc;
    public GameManager GM;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        bc = GetComponent<BoxCollider2D>();
        anim.runtimeAnimatorController = anim_controller_default as RuntimeAnimatorController;

    }

    void Update(){}


    void FixedUpdate(){
        if(PlayerInput.use_item) useCurrentItem(); 

    	checkRaycasts();

        if(movement_mode == "default") updateMovement();
        else updateMovementVehicle(movement_mode);
        
    	updateAnimationState();
    }
    


    


    /* ------------------------------------------------------------------

    [1] MOVEMENT

    ------------------------------------------------------------------ */


    void TogglePhysics(bool val){
        // switches between dynamic & kinematic
        if(!val){
            physicsOn = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else{
            physicsOn = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }


    //////////////////
    //Default movement
    //////////////////
    void updateMovement(){

        Vector2 velocity;

        //trying to climb ladder?
        if((PlayerInput.jump && ladderAbove) || (PlayerInput.crouch && ladderBelow) && !climbingLadder) {
            climbingLadder = true;
            TogglePhysics(false);
            transform.position = new Vector3 (ladderCenter, transform.position.y, transform.position.x);
        }


        float x_input = PlayerInput.horizontal;
        if(climbingLadder && inTerrain) x_input = 0; // prevents you from leaving ladder when in terrain


    	velocity = new Vector2(x_input * move_speed * (isCrouching ? crouch_speed_multiplier : 1),0);


        if(physicsOn){
            //platformer jump (https://craftgames.co/unity-2d-platformer-movement)
        	velocity.y = PlayerInput.jump && !(isSqueezed  || isCrouching) && (isGrounded  || Time.time - lastTimeGrounded < rememberGroundedFor)
        					? jump_speed : rb.velocity.y;        	
        	if (velocity.y < 0) {
    			velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
    		} else if (velocity.y > 0 && !PlayerInput.jump) {
    			velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
    		}
    	} else {
            //move up and down
            if(climbingLadder){
                velocity.y = (PlayerInput.jump ? jump_speed/2f : 0) - (PlayerInput.crouch && ladderBelow ? jump_speed/2f : 0);

                //gets off ladder
                if(x_input != 0)
                {
                    climbingLadder = false;
                    TogglePhysics(true);
                }
            }
            
        }
    	rb.velocity = velocity;
    }


    ///////////////////////////
    //Vehicle (Hot Air Balloon)
    ///////////////////////////
    void updateMovementVehicle(string vehicle_name){
        if(vehicle_name == "hot air balloon"){
            float x_vel = PlayerInput.horizontal * hot_air_balloon_speed;
            float y_vel = (PlayerInput.jump?hot_air_balloon_speed:0f) - (PlayerInput.crouch?hot_air_balloon_speed:0f); 
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(x_vel,y_vel), 0.5f);
        }

    }
    


    


    /* ------------------------------------------------------------------

    [2] ANIMATION

    ------------------------------------------------------------------ */

    void updateAnimationState(){
        //flip sprite
    	if(rb.velocity.x < 0) sr.flipX = true;
    	else if(rb.velocity.x > 0) sr.flipX = false;


        //update animation states
        if(movement_mode == "default"){
    		if(isGrounded)
    		    anim.SetInteger("airState", 0); //grounded

    		else if(Mathf.Abs(rb.velocity.y) < 0.6*jump_speed)
    		    anim.SetInteger("airState", 2); //hang time

    		else if(rb.velocity.y > 0)
    		    anim.SetInteger("airState", 1); //up

    		else
    		    anim.SetInteger("airState", 3); //down

        
    		anim.SetBool("isGround", isGrounded);
    		anim.SetBool("isMoving", rb.velocity.x != 0);
            anim.SetBool("isCrouching", isCrouching);
        }


        // item animation/sprite
        if(IM.equipped_item != null){
            IM.equipped_sr.flipX = sr.flipX;
            Vector3 offset = Vector3.right * (boxColliderOffset.x + boxColliderSize.x/2f + IM.equipped_sr.bounds.size.x/2f);
            offset *= sr.flipX ? -1 : 1;
            offset += Vector3.down * (isCrouching ? (boxColliderSize.y - crouchBoxColliderSize.y)/2f : 0);
            IM.equipped_item.transform.position = transform.position + offset;
        }
    }


    /* ------------------------------------------------------------------

    [3] ITEM FUNCTIONS

    ------------------------------------------------------------------ */

    void useCurrentItem(){
        

        if(movement_mode != "default"){
            //get off vehicle
            anim.runtimeAnimatorController = anim_controller_default as RuntimeAnimatorController;
            movement_mode = "default";
            bc.size = boxColliderSize;
            bc.offset = boxColliderOffset;
            rb.gravityScale = 1.8f;
        }

        else if(IM.equipped_item_name == "hot air balloon" && canBalloon){
            //get on hot air balloon
            anim.runtimeAnimatorController = anim_controller_vehicle as RuntimeAnimatorController;
            movement_mode = "hot air balloon";

            transform.position = transform.position + Vector3.up * ((hotAirBalloonColliderSize.y/2 - hotAirBalloonColliderOffset.y) - (bc.size.y/2 - bc.offset.y));
            bc.size = hotAirBalloonColliderSize;
            bc.offset = hotAirBalloonColliderOffset;
            rb.gravityScale = 0.25f;
            IM.destroyCurrentItem();

        } else{
            if(current_action_area == "") return;

            //signal item-AA interaction to GM
            string response = GM.activateArea(current_action_area, IM.equipped_item_name);
            if(response != "") IM.destroyCurrentItem();
        }
    }






    /* ------------------------------------------------------------------
    
    [4] COLLISIONS

    ------------------------------------------------------------------ */

    void checkRaycasts(){

        // update crouch collider box
        if(movement_mode == "default" && PlayerInput.crouch != isCrouching && !isSqueezed)
        {
            isCrouching = PlayerInput.crouch;
            bc.size = isCrouching ? crouchBoxColliderSize : boxColliderSize;
            bc.offset = isCrouching ? crouchBoxColliderOffset : boxColliderOffset;
        }


        ////////////////
        //Terrain checks
        ////////////////
        RaycastHit2D ground_hit1 = Physics2D.Raycast(transform.position - (Vector3.right * bc.size.x/2.1f), Vector2.down, (-bc.offset.y) + 0.02f + bc.size.y/2f, terrainLayer);
        RaycastHit2D ground_hit2 = Physics2D.Raycast(transform.position + (Vector3.right * bc.size.x/2.1f), Vector2.down, (-bc.offset.y) + 0.02f + bc.size.y/2f, terrainLayer);        
        Vector3 start_roof_hit1 = transform.position - (Vector3.right * bc.size.x/2f);
        Vector3 start_roof_hit2 = transform.position + (Vector3.right * bc.size.x/2f);
        RaycastHit2D roof_hit1 = Physics2D.Raycast(start_roof_hit1, Vector2.up, 10f, terrainLayer);
        RaycastHit2D roof_hit2 = Physics2D.Raycast(start_roof_hit2, Vector2.up, 10f, terrainLayer);

        // Update booleans {isGrounded, isSqueezed, canBalloon}
        isGrounded = ground_hit1.collider || ground_hit2.collider;
        if(isGrounded) lastTimeGrounded = Time.time;

        float check_head = (bc.size.y/2f - bc.offset.y) + 0.02f;
        bool head_collision = (roof_hit1 && Vector2.Distance((Vector2)start_roof_hit1, roof_hit1.point) <= check_head) || 
                              (roof_hit2 && Vector2.Distance((Vector2)start_roof_hit2, roof_hit2.point) <= check_head);
        isSqueezed = isCrouching && isGrounded && head_collision;

        //can go hot air balloon
        float check_hot_air_balloon = hotAirBalloonColliderSize.y - (bc.size.y/2f - bc.offset.y) + 0.02f;
        canBalloon = !((roof_hit1 && Vector2.Distance(start_roof_hit1, roof_hit1.point) <= check_hot_air_balloon) || 
                       (roof_hit2 && Vector2.Distance(start_roof_hit2, roof_hit2.point) <= check_hot_air_balloon));



        ////////////////
        //Ladder Checks
        ////////////////
        Vector3 pos = transform.position + (Vector3.up * (bc.size.y/4f + bc.offset.y));
        RaycastHit2D ladder_roof_hit = Physics2D.Raycast(pos, Vector2.up,  (bc.size.y/4f + 0.1f), ladderLayer);
        pos = transform.position +  (Vector3.down * (bc.size.y/2f - bc.offset.y));
        RaycastHit2D ladder_ground_hit = Physics2D.Raycast(pos, Vector2.down, 0.2f, ladderLayer);
        RaycastHit2D terrain_hit1 = Physics2D.Raycast(transform.position + (Vector3.down * bc.size.y/2f), Vector2.up, 1f, terrainLayer);
        inTerrain = terrain_hit1;
        
        ladderAbove = false;
        ladderBelow = false;
        if(ladder_roof_hit){
            ladderAbove = true;
            ladderCenter = ladder_roof_hit.collider.gameObject.transform.position.x;
        }
        if(ladder_ground_hit){
            ladderBelow = true;
            ladderCenter = ladder_ground_hit.collider.gameObject.transform.position.x;
        }
        if(!(ladder_roof_hit || ladder_ground_hit)){
            climbingLadder = false;
            TogglePhysics(true);
        }
          
    }

    void OnTriggerStay2D(Collider2D collider)
    {    
        if(collider.tag == "action_area")
            current_action_area = collider.gameObject.name;
    }
    void OnTriggerExit2D(Collider2D collider)
    {    
        if(collider.tag == "action_area")
            current_action_area = "";
    }
}   
}