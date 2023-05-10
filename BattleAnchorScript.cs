using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleAnchorScript : MonoBehaviour
{
    //This script deals with the behavior of the Tile Highlighter that lies underneath all active battle units. For now, it sizes it based on the desired footprint of the mech beneath, and switches the sprite to show the facing of the unit underneath.

    //Variables that reference needed objects
    public Sprite thisAnchorSprite;
    public ProtocolsManagerScript protocolsManagerScript;
    public BattleManager battleManager;
    //Note the "unitIAmAttachedTo" and Battle Participant variables get selected at the time when we load the Design from its save slot.
    public GameObject unitIAmAttachedTo;
    public BattleParticipant battleParticipantScriptIAmAttachedTo;
    //These variables that reference the "facing" sprites are set in the Inspector
    public Sprite northSprite;
    public Sprite eastSprite;
    public Sprite westSprite;
    public Sprite southSprite;

    //Variables that find my position
    public List<Vector2Int> gridPositionsIAmOver;
    public string myMoveType;

    //Variables that reference the gameflow
    public bool readyToInstallInProtocol = false;
    public bool installedInProtocol = false;
    public bool inBattle = false;
    //This variable is only used when dragging one object over another that's already placed. If we detect we're over another thing when we let go, we abort install. We'd like to use the "isOccupied" variable to do this, but right now it's hard to even talk to the object above it if isOccupied is true.
    public bool overAnotherThing = false;

    //Variables used when installing in the Protocols screen
    private Vector3 mouseOffset;

    private void Awake()
    {
        thisAnchorSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            protocolsManagerScript = GameObject.Find("Protocols Manager").GetComponent<ProtocolsManagerScript>();
        }
        else
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

    }

    public void SizeMe()
    {
        string parentMoveType = null;
        if (unitIAmAttachedTo.GetComponent<BattleParticipant>() != null)
        {
            parentMoveType = unitIAmAttachedTo.GetComponent<BattleParticipant>().moveType;
        }
        else if (unitIAmAttachedTo.GetComponent<CurrentlyEditedMechContainer>() != null)
        {
            parentMoveType = unitIAmAttachedTo.GetComponent<CurrentlyEditedMechContainer>().moveType;
        }

        if (parentMoveType == "Large Quad" || parentMoveType == "Large Biped")
        {
            //Debug.Log("Detected a medium-size footprint");
            gameObject.transform.localScale = new Vector3(2, 2, 1);
        }
    }

    public void FaceNorth()
    {
        thisAnchorSprite = northSprite;
        gameObject.GetComponent<SpriteRenderer>().sprite = thisAnchorSprite;
        battleParticipantScriptIAmAttachedTo.myOrientation = "North";
    }

    public void FaceEast()
    {
        thisAnchorSprite = eastSprite;
        gameObject.GetComponent<SpriteRenderer>().sprite = thisAnchorSprite;
        battleParticipantScriptIAmAttachedTo.myOrientation = "East";
    }

    public void FaceWest()
    {
        thisAnchorSprite = westSprite;
        gameObject.GetComponent<SpriteRenderer>().sprite = thisAnchorSprite;
        battleParticipantScriptIAmAttachedTo.myOrientation = "West";
    }

    public void FaceSouth()
    {
        thisAnchorSprite = southSprite;
        gameObject.GetComponent<SpriteRenderer>().sprite = thisAnchorSprite;
        battleParticipantScriptIAmAttachedTo.myOrientation = "South";
    }

    public void MirrorMe()
    {

    }

    public void UpdateMyOrientation()
    {
        if (battleParticipantScriptIAmAttachedTo.myOrientation == "North")
        {
            FaceNorth();
        }
        if (battleParticipantScriptIAmAttachedTo.myOrientation == "East")
        {
            FaceEast();
        }
        if (battleParticipantScriptIAmAttachedTo.myOrientation == "West")
        {
            FaceWest();
        }
        if (battleParticipantScriptIAmAttachedTo.myOrientation == "South")
        {
            FaceSouth();
        }
    }

    //The following functions determine click-and-drag behavior in the Protocols screen
    private void OnMouseDown()
    {
        if (installedInProtocol && !inBattle)
        {
            foreach (Vector2Int gridIWasOn in gridPositionsIAmOver)
            {
                var terrainTileIWasOn = protocolsManagerScript.terrainTileDictionary[gridIWasOn];
                terrainTileIWasOn.ClearMe();
            }
            gridPositionsIAmOver.Clear();
            protocolsManagerScript.PrepareToInstallDesign();
        }
        if ((readyToInstallInProtocol || installedInProtocol) && !inBattle)
        {
            mouseOffset = unitIAmAttachedTo.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (inBattle)
        {
            //Debug.Log("Hello!");
            battleManager.FocusOnThis(this.transform.parent.gameObject);
        }
    }

    private void OnMouseDrag()
    {
        if ((readyToInstallInProtocol || installedInProtocol) && !inBattle)
        {
            unitIAmAttachedTo.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + mouseOffset;
        }
    }

    private void OnMouseUp()
    {
        if (readyToInstallInProtocol == true)
        {
            
            
            FindWhereToInstall();
            FaceSouth();
            //protocolsManagerScript.battleParticipantsInProtocol.Add(battleParticipantScriptIAmAttachedTo);
            readyToInstallInProtocol = false;
            installedInProtocol = true;
            protocolsManagerScript.UpdateCurrentProtocolStats();
            protocolsManagerScript.AbortInstallingDesign();
            

        }
        if (installedInProtocol)
        {
            FindWhereToInstall();
            FaceSouth();
            protocolsManagerScript.UpdateCurrentProtocolStats();
            protocolsManagerScript.AbortInstallingDesign();
        }
    }

    //The following functions focus on the snap-to code when installing the mech inside the Protocols screen.
    public void FindWhereToInstall()
        //In order to find where to Install, we look at all the Grid Positions we're currently over. We then sort them by distance to my own position. Then we look at our footprint... If I'm a 1x1, install to the closest. If I'm a 2x2, install to the 4 closest, etc.
    {
        //First we look at all the grid squares the mech's Anchor is currently touching via collision detection. We sort them by proximity to find the closest one.
        gridPositionsIAmOver.Sort(SortGridSquaresByProximity);
        bool aborted = false;
        //Now we make sure the closest square isn't already occupied. If so we abort.
        if (overAnotherThing)
        {
            Debug.Log("The square seems occupied by someone else!");
            if (installedInProtocol == true)
            {
                protocolsManagerScript.battleParticipantsInProtocol.Remove(battleParticipantScriptIAmAttachedTo);
            }
            protocolsManagerScript.AbortInstallingDesign();
            protocolsManagerScript.DeleteThisMech(unitIAmAttachedTo);
            protocolsManagerScript.UpdateCurrentProtocolStats();
            aborted = true;
        }
        //The following functions are used for any mech with a 2x2 footprint.
        if (myMoveType == "Large Quad" || myMoveType == "Large Biped")
        {
            //If we're not over at least 4 squares, we're on the edge of the board and installation should fail. In that case we remove the mech if it had been previously installed and return the grid squares to normal.
            if (gridPositionsIAmOver.Count < 4)
            {
                Debug.Log("Not enough ground covered");
                if (installedInProtocol)
                {
                    protocolsManagerScript.battleParticipantsInProtocol.Remove(battleParticipantScriptIAmAttachedTo);

                }

                protocolsManagerScript.AbortInstallingDesign();
                protocolsManagerScript.DeleteThisMech(unitIAmAttachedTo);
                protocolsManagerScript.UpdateCurrentProtocolStats();
            }
            else
            {
                
                var nearestGrid = gridPositionsIAmOver[0];
                //Debug.Log("The nearest grid square is " + nearestGrid);
                //Debug.Log("The closest grid square I found is " + gridPositionsIAmOver[0]);
                //Debug.Log("Then " + gridPositionsIAmOver[1]);
                //Debug.Log("Then " + gridPositionsIAmOver[2]);
                //Debug.Log("Then " + gridPositionsIAmOver[3]);
                var nearestGridPos = protocolsManagerScript.terrainGridToWorldDictionary[nearestGrid];
                //We have to denormalize this tile position.
                //nearestGridPos += protocolsManagerScript.tileOriginOffset;
                var myPos = gameObject.transform.position;
                string myOrientationToNearestGrid = FindWhatDirectionIAmIn(myPos, nearestGridPos);
                //Debug.Log("I think I lie to the " + myOrientationToNearestGrid);
                var squaresToSnapTo = FindATwoByTwoSquareAroundOnePoint(nearestGrid, myOrientationToNearestGrid);
                //Debug.Log("One of the 2x2 squares I want to snap to is " + squaresToSnapTo[0]);
                //Debug.Log("One of the 2x2 squares I want to snap to is " + squaresToSnapTo[1]);
                //Debug.Log("One of the 2x2 squares I want to snap to is " + squaresToSnapTo[2]);
                //Debug.Log("One of the 2x2 squares I want to snap to is " + squaresToSnapTo[3]);
                //
                //Sometimes at this point we still try to snap to a nonexistent square, so we first make sure that all the squares we're looking for exist, and if any do not we abort.
                foreach (Vector2Int squareToSnapTo in squaresToSnapTo)
                {
                    //If we try to snap to an unmapped square, we abort.
                    if (protocolsManagerScript.terrainTileDictionary.ContainsKey(squareToSnapTo) == false && aborted == false)
                    {
                        Debug.Log("One of these squares doesn't work.");
                        if (installedInProtocol)
                        {
                            protocolsManagerScript.battleParticipantsInProtocol.Remove(battleParticipantScriptIAmAttachedTo);
                        }
                        protocolsManagerScript.AbortInstallingDesign();
                        protocolsManagerScript.DeleteThisMech(unitIAmAttachedTo);
                        protocolsManagerScript.UpdateCurrentProtocolStats();
                        aborted = true;
                        return;
                    }
                    if (aborted == false)
                    {
                        //If we try to snap to a square that's occupied by somebody else, we abort.
                        if (protocolsManagerScript.terrainTileDictionary[squareToSnapTo].whatOccupiesMe != unitIAmAttachedTo && protocolsManagerScript.terrainTileDictionary[squareToSnapTo].whatOccupiesMe != null)
                        {
                            Debug.Log("The square seems occupied by someone else!");
                            {
                                protocolsManagerScript.battleParticipantsInProtocol.Remove(battleParticipantScriptIAmAttachedTo);
                            }
                            protocolsManagerScript.AbortInstallingDesign();
                            protocolsManagerScript.DeleteThisMech(unitIAmAttachedTo);
                            protocolsManagerScript.UpdateCurrentProtocolStats();
                            aborted = true;
                        }
                    }

                }
                if (aborted == false)
                {
                    var worldPosToSnapTo = FindTheCenterOfTheseGridSquares(squaresToSnapTo);
                    //Debug.Log("I want to snap to " + worldPosToSnapTo);
                    var vectorXFromMeToTarget = worldPosToSnapTo.x - myPos.x;
                    var vectorYFromMeToTarget = worldPosToSnapTo.y - myPos.y;
                    var vectorToTranslateBy = new Vector3(vectorXFromMeToTarget, vectorYFromMeToTarget, 0);
                    //Debug.Log("It seems I need to translate by " + vectorToTranslateBy);
                    gameObject.transform.parent.transform.position += vectorToTranslateBy;
                    foreach (Vector2Int gridTile in gridPositionsIAmOver)
                    {
                        var tileScript = protocolsManagerScript.terrainTileDictionary[gridTile];
                        tileScript.ClearMe();
                    }
                    SetWhatSquaresIAmOn(squaresToSnapTo);
                }

                
                //Debug.Log("My new position after translation is " + gameObject.transform.position);
                //Debug.Log("The tile's position was " + nearestGridPos);





            }
        }
        if (myMoveType == "Walk" || myMoveType == "Biped")
        {
            //If I'm not over the grid, destroy me and abort. Note that gridPositionsIAmOver also gets cleared when I pick the mech up from the design, so if a mech is installed and I merely click on it, it gets deleted!
            if (gridPositionsIAmOver.Count == 0)
            {
                if (installedInProtocol == true)
                {
                    protocolsManagerScript.battleParticipantsInProtocol.Remove(battleParticipantScriptIAmAttachedTo);
                }
                protocolsManagerScript.AbortInstallingDesign();
                protocolsManagerScript.DeleteThisMech(unitIAmAttachedTo);
                protocolsManagerScript.UpdateCurrentProtocolStats();
                aborted = true;
                return;
            }
            //Debug.Log("The nearest grid position seems to be " + gridPositionsIAmOver[0]);
            var nearestGridPos = protocolsManagerScript.terrainGridToWorldDictionary[gridPositionsIAmOver[0]];
            //Debug.Log("Its world positions seems to be " + nearestGridPos);
            var myPos = gameObject.transform.position;
            //Debug.Log("My world position -- of my anchor -- seems to be " + myPos);
            var vectorXFromMeToTarget = nearestGridPos.x - myPos.x;
            var vectorYFromMeToTarget = nearestGridPos.y - myPos.y;
            var vectorToTranslateBy = new Vector3(vectorXFromMeToTarget, vectorYFromMeToTarget, 0);
            //Debug.Log("It seems I need to translate by " + vectorToTranslateBy);
            gameObject.transform.parent.transform.position += vectorToTranslateBy;
            List<Vector2Int> squareIAmOn = new List<Vector2Int>();
            //Now we identify exactly which square I'm on, and any others I happen to have overlapped we clear.
            squareIAmOn.Add(gridPositionsIAmOver[0]);
            foreach (Vector2Int gridTile in gridPositionsIAmOver)
            {
                var tileScript = protocolsManagerScript.terrainTileDictionary[gridTile];
                tileScript.ClearMe();
            }
            SetWhatSquaresIAmOn(squareIAmOn);
            //Debug.Log("My new position after translation is " + gameObject.transform.position);
            //Debug.Log("The tile's position was " + nearestGridPos);
        }
    }

    public int SortGridSquaresByProximity(Vector2Int firstVector, Vector2Int secondVector)
    
    {
        //When we sort we need to adjust for the universal offset, since by default the world positions in our Dictionary in the Protocols Manager is normalized to 0,0.
        var firstWorldPos = protocolsManagerScript.terrainGridToWorldDictionary[firstVector];
        //firstWorldPos += protocolsManagerScript.tileOriginOffset;
        //Debug.Log("The first world position I am comparing is " + firstWorldPos);
        var secondWorldPos = protocolsManagerScript.terrainGridToWorldDictionary[secondVector];
        //secondWorldPos += protocolsManagerScript.tileOriginOffset;
        //Debug.Log("The first world position I am comparing is " + secondWorldPos);
        var myVectorTwo = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        
        //Debug.Log("My world position is " + myVectorTwo);
        var firstDistance = Vector2.Distance(firstWorldPos, myVectorTwo);
        var secondDistance = Vector2.Distance(secondWorldPos, myVectorTwo);
        //Debug.Log("I seem to be " + firstDistance + " units away from the first square and " + secondDistance + "units away from the second square.");
        if (firstDistance > secondDistance)
        {
            //Debug.Log("The first distance appears to be greater.");
            return 1;
        }
        if (firstDistance < secondDistance)
        {
            //Debug.Log("The second distance appears to be greater.");
            return -1;
        }
        else
        {
            //Debug.Log("The distances appear equal.");
            return 0;
        }
        
    }

    public Vector2 FindTheCenterOfTheseGridSquares(List<Vector2Int> gridList)
    {
        float summedX = 0;
        float summedY = 0;
        float numberInList = gridList.Count;
        foreach (Vector2Int vector in gridList)
        {
            var worldVector = protocolsManagerScript.terrainGridToWorldDictionary[vector];
            //Debug.Log("One world vector I'm trying to find the center of is " + worldVector);
            //worldVector += protocolsManagerScript.tileOriginOffset;
            //Debug.Log("After adjusting the offset I think it's at " + worldVector);
            summedX += worldVector.x;
            summedY += worldVector.y;
        }
        //Debug.Log("I am going to divide X sum " + summedX + " and Y sum " + summedY + " by " + numberInList);
        float averageX = summedX / numberInList;
        float averageY = summedY / numberInList;
        var centerVector = new Vector2(averageX, averageY);
        return centerVector;
    }

    public string FindWhatDirectionIAmIn(Vector2 myWorldPos, Vector2 referenceWorldPos)
    {
        var vectorFromReferenceToMe = myWorldPos - referenceWorldPos;
        if (vectorFromReferenceToMe.x > 0)
        {
            if (vectorFromReferenceToMe.y > 0 || vectorFromReferenceToMe.y == 0)
            {
                return "Northeast";
            }
            if (vectorFromReferenceToMe.y < 0)
            {
                return "Southeast";
            }
        }
        if (vectorFromReferenceToMe.x < 0)
        {
            if (vectorFromReferenceToMe.y > 0)
            {
                return "Northwest";
            }
            if (vectorFromReferenceToMe.y < 0 || vectorFromReferenceToMe.y == 0)
            {
                return "Southwest";
            }
        }
        if (vectorFromReferenceToMe.x == 0 )
        {
            if (vectorFromReferenceToMe.y < 0)
            {
                return "Southeast";
            }
            if (vectorFromReferenceToMe.y > 0)
            {
                return "Northwest";
            }
        }
        else
        {
            return "Equal";
        }
        return "Equal";
    }

    public List<Vector2Int> FindATwoByTwoSquareAroundOnePoint(Vector2Int square, string orientation)
    {
        //This function takes a square on the battlegrid and a directional orientation, then finds the 2x2 square that lies in that orientation. If it doesn't know it returns the square to the southeast.
        var squareList = new List<Vector2Int>();
        if (orientation == "Northeast")
        {
            squareList.Add(square);
            square.x += 1;
            squareList.Add(square);
            square.y += 1;
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            return squareList;
        }
        if (orientation == "Southeast")
        {
            squareList.Add(square);
            square.x += 1;
            squareList.Add(square);
            square.y -= 1;
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            return squareList;
        }
        if (orientation == "Northwest")
        {
            squareList.Add(square);
            square.y += 1;
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            square.y -= 1;
            squareList.Add(square);
            return squareList;
        }
        if (orientation == "Southwest")
        {
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            square.y -= 1;
            squareList.Add(square);
            square.x += 1;
            squareList.Add(square);
            return squareList;
        }
        if (orientation == "Equal")
        {
            squareList.Add(square);
            square.x += 1;
            squareList.Add(square);
            square.y -= 1;
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            return squareList;
        }
        else
        {
            squareList.Add(square);
            square.x += 1;
            squareList.Add(square);
            square.y -= 1;
            squareList.Add(square);
            square.x -= 1;
            squareList.Add(square);
            return squareList;
        }

    }

    public void SetWhatSquaresIAmOn(List<Vector2Int> squaresToOccupy)
    {
        gridPositionsIAmOver.Clear();
        battleParticipantScriptIAmAttachedTo.gridPositionsIAmOver.Clear();
        battleParticipantScriptIAmAttachedTo.gridPositionsIAmOver = squaresToOccupy;
        gridPositionsIAmOver = squaresToOccupy;
        foreach (Vector2Int gridTile in squaresToOccupy)
        {
            var tileScript = protocolsManagerScript.terrainTileDictionary[gridTile];
            tileScript.isOccupied = true;
            tileScript.whatOccupiesMe = unitIAmAttachedTo;
            tileScript.searchable = false;
            tileScript.objectCanEnter = false;
        }
    }

    public void ColorMe()
    {
        //I started getting errors on this function when using the Protocols Screen, so this function aborts if on that screen.
        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            return;
        }
        var myTeam = battleParticipantScriptIAmAttachedTo.myTeam;
        if (myTeam == battleManager.playerTeamNumber)
        {
            var blue = new Color(0, 0, 255, 255);
            this.gameObject.GetComponent<SpriteRenderer>().color = blue;
        }
        else
        {
            var red = new Color(255, 0, 0, 255);
            this.gameObject.GetComponent<SpriteRenderer>().color = red;
        }
    }

    //Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<BattleParticipant>() != null)
        {
            myMoveType = GetComponentInParent<BattleParticipant>().moveType;
        }
        ColorMe();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
