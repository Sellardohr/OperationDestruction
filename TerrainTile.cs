using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    //Variables that reference the tile's performance in battle, set in Inspector
    public float moveCost = 1;
    public int height = 0;
    public float penaltyToWalk = 0;
    public float penaltyToLargeQuad = 0;
    public float penaltyToWheel = 0;
    public float penaltyToTread = 0;
    public float penaltyToFly = 0;
    public string description;
    //isInert is set in the inspector for tiles we want to build but don't want the gameplay to read.
    public bool isInert = false;

    //The combat background that loads when we fight on this square is set in the Inspector.
    public Sprite combatBackground;

    //Variables that reference the game flow
    public bool searchable = true;
    public bool candidateForCharacterMove = false;
    //ObjectCanEnter means that if I'm dragging an object around, these squares will highlight when I collide with them. It should be disabled as long as there is a mech that's part of the Protocol on those squares.
    public bool objectCanEnter = false;

    //Variables that reference the tile's position in different coordinate systems
    public float worldX;
    public float worldY;

    public int gridX;
    public int gridY;
    public Vector2Int myGridPos;

    //Variables that reference objects the tile needs to know about
    public SpriteRenderer tileHighlightSprite;
    public Collider2D myCollider;
    public Color myOriginalColor;
    //WhatOccupiesMe and IsOccupied refer to a mech that is stationed on the square.
    public GameObject whatOccupiesMe;
    public bool isOccupied = false;
    public bool impassable = false;

    //Variables for interplay with the Scenario system
    public bool missionCritical;

    // Start is called before the first frame update
    private void Awake()
    {
        FindMyPosition();
        myCollider = GetComponent<Collider2D>();
        tileHighlightSprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        searchable = true;
    }

    void Start()
    {

    }

    public void SetInactive()
    {
        tileHighlightSprite.enabled = false;
        myCollider.enabled = false;
        searchable = false;
        candidateForCharacterMove = false;
    }

    public void SetActive()
    {
        tileHighlightSprite.enabled = true;
        myCollider.enabled = true;
    }

    public void FindMyPosition()
    {
        worldX = gameObject.transform.position.x;
        worldY = gameObject.transform.position.y;
    }

    public void HighlightMe()
    {
        var myColor = tileHighlightSprite.color;
        myColor.a = .45f;
        tileHighlightSprite.color = myColor;
        //Debug.Log("I just turned on my color");
    }

    public void HighlightMeInGreen()
    {
        var myColor = tileHighlightSprite.color;
        myColor.a = .75f;
        myColor.g = 1;
        tileHighlightSprite.color = myColor;
        //Debug.Log("I just turned on my color");
    }

    public void UnHighlightMe()
    {
        var myColor = tileHighlightSprite.color;
        myColor.a = 0;
        tileHighlightSprite.color = myColor;
    }

    public void PrepareMeForMechMove()
    {
        SetActive();
        HighlightMe();
        searchable = false;
        candidateForCharacterMove = true;
    }

    public void EndPreparingForMechMove()
    {
        candidateForCharacterMove = false;
        UnHighlightMe();
        SetInactive();
    }

    private void OnMouseEnter()
    {
        //Debug.Log("The mouse hit me!");
        if (searchable)
        {
            HighlightMe();
        }
        if (candidateForCharacterMove)
        {

        }

    }

    private void OnMouseExit()
    {
        if (searchable)
        {
            UnHighlightMe();
        }
        if (candidateForCharacterMove)
        {

        }

    }

    private void OnMouseDown()
    {
        if (searchable)
        {

        }
        if (candidateForCharacterMove)
        {

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (objectCanEnter == true)
        {
            HighlightMe();
            myGridPos = new Vector2Int(gridX, gridY);
            //Temporarily disabling this line. WhatOccupiesMe should only be filled when the ojbect is actually placed, not merely hovered over.            
            //whatOccupiesMe = collision.gameObject.transform.parent.gameObject;
            collision.gameObject.GetComponent<BattleAnchorScript>().gridPositionsIAmOver.Add(myGridPos);
        }
        if (isOccupied == true)
        {
            if (collision.gameObject.GetComponent<BattleAnchorScript>() != null)
            {
                collision.gameObject.GetComponent<BattleAnchorScript>().overAnotherThing = true;
            }

        }
        if (candidateForCharacterMove)
        {
            if (collision.gameObject.GetComponent<CursorAnchorScript>() != null)
            {
                var enteredAnchor = collision.gameObject.GetComponent<CursorAnchorScript>();
                if (!enteredAnchor.isSnapped)
                {
                    enteredAnchor.isSnapped = true;
                }
            }
        }
    }






    private void OnTriggerExit2D(Collider2D collision)
    {
        if (objectCanEnter)
        {
            UnHighlightMe();
            if (collision.gameObject.GetComponent<BattleAnchorScript>() != null)
            {
                collision.gameObject.GetComponent<BattleAnchorScript>().gridPositionsIAmOver.Remove(myGridPos);
            }
            
            //whatOccupiesMe = null;
        }
        if (isOccupied == true)
        {
            if (collision.gameObject.GetComponent<BattleAnchorScript>() != null)
            {
                collision.gameObject.GetComponent<BattleAnchorScript>().overAnotherThing = false;
            }

        }
    }

    public void ClearMe()
    {
        UnHighlightMe();
        whatOccupiesMe = null;
        isOccupied = false;
        objectCanEnter = false;
        searchable = true;
    }
}


    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

