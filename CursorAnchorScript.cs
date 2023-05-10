using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CursorAnchorScript : MonoBehaviour
{
    //Variables that reference the game flow
    public bool isSnapped = false;
    public bool readyToChooseOrientation = false;
    public bool waiting = false;
    public bool readyToConfirm = false;
    public bool snapToListBuilt = false;
    public bool validDirectionsDetermined = false;
    public bool readyToChooseAbilityLocus = false;

        //These variables that reference the "facing" sprites are set in the Inspector
    public Sprite northSprite;
    public Sprite eastSprite;
    public Sprite westSprite;
    public Sprite southSprite;
    public Sprite neutralSprite;

    public string orientationToChoose;
    public bool northValid = false;
    public bool eastValid = false;
    public bool southValid = false;
    public bool westValid = false;

    //Variables that reference needed game objects
    public BattleManager battleManager;
    //The Snap-To Node list is fed to this script from the Battle Manager's ActivateMoveMode() function. The node mouse distances are calculated inside this script.
    public Dictionary<BattleManager.CharacterPosition, Vector2> snapToNodesDictionary;
    public List<Vector2> snapToNodes;
    public List<float> nodeMouseDistances;
    public int heightIAmOver;
    public ActiveAbilityButtonScript abilityIAmCursingFor;

    private void Awake()
    {
        battleManager = FindObjectOfType<BattleManager>();
        nodeMouseDistances = new List<float>();
        neutralSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        if (snapToNodesDictionary == null)
        {
            snapToNodesDictionary = new Dictionary<BattleManager.CharacterPosition, Vector2>();
        }

        if (snapToNodes == null)
        {
            snapToNodes = new List<Vector2>();
        }

        snapToListBuilt = false;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToChooseAbilityLocus && !isSnapped)
        {
            //This condition is met after clicking on an active ability from the Act menu but before clicking on a target square.
            //The cursor Anchor follows the mouse until it hits a snap node.
            var VectorToReach = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
            transform.position = VectorToReach;
            return;
        }
        else if (readyToChooseAbilityLocus && isSnapped)
        {
            //This condition is met after clicking on an ability and becoming snapped to the nodes.
            //At this point the user clicks on a square to choose the target for the ability.
            //Once he does so the ability preview opens up, and the Cursor Anchor goes away.
            SnapToNearestNodeToMouse();
            if (Input.GetMouseButtonDown(0))
            {
                var gridIAmOn = battleManager.terrainWorldToGridDictionary[UnHeightAdjustASnapToNode(transform.position)];
                battleManager.ConsiderAnAction(abilityIAmCursingFor.myActiveAbility, battleManager.focusedMech, gridIAmOn);
                InitializeMe();
                this.gameObject.SetActive(false);
            }
            return;
        }
        if (!isSnapped && !readyToChooseOrientation && !waiting && !readyToConfirm)
        {
            //When we first initialize, the cursor anchor follows the mouse frame-by-frame.
            //In order to initialize, we need to take the snapToNodes Dictionary, exported from the Battle Manager, and unpack the Vector2 list which is the actual nodes.
            var VectorToReach = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
            transform.position = VectorToReach;
            if (!snapToListBuilt)
            {
                foreach (Vector2 worldLoc in snapToNodesDictionary.Values)
                {
                    if (!snapToNodes.Contains(worldLoc))
                    {
                        snapToNodes.Add(worldLoc);
                    }
                    
                }
                snapToListBuilt = true;
            }
            if (snapToListBuilt)
            {
                if (snapToNodes.Count == 1)
                {
                    SnapToNearestNodeToMouse();
                    readyToChooseOrientation = true;
                }
            }
        }
        else if (isSnapped)
        {

            SnapToNearestNodeToMouse();
            if (Input.GetMouseButtonDown(0))
            {
                waiting = true;
                isSnapped = false;
                StartCoroutine(WaitThenReadyForOrientation(0.2f));
            }
        }
        else if (waiting)
        {
            if (!validDirectionsDetermined)
            {
                DetermineValidDirections();
            }
            DecideWhatWayToFace(northValid, eastValid, southValid, westValid);
        }
        else if (readyToChooseOrientation)
        {
            if (!validDirectionsDetermined)
            {
                DetermineValidDirections();
            }
            DecideWhatWayToFace(northValid, eastValid, southValid, westValid);
            if (Input.GetMouseButtonDown(0) || OnlyOneValidDirection())
            {
                if (OnlyOneValidDirection())
                {
                    //Debug.Log("We think there is only one valid direction and it is " + orientationToChoose);
                }
                readyToConfirm = true;
                battleManager.ConsiderAMove(ExportPositionDataToBattleManager());
                readyToChooseOrientation = false;
            }
        }
        else if (readyToConfirm)
        {

        }
    }

    public bool OnlyOneValidDirection()
    {
        if (northValid && !eastValid && !southValid && !westValid)
        {
            return true;
        }
        else if (!northValid && eastValid && !southValid && !westValid)
        {
            return true;
        }
        else if (!northValid && !eastValid && southValid && !westValid)
        {
            return true;
        }
        else if (!northValid && !eastValid && !southValid && westValid)
        {
            return true;
        }
        else return false;
    }

    public void SnapToNearestNodeToMouse()
    {
        snapToNodes.Sort(SortByProximityToMouse);
        var snapToPlace = HeightAdjustASnapToNode(snapToNodes[0]);
        this.transform.position = new Vector3(snapToPlace.x, snapToPlace.y, this.transform.position.z);

    }

    public static int SortByProximityToMouse(Vector2 firstVector, Vector2 secondVector)
    {
        var mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        var distanceOne = Vector2.SqrMagnitude(firstVector - mousePosition);
        var distanceTwo = Vector2.SqrMagnitude(secondVector - mousePosition);
        if (distanceOne < distanceTwo)
        {
            return -1;
        }
        else if (distanceOne > distanceTwo)
        {
            return 1;
        }
        else return 0;
    }

    public void DetermineValidDirections()
    {
        bool foundAValidCharPos = false;
        //Debug.Log("Determining valid directions.");
        foreach (BattleManager.CharacterPosition charPos in snapToNodesDictionary.Keys)
        {
            if (snapToNodesDictionary[charPos] == UnHeightAdjustASnapToNode(new Vector2(transform.position.x, transform.position.y)))
            {
                foundAValidCharPos = true;
                if (charPos.orientation == "North")
                {
                    northValid = true;
                }
                else if (charPos.orientation == "East")
                {
                    eastValid = true;
                }
                else if (charPos.orientation == "South")
                {
                    southValid = true;
                }
                else if (charPos.orientation == "West")
                {
                    westValid = true;
                }
                if (!northValid && !eastValid && !southValid && !westValid)
                {
                    Debug.Log("We are at a node that appears to have no valid orientations!");
                    Debug.Log("My position is " + transform.position.x + ", " + transform.position.y);
                    Debug.Log("Outputting a list of snap-to nodes, try and see if one matches... If not, how did I wind up here?");
                    foreach (Vector2 snapToNode in snapToNodesDictionary.Values)
                    {
                        Debug.Log(snapToNode);
                    }
                }
            }
        }
        if (!foundAValidCharPos)
        {
            Debug.Log("We never found a character position that matches this position!");
            Debug.Log("My position is " + transform.position.x + ", " + transform.position.y);
            Debug.Log("Outputting a list of snap-to nodes, try and see if one matches... If not, how did I wind up here?");
            foreach (Vector2 snapToNode in snapToNodesDictionary.Values)
            {
                Debug.Log(snapToNode);
            }
        }
        //Debug.Log("We have determined valid directions.");
        validDirectionsDetermined = true;
    }

    public void DecideWhatWayToFace(bool northValid, bool eastValid, bool southValid, bool westValid)
    {
        var mouseRelativeToMe = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        if (mouseRelativeToMe.x > 0 && mouseRelativeToMe.y > 0 && orientationToChoose != "North" && northValid)
        {
            FaceNorth();
        }
        else if (mouseRelativeToMe.x > 0 && mouseRelativeToMe.y < 0 && orientationToChoose != "East" && eastValid)
        {
            FaceEast();
        }
        else if (mouseRelativeToMe.x < 0 && mouseRelativeToMe.y < 0 && orientationToChoose != "South" && southValid)
        {
            FaceSouth();
        }
        else if (mouseRelativeToMe.x < 0 && mouseRelativeToMe.y > 0 && orientationToChoose != "West" && westValid)
        {
            FaceWest();
        }
        else if (OnlyOneValidDirection())
        {
            if (northValid)
            {
                FaceNorth();
            }
            else if (eastValid)
            {
                FaceEast();
            }
            else if (southValid)
            {
                FaceSouth();
            }
            else if (westValid)
            {
                FaceWest();
            }
        }
    }

    public void FaceNorth()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = northSprite;
        orientationToChoose = "North";
    }

    public void FaceEast()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = eastSprite;
        orientationToChoose = "East";
    }

    public void FaceSouth()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = southSprite;
        orientationToChoose = "South";
    }

    public void FaceWest()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = westSprite;
        orientationToChoose = "West";
    }

    public void FaceNeutral()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = neutralSprite;
        orientationToChoose = null;
    }



    public BattleManager.CharacterPosition ExportPositionDataToBattleManager()
    {
        BattleParticipant characterMoved = battleManager.focusedMech;
        string orientation = orientationToChoose;
        if (orientationToChoose == null || orientationToChoose == "")
        {
            Debug.Log("The attempted orientation is " + orientationToChoose);
            Debug.Log("Northvalid is " + northValid);
            Debug.Log("Eastvalid is " + eastValid);
            Debug.Log("Southvalid is " + southValid);
            Debug.Log("Westvalid is " + westValid);
            Debug.Log("ValidDirectionsDetermined is " + validDirectionsDetermined);
        }
        List<Vector2Int> gridSquaresOccupied = new List<Vector2Int>();
        var unHeightAdjustedPositionVector = UnHeightAdjustASnapToNode(new Vector2(transform.position.x, transform.position.y));
        if (characterMoved.gridPositionsIAmOver.Count == 1)
        {
            gridSquaresOccupied.Add(battleManager.terrainWorldToGridDictionary[unHeightAdjustedPositionVector]);
        }
        else if (characterMoved.gridPositionsIAmOver.Count == 4)
        {
            var currentSpot = unHeightAdjustedPositionVector;
            var eastStep = new Vector2(0.5f, 0);
            var northStep = new Vector2(0, 0.25f);
            var southStep = new Vector2(0, -0.25f);
            var westStep = new Vector2(-0.5f, 0);
            if (!battleManager.terrainWorldToGridDictionary.ContainsKey(currentSpot + eastStep))
            {
                Debug.Log("We couldn't find the square we wanted, had to search.");
                gridSquaresOccupied.Add(battleManager.SearchForTheNearestGridSquare(currentSpot + eastStep));
            }
            else
            {
                gridSquaresOccupied.Add(battleManager.terrainWorldToGridDictionary[currentSpot + eastStep]);
            }
            if (!battleManager.terrainWorldToGridDictionary.ContainsKey(currentSpot + northStep))
            {
                Debug.Log("We couldn't find the square we wanted, had to search.");
                gridSquaresOccupied.Add(battleManager.SearchForTheNearestGridSquare(currentSpot + northStep));
            }
            else
            {
                gridSquaresOccupied.Add(battleManager.terrainWorldToGridDictionary[currentSpot + northStep]);
            }
            if (!battleManager.terrainWorldToGridDictionary.ContainsKey(currentSpot + southStep))
            {
                Debug.Log("We couldn't find the square we wanted, had to search.");
                gridSquaresOccupied.Add(battleManager.SearchForTheNearestGridSquare(currentSpot + southStep));
            }
            else
            {
                gridSquaresOccupied.Add(battleManager.terrainWorldToGridDictionary[currentSpot + southStep]);
            }
            if (!battleManager.terrainWorldToGridDictionary.ContainsKey(currentSpot + westStep))
            {
                Debug.Log("We couldn't find the square we wanted, had to search.");
                gridSquaresOccupied.Add(battleManager.SearchForTheNearestGridSquare(currentSpot + westStep));
            }
            else
            {
                gridSquaresOccupied.Add(battleManager.terrainWorldToGridDictionary[currentSpot + westStep]);
            }
        }
        else if (characterMoved.gridPositionsIAmOver.Count == 9)
        {

        }
        return new BattleManager.CharacterPosition(characterMoved, gridSquaresOccupied, orientation);
    }

    public Vector2 HeightAdjustASnapToNode(Vector2 nodeToAdjust)
    {
        float adjustFactor = battleManager.tileHeightToWorldYScaleFactor;
        if (readyToChooseAbilityLocus)
        {
            var gridIAmOver = battleManager.terrainWorldToGridDictionary[nodeToAdjust];
            heightIAmOver = battleManager.terrainTileDictionary[gridIAmOver].height;
        }
        else
        {
            foreach (BattleManager.CharacterPosition cp in snapToNodesDictionary.Keys)
            {
                if (snapToNodesDictionary[cp] == nodeToAdjust)
                {
                    heightIAmOver = battleManager.terrainTileDictionary[cp.gridPositions[0]].height;
                    break;
                }
            }
        }

        Vector2 adjustedNode = new Vector2(nodeToAdjust.x, (nodeToAdjust.y + adjustFactor * heightIAmOver));
        return adjustedNode;
    }

    public Vector2 UnHeightAdjustASnapToNode(Vector2 nodeToUnadjust)
    {
        float adjustFactor = battleManager.tileHeightToWorldYScaleFactor;
        Vector2 adjustedNode = new Vector2(nodeToUnadjust.x, (nodeToUnadjust.y - adjustFactor * heightIAmOver));
        return adjustedNode;
    }




    public void OnMouseDown()
    {
        if (isSnapped)
        {

        }
        if (readyToChooseOrientation)
        {
            
        }
    }

    IEnumerator WaitThenReadyForOrientation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        waiting = false;
        readyToChooseOrientation = true;
        yield return null;
    }
    

    public void InitializeMe()
    {
        //Debug.Log("Initializing the Cursor Anchor.");
        isSnapped = false;
        readyToConfirm = false;
        abilityIAmCursingFor = null;
        transform.position = new Vector2(-5, 3.75f);
        transform.localScale = new Vector3(1, 1, 1);
        snapToListBuilt = false;
        validDirectionsDetermined = false;
        northValid = false;
        eastValid = false;
        southValid = false;
        westValid = false;
        readyToChooseAbilityLocus = false;
        readyToChooseOrientation = false;
        FaceNeutral();
        snapToNodes.Clear();
        snapToNodesDictionary.Clear();
        nodeMouseDistances.Clear();
        for (int i = 0; i < 20; i++)
        {
            if (transform.childCount == i)
            {
                break;
            }
            else
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

    }

}
