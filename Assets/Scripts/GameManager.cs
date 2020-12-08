using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;  // make GameManager singleton. (any scripts can access public functions and variables of GameManager)
    public BoardManager boardScript;

    private int level = 3;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)  // destroy the gameManager not to make double gameManager
            Destroy(gameObject);

        // all of objects in hierarchy is destroyed when new scene is loaded
        // but GameManager has to keep calculating the score even though the scene is over.
        // So, used 'DontDestroyOnLoad' function to protect GameManager from destroying.
        DontDestroyOnLoad(gameObject);
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    void InitGame()
    {
        boardScript.SetupScene(level);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
