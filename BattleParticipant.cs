using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BattleParticipant : MonoBehaviour
{
    //These variables store the long-term attributes of the battle participant. 
    public string characterName = "Mech Name";

    public Sprite characterPortrait;
    public string characterPortraitPath;

    public int healthMax = 0;
    public int currentHealth;
    public int powerUsed = 0;
    public int powerMax = 0;
    public int weightUsed = 0;
    public int weightMax = 0;
    public int cpuUsed = 0;
    public int cpuMax = 0;
    public int currentCpu;
    public string moveType = "Walk";
    public int jump = 0;
    public int movePower = 0;
    public float currentMove = 0;
    public float rotateCost = 0;
    public float evade = 0;
    public float accuracy = 1;

    public List<PassiveAbility> passiveAbilitiesAffectingMe;

    public bool isCloaked = false;

    //Variables for organizing the game flow during combat
    public int myTeam;

    //Variables that find my position on the battlemap
    public List<Vector2Int> gridPositionsIAmOver;
    public string myOrientation;

    //Variables that reference other objects in the Scene
    public BattleAnchorScript myBattleAnchor;
    public BattleManager battleManager;

    //Variables for obstacle behavior
    public bool isObstacle = false;
    public int obstacleHeight = 0;

    //Variables for interplay with the Scenario system
    public bool leader = false;


    private void Awake()
    {
        passiveAbilitiesAffectingMe = new List<PassiveAbility>();
        gridPositionsIAmOver = new List<Vector2Int>();
        battleManager = FindObjectOfType<BattleManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = healthMax;

        if (!isObstacle)
        {
            myBattleAnchor = GetComponentInChildren<BattleAnchorScript>();

            currentMove = movePower;
            currentCpu = cpuMax - cpuUsed;
            FindMyJumpValue();
        }

        characterPortrait = FindMyCharacterPortrait();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FindMyJumpValue()
    {
        if (moveType == "Walk")
        {
            jump = 2;
        }
        if (moveType == "Large Quad")
        {
            jump = 1;
        }
    }

    public Sprite FindMyCharacterPortrait()
    {
        if (characterPortraitPath != null && characterPortraitPath != "")
        {
            var portraitBytes = File.ReadAllBytes(characterPortraitPath);
            var portrait = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            portrait.LoadImage(portraitBytes);
            var portraitSprite = Sprite.Create(portrait, new Rect(0, 0, portrait.width, portrait.height), new Vector2(0, 0));
            return portraitSprite;
        }

        else
        {
            return BattleManager.defaultCharacterImage;
        }
    }

    public void CheckMyTransparency()
    {

    }

    public void ResetMyTurn()
    {
        if (!isObstacle)
        {
            currentMove = movePower;
            currentCpu = (cpuMax - cpuUsed);
        }

    }

    public void StartMyTurn()
    {
        
    }

    public void EndMyTurn()
    {

    }

    public void Die()
    {
        //
        //This function is called when health is reduced to 0.
        //For an obstacle, it causes the obstacle's total destruction.
        //For a mech, it transforms the mech into an inert obstacle on team 99.
        //
        if (isObstacle)
        {
            TrulyDie();
        }
        else
        {
            CeaseFunctioning();
        }
    }

    public void CeaseFunctioning()
    {
        var darkColor = new Color(0.5f, 0.5f, 0.5f, 1);
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.color = darkColor;
        }
        isObstacle = true;
        RemoveMeFromTheTeamsList();
        myTeam = 99;
        characterName = "Shattered hulk";
        currentHealth = Mathf.RoundToInt(healthMax * 0.25f);
    }

    public void RemoveMeFromTheTeamsList()
    {
        foreach (List<BattleParticipant> bpList in battleManager.battleParticipantsByTeam)
        {
            if (bpList[0].myTeam == myTeam)
            {
                bpList.Remove(this);
                return;
            }
        }
    }

    public void TrulyDie()
    {
        //This function gets called in order to completely delete the object from the map.
        //In the future, we might want to start some explosions on the object.
        foreach (Vector2Int tileIAmOn in gridPositionsIAmOver)
        {
            var tile = battleManager.terrainTileDictionary[tileIAmOn];
            tile.ClearMe();
        }
        Destroy(gameObject);
    }

}
