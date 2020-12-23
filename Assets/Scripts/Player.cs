using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    // Damage to wall when player hit the wall
    public int wallDamage = 1;
    // Score of each food objects.
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;

    private Animator animator;
    // Before switching levels and entering the scores back into Game Manager, store the player scores during those levels
    private int food;

    protected override void Start()
    {
        animator = GetComponent<Animator>();

        food = GameManager.instance.playerFoodPoints;

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

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        // Prevents the player from moving diagonally
        if (horizontal != 0)
            vertical = 0;

        // Player moves
        if(horizontal!=0 || vertical!=0)
        {
            // Player can interact with only the wall
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    protected override void AttemptMove<T>(int xDir,int yDir)
    {
        // player lose 1 food point for every moving
        food--;

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

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
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
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
        SceneManager.GetActiveScene();
    }

    // Called when Enemy hits the player
    public void LoseFood(int loss)
    {
        animator.SetTrigger("PlayerHit");

        food -= loss;

        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(food<=0)
        {
            GameManager.instance.GameOver();
        }
    }
}
