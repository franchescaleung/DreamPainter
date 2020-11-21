using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Dreamwork
{
    public class PlayerController : MonoBehaviour
    {
        #region Unity
        [Header("LayerMask")]
        public LayerMask groundLayer;
        public LayerMask wallLayer;
        public LayerMask paintLayer;

        [Header("Parameters")]
        public float speedMove;
        public float speedJump, speedShift;
        public float defaultBoxY,crouchBoxY;
        public bool isGround = false, isFalling = false, isCrouch = false;

        Transform playerSprite;
        DetectWallAndGround detect;
        bool isMoving = false;
        Animator ani;

        void Start()
        {
            playerSprite = transform;
            detect = GetComponent<DetectWallAndGround>();
            ani = GetComponent<Animator>();
        }

        void Update()
        {
            CheckCrouch();
            CheckJump();

        }

        void FixedUpdate()
        {
            float jumpVelocity = GetComponent<Rigidbody2D>().velocity.y;
            
            if (detect.IsGround(groundLayer) || detect.IsGround(paintLayer))
            {
                isGround = true;
                if (isFalling)
                {
                    isFalling = false;
                }
            }
            else if (jumpVelocity < 0)
            {
                isFalling = true;
                isGround = false;
            }
            else if (jumpVelocity > 0)
            {
                isGround = false;
            }
            Move();

            // animation
            

            if(isGround)
                ani.SetInteger("airState", 0); //grounded

            else if(Mathf.Abs(jumpVelocity) < 0.25*speedJump)
                ani.SetInteger("airState", 2); //hang time

            else if(jumpVelocity > 0)
                ani.SetInteger("airState", 1); //up

            else
                ani.SetInteger("airState", 3); //down

            ani.SetBool("isGround", isGround);
            ani.SetBool("isMoving", isMoving);
            

        }
        #endregion
        #region Action
        void CheckJump()
        {
            if ((InputManager.GetKeyDown("Jump1") || InputManager.GetKeyDown("Jump2")) && isGround)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(0, speedJump);
            }
        }

        void CheckCrouch()
        {
            if (InputManager.GetKeyDown("Crouch") && isGround)
            {
                isCrouch = true;
                GetComponent<BoxCollider2D>().size = new Vector2(GetComponent<BoxCollider2D>().size.x, crouchBoxY);
            }
            else if (InputManager.GetKeyUp("Crouch") && isGround)
            {
                isCrouch = false;
                GetComponent<BoxCollider2D>().size = new Vector2(GetComponent<BoxCollider2D>().size.x, defaultBoxY);
            }
        }
        #endregion
        #region Move

        Vector3 CheckInput(string whichWall)
        {
            float xMove = 0;
            float isShift = 0;
            Vector2 move = new Vector2(0,0);

            if (InputManager.GetKey("Speedup"))
            {
                isShift = 1;
            }
            if (whichWall == "LeftWall")
            {
                if (InputManager.Horizontal > 0)
                    xMove = InputManager.Horizontal * (speedMove + speedShift * isShift) * Time.deltaTime;
            }
            else if (whichWall == "RightWall")
            {
                if (InputManager.Horizontal < 0)
                    xMove = InputManager.Horizontal * (speedMove + speedShift * isShift) * Time.deltaTime;
            }
            else
                xMove = InputManager.Horizontal * (speedMove + speedShift * isShift) * Time.deltaTime;

            if (isCrouch)
            {
                move = new Vector2(xMove / 2.6f, 0);
                playerSprite.localScale = new Vector2(playerSprite.localScale.x, crouchBoxY);            }
            else
            {
                playerSprite.localScale = new Vector2(playerSprite.localScale.x, defaultBoxY);
                move = new Vector2(xMove, 0);
            }
            
            isMoving = Mathf.Abs(move.x) > 0;

            return move;
        }

        void Move()
        {
            //Rotate by X move 
            Rotation(CheckInput("").x);

            if (detect.IsWall(wallLayer) == 1 || detect.IsWall(paintLayer) == 1)
                transform.Translate(CheckInput("LeftWall"));
            else if (detect.IsWall(wallLayer) == 2 || detect.IsWall(paintLayer) == 2)
                transform.Translate(CheckInput("RightWall"));
            else
                transform.Translate(CheckInput(""));
        }

        //Rotation method
        void Rotation(float x)
        {
            //Change X in scale
            if (x > 0)
                playerSprite.localScale = new Vector2(Mathf.Abs(playerSprite.localScale.x), Mathf.Abs(playerSprite.localScale.y));
            else if (x < 0)
                playerSprite.localScale = new Vector2(Mathf.Abs(playerSprite.localScale.x) * -1, Mathf.Abs(playerSprite.localScale.y));
        }
        #endregion
    }
}
