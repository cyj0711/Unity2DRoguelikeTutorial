﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{
    // Damage to wall when player hit the wall
    public int wallDamage = 1;
    // Score of each food objects.
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;


    private Animator animator;
    // Before switching levels and entering the scores back into Game Manager, store the player scores during those levels
    private int food;
    // location that the player's touch begins
    private Vector2 touchOrigin = -Vector2.one; // -Vector2.one means location of the ouside of the screen

    protected override void Start()
    {
        animator = GetComponent<Animator>();

        food = GameManager.instance.playerFoodPoints;

        foodText.text = "Food: " + food;

        base.Start();
    }

    private void OnDisable()
    {
        // Store food points to GameManager when level is chainging
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

// for pc player
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        // Prevents the player from moving diagonally
        if (horizontal != 0)
            vertical = 0;

 // for mobile player
#else
        // system detects more than one touches
        if(Input.touchCount>0)
        {
            // This game supports one finger swiping for one direction. so ignore the other touches.
            Touch myTouch = Input.touches[0];

            // Means begining of the touch
            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }

            // Means end of the touch, and touched inside of the screen
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;

                // Protect from repeating true value
                touchOrigin.x = -1;

                // Check player's diretion
                if(Mathf.Abs(x)>Mathf.Abs(y))
                {
                    horizontal = x > 0 ? 1 : -1;
                }
                else
                {
                    vertical = y > 0 ? 1 : -1;
                }
            }
        }

#endif

        // Player moves
        if (horizontal != 0 || vertical != 0)
        {
            // Player can interact with only the wall
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        // player lose 1 food point for every moving
        food--;
        foodText.text = "Food: " + food;

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;
        if(Move(xDir,yDir,out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        // check game over because player lose food by moving
        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            // Stop for 1 second and restart the level
            Invoke("Restart", 1f);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
        
    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("PlayerChop");
    }
    
    // Called when load level again.
    private void Restart()
    {
        // Load the last loaded scene(In this case, load main scene, the only scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called when Enemy hits the player
    public void LoseFood(int loss)
    {
        animator.SetTrigger("PlayerHit");

        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;

        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(food<=0)
        {
            SoundManager.instance.RandomizeSfx(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }
}
