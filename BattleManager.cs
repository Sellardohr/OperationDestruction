using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class BattleManager : MonoBehaviour
{

    //Variables for working with the battle grid
    public List<TerrainTile> terrainTileScriptsInGrid;
    public List<Vector2> problemTilePositions;
    public List<Vector2> terrainTilePositions;
    public Dictionary<CharacterPosition, Vector2> snapToPositions;
    public List<Vector2Int> terrainTileGridPositions;
    public List<Vector2Int> subgridGridPositions;
    public Dictionary<Vector2, Vector2Int> terrainWorldToGridDictionary;
    public Dictionary<Vector2Int, Vector2> terrainGridToWorldDictionary;
    public Dictionary<Vector2, Vector2Int> subgridWorldToGridDictionary;
    public Dictionary<Vector2Int, Vector2> subgridGridToWorldDictionary;
    public Dictionary<Vector2Int, TerrainTile> terrainTileDictionary;
    public Dictionary<TerrainTile, Vector2Int> terrainGridFromTileDictionary;
    public Vector2 tileOriginOffset;
    public float tileHeightToWorldYScaleFactor = 0.125f;

    //Variables that determine the current scenario
    public Scenario thisScenario;
    public WinCondition winConditionsForThisBattle;

    //Variables for working with Active Abilities
    public ProposedAction proposedActionToConsider;
    public RefinedProposedAction refinedActionToConsider;
    public static Sprite defaultCharacterImage;

    //Variables that help a mech move by storing position and path data
    public MovePath movePathToExecute;
    public Dictionary<CharacterPosition, MovePath> characterPathDictionary;
    public GameObject previewGhost;
    public GameObject unGhost;
    public List<Vector2Int> moveableSquares;
    //This fudge factor is used when identifying where tiles lie on the grid. In case a tile gets nudged a tiny bit in the editor by accident, we allow a little inaccuracy.
    public float tileToGridFudgeFactor = 0.0025f;

    //Variables that store information about the Combat Animation system
    public GameObject combatAnimationOverlay;

    //The battle anchor prefab will be set in the inspector:
    public GameObject battleAnchor;

    //Variables that organize save data for Protocols
    //When we initialize a battle, e.g. from the main Robot base, we can serialize and save a List of participants then load it up in this script to Instantiate.
    public List<ProtocolSaveData> protocolsInBattle;
    public List<BattleParticipant> battleParticipantsInProtocol;
    public ProtocolSaveSlotScript[] protocolSaveSlotList;
    public string protocolSlotToSaveOrLoad = "1";
    public string protocolRootFilepath;
    public string protocolFilepathToSaveOrLoadToOrFrom;
    //The Battle Anchor prefab is set in the inspector.

    //Variables that organize the Protocol Anchors for the Protocol start positions.
    public ProtocolAnchor[] protocolAnchorsInMap;
    public List<List<ProtocolAnchor>> protocolAnchorsByTeam;

    //Variables that reference the game flow
    public int playerTeamNumber = 0;
    //0 = my turn, 1 = first enemy, 2 = 2nd enemy, etc.
    public int whoseTurn = 0;
    public int numberOfTeams;
    public bool readyToMove = false;
    public bool readyToAct = false;
    public bool readyToEndTurn = false;
    public bool sureIWantToEndTurn = false;
    public bool waiting = false;
    public bool shoppingForAnActiveAbility = false;
    public bool consideringAProposedAbility = false;
    public bool scanningTheBattlefield = false;
    public int turnCounter = 1;
    public bool playerControlAvailable = true;
    public BattleParticipant focusedMech;

    //Variables that reference needed in-game objects
    public Camera mainCamera;
    public Camera focusCamera;
    public TextMeshProUGUI focusWindowText;
    public GameObject cursorAnchor;
    public CursorAnchorScript cursorAnchorScript;
    public GameObject confirmButtons;
    public GameObject statBarsPanel;
    public GameObject moveBar;
    public TextMeshProUGUI moveBarText;
    public GameObject cpuBar;
    public TextMeshProUGUI cpuBarText;
    public GameObject turnEndWarning;
    public GameObject whoseTurnTextObject;
    public TextMeshProUGUI whoseTurnText;
    public GameObject activeAbilitiesMenu;
    public List<GameObject> abilitiesInAbilityMenu;
    public ActiveAbilityButtonScript abilityToConsider;
    public Coroutine anchorPulseCoroutine;
    public GameObject battleActionPreview;
    public Vector3 battleActionPreviewPosition;
    public MechPlayerManager mechPlayerManager;
    public GameObject battleObjectivesPanel;
    public TextMeshProUGUI battleObjectivesText;
    //The template is set in the Inspector.
    public GameObject activeAbilityButtonTemplate;

    public List<List<BattleParticipant>> battleParticipantsByTeam;
    public Dictionary<BattleParticipant, List<ActiveAbility>> activeAbilityParticipantDictionary;

    public virtual void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        focusCamera = GameObject.Find("Focus Camera").GetComponent<Camera>();
        focusCamera.enabled = false;
        focusWindowText = GameObject.Find("Focus Info Text").GetComponent<TextMeshProUGUI>();
        confirmButtons = GameObject.Find("Confirm Buttons");
        statBarsPanel = GameObject.Find("Stat Bars Panel");
        moveBar = GameObject.Find("Move Bar");
        cpuBar = GameObject.Find("CPU Bar");
        activeAbilitiesMenu = GameObject.Find("Active Abilities Menu");
        abilitiesInAbilityMenu = new List<GameObject>();
        activeAbilitiesMenu.SetActive(false);
        turnEndWarning = GameObject.Find("Turn End Warning");
        whoseTurnTextObject = GameObject.Find("Whose Turn Text");
        whoseTurnText = GameObject.Find("Whose Turn Text").GetComponent<TextMeshProUGUI>();
        battleActionPreview = GameObject.Find("Battle Preview");
        combatAnimationOverlay = GameObject.Find("Combat Animation Overlay");
        mechPlayerManager = GameObject.FindObjectOfType<MechPlayerManager>();
        combatAnimationOverlay.SetActive(false);
        whoseTurnTextObject.SetActive(false);
        turnEndWarning.SetActive(false);
        moveBarText = moveBar.GetComponentInChildren<TextMeshProUGUI>();
        cpuBarText = cpuBar.GetComponentInChildren<TextMeshProUGUI>();
        confirmButtons.SetActive(false);
        cursorAnchor = FindObjectOfType<CursorAnchorScript>().gameObject;
        cursorAnchorScript = cursorAnchor.GetComponent<CursorAnchorScript>();
        battleObjectivesPanel = GameObject.Find("Battle Objectives Panel");
        battleObjectivesText = battleObjectivesPanel.GetComponentInChildren<TextMeshProUGUI>();
        battleObjectivesPanel.SetActive(false);
        cursorAnchor.SetActive(false);
        statBarsPanel.SetActive(false);
        problemTilePositions = new List<Vector2>();
        subgridGridPositions = new List<Vector2Int>();
        moveableSquares = new List<Vector2Int>();
        snapToPositions = new Dictionary<CharacterPosition, Vector2>();
        terrainWorldToGridDictionary = new Dictionary<Vector2, Vector2Int>();
        terrainGridToWorldDictionary = new Dictionary<Vector2Int, Vector2>();
        subgridWorldToGridDictionary = new Dictionary<Vector2, Vector2Int>();
        subgridGridToWorldDictionary = new Dictionary<Vector2Int, Vector2>();
        characterPathDictionary = new Dictionary<CharacterPosition, MovePath>();
        terrainTileDictionary = new Dictionary<Vector2Int, TerrainTile>();
        terrainGridFromTileDictionary = new Dictionary<TerrainTile, Vector2Int>();
        protocolAnchorsInMap = GameObject.FindObjectsOfType<ProtocolAnchor>();
        battleParticipantsByTeam = new List<List<BattleParticipant>>();
        activeAbilityParticipantDictionary = new Dictionary<BattleParticipant, List<ActiveAbility>>();
    }

    public virtual void Start()
    {
        InitializeProtocolSaveLoadFilepath();
        terrainTileScriptsInGrid = FindAllTerrainTiles();
        DeactivateInertTiles(terrainTileScriptsInGrid);
        AssignTerrainTileScriptsTheirHeights(terrainTileScriptsInGrid);
        terrainTilePositions = ReadTheBattleGridSquares(terrainTileScriptsInGrid);
        SetUpTheGridPositions(terrainTilePositions, false);
        AssignTilesToTheGrid(terrainTileScriptsInGrid);
        protocolAnchorsByTeam = OrganizeTheProtocolAnchorsByTeam(protocolAnchorsInMap);
        AssignProtocolAnchorsOriginalGridSquares(protocolAnchorsByTeam);
        thisScenario = LoadTheScenario();
        winConditionsForThisBattle = thisScenario.winCondition;
        PopulateTheBattlefield(protocolAnchorsByTeam, thisScenario);
        ClearTheProtocolAnchors();
        battleParticipantsByTeam = FindAllParticipants();
        activeAbilityParticipantDictionary = FindParticipantActiveAbilities();
        InitializeDefaultPortraitSprite();
        InitializeTheBattlePreview();
        //Battle specific functions here...
        StartTurn();
    }

    //
    //Begin interoperable UI button scripts
    //
    //These scripts govern the behavior of UI buttons that are context-sensitive and reference the state of game flow.
    //

    public void ConfirmButton()
    {
        if (readyToMove)
        {
            ExecuteMove();
            FocusOnThis(unGhost.gameObject);
        }
        else if (readyToEndTurn)
        {
            sureIWantToEndTurn = true;
            readyToEndTurn = false;
            confirmButtons.SetActive(false);
            turnEndWarning.SetActive(false);
            EndTurn();
        }
        else if (shoppingForAnActiveAbility)
        {
            //This is what happens if you click the confirm button while shopping for an Active Ability, before choosing an ability locus.
            //At the moment this is glitchy because the cursor anchor will register a click, and an ability locus will be immediately determined. Perhaps this is ok.
        }
        else if (consideringAProposedAbility)
        {
            StartCoroutine(ExecuteAnAction(refinedActionToConsider));
        }
        else if (scanningTheBattlefield)
        {
            
        }
    }

    public void CancelButton()
    {
        if (readyToMove)
        {
            AbortMove();
        }
        else if (readyToEndTurn)
        {
            sureIWantToEndTurn = false;
            readyToEndTurn = false;
            confirmButtons.SetActive(false);
            turnEndWarning.SetActive(false);
        }
        else if (shoppingForAnActiveAbility)
        {
            CancelAnAction();
        }
        else if (consideringAProposedAbility)
        {
            abilityToConsider.AbortAbility();
            StartCoroutine(battleActionPreview.GetComponent<BattleActionPreviewScript>().SlideOutThePreviewContent());
            //We used to just call "CancelAnAction()" here but it also sets the preview to null, so we have to cut that line out.
            if (abilityToConsider != null)
            {
                abilityToConsider.AbortAbility();
            }
            activeAbilitiesMenu.SetActive(false);
            shoppingForAnActiveAbility = false;
            consideringAProposedAbility = false;
            proposedActionToConsider = null;
            refinedActionToConsider = null;
            confirmButtons.SetActive(false);
        }
        else if (scanningTheBattlefield)
        {
            EndScanningTheBattlefield();
        }
    }

    //
    //End interoperable UI scripts
    //




    //
    //Begin turn-order flow scripts
    //
    //These scripts govern the passage of turns.
    //

    public void EndTurn()
    {
        if (!CheckForTurnEndWarning() || sureIWantToEndTurn)
        {
            foreach (List<BattleParticipant> list in battleParticipantsByTeam)
            {
                if (list[0].myTeam == whoseTurn)
                {
                    foreach (BattleParticipant bp in list)
                    {
                        bp.ResetMyTurn();
                    }
                }
            }
            if (whoseTurn == (numberOfTeams - 1))
            {
                whoseTurn = 0;
            }
            else
            {
                whoseTurn++;
            }
            sureIWantToEndTurn = false;
            playerControlAvailable = false;
            StartTurn();
        }
        else
        {
            confirmButtons.SetActive(true);
            turnEndWarning.SetActive(true);
            readyToEndTurn = true;
        }
    }

    public bool CheckForTurnEndWarning()
    {
        foreach (List<BattleParticipant> bpList in battleParticipantsByTeam)
        {
            if (bpList[0].myTeam == whoseTurn)
            {
                foreach (BattleParticipant bp in bpList)
                {
                    if (bp.currentMove == bp.movePower || bp.currentCpu == (bp.cpuMax = bp.cpuUsed))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void StartTurn()
    {
        //
        //Functions which update information to be called every turn
        //
        AssignPassivesToThoseAffected();
        //

        //
        //Functions that bifurcate depending on whose turn it is
        //
        if (whoseTurn == playerTeamNumber)
        {
            playerControlAvailable = false;
            StartCoroutine(FlashWhoseTurn());
            playerControlAvailable = true;
        }
        else
        {
            StartCoroutine(ExecuteAITurn());
        }
        //
    }

    public IEnumerator ExecuteAITurn()
    {
        playerControlAvailable = false;
        StartCoroutine(FlashWhoseTurn());
        yield return new WaitForSeconds(4);
        //Other AI turn functions go here...
        sureIWantToEndTurn = true;
        EndTurn();
        yield return null;
    }

    public IEnumerator FlashWhoseTurn()
    {
        //Debug.Log("Flashing whose turn...");
        waiting = true;
        var blue = new Color(0, 0, 1);
        var red = new Color(1, 0, 0);
        if (CheckForWinConditions())
        {
            whoseTurnTextObject.SetActive(true);
            whoseTurnText.color = blue;
            whoseTurnText.text = "Victory Secured";
        }
        else if (CheckForLossConditions())
        {
            whoseTurnTextObject.SetActive(true);
            whoseTurnText.color = red;
            whoseTurnText.text = "Defeat and Shame";
        }
        else if (whoseTurn == playerTeamNumber)
        {
            whoseTurnTextObject.SetActive(true);
            whoseTurnText.color = blue;
            whoseTurnText.text = "Player Turn";

        }
        else
        {
            whoseTurnTextObject.SetActive(true);
            whoseTurnText.color = red;
            whoseTurnText.text = "Enemy Turn";
        }
        yield return new WaitForSeconds(1);
        whoseTurnTextObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        whoseTurnTextObject.SetActive(true);
        yield return new WaitForSeconds(1);
        whoseTurnTextObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        whoseTurnTextObject.SetActive(true);
        yield return new WaitForSeconds(1);
        whoseTurnTextObject.SetActive(false);
        //Debug.Log("Coroutine complete");
        waiting = false;
        yield return null;
    }



    //
    //End turn-order flow scripts
    //




    //
    //Begin Battle UI functions
    //
    //These functions control the battle UI elements.
    //

    public void FocusOnThis(GameObject objectToFocus)
    {
        //This function controls the behavior of the focus camera and focus text. If we click on a mech, we focus that mech and get its stats, and if it can move we can control it. If we are searching terrain tiles and we click on one, we get information from it.
        //This function gets called by every battle anchor when clicked on.
        if (!focusCamera.isActiveAndEnabled)
        {
            focusCamera.enabled = true;
        }
        if (anchorPulseCoroutine != null)
        {
            AnimationManager.abortPulseTheObjectSize = true;
        }
        //If we are currently looking at active abilities for a focused mech and we click on a different mech, or similarly if we're trying to move, we want special behavior -- specifically cancelling.
        if (shoppingForAnActiveAbility)
        {
            CancelButton();
        }
        if (readyToMove)
        {
            return;
        }
        if (objectToFocus.GetComponent<BattleParticipant>() != null)
        {

            var bpScript = objectToFocus.GetComponent<BattleParticipant>();
            focusedMech = bpScript;
            var anchor = objectToFocus.GetComponentInChildren<BattleAnchorScript>();
            var panBottom = new Vector2(anchor.gameObject.transform.position.x, anchor.gameObject.transform.position.y);
            panBottom = panBottom + new Vector2(0, 0.15f);
            var panTop = panBottom + new Vector2(0, 1.5f);
            focusCamera.gameObject.GetComponent<FocusCameraScript>().focusBottom = panBottom;
            focusCamera.gameObject.GetComponent<FocusCameraScript>().focusTop = panTop;
            if (anchor.myMoveType == "Large Quad" || anchor.myMoveType == "Large Biped")
            {
                focusCamera.orthographicSize = 0.65f;
            }
            if (anchor.myMoveType == "Walk" || anchor.myMoveType == "Biped")
            {
                panTop *= 0.66f;
                focusCamera.orthographicSize = 0.45f;
            }
            focusCamera.gameObject.GetComponent<FocusCameraScript>().SwitchFocus();
            focusWindowText.text = bpScript.characterName + "\n" + "Health: " + bpScript.currentHealth;
            anchorPulseCoroutine = StartCoroutine(AnimationManager.PulseTheObjectSize(anchor.gameObject, 33f, 1, 3));

            UpdateFocusedMoveAndCpu();
        }
    }

    public void UpdateFocusedMoveAndCpu()
    {
        statBarsPanel.SetActive(true);
        moveBarText.text = "Move: " + focusedMech.currentMove + " / " + focusedMech.movePower;
        cpuBarText.text = "CPU: " + focusedMech.currentCpu + " / " + (focusedMech.cpuMax - focusedMech.cpuUsed);
        float moveRatio = focusedMech.currentMove / focusedMech.movePower;
        float cpuRatio;
        if (focusedMech.cpuMax - focusedMech.cpuUsed != 0)
        {
            cpuRatio = focusedMech.currentCpu / (focusedMech.cpuMax - focusedMech.cpuUsed);
        }
        else
        {
            cpuRatio = 0;
        }

        if (moveRatio == 0 || focusedMech.movePower == 0)
        {
            moveRatio = 0.01f;
        }
        if (cpuRatio == 0)
        {
            cpuRatio = 0.01f;
        }

        var scaleToSet = new Vector3(1, 1, 1);
        scaleToSet.x *= moveRatio;
        moveBar.transform.localScale = scaleToSet;
        scaleToSet.x /= moveRatio;
        scaleToSet.x /= moveRatio;
        moveBarText.transform.localScale = scaleToSet;
        scaleToSet = new Vector3(1, 1, 1);
        scaleToSet *= cpuRatio;
        cpuBar.transform.localScale = scaleToSet;
        scaleToSet /= cpuRatio;
        scaleToSet /= cpuRatio;
        cpuBarText.transform.localScale = scaleToSet;

    }

    public void OpenActiveAbilityMenu()
    {
        //This function executes when you click on Act with a focused mech.
        //It activates the ability menu and creates a button for each active ability on the focused mech.

        ClearTheActiveAbilityMenu();
        activeAbilitiesMenu.SetActive(true);
        confirmButtons.SetActive(true);
        readyToMove = false;
        shoppingForAnActiveAbility = true;
        var abilitiesToLoad = new List<ActiveAbility>();
        abilitiesToLoad = activeAbilityParticipantDictionary[focusedMech];
        GameObject abilityMenuContent = GameObject.Find("Active Abilities Content");
        foreach (ActiveAbility ability in abilitiesToLoad)
        {
            var buttonToCreate = Instantiate(activeAbilityButtonTemplate, abilityMenuContent.transform);
            buttonToCreate.GetComponent<ActiveAbilityButtonScript>().myActiveAbility = ability;
            var buttonText = buttonToCreate.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "" + ability.abilityName + "\n" + "\n" + ability.cpuCost + " CPU";
            var buttonImage = buttonToCreate.transform.GetChild(1).gameObject.GetComponent<Image>();
            buttonImage.sprite = ability.abilitySprite;
            abilitiesInAbilityMenu.Add(buttonToCreate);
        }


    }

    public void ClearTheActiveAbilityMenu()
    {
        //
        //This function clears old active abilities out of the active ability menu.
        //

        foreach (GameObject aaButton in abilitiesInAbilityMenu)
        {
            aaButton.SetActive(false);
            Destroy(aaButton);
        }
        abilitiesInAbilityMenu.Clear();
    }

    public void ScanTheBattlefield()
    {
        //
        //This function executes when you click the Scan Battlefield button in the main UI.
        //It allows you to click on terrain tiles in order to examine their heights and characteristics.
        //It also allows you to click on mechs to bring up a detailed overview, including active abilities.
        //It also displays the win conditions at the top of the screen.
        //
        scanningTheBattlefield = true;

        foreach (TerrainTile tile in terrainTileDictionary.Values)
        {
            if (!tile.isOccupied)
            {
                tile.SetActive();
                tile.searchable = true;
            }
        }

        OpenTheWinConditionsView();

        confirmButtons.SetActive(true);

    }

    public void OpenTheWinConditionsView()
    {
        battleObjectivesPanel.SetActive(true);
        battleObjectivesText.text = ReadTheBattleWinConditions();
    }

    public string ReadTheBattleWinConditions()
    {
        string outputText = "Victory conditions:";
        if (winConditionsForThisBattle.defeatAllMechs)
        {
           outputText = outputText + "\n" + "Defeat all enemies";
        }
        return outputText;
    }



    public void ExecuteZoomOnTargets()
    {
        StartCoroutine(ZoomInOnTargets());
    }

    public IEnumerator ZoomInOnTargets()
    {
        yield return null;
    }

    public void EndScanningTheBattlefield()
    {
        //
        //This function exits scanning the battlefield mode, reversing all steps taken to start it.
        //

        foreach (TerrainTile tile in terrainTileDictionary.Values)
        {
            if (!tile.isOccupied)
            {
                tile.SetActive();
                tile.searchable = true;
            }
        }

        battleObjectivesPanel.SetActive(false);

        scanningTheBattlefield = false;

        confirmButtons.SetActive(false);
    }

    //
    //End Battle UI functions
    //


    //
    //Start Move functions
    //
    //The following functions allow you to control the focused mech during your turn.
    //

    public void ActivateMoveMode()
    {

        //This function is called when the player clicks the Move button. 
        //
        //It exports a Dictionary of Character Positions with Vector2s for snap-to-nodes to the Cursor Anchor, which it activates.

        if (focusedMech == null || focusedMech.myTeam != playerTeamNumber)
        {
            return;
        }

        readyToMove = true;

        snapToPositions.Clear();

        //These functions are written with the strategy of simply finding all possible paths from the starting square.
        var startPos = new CharacterPosition(focusedMech, focusedMech.gridPositionsIAmOver, focusedMech.myOrientation);
        //Debug.Log("The starting orientation is " + focusedMech.myOrientation + " which we record as " + startPos.orientation);

        //Make a dictionary of destinations and paths:
        var untrimmedPathDictionary = FindAllPaths2(startPos);
        //Trim out the uneven heights:
        var pathDictionary = TrimOutUnequalHeights(untrimmedPathDictionary);
        characterPathDictionary = pathDictionary;

        //Sort the destinations into a List<> and prepare them to receive a character move.
        var gridDestinationList = new List<Vector2Int>();


        foreach (CharacterPosition charPos in pathDictionary.Keys)
        {
            foreach (Vector2Int gridPos in charPos.gridPositions)
            {
                gridDestinationList.Add(gridPos);
            }
        }

        moveableSquares = gridDestinationList;

        foreach (Vector2Int grid in gridDestinationList)
        {
            terrainTileDictionary[grid].PrepareMeForMechMove();
        }

        snapToPositions = FindSnapToNodesForMechMove(pathDictionary);

        //I was geting a bug where the cursor anchor would show up at the pulsed size caused by the Animation Manager script. To address that, we store the original anchor size in the Animation Manager and restore it here before we proceed.
        AnimationManager.abortPulseTheObjectSize = true;
        cursorAnchor.SetActive(true);
        cursorAnchor.transform.localScale = AnimationManager.initialScaleForPulsing;
        cursorAnchor.GetComponent<CursorAnchorScript>().snapToNodesDictionary = snapToPositions;


        //StartCoroutine(WaitThenUnHighlight(gridDestinationList, 10));
    }

    public void ConsiderAMove(CharacterPosition endPos)
    {
        //This function gets called by the Cursor Anchor when moving. In particular, once a rotation has been locked in, the Cursor Anchor figures out the desired Character Position and calls this function. It automatically flows into the DrawAGhostMech function.
        confirmButtons.SetActive(true);
        //DebugLogACharacterPosition(endPos);
        movePathToExecute = IdentifyAPath(endPos);
        var ghostAtTarget = DrawAGhostMechAtTarget(focusedMech.gameObject, movePathToExecute);
    }

    public void DebugLogACharacterPosition(CharacterPosition positionToDebug)
    {
        Debug.Log("Character position info is as follows:");
        Debug.Log("The grid list has " + positionToDebug.gridPositions.Count + " entries.");
        foreach (Vector2Int grid in positionToDebug.gridPositions)
        {
            Debug.Log("One entry is " + grid);
        }
        Debug.Log("The orientation is " + positionToDebug.orientation);
    }

    public GameObject DrawAGhostMechAtTarget(GameObject mechToDraw, MovePath pathToTrace)
    {
        //This function previews a move.
        //For now it Instantiates a copy of the target mech at the end location and sets the original mech to be highly transparent.
        //Later on we'll draw a dotted line through the entire path the mech will walk to reach there.

        //First we find the world coords to instantiate to.
        //We also need to adjust the y value based on the height of the tile we're Instantiating to. 
        Vector2 vectorToSpawnTo = new Vector2(0, 0);
        if (pathToTrace.totalEndPos.gridPositions.Count == 1)
        {
            vectorToSpawnTo = terrainGridToWorldDictionary[pathToTrace.totalEndPos.gridPositions[0]];
            int targetHeight = terrainTileDictionary[pathToTrace.totalEndPos.gridPositions[0]].height;
            //Debug.Log("The target height is " + targetHeight);
            vectorToSpawnTo += new Vector2(0, targetHeight * tileHeightToWorldYScaleFactor);
        }
        if (pathToTrace.totalEndPos.gridPositions.Count == 4)
        {
            vectorToSpawnTo = FindTheCenterOfTheseGridSquares(pathToTrace.totalEndPos.gridPositions);
            int targetHeight = terrainTileDictionary[pathToTrace.totalEndPos.gridPositions[0]].height;
            //Debug.Log("The target height is " + targetHeight);
            vectorToSpawnTo += new Vector2(0, targetHeight * tileHeightToWorldYScaleFactor);
        }
        if (pathToTrace.totalEndPos.gridPositions.Count == 9)
        {

        }
        var ghost = Instantiate(mechToDraw, new Vector3(vectorToSpawnTo.x, vectorToSpawnTo.y, mechToDraw.transform.position.z), mechToDraw.transform.rotation);
        ghost.transform.Translate(-ghost.GetComponentInChildren<BattleAnchorScript>().transform.localPosition);
        unGhost = focusedMech.gameObject;
        previewGhost = ghost;

        //Before we're done we still need to set the ghost's anchor orientation and adjust transparencies.
        string directionToFace = pathToTrace.totalEndPos.orientation;
        if (directionToFace == "North")
        {
            ghost.GetComponentInChildren<BattleAnchorScript>().FaceNorth();
        }
        if (directionToFace == "East")
        {
            ghost.GetComponentInChildren<BattleAnchorScript>().FaceEast();
        }
        if (directionToFace == "South")
        {
            ghost.GetComponentInChildren<BattleAnchorScript>().FaceSouth();
        }
        if (directionToFace == "West")
        {
            ghost.GetComponentInChildren<BattleAnchorScript>().FaceWest();
        }

        SetAMechTransparency(ghost, 0.66f);
        SetAMechTransparency(mechToDraw, 0.33f);

        ghost.GetComponent<BattleParticipant>().currentMove -= pathToTrace.totalMoveCost;
        FocusOnThis(ghost);



        return ghost;
    }

    public void SetAMechTransparency(GameObject mechToSet, float alphaToSet)
    {
        var spriteRenderers = mechToSet.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in spriteRenderers)
        {
            if (sprite.gameObject.GetComponent<BattleAnchorScript>() == null)
            {
                var color = sprite.color;
                color.a = alphaToSet;
                sprite.color = color;
            }

        }
    }

    public MovePath IdentifyAPath(CharacterPosition endPos)
    {
        //This function selects the Path that leads to a given Character Position out of the paths we found in the FindAllPaths() process.
        foreach (CharacterPosition pathEndPos in characterPathDictionary.Keys)
        {
            if (CompareTwoCharacterPositions(pathEndPos, endPos))
            {
                return characterPathDictionary[pathEndPos];
            }
        }
        Debug.Log("Could not identify a path to this Character Position!");
        return null;
    }

    public void ExecuteMove()
    {
        unGhost.transform.position = previewGhost.transform.position;
        Destroy(previewGhost);
        TellSquaresAboutAMove(movePathToExecute);
        snapToPositions.Clear();
        characterPathDictionary.Clear();
        cursorAnchor.GetComponent<CursorAnchorScript>().InitializeMe();
        cursorAnchor.SetActive(false);
        foreach (Vector2Int gridSquare in moveableSquares)
        {
            var tile = terrainTileDictionary[gridSquare];
            tile.UnHighlightMe();
            tile.SetInactive();
        }
        moveableSquares.Clear();
        confirmButtons.SetActive(false);
        SetAMechTransparency(unGhost.gameObject, 1);
        FocusOnThis(unGhost.gameObject);
        UpdateFocusedMoveAndCpu();
        readyToMove = false;
    }

    public void TellSquaresAboutAMove(MovePath pathToExecute)
    {
        //This function updates all of the scripts of the squares and participant to tell them what's happened in a just-executed move.
        var startSquares = pathToExecute.totalStartPos.gridPositions;
        var endSquares = pathToExecute.totalEndPos.gridPositions;
        var movingMech = pathToExecute.totalEndPos.character;
        var mechAnchor = movingMech.myBattleAnchor;
        foreach (Vector2Int grid in startSquares)
        {
            var tile = terrainTileDictionary[grid];
            tile.ClearMe();
            tile.SetInactive();
        }
        foreach (Vector2Int grid in endSquares)
        {
            var tile = terrainTileDictionary[grid];
            tile.ClearMe();
            tile.SetInactive();
            tile.isOccupied = true;
            tile.whatOccupiesMe = movingMech.gameObject;
        }
        movingMech.gridPositionsIAmOver = endSquares;
        movingMech.myOrientation = pathToExecute.totalEndPos.orientation;
        movingMech.currentMove -= pathToExecute.totalMoveCost;
        mechAnchor.gridPositionsIAmOver = endSquares;
        mechAnchor.UpdateMyOrientation();
    }

    public void AbortMove()
    {
        Destroy(previewGhost);
        FocusOnThis(unGhost);
        readyToMove = false;
        snapToPositions.Clear();
        characterPathDictionary.Clear();
        cursorAnchor.GetComponent<CursorAnchorScript>().InitializeMe();
        cursorAnchor.SetActive(false);
        foreach (Vector2Int gridSquare in moveableSquares)
        {
            var tile = terrainTileDictionary[gridSquare];
            tile.UnHighlightMe();
            tile.SetInactive();
        }
        moveableSquares.Clear();
        confirmButtons.SetActive(false);
        SetAMechTransparency(focusedMech.gameObject, 1);
    }

    public Dictionary<CharacterPosition, Vector2> FindSnapToNodesForMechMove(Dictionary<CharacterPosition, MovePath> movePathDictionary)
    {
        var listToReturn = new Dictionary<CharacterPosition, Vector2>();
        var positionKeys = movePathDictionary.Keys;
        var characterPositions = new CharacterPosition[positionKeys.Count];
        positionKeys.CopyTo(characterPositions, 0);
        int mechSize = characterPositions[0].gridPositions.Count;
        if (mechSize == 1)
        {
            foreach (CharacterPosition charPos in characterPositions)
            {
                if (!listToReturn.ContainsKey(charPos))
                {
                    listToReturn.Add(charPos, terrainGridToWorldDictionary[charPos.gridPositions[0]]);
                }

            }
        }
        else if (mechSize == 4)
        {
            foreach (CharacterPosition charPos in characterPositions)
            {
                var centerVector = FindTheCenterOfTheseGridSquares(charPos.gridPositions);
                if (!listToReturn.ContainsKey(charPos))
                {
                    listToReturn.Add(charPos, centerVector);
                }

            }
        }
        else if (mechSize == 9)
        {

        }

        return listToReturn;
    }

    public List<Vector2Int> UnpackAListOfListsOfVector2Int(List<List<Vector2Int>> listToUnpack)
    {
        var unpackedList = new List<Vector2Int>();
        foreach (List<Vector2Int> listOfVectors in listToUnpack)
        {
            foreach (Vector2Int vector in listOfVectors)
            {
                unpackedList.Add(vector);
            }
        }
        return unpackedList;
    }

    public bool CheckVectorListForValidity(List<Vector2Int> vectorListToCheck, GameObject mechToCheckFor)
    {
        //This function simply verifies that a list of grid positions exists and is unoccupied (or occupied by me) and not impassable. It also checks to make sure the destination is flat.

        foreach (Vector2Int gridPosition in vectorListToCheck)
        {
            if (!terrainTileDictionary.ContainsKey(gridPosition))
            {
                return false;
            }
            if ((terrainTileDictionary[gridPosition].isOccupied == true && terrainTileDictionary[gridPosition].whatOccupiesMe != mechToCheckFor) || terrainTileDictionary[gridPosition].impassable == true)
            {
                return false;
            }
            if (terrainTileDictionary[gridPosition].height != terrainTileDictionary[vectorListToCheck[0]].height)
            {
                return false;
            }
        }
        return true;
    }


    //The following functions provide a data structure for individual steps and rotations. These are used to algorithmically generate possible paths for a character between two squares, which can then be filtered for those paths that are legitimate so that players and the AI can make moves.

    public class CharacterPosition
    {
        public BattleParticipant character;
        public List<Vector2Int> gridPositions;
        public string orientation;

        public CharacterPosition(BattleParticipant character, List<Vector2Int> grid, string orientation)
        {
            this.character = character;
            this.gridPositions = grid;
            this.orientation = orientation;
        }
    }

    public class Step
    {
        public float moveCost;
        public CharacterPosition startPos;
        public CharacterPosition endPos;

        public Step(float cost, CharacterPosition start, CharacterPosition end)
        {
            this.moveCost = cost;
            this.startPos = start;
            this.endPos = end;
        }
    }

    public class MovePath
    {
        public float totalMoveCost;
        public List<Step> steps;
        public CharacterPosition totalStartPos;
        public CharacterPosition totalEndPos;

        public MovePath(float totalCost, List<Step> listOfSteps, CharacterPosition start, CharacterPosition end)
        {
            this.totalMoveCost = totalCost;
            this.steps = listOfSteps;
            this.totalStartPos = start;
            this.totalEndPos = end;
        }
    }

    public MovePath NullPath(CharacterPosition positionToBeginPath)
    {
        var nullPath = new MovePath(0f, new List<Step>(), positionToBeginPath, positionToBeginPath);
        return nullPath;
    }

    public class AttemptedPath
    {
        public MovePath attemptedPath;
        public bool wasSuccess;

        public AttemptedPath(MovePath path, bool wasSuccess)
        {
            this.attemptedPath = path;
            this.wasSuccess = wasSuccess;
        }
    }

    public CharacterPosition TakeAStep(CharacterPosition startPos, bool isStep, bool isLeft, bool isRight)
    {
        //This function outputs the resulting CharacterPosition is we take a single move action -- step forward, turn left, or turn right -- from a given Character Position. It's used as a building block in other functions.
        if (isStep)
        {
            var startingGrid = startPos.gridPositions;
            var newGridPositions = new List<Vector2Int>();
            if (startPos.orientation == "North")
            {
                foreach (Vector2Int grid in startingGrid)
                {
                    var gridToAdd = grid + new Vector2Int(0, 1);
                    newGridPositions.Add(gridToAdd);
                }

            }
            else if (startPos.orientation == "East")
            {
                foreach (Vector2Int grid in startingGrid)
                {
                    newGridPositions.Add(grid + new Vector2Int(1, 0));
                }

            }
            else if (startPos.orientation == "South")
            {
                foreach (Vector2Int grid in startingGrid)
                {
                    newGridPositions.Add(grid + new Vector2Int(0, -1));
                }

            }
            else if (startPos.orientation == "West")
            {
                foreach (Vector2Int grid in startingGrid)
                {
                    newGridPositions.Add(grid + new Vector2Int(-1, 0));
                }

            }
            else
            {
                Debug.Log("We couldn't read the starting orientation! It seems to be " + startPos.orientation);
            }
            var newCharacterPosition = new CharacterPosition(startPos.character, newGridPositions, startPos.orientation);
            return newCharacterPosition;
        }
        if (isLeft)
        {
            string newOrientation = "North";
            if (startPos.orientation == "North")
            {
                newOrientation = "West";
            }
            else if (startPos.orientation == "East")
            {
                newOrientation = "North";
            }
            else if (startPos.orientation == "South")
            {
                newOrientation = "East";
            }
            else if (startPos.orientation == "West")
            {
                newOrientation = "South";
            }
            else
            {
                Debug.Log("We couldn't read the starting orientation! It seems to be " + startPos.orientation);
            }
            var newCharacterPosition = new CharacterPosition(startPos.character, startPos.gridPositions, newOrientation);
            return newCharacterPosition;
        }
        if (isRight)
        {
            string newOrientation = "North";
            if (startPos.orientation == "North")
            {
                newOrientation = "East";
            }
            else if (startPos.orientation == "East")
            {
                newOrientation = "South";
            }
            else if (startPos.orientation == "South")
            {
                newOrientation = "West";
            }
            else if (startPos.orientation == "West")
            {
                newOrientation = "North";
            }
            else
            {
                Debug.Log("We couldn't read the starting orientation! It seems to be " + startPos.orientation);
            }
            var newCharacterPosition = new CharacterPosition(startPos.character, startPos.gridPositions, newOrientation);
            return newCharacterPosition;
        }
        else
        {
            Debug.Log("Take a step failed!");
            return startPos;
        }

    }

    public Dictionary<CharacterPosition, MovePath> FindAllPaths2(CharacterPosition startPos)
    {

        //This function will take a character's starting position and output a Dictionary of positions they can reach and the path they can take to get there.

        BattleParticipant characterToMove = startPos.character;
        var availableMove = characterToMove.currentMove;
        //Debug.Log("We appear to have " + availableMove + " move power.");
        var outputDictionary = new Dictionary<CharacterPosition, MovePath>();
        var nullPath = NullPath(startPos);
        outputDictionary.Add(startPos, nullPath);
        //Debug.Log("The first entry we added to the ouput dictionary costs " + nullPath.totalMoveCost + " move power.");

        //Now we'll repeatedly iterate into something called a Step Dictionary, then feed out of the Step Dictionary into the Output Dictionary.

        //First the first seven steps.

        var stepDictionary = new Dictionary<CharacterPosition, MovePath>();
        stepDictionary = TakeTheSevenSteps2(nullPath, true);

        //We only add each Character Position to the dictionary once. We assume that, since move cost increases with each step, the first way we reach a position is the lowest cost way.

        foreach (CharacterPosition charPos in stepDictionary.Keys)
        {
            if (!outputDictionary.ContainsKey(charPos))
            {
                if (stepDictionary[charPos].totalMoveCost < availableMove)
                {
                    outputDictionary.Add(charPos, stepDictionary[charPos]);
                }
            }
        }

        stepDictionary.Clear();

        var movePathsToExpand = new List<MovePath>();
        var movePathsAddedThisLoop = new List<MovePath>();

        foreach (MovePath path in outputDictionary.Values)
        {
            movePathsToExpand.Add(path);
            //Debug.Log("One of the first Paths we're working with has a move cost of " + path.totalMoveCost);
        }



        //Now the goal is this: for each of the MovePaths generated in the previous step, we want to run TakeTheSevenSteps again, producing seven more output MovePaths for that input MovePath. But we only want to do this if A) we haven't used that input MovePath already in the process and B) if the output MovePath didn't leave us with too little power.
        //Now how many times do we run this loop? The idea is that we want to run it for as long as there's movePower left, e.g. as long as running it would be productive. So let's make a counter of how many things we added to the output dictionary, and if that counter is ever 0, shut the loop off.
        //As a failsafe we'll add a loop counter, and shut the loop off after the loop counter gets too high. 

        int outputsAdded = 1;
        int timesLooped = 0;

        while (outputsAdded != 0 && timesLooped < 10)
        {
            //Debug.Log("This is loop " + timesLooped + " and last time we added " + outputsAdded + " new Paths");

            outputsAdded = 0;

            //Debug.Log("Our list of movepaths to expand has " + movePathsToExpand.Count + " entries.");

            foreach (MovePath movePathToExpand in movePathsToExpand)
            {
                stepDictionary = TakeTheSevenSteps2(movePathToExpand, false);
                foreach (CharacterPosition charPos in stepDictionary.Keys)
                {
                    //Debug.Log("Checking a character position...");
                    //For each character position we reached, look at the path to get there...
                    bool charPosFails = false;
                    if (stepDictionary[charPos].totalMoveCost > availableMove)
                    {
                        //Debug.Log("We found a MovePath that is too expensive!");
                        //Debug.Log("This path appears to cost " + stepDictionary[charPos].totalMoveCost + " move points.");
                        charPosFails = true;
                    }
                    foreach (CharacterPosition charPosReached in outputDictionary.Keys)
                    {
                        if (CompareTwoCharacterPositions(charPos, charPosReached))
                        {
                            charPosFails = true;
                            break;
                        }
                    }
                    if (!charPosFails)
                    {

                        //Debug.Log("We found a good position, adding it!");
                        outputDictionary.Add(charPos, stepDictionary[charPos]);
                        outputsAdded += 1;
                        movePathsAddedThisLoop.Add(stepDictionary[charPos]);
                    }
                }
            }

            movePathsToExpand.Clear();
            foreach (MovePath path in movePathsAddedThisLoop)
            {
                movePathsToExpand.Add(path);
            }
            movePathsAddedThisLoop.Clear();

            timesLooped++;
            //Debug.Log("In this loop we added " + outputsAdded + " outputs.");
        }



        //Finally the dictionary is ready to export.

        return outputDictionary;
    }

    public Dictionary<CharacterPosition, MovePath> TrimOutUnequalHeights(Dictionary<CharacterPosition, MovePath> dictToTrim)
    {

        //At this point we need to trim out all of the entries in the Output Dictionary which involve uneven terrain. We still need to pass through these squares sometimes, so the function waits until the end to trim them out so we don't land on them.

        var outputDictionary = new Dictionary<CharacterPosition, MovePath>();

        foreach (CharacterPosition charPos in dictToTrim.Keys)
        {
            bool invalid = false;
            List<Vector2Int> tiles = charPos.gridPositions;
            foreach (Vector2Int tile in tiles)
            {
                if (terrainTileDictionary[tile].height != terrainTileDictionary[tiles[0]].height)
                {
                    Debug.Log("Eliminating an uneven character position");
                    invalid = true;
                    break;
                }
            }
            if (!invalid)
            {
                outputDictionary.Add(charPos, dictToTrim[charPos]);
            }
        }

        return outputDictionary;
    }

    public bool CompareTwoCharacterPositions(CharacterPosition cpOne, CharacterPosition cpTwo)
    {
        //This function attempts to determine if two Character Positions are identical and returns True if so.
        if (cpOne.orientation != cpTwo.orientation)
        {
            return false;
        }
        if (cpOne.character != cpTwo.character)
        {
            return false;
        }
        foreach (Vector2Int gridPosition in cpOne.gridPositions)
        {
            bool matchFound = false;
            foreach (Vector2Int gridPosition2 in cpTwo.gridPositions)
            {
                if (gridPosition == gridPosition2)
                {
                    matchFound = true;
                }
            }
            if (matchFound == false)
            {
                return false;
            }
        }
        return true;
    }

    public MovePath AddAStepToAPath(MovePath pathToAddTo, Step stepToAdd)
    {
        //This function just does what it says, generates a new MovePath that's received a new Step.
        var pathToReturn = new MovePath(pathToAddTo.totalMoveCost, pathToAddTo.steps, pathToAddTo.totalStartPos, pathToAddTo.totalEndPos);
        pathToReturn.steps.Add(stepToAdd);
        pathToReturn.totalMoveCost += stepToAdd.moveCost;
        pathToReturn.totalEndPos = stepToAdd.endPos;
        return pathToReturn;
    }

    public Dictionary<CharacterPosition, MovePath> TakeTheSevenSteps2(MovePath pathToBuild, bool firstStep)
    {

        //This function needs to be slightly rewritten. First, it needs an input MovePath, which is the MovePath that it's building on to when it takes the Seven Steps. Next the output needs to be changed to a MovePath, e.g. the one that starts where the Input MovePath starts and ends in the resulting position. So each time the function runs it outputs 7 entries to a Dictionary<CharacterPosition, MovePath>.
        //When we run FindAllPaths(), at each step it will then output these 7 MovePaths to the master Dictionary of the same structure that function builds. Now, however, we'll be taking count of MoveCost cumulatively through the process. We can then cut it off -- we can stop calling TakeTheSevenSteps from those CharacterPositions where the MovePath that ends there exhausts our move power. That should greatly improve efficiency.

        //This function simply outputs the seven possible steps that can be taken from a single square and the accompanying positions that are reached: turn left, turn right, turn around, forward, left and forward, right and forward, and turn around and forward. The output is a Dictionary that connects these positions and the series of Steps needed to reach them, including a zero-step for the present position.
        var dictionaryToBuild = new Dictionary<CharacterPosition, MovePath>();
        var costToTurn = pathToBuild.totalStartPos.character.rotateCost;
        //Debug.Log("The cost to turn is " + costToTurn);
        var startPos = pathToBuild.totalEndPos;

        //Standing still
        if (firstStep)
        {
            var zeroStep = new Step(0f, startPos, startPos);
            var zeroPath = AddAStepToAPath(pathToBuild, zeroStep);
            //Debug.Log("The zero path has a cost of " + zeroPath.totalMoveCost);
            dictionaryToBuild.Add(zeroPath.totalEndPos, zeroPath);
        }



        //Turning left


        var leftPosition = TakeAStep(startPos, false, true, false);
        var leftStep = new Step(costToTurn, startPos, leftPosition);
        var leftPath = AddAStepToAPath(pathToBuild, leftStep);
        //Debug.Log("Turning left has a cost of " + leftPath.totalMoveCost);
        dictionaryToBuild.Add(leftPosition, leftPath);

        //Turning right

        var rightPosition = TakeAStep(startPos, false, false, true);
        var rightStep = new Step(costToTurn, startPos, rightPosition);
        var rightPath = AddAStepToAPath(pathToBuild, rightStep);
        dictionaryToBuild.Add(rightPosition, rightPath);

        //Turning around

        var rearPosition = TakeAStep(rightPosition, false, false, true);
        var rearStep = new Step(costToTurn, rightPosition, rearPosition);
        var rearPath = AddAStepToAPath(rightPath, rearStep);
        //Debug.Log("Turning around has a cost of " + rearPath.totalMoveCost);
        dictionaryToBuild.Add(rearPosition, rearPath);

        //Step forward

        var forwardPosition = TakeAStep(startPos, true, false, false);
        var reachedGrid = forwardPosition.gridPositions;
        var currentHeight = terrainTileDictionary[startPos.gridPositions[0]].height;
        bool skipForward = false;

        //For all of the positions I'm about to move to, I need to run through a checklist. Does the square exist? Is it unoccupied? Are all target heights equal? Is my jump power sufficient?
        //If it's all working out, I need to check the move cost.

        foreach (Vector2Int gridToReach in reachedGrid)
        {
            if (!terrainTileDictionary.ContainsKey(gridToReach))
            {
                //Debug.Log("Can't find the target tile, skipping!");
                skipForward = true;
                break;
            }
        }
        if (!skipForward)
        {
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                if (terrainTileDictionary[gridToReach].isOccupied)
                {
                    if (terrainTileDictionary[gridToReach].whatOccupiesMe != startPos.character.gameObject)
                    {
                        skipForward = true;
                        break;
                    }
                    //Debug.Log("Target tiled is occupied, skipping!");

                }
            }
        }
        if (!skipForward)
        {
            var targetHeights = new List<int>();
            foreach (Vector2Int grid in reachedGrid)
            {
                var targetHeight = terrainTileDictionary[grid].height;
                targetHeights.Add(targetHeight);
            }
            foreach (int height in targetHeights)
            {
                if (Mathf.Abs(currentHeight - height) > startPos.character.jump)
                {
                    //Debug.Log("We don't have enough jump!");
                    skipForward = true;
                    break;
                }
            }


            //This function used to abort the step if the heights to be reached are unequal, but that causes an error since we still need to pass THROUGH unequal heights. So instead this filtering needs to take place at a later step.

            //foreach (Vector2Int gridToReach in reachedGrid)
            //{
            //    if (terrainTileDictionary[gridToReach].height != targetHeight)
            //    {
            //        //Debug.Log("The target heights are unequal!");
            //        skipForward = true;
            //    }
            //}


        }

        if (!skipForward)
        {
            //At this point we think we can make the move, so now we find the greatest move cost associated with translating across all the squares. 
            var moveCostList = new List<float>();
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                var cost = terrainTileDictionary[gridToReach].moveCost;
                cost += AssignMovePenaltiesBasedOnMoveType(startPos.character, terrainTileDictionary[gridToReach]);
                moveCostList.Add(cost);
            }
            moveCostList.Sort();
            moveCostList.Reverse();
            var costToMove = moveCostList[0];
            var forwardStep = new Step(costToMove, startPos, forwardPosition);
            var forwardPath = AddAStepToAPath(pathToBuild, forwardStep);
            //Debug.Log("Walking forward has a cost of " + forwardPath.totalMoveCost);
            dictionaryToBuild.Add(forwardPosition, forwardPath);
        }







        //Step left

        var leftStepPosition = TakeAStep(leftPosition, true, false, false);
        reachedGrid = leftStepPosition.gridPositions;
        bool skipLeft = false;

        //For all of the positions I'm about to move to, I need to run through a checklist. Does the square exist? Is it unoccupied? Are all target heights equal? Is my jump power sufficient?
        foreach (Vector2Int gridToReach in reachedGrid)
        {
            if (!terrainTileDictionary.ContainsKey(gridToReach))
            {
                skipLeft = true;
                break;
            }
        }
        if (!skipLeft)
        {
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                if (terrainTileDictionary[gridToReach].isOccupied)
                {
                    if (terrainTileDictionary[gridToReach].whatOccupiesMe != startPos.character.gameObject)
                    {
                        skipLeft = true;
                        break;
                    }

                }
            }
        }
        if (!skipLeft)
        {
            var targetHeights = new List<int>();
            foreach (Vector2Int grid in reachedGrid)
            {
                var targetHeight = terrainTileDictionary[grid].height;
                targetHeights.Add(targetHeight);
            }
            foreach (int height in targetHeights)
            {
                if (Mathf.Abs(currentHeight - height) > startPos.character.jump)
                {
                    //Debug.Log("We don't have enough jump!");
                    skipLeft = true;
                    break;
                }
            }


            //This function used to abort the step if the heights to be reached are unequal, but that causes an error since we still need to pass THROUGH unequal heights. So instead this filtering needs to take place at a later step.

            //foreach (Vector2Int gridToReach in reachedGrid)
            //{
            //    if (terrainTileDictionary[gridToReach].height != targetHeight)
            //    {
            //        //Debug.Log("The target heights are unequal!");
            //        skipForward = true;
            //    }
            //}


        }

        if (!skipLeft)
        {
            //At this point we think we can make the move, so now we find the greatest move cost associated with translating across all the squares. 
            var moveCostList = new List<float>();
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                var cost = terrainTileDictionary[gridToReach].moveCost;
                cost += AssignMovePenaltiesBasedOnMoveType(startPos.character, terrainTileDictionary[gridToReach]);
                moveCostList.Add(cost);
            }
            moveCostList.Sort();
            moveCostList.Reverse();
            var costToMove = moveCostList[0];
            //Debug.Log("The cost to take a step to the left is " + costToMove);

            var leftStepStep = new Step(costToMove, leftPosition, leftStepPosition);
            var leftStepPath = AddAStepToAPath(leftPath, leftStepStep);
            dictionaryToBuild.Add(leftStepPosition, leftStepPath);
        }


        //Step right

        var rightStepPosition = TakeAStep(rightPosition, true, false, false);
        reachedGrid = rightStepPosition.gridPositions;
        bool skipRight = false;
        //For all of the positions I'm about to move to, I need to run through a checklist. Does the square exist? Is it unoccupied? Are all target heights equal? Is my jump power sufficient?
        foreach (Vector2Int gridToReach in reachedGrid)
        {
            if (!terrainTileDictionary.ContainsKey(gridToReach))
            {
                skipRight = true;
                break;
            }
        }
        if (!skipRight)
        {
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                if (terrainTileDictionary[gridToReach].isOccupied)
                {
                    if (terrainTileDictionary[gridToReach].whatOccupiesMe != startPos.character.gameObject)
                    {
                        skipRight = true;
                        break;
                    }

                }
            }
        }
        if (!skipRight)
        {
            var targetHeights = new List<int>();
            foreach (Vector2Int grid in reachedGrid)
            {
                var targetHeight = terrainTileDictionary[grid].height;
                targetHeights.Add(targetHeight);
            }
            foreach (int height in targetHeights)
            {
                if (Mathf.Abs(currentHeight - height) > startPos.character.jump)
                {
                    //Debug.Log("We don't have enough jump!");
                    skipRight = true;
                    break;
                }
            }


            //This function used to abort the step if the heights to be reached are unequal, but that causes an error since we still need to pass THROUGH unequal heights. So instead this filtering needs to take place at a later step.

            //foreach (Vector2Int gridToReach in reachedGrid)
            //{
            //    if (terrainTileDictionary[gridToReach].height != targetHeight)
            //    {
            //        //Debug.Log("The target heights are unequal!");
            //        skipForward = true;
            //    }
            //}


        }

        if (!skipRight)
        {
            //At this point we think we can make the move, so now we find the greatest move cost associated with translating across all the squares. 
            var moveCostList = new List<float>();
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                var cost = terrainTileDictionary[gridToReach].moveCost;
                cost += AssignMovePenaltiesBasedOnMoveType(startPos.character, terrainTileDictionary[gridToReach]);
                moveCostList.Add(cost);
            }
            moveCostList.Sort();
            moveCostList.Reverse();
            var costToMove = moveCostList[0];

            var rightStepStep = new Step(costToMove, rightPosition, rightStepPosition);
            var rightStepPath = AddAStepToAPath(rightPath, rightStepStep);
            dictionaryToBuild.Add(rightStepPosition, rightStepPath);
        }


        //Step back... We only need to do this from the very first square, because after that, stepping backwards will always result in landing on a position we already occupied or generated
        if (firstStep)
        {
            var rearStepPosition = TakeAStep(rearPosition, true, false, false);
            reachedGrid = rearStepPosition.gridPositions;
            bool skipBack = false;
            foreach (Vector2Int gridToReach in reachedGrid)
            {
                if (!terrainTileDictionary.ContainsKey(gridToReach))
                {
                    skipBack = true;
                    break;
                }
            }
            if (!skipBack)
            {
                foreach (Vector2Int gridToReach in reachedGrid)
                {
                    if (terrainTileDictionary[gridToReach].isOccupied)
                    {
                        if (terrainTileDictionary[gridToReach].whatOccupiesMe != startPos.character.gameObject)
                        {
                            skipBack = true;
                            break;
                        }

                    }
                }
            }
            if (!skipBack)
            {
                var targetHeights = new List<int>();
                foreach (Vector2Int grid in reachedGrid)
                {
                    var targetHeight = terrainTileDictionary[grid].height;
                    targetHeights.Add(targetHeight);
                }
                foreach (int height in targetHeights)
                {
                    if (Mathf.Abs(currentHeight - height) > startPos.character.jump)
                    {
                        //Debug.Log("We don't have enough jump!");
                        skipBack = true;
                        break;
                    }
                }


                //This function used to abort the step if the heights to be reached are unequal, but that causes an error since we still need to pass THROUGH unequal heights. So instead this filtering needs to take place at a later step.

                //foreach (Vector2Int gridToReach in reachedGrid)
                //{
                //    if (terrainTileDictionary[gridToReach].height != targetHeight)
                //    {
                //        //Debug.Log("The target heights are unequal!");
                //        skipForward = true;
                //    }
                //}


            }
            if (!skipBack)
            {

                var moveCostList = new List<float>();
                foreach (Vector2Int gridToReach in reachedGrid)
                {
                    var cost = terrainTileDictionary[gridToReach].moveCost;
                    cost += AssignMovePenaltiesBasedOnMoveType(startPos.character, terrainTileDictionary[gridToReach]);
                    moveCostList.Add(cost);
                }
                moveCostList.Sort();
                moveCostList.Reverse();
                var costToMove = moveCostList[0];

                var rearStepStep = new Step(costToMove, rearPosition, rearStepPosition);
                var rearStepPath = AddAStepToAPath(rearPath, rearStepStep);
                //Debug.Log("Walking backwards has a cost of " + rearStepPath.totalMoveCost);
                dictionaryToBuild.Add(rearStepPosition, rearStepPath);



            }
        }




        return dictionaryToBuild;
    }

    public float AssignMovePenaltiesBasedOnMoveType(BattleParticipant mover, TerrainTile enteredLocation)
    {

        //This function returns the move penalty for entering a particular square with a particular unit.
        if (mover.moveType == "Walk")
        {
            return enteredLocation.penaltyToWalk;
        }
        else if (mover.moveType == "Large Quad")
        {
            return enteredLocation.penaltyToLargeQuad;
        }
        else if (mover.moveType == "Wheel")
        {
            return enteredLocation.penaltyToWheel;
        }
        else if (mover.moveType == "Tread")
        {
            return enteredLocation.penaltyToTread;
        }
        else if (mover.moveType == "Fly")
        {
            return enteredLocation.penaltyToFly;
        }

        else return 0f;
    }

    //
    //End Move functions
    //



    //
    //Begin Rangefind() functions
    //The following functions involve range-finding and LOS-detection for active abilities.
    //



    public List<Vector2Int> FindAFiringArc(CharacterPosition firingCharacter, ActiveAbility abilityToRangefind)
    {
        //This function finds the firing range for a given ability from a given character. It returns a list of squares that should be highlighted and selected from. It does NOT include LOS checks, which are done after selecting a square to shoot at.
        int energy = abilityToRangefind.maxRange - abilityToRangefind.minRange;
        var energyDictionary = new Dictionary<Vector2Int, float>();
        var startingSquares = new List<Vector2Int>();
        var squaresReached = new List<Vector2Int>();
        var listToReturn = new List<Vector2Int>();
        string orientation = firingCharacter.orientation;
        int startingHeight = terrainTileDictionary[firingCharacter.gridPositions[0]].height;
        if (firingCharacter.character.moveType == "Large Quad")
        {
            startingHeight += 1;
        }

        startingSquares = FindSquaresToShootFrom(firingCharacter);
        //Debug.Log("We are starting our rangefinding from " + startingSquares.Count + " squares.");

        bool heightAffected = false;
        if (abilityToRangefind.trajectory == "Ballistic" || abilityToRangefind.trajectory == "Melee")
        {
            heightAffected = true;
        }

        //This function operates in 4 different ways depending on the firing arc of the ability.

        //First, the 90 degree firing arc.

        if (abilityToRangefind.firingArc == 90)
        {
            //First we check for a minimum range, and we set the starting squares further out based on it.
            if (abilityToRangefind.minRange > 0)
            {
                for (int i = 0; i < abilityToRangefind.minRange; i++)
                {
                    startingSquares = ExpandBy90Degrees(startingSquares, orientation);
                }
            }

            squaresReached = startingSquares;


            //If not height-affected, all we have to do is loop the ExpandBy function the appropriate number of times, adding the results each time to the list of squares to output.
            if (!heightAffected)
            {
                for (int i = energy; i > 0; i--)
                {
                    squaresReached = ExpandBy90Degrees(squaresReached, orientation);

                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            if (terrainTileDictionary.ContainsKey(squareReached))
                            {
                                listToReturn.Add(squareReached);
                            }
                        }
                    }
                }
            }

            //If we are height-affected...
            //Now we keep track of the energy left for every square we reach when we do the ExpandBy function.
            //The baseline is given by "minRemainingEnergy," which loses 1 point every time we iterate. It's then boosted up or down by the height differential.
            //That energy must be positive to record the square in the output list.
            //
            //(?)
            else
            {
                float maxRemainingEnergy = energy;
                float minRemainingEnergy = energy;
                List<float> energiesLeft = new List<float>();
                int timesLooped = 0;
                while (maxRemainingEnergy > 0 && timesLooped < 10)
                {
                    energiesLeft.Clear();
                    squaresReached = ExpandBy90Degrees(squaresReached, orientation);
                    //Debug.Log("At the beginning of loop " + timesLooped + " we are working with " + squaresReached.Count + " squares.");
                    var squaresToIterateThrough = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        squaresToIterateThrough.Add(squareReached);
                    }
                    squaresReached.Clear();
                    minRemainingEnergy--;
                    timesLooped++;
                    //Debug.Log("Squares to iterate through has Count " + squaresToIterateThrough.Count);
                    foreach (Vector2Int squareReached in squaresToIterateThrough)
                    {

                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;

                        //Debug.Log("The energy left for the square " + squareReached + " is " + energyLeft);
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached.Add(squareReached);
                        }
                    }

                    energiesLeft.Sort();
                    energiesLeft.Reverse();
                    if (energiesLeft.Count == 0)
                    {
                        maxRemainingEnergy = 0;
                    }
                    else
                    {
                        maxRemainingEnergy = energiesLeft[0];
                    }

                    //Debug.Log("Times looped " + timesLooped);
                    //Debug.Log("Max energy remaining is " + maxRemainingEnergy);
                    //Debug.Log("Min remaining energy is " + minRemainingEnergy);

                }

            }

        }

        //Next the 180 degree arc...

        else if (abilityToRangefind.firingArc == 180)
        {
            //First we check for a minimum range, and we set the starting squares further out based on it.
            //We also reset "startingSquares" to all my squares, which is correct for the 180 and 360 degree firing arcs.
            //The Expand() function seems to cause some misfires with minimum range, so as a jury-rig we record a blacklist in the min-range setting process and subtract all the blacklist squares at the end.
            startingSquares = firingCharacter.gridPositions;
            var blacklistSquares = new List<Vector2Int>();

            if (abilityToRangefind.minRange > 0)
            {
                for (int i = 0; i < abilityToRangefind.minRange; i++)
                {
                    startingSquares = ExpandBy180Degrees(startingSquares, orientation);
                    foreach (Vector2Int badGrid in startingSquares)
                    {
                        blacklistSquares.Add(badGrid);
                    }
                }
            }

            squaresReached = startingSquares;


            //If not height-affected, all we have to do is loop the ExpandBy function the appropriate number of times, adding the results each time to the list of squares to output.
            if (!heightAffected)
            {
                for (int i = energy; i > 0; i--)
                {
                    squaresReached = ExpandBy180Degrees(squaresReached, orientation);

                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            if (terrainTileDictionary.ContainsKey(squareReached))
                            {
                                if (terrainTileDictionary[squareReached].whatOccupiesMe != firingCharacter.character.gameObject)
                                {
                                    listToReturn.Add(squareReached);
                                }
                            }

                        }
                    }
                }
                foreach (Vector2Int badGrid in blacklistSquares)
                {
                    if (listToReturn.Contains(badGrid))
                    {
                        listToReturn.Remove(badGrid);
                    }
                }
            }

            //If we are height-affected...
            //Now we keep track of the energy left for every square we reach when we do the ExpandBy function.
            //The baseline is given by "minRemainingEnergy," which loses 1 point every time we iterate. It's then boosted up or down by the height differential.
            //That energy must be positive to record the square in the output list.
            //
            //(?)
            else
            {
                float maxRemainingEnergy = energy;
                float minRemainingEnergy = energy;
                List<float> energiesLeft = new List<float>();
                int timesLooped = 0;
                while (maxRemainingEnergy > 0 && timesLooped < 10)
                {
                    energiesLeft.Clear();
                    squaresReached = ExpandBy180Degrees(squaresReached, orientation);
                    var squaresToIterateThrough = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        squaresToIterateThrough.Add(squareReached);
                    }
                    squaresReached.Clear();
                    minRemainingEnergy--;
                    timesLooped++;
                    foreach (Vector2Int squareReached in squaresToIterateThrough)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached.Add(squareReached);
                        }
                    }

                    if (energiesLeft.Count == 0)
                    {
                        maxRemainingEnergy = 0;
                    }
                    else
                    {
                        maxRemainingEnergy = energiesLeft[0];
                    }
                }

                foreach (Vector2Int badGrid in blacklistSquares)
                {
                    if (listToReturn.Contains(badGrid))
                    {
                        listToReturn.Remove(badGrid);
                    }
                }

            }

        }

        //Next the 270 degree arc, which involves running the 90 degree arc routine in 3 directions -- forward, left, and right.

        else if (abilityToRangefind.firingArc == 270)
        {

            //First we find our secondary orientations to iterate by.
            string orientation2 = orientation;
            string orientation3 = orientation;
            if (firingCharacter.orientation == "North")
            {
                orientation2 = "West";
                orientation3 = "East";
            }
            else if (firingCharacter.orientation == "South")
            {
                orientation2 = "East";
                orientation3 = "West";
            }
            else if (firingCharacter.orientation == "East")
            {
                orientation2 = "North";
                orientation3 = "South";
            }
            if (firingCharacter.orientation == "West")
            {
                orientation2 = "South";
                orientation3 = "North";
            }
            //First we check for a minimum range, and we set the starting squares further out based on it.
            var characterInOrientation2 = new CharacterPosition(firingCharacter.character, firingCharacter.gridPositions, orientation2);
            var characterInOrientation3 = new CharacterPosition(firingCharacter.character, firingCharacter.gridPositions, orientation3);

            var startingSquares2 = FindSquaresToShootFrom(characterInOrientation2);
            var startingSquares3 = FindSquaresToShootFrom(characterInOrientation3);

            if (abilityToRangefind.minRange > 0)
            {
                for (int i = 0; i < abilityToRangefind.minRange; i++)
                {
                    startingSquares = ExpandBy90Degrees(startingSquares, orientation);
                    startingSquares2 = ExpandBy90Degrees(startingSquares2, orientation2);
                    startingSquares3 = ExpandBy90Degrees(startingSquares3, orientation3);
                }
            }

            squaresReached = startingSquares;
            var squaresReached2 = startingSquares2;
            var squaresReached3 = startingSquares3;


            //If not height-affected, all we have to do is loop the ExpandBy function the appropriate number of times, adding the results each time to the list of squares to output.

            if (!heightAffected)
            {
                for (int i = energy; i > 0; i--)
                {
                    squaresReached = ExpandBy90Degrees(squaresReached, orientation);

                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            listToReturn.Add(squareReached);
                        }
                    }

                    squaresReached2 = ExpandBy90Degrees(squaresReached2, orientation2);

                    foreach (Vector2Int squareReached in squaresReached2)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            listToReturn.Add(squareReached);
                        }
                    }

                    squaresReached3 = ExpandBy90Degrees(squaresReached3, orientation3);

                    foreach (Vector2Int squareReached in squaresReached3)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            listToReturn.Add(squareReached);
                        }
                    }
                }
            }

            //If we are height-affected...
            //Now we keep track of the energy left for every square we reach when we do the ExpandBy function.
            //The baseline is given by "minRemainingEnergy," which loses 1 point every time we iterate. It's then boosted up or down by the height differential.
            //That energy must be positive to record the square in the output list.
            //
            //(?)
            else
            {
                float maxRemainingEnergy = energy;
                float minRemainingEnergy = energy;
                List<float> energiesLeft = new List<float>();
                int timesLooped = 0;
                while (maxRemainingEnergy > 0 && timesLooped < 5)
                {
                    energiesLeft.Clear();
                    squaresReached = ExpandBy90Degrees(squaresReached, orientation);
                    squaresReached2 = ExpandBy90Degrees(squaresReached2, orientation2);
                    squaresReached3 = ExpandBy90Degrees(squaresReached3, orientation3);
                    var squaresToIterateThrough = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        squaresToIterateThrough.Add(squareReached);
                    }
                    var squaresToIterateThrough2 = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached2)
                    {
                        squaresToIterateThrough2.Add(squareReached);
                    }
                    var squaresToIterateThrough3 = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached3)
                    {
                        squaresToIterateThrough3.Add(squareReached);
                    }
                    squaresReached.Clear();
                    squaresReached2.Clear();
                    squaresReached3.Clear();
                    minRemainingEnergy--;
                    timesLooped++;
                    foreach (Vector2Int squareReached in squaresToIterateThrough)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached.Add(squareReached);
                        }
                    }
                    foreach (Vector2Int squareReached in squaresToIterateThrough2)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached2.Add(squareReached);
                        }
                    }
                    foreach (Vector2Int squareReached in squaresToIterateThrough3)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached3.Add(squareReached);
                        }
                    }

                    if (energiesLeft.Count == 0)
                    {
                        maxRemainingEnergy = 0;
                    }
                    else
                    {
                        maxRemainingEnergy = energiesLeft[0];
                    }
                }

            }

        }

        //Finally the 360 degree arc, which runs the 180 degree arc forward and backwards.

        else if (abilityToRangefind.firingArc == 360)
        {

            //First we find our secondary orientations to iterate by.
            string orientation2 = orientation;
            if (firingCharacter.orientation == "North")
            {
                orientation2 = "South";
            }
            else if (firingCharacter.orientation == "South")
            {
                orientation2 = "North";
            }
            else if (firingCharacter.orientation == "East")
            {
                orientation2 = "West";
            }
            else if (firingCharacter.orientation == "West")
            {
                orientation2 = "East";
            }
            //First we check for a minimum range, and we set the starting squares further out based on it.

            startingSquares = firingCharacter.gridPositions;

            var characterInOrientation2 = new CharacterPosition(firingCharacter.character, firingCharacter.gridPositions, orientation2);

            var startingSquares2 = firingCharacter.gridPositions;

            var blacklistSquares = new List<Vector2Int>();

            foreach (Vector2Int charGrid in firingCharacter.gridPositions)
            {
                blacklistSquares.Add(charGrid);
            }

            if (abilityToRangefind.minRange > 0)
            {
                for (int i = 0; i < abilityToRangefind.minRange; i++)
                {
                    startingSquares = ExpandBy180Degrees(startingSquares, orientation);
                    startingSquares2 = ExpandBy180Degrees(startingSquares2, orientation2);
                    foreach (Vector2Int badGrid in startingSquares)
                    {
                        blacklistSquares.Add(badGrid);
                    }
                    foreach (Vector2Int badGrid in startingSquares2)
                    {
                        blacklistSquares.Add(badGrid);
                    }
                }
            }

            squaresReached = startingSquares;
            var squaresReached2 = startingSquares2;


            //If not height-affected, all we have to do is loop the ExpandBy function the appropriate number of times, adding the results each time to the list of squares to output.

            if (!heightAffected)
            {
                for (int i = energy; i > 0; i--)
                {
                    squaresReached = ExpandBy180Degrees(squaresReached, orientation);

                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        if (!listToReturn.Contains(squareReached))
                        {
                            if (terrainTileDictionary.ContainsKey(squareReached))
                            {
                                if (terrainTileDictionary[squareReached].whatOccupiesMe != firingCharacter.character.gameObject)
                                {
                                    listToReturn.Add(squareReached);
                                }
                            }

                        }
                    }

                    squaresReached2 = ExpandBy180Degrees(squaresReached2, orientation2);

                    foreach (Vector2Int squareReached in squaresReached2)
                    {
                        if (!listToReturn.Contains(squareReached))
                            if (terrainTileDictionary.ContainsKey(squareReached))
                            {
                                if (terrainTileDictionary[squareReached].whatOccupiesMe != firingCharacter.character.gameObject)
                                {
                                    listToReturn.Add(squareReached);
                                }
                            }
                    }

                }

                foreach (Vector2Int badGrid in blacklistSquares)
                {
                    if (listToReturn.Contains(badGrid))
                    {
                        listToReturn.Remove(badGrid);
                    }
                }

            }

            //If we are height-affected...
            //Now we keep track of the energy left for every square we reach when we do the ExpandBy function.
            //The baseline is given by "minRemainingEnergy," which loses 1 point every time we iterate. It's then boosted up or down by the height differential.
            //That energy must be positive to record the square in the output list.
            //
            //(?)
            else
            {
                float maxRemainingEnergy = energy;
                float minRemainingEnergy = energy;
                List<float> energiesLeft = new List<float>();
                int timesLooped = 0;
                while (maxRemainingEnergy > 0 && timesLooped < 5)
                {
                    energiesLeft.Clear();
                    squaresReached = ExpandBy180Degrees(squaresReached, orientation);
                    squaresReached2 = ExpandBy180Degrees(squaresReached2, orientation2);
                    var squaresToIterateThrough = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached)
                    {
                        squaresToIterateThrough.Add(squareReached);
                    }
                    var squaresToIterateThrough2 = new List<Vector2Int>();
                    foreach (Vector2Int squareReached in squaresReached2)
                    {
                        squaresToIterateThrough2.Add(squareReached);
                    }
                    squaresReached.Clear();
                    squaresReached2.Clear();
                    minRemainingEnergy--;
                    timesLooped++;
                    foreach (Vector2Int squareReached in squaresToIterateThrough)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        if (abilityToRangefind.trajectory == "Melee" && energyBoost > 0)
                        {
                            energyBoost = 0;
                        }
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached.Add(squareReached);
                        }
                    }
                    foreach (Vector2Int squareReached in squaresToIterateThrough2)
                    {
                        int targetHeight = terrainTileDictionary[squareReached].height;
                        int heightDifferential = startingHeight - targetHeight;
                        float energyBoost = heightDifferential / 2;
                        float energyLeft = minRemainingEnergy + energyBoost;
                        energiesLeft.Add(energyLeft);
                        if (energyLeft >= 0)
                        {
                            if (!energyDictionary.ContainsKey(squareReached))
                            {
                                energyDictionary.Add(squareReached, energyLeft);
                                if (!listToReturn.Contains(squareReached))
                                {
                                    listToReturn.Add(squareReached);
                                }

                            }
                            squaresReached2.Add(squareReached);
                        }
                    }
                    if (energiesLeft.Count == 0)
                    {
                        maxRemainingEnergy = 0;
                    }
                    else
                    {
                        maxRemainingEnergy = energiesLeft[0];
                    }
                }

                foreach (Vector2Int badGrid in blacklistSquares)
                {
                    if (listToReturn.Contains(badGrid))
                    {
                        listToReturn.Remove(badGrid);
                    }
                }

            }

        }



        return listToReturn;
    }

    public Vector2Int CheckForLOS(List<Vector2Int> attackerSquares, Vector2Int targetSquare)
    {
        //This function calculates a line connecting all of an attacker's squares with a defender.
        //It then runs through all the intervening squares and checks for anything with a height value greater than the height value of the intervening line at that location.
        //If any blocking objects are found it add them to a list and adds a "true" bool to a list.
        //Finally, if the bool list contains only "true," then LOS is blocked and it returns the Vector2Int of the first blocking square it found.
        //Otherwise, there is at least one LOS to the target and it returns (0,0), signifying clear line of sight.
        List<bool> losBlockedList = new List<bool>();
        List<bool> losPathBlockedList = new List<bool>();
        List<Vector2Int> blockingSquaresList = new List<Vector2Int>();
        foreach (Vector2Int attackerSquare in attackerSquares)
        {
            //
            //For each attacker square, we run the LOS check to the target.
            //If the LOS from that square is blocked, we add a "true" to the losPathBlockedlist.
            //If not, we add a "false."
            //After checking all attacker squares, as long as there is one "false," we return 0,0 and say LOS isn't blocked.
            //

            var squaresToCheck = FindAllInterveningSquares(attackerSquare, targetSquare);


            //First we get the attacker and defender's heights, including adjustment for leg sizes.
            int attackerHeight = terrainTileDictionary[attackerSquare].height;
            int defenderHeight = terrainTileDictionary[targetSquare].height;
            if (terrainTileDictionary[attackerSquare].whatOccupiesMe != null && terrainTileDictionary[attackerSquare].whatOccupiesMe.GetComponent<BattleParticipant>() != null)
            {
                attackerHeight += FindHeightBonusBasedOnMoveType(terrainTileDictionary[attackerSquare].whatOccupiesMe.GetComponent<BattleParticipant>());
                //string attackerMoveType = terrainTileDictionary[attackerSquare].whatOccupiesMe.GetComponent<BattleParticipant>().moveType;
                //if (attackerMoveType == "Biped" || attackerMoveType == "Walk")
                //{
                //    attackerHeight += 1;
                //}
                //else if (attackerMoveType == "Large Quad" || attackerMoveType == "Large Biped")
                //{
                //    attackerHeight += 2;
                //}
                //else if (attackerMoveType == "Fly" || attackerMoveType == "Large Flier")
                //{
                //    attackerHeight += 3;
                //}
                //else
                //{
                //    Debug.Log("We found an unrecognized move type while LOS checking, it is " + attackerMoveType);
                //}
            }
            else if (terrainTileDictionary[attackerSquare].whatOccupiesMe == null)
            {
                Debug.Log("Encountered an interesting problem... The attacker's square thinks it's unoccupied, square is " + attackerSquare);
            }
            if (terrainTileDictionary[targetSquare].whatOccupiesMe != null && terrainTileDictionary[targetSquare].whatOccupiesMe.GetComponent<BattleParticipant>() != null)
            {
                defenderHeight += FindHeightBonusBasedOnMoveType(terrainTileDictionary[targetSquare].whatOccupiesMe.GetComponent<BattleParticipant>());
                //string defenderMoveType = terrainTileDictionary[targetSquare].whatOccupiesMe.GetComponent<BattleParticipant>().moveType;
                //if (defenderMoveType == "Biped" || defenderMoveType == "Walk")
                //{
                //    defenderHeight += 1;
                //}
                //else if (defenderMoveType == "Large Quad" || defenderMoveType == "Large Biped")
                //{
                //    defenderHeight += 2;
                //}
                //else if (defenderMoveType == "Fly" || defenderMoveType == "Large Flier")
                //{
                //    defenderHeight += 3;
                //}
                //else
                //{
                //    Debug.Log("We found an unrecognized move type while LOS checking, it is " + defenderMoveType);
                //}
            }
            else if (terrainTileDictionary[targetSquare].whatOccupiesMe == null)
            {
                Debug.Log("Encountered an interesting problem... The target's square thinks it's unoccupied, square is " + targetSquare);
            }
            //

            //Next we set up y = mx + b.
            float linearDistance = Mathf.Sqrt(Vector2.SqrMagnitude(targetSquare - attackerSquare));
            //Debug.Log("The linear distance in the LOS check between " + targetSquare + " and " + attackerSquare + " is " + linearDistance);
            int heightDifference = defenderHeight - attackerHeight;
            float slope = heightDifference / linearDistance;
            //Debug.Log("And the slope appears to be " + slope);
            int intercept = attackerHeight;
            //

            //Next we check the height for each intervening square.
            //I got a bug where this function detected as occupied one of the squares that the large unit itself was on, so need to filter that case out.
            foreach (Vector2Int interveningSquare in squaresToCheck)
            {
                //The way this works is it looks at all the intervening squares between each of the attacker's squares and the target.
                //At least one of these pathways needs to be clear.
                //And it only takes one obstacle to block a pathway.
                losBlockedList.Clear();
                bool attackerIsOnThisSquare = false;
                if (attackerSquares.Contains(interveningSquare))
                {
                    attackerIsOnThisSquare = true;
                }

                if (!attackerIsOnThisSquare)
                {

                    int squareHeight = terrainTileDictionary[interveningSquare].height;

                    if (terrainTileDictionary[interveningSquare].whatOccupiesMe != null && terrainTileDictionary[interveningSquare].whatOccupiesMe.GetComponent<BattleParticipant>() != null)
                    {
                        //Debug.Log("Detected a unit on one of the intervening squares.");
                        squareHeight += FindHeightBonusBasedOnMoveType(terrainTileDictionary[interveningSquare].whatOccupiesMe.GetComponent<BattleParticipant>());
                        //string interveningUnitMoveType = terrainTileDictionary[attackerSquare].whatOccupiesMe.GetComponent<BattleParticipant>().moveType;
                        //if (interveningUnitMoveType == "Biped" || interveningUnitMoveType == "Walk")
                        //{
                        //    squareHeight += 1;
                        //}
                        //else if (interveningUnitMoveType == "Large Quad" || interveningUnitMoveType == "Large Biped")
                        //{
                        //    squareHeight += 2;
                        //}
                        //else if (interveningUnitMoveType == "Fly" || interveningUnitMoveType == "Large Flier")
                        //{
                        //    squareHeight += 3;
                        //}
                        //else
                        //{
                        //    Debug.Log("We found an unrecognized move type while LOS checking, it is " + interveningUnitMoveType);
                        //}
                        //Debug.Log("The height with the intervening unit is " + squareHeight);
                    }
                    float distanceToThisSquare = Mathf.Sqrt(Vector2.SqrMagnitude(attackerSquare - interveningSquare));
                    float heightOfTheStraightLineAtThisLocation = slope * distanceToThisSquare + intercept;
                    //Debug.Log("The line of sight should be at a height value of " + heightOfTheStraightLineAtThisLocation);
                    if (squareHeight >= heightOfTheStraightLineAtThisLocation)
                    {
                        //Debug.Log("The intervening square appears to block LOS, this path is blocked..");
                        blockingSquaresList.Add(interveningSquare);
                        losBlockedList.Add(true);
                        losPathBlockedList.Add(true);
                        break;
                    }
                    else
                    {
                        //Debug.Log("The intervening square appears lower than LOS, LOS seems clear.");
                        losBlockedList.Add(false);
                    }
                }

            }
            //
            //We've now checked all intervening squares along one path.
            //If there are no Trues in the losBlockedList, the path is open.
            //

            if (!losBlockedList.Contains(true))
            {
                losPathBlockedList.Add(false);
            }
        }

        //At this point we have checked all pathways and we want to know if any of them are clear.

        if (losPathBlockedList.Contains(false))
        {
            Debug.Log("Overall line of sight to this target appears clear");
            return new Vector2Int(0, 0);
        }
        else
        {
            Debug.Log("LOS appears blocked, the first blocking square is " + blockingSquaresList[0]);
            return blockingSquaresList[0];
        }

        //If we didn't return anything along the way, we return the null Vector.

    }



    public List<Vector2Int> FindSquaresToShootFrom(CharacterPosition shootingCharacter)
    {
        //This function identifies which squares are doing the firing and should be the starting squares for a 90-degree firing arc.
        //For a 180 degree firing arc this is not needed, as all the squares are doing the shooting.
        var listToReturn = new List<Vector2Int>();
        var squaresIAmOn = shootingCharacter.gridPositions;

        if (squaresIAmOn.Count == 1)
        {
            listToReturn.Add(squaresIAmOn[0]);
            return listToReturn;
        }

        if (shootingCharacter.orientation == "North")
        {
            squaresIAmOn.Sort(SortSquaresByNorthwardness);
        }
        else if (shootingCharacter.orientation == "South")
        {
            squaresIAmOn.Sort(SortSquaresBySouthwardness);
        }
        else if (shootingCharacter.orientation == "East")
        {
            squaresIAmOn.Sort(SortSquaresByEastwardness);
        }
        else if (shootingCharacter.orientation == "West")
        {
            squaresIAmOn.Sort(SortSquaresByWestwardness);
        }



        if (squaresIAmOn.Count == 4)
        {
            listToReturn.Add(squaresIAmOn[0]);
            listToReturn.Add(squaresIAmOn[1]);
            return listToReturn;
        }

        if (squaresIAmOn.Count == 9)
        {
            listToReturn.Add(squaresIAmOn[0]);
            listToReturn.Add(squaresIAmOn[1]);
            listToReturn.Add(squaresIAmOn[2]);
            return listToReturn;
        }


        return listToReturn;
    }

    //
    //End Rangefind() functions
    //


    //
    //Begin Taking-An-Action related functions
    //
    //The following classes and functions concern the previews and outcomes of active abilities.
    //

    public void PreviewAnAction(RefinedProposedAction refinedActionToPreview)
    {


    }

    public void ConsiderAnAction(ActiveAbility abilityToUse, BattleParticipant user, Vector2Int targetLocus)
    {
        //This function generates a ProposedAction from the inputs given to it by the Cursor Anchor when selecting an ability.
        //The previous game flow variable is shoppingForAnActiveAbility, this function actives consideringAProposedAbility.
        //The first job of this function is to create a ProposedAction class that gathers together all the parameters that could possibly influence the outcome of the action.
        //Next it generates an action preview based on the parameters in the Proposed Action and displayed this on screen.
        //Finally it allows the user to Confirm or Cancel the action -- a Cancel returns to the base turn state, a Confirm moves into the execution phase.

        focusCamera.targetDisplay = 5;

        shoppingForAnActiveAbility = false;
        consideringAProposedAbility = true;

        UnHighlightAListOfSquares(abilityToConsider.abilityRange);

        proposedActionToConsider = GenerateAProposedAction(abilityToUse, user, targetLocus);
        if (proposedActionToConsider.targetList == null || proposedActionToConsider.targetList.Count == 0)
        {
            Debug.Log("There are no targets at this location, perhaps we should play a little beep and implement logic?");
            shoppingForAnActiveAbility = true;
            consideringAProposedAbility = false;
            return;
        }
        var refinedAction = RefineAnAction(proposedActionToConsider);
        refinedActionToConsider = refinedAction;
        battleActionPreview.SetActive(true);
        battleActionPreview.GetComponent<BattleActionPreviewScript>().InitializeTheActionPreview(refinedAction);
        //Debug.Log("Created a RefinedAction, which has accuracy stat eg " + refinedAction.chanceToHit);

        return;
    }

    public IEnumerator ExecuteAnAction(RefinedProposedAction actionToExecute)
    {
        focusCamera.targetDisplay = 5;
        var actionOutcome = GenerateAnActionOutcome(actionToExecute);

        //First we slide out the battle action preview then disable it.
        StartCoroutine(battleActionPreview.GetComponent<BattleActionPreviewScript>().SlideOutThePreviewContent());
        yield return new WaitForSeconds(BattleActionPreviewScript.secondsToSlideIn);
        //

        //Next we run the Battle Animation by initializing and running the Combat Animation Overlay.
        combatAnimationOverlay.SetActive(true);
        combatAnimationOverlay.GetComponent<CombatAnimationManager>().InitializeTheCombatAnimation(actionOutcome);
        yield return StartCoroutine(combatAnimationOverlay.GetComponent<CombatAnimationManager>().RunTheCombatAnimation());
        Debug.Log("Control has been returned to the Battle Manager ExecuteAnActionScript at time " + Time.time);
        combatAnimationOverlay.SetActive(false);
        //


        //Then we tell all the battle participants what just happened.
        TellAllBattleParticipantsAboutAnActionOutcome(actionOutcome);
        //

        //Then we execute any critical outcomes, like deaths.
        if (CheckForWinConditions())
        {
            StartCoroutine(WinTheBattle());
        }
        else if (CheckForLossConditions())
        {
            StartCoroutine(LoseTheBattle());
        }
        //

        //Finally we return player control to the pre-action state.
        CancelAnAction();
        //
        focusCamera.targetDisplay = 1;
        yield return null;
    }

    public void CancelAnAction()
    {
        //This function should be accessed while clicking thd Cancel button any time after clicking Act.
        //It should return the player to the game state prior to clicking Act.
        if (abilityToConsider != null)
        {
            abilityToConsider.AbortAbility();
        }
        battleActionPreview.SetActive(false);
        activeAbilitiesMenu.SetActive(false);
        shoppingForAnActiveAbility = false;
        consideringAProposedAbility = false;
        proposedActionToConsider = null;
        refinedActionToConsider = null;
        confirmButtons.SetActive(false);
        focusCamera.targetDisplay = 1;

    }

    public ProposedAction GenerateAProposedAction(ActiveAbility ability, BattleParticipant user, Vector2Int targetLocus)
    {
        var outputAction = new ProposedAction();
        var losCheckedTargetLocus = targetLocus;
        outputAction.abilityToUse = ability;
        outputAction.actor = user;

        List<Vector2Int> squaresToSearchForTargets = new List<Vector2Int>();
        List<BattleParticipant> targetList = new List<BattleParticipant>();
        List<PassiveAbility> influencerList = new List<PassiveAbility>();

        //First we get what squares to search for based on the target area of the ability.

        //In order to do that, we first need to check for LOS if the ability requires it and adjust the targetLocus.
        if (ability.trajectory != "Homing" && ability.trajectory != "Ballistic")
        {
            if (CheckForLOS(user.gridPositionsIAmOver, targetLocus) != new Vector2(0, 0))
            {
                losCheckedTargetLocus = CheckForLOS(user.gridPositionsIAmOver, targetLocus);
                Debug.Log("Since LOS is blocked, we are changing the target locus from " + targetLocus + " to " + losCheckedTargetLocus);
            }
        }


        //Effect areas are as follows:
        //"Single" is a single square, for the average single-target attack
        //"Plus" is a plus-shape
        //"Ninesquare" is a 3x3 square
        //"All" affects all squares in the range
        //"Diamond" is a diamond centered on the target square, e.g. +2 in X and Y
        //"Random" affects random squares in the range

        if (ability.effectArea == "Single")
        {
            squaresToSearchForTargets.Add(losCheckedTargetLocus);
        }
        else if (ability.effectArea == "Plus")
        {
            Debug.Log("Haven't programmed this yet");
            return null;
        }
        else if (ability.effectArea == "Ninesquare")
        {
            Debug.Log("Haven't programmed this yet");
            return null;
        }
        else if (ability.effectArea == "All")
        {
            Debug.Log("Haven't programmed this yet");
            return null;
        }
        else if (ability.effectArea == "Diamond")
        {
            Debug.Log("Haven't programmed this yet");
            return null;
        }
        else if (ability.effectArea == "Random")
        {
            Debug.Log("Haven't programmed this yet");
            return null;
        }

        //Target squares are now known.

        //Next we find which Battle Participants are on the target squares and add them to the targetList.


        foreach (Vector2Int targetGrid in squaresToSearchForTargets)
        {
            var tileScript = terrainTileDictionary[targetGrid];
            if (tileScript.isOccupied)
            {
                var occupierBpScript = tileScript.whatOccupiesMe.GetComponent<BattleParticipant>();
                if (!targetList.Contains(occupierBpScript))
                {
                    targetList.Add(occupierBpScript);
                    Debug.Log("Adding " + occupierBpScript.characterName + " to the target list.");
                }
            }
        }

        //Now the Battle Participants on target squares are known.

        //Finally we search the battlefield for Passive Abilities that could influence the outcome.
        //
        //Technically this step is defunct since I wrote a universal Passive-finding function, but leaving it for now until I have a reason not to.
        //
        //

        var passiveAbilitiesOnField = FindObjectsOfType<PassiveAbility>();
        foreach (PassiveAbility passiveAbility in passiveAbilitiesOnField)
        {
            //For each passive ability, we check to see if it can influence the outcome. First, if it's ON the user or on one of the targets.
            if (passiveAbility.participantIAmAttachedTo == user)
            {
                influencerList.Add(passiveAbility);
            }
            else if (targetList.Contains(passiveAbility.participantIAmAttachedTo))
            {
                influencerList.Add(passiveAbility);
            }
            else if (passiveAbility.abilityAreaOfEffect > 0)
            {
                bool abilityIsGood = false;
                List<int> rangeList = new List<int>();
                //If we're going to check if an ability of somebody nearby influences, we first need to know if they're an ally or an enemy.
                //If an ally, and the ability affects allies, we check the range to the ability user.
                //If an ally, and the ability affects enemies, we check the range to each of the targets.
                //Vice versa if an enemy.


                //First we do the checks if the passive ability belongs to an ally.
                //If at any point we get a positive hit, we add the ability and skip the rest.
                if (passiveAbility.participantIAmAttachedTo.myTeam == user.myTeam)
                {
                    if (passiveAbility.affectsAllies)
                    {
                        foreach (Vector2Int influencerGrid in passiveAbility.participantIAmAttachedTo.gridPositionsIAmOver)
                        {
                            if (abilityIsGood)
                            {
                                break;
                            }
                            foreach (Vector2Int influencedGrid in user.gridPositionsIAmOver)
                            {
                                //If the distance between any of the influencer's squares and any of the target's squares is = or less than the ability's area of effect, it's relevant.
                                int squareRange = GetTheNumberOfStepsBetweenTwoGrid(influencerGrid, influencedGrid);
                                if (squareRange <= passiveAbility.abilityAreaOfEffect)
                                {
                                    influencerList.Add(passiveAbility);
                                    abilityIsGood = true;
                                    break;

                                }
                            }
                        }
                    }
                    else if (passiveAbility.affectsEnemies && !abilityIsGood)
                    {
                        foreach (Vector2Int influencerGrid in passiveAbility.participantIAmAttachedTo.gridPositionsIAmOver)
                        {
                            if (abilityIsGood)
                            {
                                break;
                            }
                            foreach (BattleParticipant target in targetList)
                            {
                                foreach (Vector2Int influencedGrid in user.gridPositionsIAmOver)
                                {
                                    int squareRange = GetTheNumberOfStepsBetweenTwoGrid(influencerGrid, influencedGrid);
                                    if (squareRange <= passiveAbility.abilityAreaOfEffect)
                                    {
                                        influencerList.Add(passiveAbility);
                                        abilityIsGood = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                //Next we do the checks if the passive ability belongs to an enemy.
                else if (passiveAbility.participantIAmAttachedTo.myTeam != user.myTeam && !abilityIsGood)
                {
                    if (passiveAbility.affectsAllies)
                    {
                        foreach (Vector2Int influencerGrid in passiveAbility.participantIAmAttachedTo.gridPositionsIAmOver)
                        {
                            if (abilityIsGood)
                            {
                                break;
                            }
                            foreach (BattleParticipant target in targetList)
                            {
                                foreach (Vector2Int influencedGrid in user.gridPositionsIAmOver)
                                {
                                    int squareRange = GetTheNumberOfStepsBetweenTwoGrid(influencerGrid, influencedGrid);
                                    if (squareRange <= passiveAbility.abilityAreaOfEffect)
                                    {
                                        influencerList.Add(passiveAbility);
                                        abilityIsGood = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (passiveAbility.affectsEnemies)
                    {
                        foreach (Vector2Int influencerGrid in passiveAbility.participantIAmAttachedTo.gridPositionsIAmOver)
                        {
                            if (abilityIsGood)
                            {
                                break;
                            }
                            foreach (Vector2Int influencedGrid in user.gridPositionsIAmOver)
                            {
                                //If the distance between any of the influencer's squares and any of the target's squares is = or less than the ability's area of effect, it's relevant.
                                int squareRange = GetTheNumberOfStepsBetweenTwoGrid(influencerGrid, influencedGrid);
                                if (squareRange <= passiveAbility.abilityAreaOfEffect)
                                {
                                    influencerList.Add(passiveAbility);
                                    abilityIsGood = true;
                                    break;

                                }
                            }
                        }
                    }
                }

            }



        }

        //At this point the influencerList contains all the passive abilities that are relevant.

        outputAction.influencingPassiveAbilityList = influencerList;
        outputAction.targetList = targetList;

        return outputAction;
    }

    public RefinedProposedAction RefineAnAction(ProposedAction actionToRefine)
    {
        var outputAction = new RefinedProposedAction();
        outputAction.actionToRefine = actionToRefine;
        outputAction.chanceToHit = CalculateToHitOdds(actionToRefine);
        outputAction.defenderEvadeChanceDictionary = CalculateChanceToEvade(actionToRefine);
        outputAction.meanRawDmg = CalculateMeanRawDmg(actionToRefine);
        outputAction.dmgVariance = actionToRefine.abilityToUse.powerVariance;
        outputAction.defenderFlatDefDictionary = CalculateFlatDmgReduction(actionToRefine);
        outputAction.defenderScaledDefDictionary = CalculateScaledDmgReduction(actionToRefine);
        outputAction.hitType = actionToRefine.abilityToUse.accuracyType;
        return outputAction;
    }

    public ActionOutcome GenerateAnActionOutcome(RefinedProposedAction actionToExecute)
    {
        //
        //This function performs all the to-hit-checks and damage rolls needed to calculate the total outcome of an action.
        //
        //It also saves a list of the hits, misses, and evades for the main defender who will appear in the combat animation so that that animation can be run accurately.
        //

        //First we set up the basic parameters of the action outcome, to be filled up below.
        var outputOutcome = new ActionOutcome();
        outputOutcome.actionTaken = actionToExecute;
        var actionAnimationDictionary = new Dictionary<int, string>();
        var actionDamageDictionary = new Dictionary<BattleParticipant, int>();
        var actionPushPullDictionary = new Dictionary<BattleParticipant, CharacterPosition>();
        var actionPassivesGivenDictionary = new Dictionary<BattleParticipant, List<PassiveAbility>>();
        var actionPassivesRemovedDictionary = new Dictionary<BattleParticipant, List<PassiveAbility>>();

        BattleParticipant primaryTarget = actionToExecute.actionToRefine.targetList[0];

        //Next we run through the process for the main target. If there's only one target, we stop there.
        //This is the target we need to make the special animation output dictionary for.

        actionAnimationDictionary = FindOutWhatHappensForCombatAnim(actionToExecute, primaryTarget);
        int primaryTargetDamage = ParseACombatAnimationDictionaryIntoASingleOutputDamage(actionAnimationDictionary);
        actionDamageDictionary.Add(primaryTarget, primaryTargetDamage);

        if (actionToExecute.actionToRefine.abilityToUse.movesTheTargets)
        {
            //Code here for moving targets
        }
        if (actionToExecute.actionToRefine.abilityToUse.movesTheUser)
        {
            //Code here for moving the user
        }
        if (actionToExecute.actionToRefine.abilityToUse.givesAPassiveAbility)
        {
            //Code here for adding passives
        }
        if (actionToExecute.actionToRefine.abilityToUse.removesAPassiveAbility)
        {
            //Code here for removing passives
        }

        //At this point for a single-target ability we're done, so we check for that outcome and return.



        if (actionToExecute.actionToRefine.targetList.Count == 1)
        {
            outputOutcome.actionAnimationHitOutcomeDictionary = actionAnimationDictionary;
            outputOutcome.actionDamageDictionary = actionDamageDictionary;
            outputOutcome.actionPushPullDictionary = actionPushPullDictionary;
            outputOutcome.actionPassiveAbilityGivenDictionary = actionPassivesGivenDictionary;
            outputOutcome.actionPassiveAbilityRemovedDictionary = actionPassivesRemovedDictionary;
            //Debug.Log("If we executed this ability, the total damage against the primary target would be " + outputOutcome.actionDamageDictionary[primaryTarget]);
            return outputOutcome;
        }

        else
        {
            //Otherwise we repeat the process for all the remaining tagets in the list.
            for (int i = 1; i < actionToExecute.actionToRefine.targetList.Count; i++)
            {
                BattleParticipant targetToWorkWith = actionToExecute.actionToRefine.targetList[i];
            }
        }


        return null;
    }

    public void TellAllBattleParticipantsAboutAnActionOutcome(ActionOutcome actionToTalkAbout)
    {
        //
        //This function executes after a combat animation plays.
        //Its job is to update the health, stamina, positions, and passive abilities of all units on the field in the wake of the action.
        //

        BattleParticipant attacker = actionToTalkAbout.actionTaken.actionToRefine.actor;
        attacker.currentCpu -= actionToTalkAbout.actionTaken.actionToRefine.abilityToUse.cpuCost;

        foreach (BattleParticipant bp in actionToTalkAbout.actionTaken.actionToRefine.targetList)
        {
            bp.currentHealth -= actionToTalkAbout.actionDamageDictionary[bp];
            if (bp.currentHealth <= 0)
            {
                bp.Die();
            }
            //Additional changes to this function will happen as we implement other kinds of action outcomes.
        }

    }

    public bool RollForAccuracy(RefinedProposedAction actionToRollFor, BattleParticipant targetInTargetList)
    {
        //This function rolls whether or not a given hit of an ability hits or misses.
        //It returns true for a hit, false for a miss.
        //This applies only to one hit.

        float accuracyAgainstTarget = actionToRollFor.chanceToHit[targetInTargetList];

        float randomRoll = Random.Range(0f, 1f);

        //Debug.Log("For this hit accuracy is " + accuracyAgainstTarget + " and the random roll is " + randomRoll);

        if (randomRoll > accuracyAgainstTarget)
        {
            //Debug.Log("This should be a miss, returning false.");
            return false;
        }

        return true;
    }

    public bool RollForEvade(RefinedProposedAction actionToRollFor, BattleParticipant targetInTargetList)
    {
        //This function returns true if the defender evades a given hit, false if not.
        //This applies only for one hit.

        float targetEvade = actionToRollFor.defenderEvadeChanceDictionary[targetInTargetList];

        float randomRoll = Random.Range(0f, 1f);

        if (randomRoll < targetEvade)
        {
            return true;
        }

        return false;
    }

    public int RollForDamage(RefinedProposedAction actionToRollFor, BattleParticipant targetInTargetList)
    {
        //This function returns the damage of a given hit of an ability.
        //This applies only for one hit.

        //We start with the base damage.

        float baseDamage = actionToRollFor.meanRawDmg;
        float outputDamageFloat = baseDamage;

        //Then we take a variance roll.

        float damageVariance = actionToRollFor.dmgVariance;
        float varianceRoll = Random.Range(-damageVariance, damageVariance);
        outputDamageFloat += outputDamageFloat * varianceRoll;

        //Then we modify the damage based on accuracy if the attack is of Gaussian or Hybrid hit types.

        //

        //Then we check for flanking bonuses.

        if (CheckForFlanking(actionToRollFor.actionToRefine, targetInTargetList) == "Rear")
        {
            outputDamageFloat *= 1.33f;
        }

        //Then we adjust for defense
        //This remains commented until we implement defense with the humans

        //int defenderFlatReduction = actionToRollFor.defenderFlatDefDictionary[targetInTargetList];
        //outputDamageFloat -= defenderFlatReduction;

        //float defenderScaledReduction = actionToRollFor.defenderScaledDefDictionary[targetInTargetList];
        //outputDamageFloat *= defenderScaledReduction;

        //Finally we round to int

        return Mathf.RoundToInt(outputDamageFloat);
    }

    public int FindDamageResultForActionForOneCharacter(RefinedProposedAction actionToRollFor, BattleParticipant target)
    {
        //This function sums up over all the hits of an ability and outputs the total damage the target would receive.
        //To do so it repeatedly calls the RollFor() functions.
        return 0;
    }

    public CharacterPosition FindPositionChangeForOneCharacter(RefinedProposedAction actionToRollFor, BattleParticipant participantToMove)
    {
        return null;
    }

    public void ProcessPassiveAbilityResultForOneCharacter(RefinedProposedAction actionToRollFor, BattleParticipant targetInTargetList)
    {
        return;
    }

    public Dictionary<int, string> FindOutWhatHappensForCombatAnim(RefinedProposedAction actionToRollFor, BattleParticipant primaryTarget)
    {
        //
        //This function outputs the exact miss/evade/damage status of an ability for exporting to the Battle Animation Controller.
        //The "int" is the hit index, the "string" is either Miss, Evade, or a string representing the number value of the damage.
        //

        var outputDictionary = new Dictionary<int, string>();
        int numberOfHits = actionToRollFor.actionToRefine.abilityToUse.numberOfBursts * actionToRollFor.actionToRefine.abilityToUse.roundsPerBurst;

        for (int i = 0; i < numberOfHits; i++)
        {
            if (RollForAccuracy(actionToRollFor, primaryTarget) == false)
            {
                outputDictionary.Add(i, "Miss");
            }
            else
            {
                if (RollForEvade(actionToRollFor, primaryTarget))
                {
                    outputDictionary.Add(i, "Evade");
                }
                else
                {
                    int damage = RollForDamage(actionToRollFor, primaryTarget);
                    string dmgString = damage.ToString();
                    outputDictionary.Add(i, dmgString);
                }
            }
        }

        //This code debug logs the dictionary.

        foreach (int i in outputDictionary.Keys)
        {
            Debug.Log("Hit number " + i + " has damage roll " + outputDictionary[i]);
        }

        //

        return outputDictionary;
    }

    public int ParseACombatAnimationDictionaryIntoASingleOutputDamage(Dictionary<int, string> combatAnimDictionary)
    {

        int totalDamage = 0;

        foreach (string hitOutcomeString in combatAnimDictionary.Values)
        {
            if (hitOutcomeString != "Miss" && hitOutcomeString != "Evade")
            {
                int hitDamage = int.Parse(hitOutcomeString);
                totalDamage += hitDamage;
            }
        }

        return totalDamage;
    }

    public Dictionary<BattleParticipant, float> CalculateToHitOdds(ProposedAction actionToCalculate)
    {
        //This function returns the to-hit number as a float between 0 and 1 for each hit of the ability to use for each target.

        var outputDictionary = new Dictionary<BattleParticipant, float>();

        foreach (BattleParticipant targetInTargetList in actionToCalculate.targetList)
        {
            if (actionToCalculate.abilityToUse.accuracyType == "Gaussian")
            {
                //If it's a Gaussian ability then accuracy affects damage, not to-hit chance, so we say 1.
                outputDictionary.Add(targetInTargetList, 1);
            }

            else
            {
                //The first number we look at is the ability's base accuracy. It's the starting building block.
                float abilityBaseAccuracy = actionToCalculate.abilityToUse.accuracyMean;
                float outputOdds = abilityBaseAccuracy;
                //The first modification comes from the battle participant. Its accuracy modifier is multiplied by the base accuracy.
                float battleParticipantBaseAccuracy = actionToCalculate.actor.accuracy;
                outputOdds *= battleParticipantBaseAccuracy;
                //The next modification comes from passives affecting the actor.
                var relevantPassives = actionToCalculate.actor.passiveAbilitiesAffectingMe;
                float passiveAccMods = 1;
                float passiveAccFlatMods = 0;
                foreach (PassiveAbility passive in relevantPassives)
                {
                    passiveAccMods *= passive.accuracyModifier;
                    passiveAccFlatMods += passive.accuracyBonus;
                }
                outputOdds *= passiveAccMods;
                outputOdds += passiveAccFlatMods;

                if (actionToCalculate.abilityToUse.accuracyType == "Hybrid")
                {
                    //If accuracy is hybrid, then only half of the misses are true misses, and the rest are power-adjusted.
                    //In this case we modify output odds so that the actual to-hit chance is halved.
                    outputOdds += (1f - outputOdds) * 0.5f;
                }

                //The final modification comes from checking for height difference, as long as the attack isn't a Homing or Melee type.

                if (actionToCalculate.abilityToUse.trajectory != "Homing" && actionToCalculate.abilityToUse.trajectory != "Melee")
                {
                    float heightBonus = AdjustAccuracyForHeight(actionToCalculate, targetInTargetList);
                    outputOdds += heightBonus;
                }

                outputDictionary.Add(targetInTargetList, outputOdds);


            }

        }
        return outputDictionary;
    }

    public string CheckForFlanking(ProposedAction actionToCalculate, BattleParticipant target)
    {
        //
        //This function checks for a given target of an ability whether they are facing toward the line of sight from the actor.
        //If not, it's either a flank or a rear strike.
        //It outputs "Direct," "Side," or "Rear."
        //

        string dominantVector = FindCompassDirectionToTarget(actionToCalculate, target);
        string targetFacing = target.myOrientation;

        return FindRelativeFacingToCompassVector(targetFacing, dominantVector);
    }

    public float AdjustAccuracyForHeight(ProposedAction actionToCalculate, BattleParticipant target)
    {
        //
        //This function outputs a bonus accuracy, positive or negative, based on the height difference between the actor and target.
        //
        float bonusPerHeightUnit = 0.035f;
        int attackerSquareHeight = terrainTileDictionary[actionToCalculate.actor.gridPositionsIAmOver[0]].height;
        attackerSquareHeight += FindHeightBonusBasedOnMoveType(actionToCalculate.actor);
        int defenderSquareHeight = terrainTileDictionary[target.gridPositionsIAmOver[0]].height;
        defenderSquareHeight += FindHeightBonusBasedOnMoveType(target);
        int heightDifference = attackerSquareHeight - defenderSquareHeight;
        return heightDifference * bonusPerHeightUnit;
    }

    public Dictionary<BattleParticipant, float> CalculateChanceToEvade(ProposedAction actionToCalculate)
    {
        //
        //This function returns the chance-to-evade number as a float between 0 and 1 for each hit of the ability to use.
        //
        //Since there are potentially multiple targets, we can't output a single number, instead we output a Dictionary that links the Battle Participant with their Chance to Evade.
        //

        var outputDictionary = new Dictionary<BattleParticipant, float>();

        foreach (BattleParticipant target in actionToCalculate.targetList)
        {
            //The first number we look at is the defender's base evade. It's the starting building block.
            float outputEvade = target.evade;

            //The next modification comes from passives affecting the defender.
            var relevantPassives = target.passiveAbilitiesAffectingMe;
            foreach (PassiveAbility passive in relevantPassives)
            {
                outputEvade += passive.evadeBonus;
            }

            //Finally we check to see if it's a flanking strike. If so, evade chance is either halved or nullified.

            string flankStatus = CheckForFlanking(actionToCalculate, target);
            if (flankStatus == "Side")
            {
                outputEvade *= 0.5f;
            }
            else if (flankStatus == "Rear")
            {
                outputEvade = 0;
            }

            //That's all the potential modifiers for Evade, so we set up the dictionary entry.
            outputDictionary.Add(target, outputEvade);
        }
        return outputDictionary;
    }

    public float CalculateMeanRawDmg(ProposedAction actionToCalculate)
    {
        //This function returns the mean damage value as a float as modified by various passives.

        //The first number we look at is the ability's base power. It's the starting building block.
        float abilityBasePower = actionToCalculate.abilityToUse.power;
        float outputDmg = abilityBasePower;
        //The next modification comes from passives affecing the actor.
        var relevantPassives = actionToCalculate.actor.passiveAbilitiesAffectingMe;
        float passivePowerMods = 1;
        float passivePowerFlatMods = 0;
        foreach (PassiveAbility passive in relevantPassives)
        {
            passivePowerMods *= passive.powerModifier;
            passivePowerFlatMods += passive.powerBonus;

        }
        outputDmg *= passivePowerMods;
        outputDmg += passivePowerFlatMods;

        return outputDmg;
    }

    public Dictionary<BattleParticipant, int> CalculateFlatDmgReduction(ProposedAction actionToCalculate)
    {
        Debug.Log("Flat damage reduction is not implemented yet, but if it were, we would calculate it here");
        return null;
    }

    public Dictionary<BattleParticipant, float> CalculateScaledDmgReduction(ProposedAction actionToCalculate)
    {
        Debug.Log("Scaled damage reduction is not implemented, it would be calculated here");
        return null;
    }


    public class ProposedAction
    {
        //A proposed action is the first step in executing an active ability. It gathers together all the info in the battle map that could possible affect the outcome of the action.
        public ActiveAbility abilityToUse;
        public BattleParticipant actor;
        public List<BattleParticipant> targetList;
        public List<PassiveAbility> influencingPassiveAbilityList;
    }

    public class RefinedProposedAction
    {
        //A refined proposed action is the second step and is used to do the actual to-hit rolls and generate the ability outcome preview.
        public ProposedAction actionToRefine;
        public Dictionary<BattleParticipant, float> chanceToHit;
        public Dictionary<BattleParticipant, float> defenderEvadeChanceDictionary;
        public float meanRawDmg;
        public float dmgVariance;
        public Dictionary<BattleParticipant, int> defenderFlatDefDictionary;
        public Dictionary<BattleParticipant, float> defenderScaledDefDictionary;
        public string hitType;
    }

    public class ActionOutcome
    {
        public RefinedProposedAction actionTaken;
        public Dictionary<int, string> actionAnimationHitOutcomeDictionary;
        public Dictionary<BattleParticipant, int> actionDamageDictionary;
        public Dictionary<BattleParticipant, CharacterPosition> actionPushPullDictionary;
        public Dictionary<BattleParticipant, List<PassiveAbility>> actionPassiveAbilityGivenDictionary;
        public Dictionary<BattleParticipant, List<PassiveAbility>> actionPassiveAbilityRemovedDictionary;
    }

    //
    //End Taking-An-Action related functions
    //

    //
    //Begin Passive Ability related functions
    //
    //These functions govern Passive Ability behavior -- who has them, what they do, how long they last.
    //

    public bool DoesThisPassiveAffectThisActor(PassiveAbility passiveToCheck, BattleParticipant personAffected)
    {
        //This function checks whether or not a passive ability affects the character checked based on whom it affects, who has the passive, and where that person is.
        return true;
    }

    public List<BattleParticipant> UpdateActorsAffectedByPassiveAbility(PassiveAbility passiveToCheck)
    {
        //This function takes a Passive Ability on the map and finds all Battle Participants it affects. It refreshes its list of people affected by it and repopulates it. It then adds itself to the list of Passive Abilities affecting that participant. If it cannot do so and the Participant thinks it's affected, it clears that.

        passiveToCheck.participantsIAffect.Clear();
        var outputList = new List<BattleParticipant>();
        var participantsInMap = FindObjectsOfType<BattleParticipant>();
        var participantsInRange = new List<BattleParticipant>();

        //This simplest case is if the passive only affects self, which should be true in a lot of cases. If so we flag it as such and end.
        if (passiveToCheck.affectsSelf && !passiveToCheck.affectsAllies && !passiveToCheck.affectsEnemies)
        {
            passiveToCheck.participantsIAffect.Add(passiveToCheck.participantIAmAttachedTo);
            outputList.Add(passiveToCheck.participantIAmAttachedTo);
            return outputList;
        }

        //Otherwise we need to do a scan.

        foreach (BattleParticipant bp in participantsInMap)
        {
            //First we check if the given battle participant is eligible based on who the ability affects
            bool participantValidTarget = false;
            if (bp.myTeam == passiveToCheck.participantIAmAttachedTo.myTeam && passiveToCheck.affectsAllies)
            {
                participantValidTarget = true;
            }
            else if (passiveToCheck.participantIAmAttachedTo == bp && passiveToCheck.affectsSelf)
            {
                participantValidTarget = true;
            }
            else if (bp.myTeam != passiveToCheck.participantIAmAttachedTo.myTeam && passiveToCheck.affectsEnemies)
            {
                participantValidTarget = true;
            }

            //At this point we know it's a valid target, so we compare the range.

            bool inRange = false;
            foreach (Vector2Int abilityHaverGrid in passiveToCheck.participantIAmAttachedTo.gridPositionsIAmOver)
            {
                if (inRange)
                {
                    break;
                }
                foreach (Vector2Int targetGrid in bp.gridPositionsIAmOver)
                {
                    int rangeToTarget = GetTheNumberOfStepsBetweenTwoGrid(abilityHaverGrid, targetGrid);
                    if (rangeToTarget <= passiveToCheck.abilityAreaOfEffect)
                    {
                        inRange = true;
                        break;
                    }
                }
            }

            //If we're either not in range or not valid, and we THINK the ability affects us, we need to correct the record by deleting the reference inside the BP script we're looking at (assuming it exists).

            if (!inRange || !participantValidTarget)
            {
                if (bp.passiveAbilitiesAffectingMe.Contains(passiveToCheck))
                {
                    bp.passiveAbilitiesAffectingMe.Remove(passiveToCheck);
                }
            }

            //At this point we know we're in range, and we know it's a valid ability, so we should link it all up, first checking if it's already linked up..

            else
            {
                if (!outputList.Contains(bp))
                {
                    outputList.Add(bp);
                }
                if (!bp.passiveAbilitiesAffectingMe.Contains(passiveToCheck))
                {
                    bp.passiveAbilitiesAffectingMe.Add(passiveToCheck);
                }
                if (!passiveToCheck.participantsIAffect.Contains(bp))
                {
                    passiveToCheck.participantsIAffect.Add(bp);
                }
            }

        }
        return outputList;
    }

    public void AssignPassivesToThoseAffected()
    {
        //This function is designed to be called every time a unit moved.
        //It looks through the battlefield for all Passive Abilities.
        //Then, based on their areas of effect and whom they affect, it fills up the passivesAffectingMeList of all battle participants.
        var passivesInMap = FindObjectsOfType<PassiveAbility>();
        foreach (PassiveAbility passive in passivesInMap)
        {
            UpdateActorsAffectedByPassiveAbility(passive);
        }
    }


    //
    //End Passive Ability related functions
    //

    //
    //Begin Miscellaneous Helper Functions for working with the battle grid
    //

    public void HighlightAListOfSquares(List<Vector2Int> list)
    {
        foreach (Vector2Int gridSquare in list)
        {
            terrainTileDictionary[gridSquare].SetActive();
            terrainTileDictionary[gridSquare].HighlightMe();
        }
    }

    public void UnHighlightAListOfSquares(List<Vector2Int> list)
    {
        foreach (Vector2Int gridSquare in list)
        {
            terrainTileDictionary[gridSquare].EndPreparingForMechMove();
        }
    }

    public IEnumerator WaitThenUnHighlight(List<Vector2Int> listOfSquares, float waitTime)
    {
        //Debug.Log("Waiting...");
        yield return new WaitForSeconds(waitTime);
        //Debug.Log("Wait done!");
        UnHighlightAListOfSquares(listOfSquares);
    }

    public Vector2Int SearchForTheNearestGridSquare(Vector2 worldLocation)
    {
        //This search function uses an increasing fudge factor, searching through the dictionary of terrain tile positions and finding one that is within the search range.
        //It is more reliable than the other function but may in some cases return the wrong square, though that hasn't happened yet.

        foreach (Vector2 worldPos in terrainTilePositions)
        {
            if (Vector2.SqrMagnitude(worldPos - worldLocation) < 0.005)
            {
                return terrainWorldToGridDictionary[worldPos];
            }
        }
        foreach (Vector2 worldPos in terrainTilePositions)
        {
            if (Vector2.SqrMagnitude(worldPos - worldLocation) < 0.01)
            {
                return terrainWorldToGridDictionary[worldPos];
            }
        }
        foreach (Vector2 worldPos in terrainTilePositions)
        {
            if (Vector2.SqrMagnitude(worldPos - worldLocation) < 0.1)
            {
                return terrainWorldToGridDictionary[worldPos];
            }
        }
        Debug.Log("The function SearchForTheNearestGridSquare failed and is returning 0, 0.");
        return new Vector2Int(0, 0);
    }

    public Vector2Int FindTheNearestGridPosition(Vector2 positionToSearchFrom)
    {
        //This search function works by converting the Vector2 position of a location into a series of North and East steps, then rounding the quotient to the nearest int.
        //It is not 100% reliable and fails with different elevations.
        //
        //This function may struggle when dealing with elevations. In fact, it ought only to work if the output Vector2Int is the same elevation as the origin tile.
        //The reason is that this function starts from the actual world position of the Protocol Anchor, and assigns a grid position based on the number of north / east steps to get there. But if the protocol anchor is on a higher height, the point we reach won't be at the location defined by the number of steps -- it'll be plus some value of y.
        //The workaround is to call this function not on the actual Vector2 of the protocol grid position, but on a Vector2 adjusted by a factor determined by the known height (set up in the Inspector).
        var worldOrigin = terrainTilePositions[0];
        var distanceFromOrigin = positionToSearchFrom - worldOrigin;
        var gridStepsRight = distanceFromOrigin.x * 2;
        var gridStepsUpDown = distanceFromOrigin.y * 4;
        var stepsEast = (gridStepsRight - gridStepsUpDown) / 2;
        var stepsNorth = gridStepsUpDown + stepsEast;
        var gridEast = Mathf.RoundToInt(stepsEast);
        var gridNorth = Mathf.RoundToInt(stepsNorth);
        return new Vector2Int(gridEast, gridNorth);
    }

    public static int SortSquaresByNorthwardness(Vector2Int firstSquare, Vector2Int secondSquare)
    {

        if (firstSquare.y > secondSquare.y)
        {
            return 1;
        }
        else if (firstSquare.y < secondSquare.y)
        {
            return -1;
        }
        else return 0;

    }

    public static int SortSquaresBySouthwardness(Vector2Int firstSquare, Vector2Int secondSquare)
    {

        if (firstSquare.y > secondSquare.y)
        {
            return -1;
        }
        else if (firstSquare.y < secondSquare.y)
        {
            return 1;
        }
        else return 0;

    }

    public static int SortSquaresByEastwardness(Vector2Int firstSquare, Vector2Int secondSquare)
    {

        if (firstSquare.x > secondSquare.x)
        {
            return 1;
        }
        else if (firstSquare.x < secondSquare.x)
        {
            return -1;
        }
        else return 0;
    }

    public static int SortSquaresByWestwardness(Vector2Int firstSquare, Vector2Int secondSquare)
    {

        if (firstSquare.x > secondSquare.x)
        {
            return -1;
        }
        else if (firstSquare.x < secondSquare.x)
        {
            return 1;
        }
        else return 0;

    }

    public Vector2Int FindTheNearestApproach(Vector2Int startSquare, Vector2Int squareToApproach)
    {
        //This function outputs the square to hop to out of the 8 surrounding in order to get closest to the target square.
        var distanceDictionary = new Dictionary<float, Vector2Int>();
        var distanceList = new List<float>();
        var surroundingSquares = FindAllSurroundingSquares(startSquare);
        foreach (Vector2Int grid in surroundingSquares)
        {
            var distanceToTarget = Vector2.SqrMagnitude(squareToApproach - grid);
            distanceList.Add(distanceToTarget);
            if (!distanceDictionary.ContainsKey(distanceToTarget))
            {
                distanceDictionary.Add(distanceToTarget, grid);
            }
        }
        distanceList.Sort();
        return distanceDictionary[distanceList[0]];
    }

    public List<Vector2Int> FindAllSurroundingSquares(Vector2Int startSquare)
    {
        //This function simply outputs a list of all 8 squares around a target square.
        //Note it does not filter out nonexistent squares, since for now its main use is in FindTheNearestApproach(), e.g. find the square nearest a target point.
        //That function won't ever try to approach off the board, so it's OK to have nonexistent squares in the list for now.
        var listToReturn = new List<Vector2Int>();
        var upVector = new Vector2Int(0, 1);
        var rightVector = new Vector2Int(1, 0);
        listToReturn.Add(startSquare + upVector);
        listToReturn.Add(startSquare - upVector);
        listToReturn.Add(startSquare + rightVector);
        listToReturn.Add(startSquare - rightVector);
        listToReturn.Add(startSquare + upVector + rightVector);
        listToReturn.Add(startSquare + upVector - rightVector);
        listToReturn.Add(startSquare - upVector + rightVector);
        listToReturn.Add(startSquare - upVector - rightVector);
        return listToReturn;
    }

    public List<Vector2Int> FindAllInterveningSquares(Vector2Int startSquare, Vector2Int squareToApproach)
    {
        var listToReturn = new List<Vector2Int>();
        //This function finds all squares that lie on the line connecting two squares, for the purpose of checking whether LOS is available.
        //It traces the most direct route from the beginning to the target, as well as from the target to the beginning, enclosing a space.
        //It then walks across the enclosed space step-wise, taking a transverse slice each step, and takes the center or center 2 squares in the slice.
        var squaresFoundInApproaches = new List<Vector2Int>();
        //As a safety valve we set a maxTimesToLoop variable which is twice the magnitude of the vector connecting the two squares.
        //We now run the Approach() algorithm repeatedly in both directions to find the boundaries of our intervening area.
        int maxTimesToLoop = Mathf.RoundToInt(Mathf.RoundToInt(Mathf.Sqrt(Vector2.SqrMagnitude(squareToApproach - startSquare))) * 2);
        var squareToSearchFromForward = startSquare;
        var squareToSearchFromBackward = squareToApproach;
        int loopNumber = 0;
        while (loopNumber < maxTimesToLoop && squareToSearchFromForward != squareToSearchFromBackward && squareToSearchFromForward != squareToApproach && squareToSearchFromBackward != startSquare)
        {
            loopNumber++;
            squareToSearchFromForward = FindTheNearestApproach(squareToSearchFromForward, squareToApproach);
            squareToSearchFromBackward = FindTheNearestApproach(squareToSearchFromBackward, startSquare);
            if (!squaresFoundInApproaches.Contains(squareToSearchFromForward) && squareToSearchFromForward != startSquare && squareToSearchFromForward != squareToApproach)
            {
                squaresFoundInApproaches.Add(squareToSearchFromForward);
            }
            if (!squaresFoundInApproaches.Contains(squareToSearchFromBackward) && squareToSearchFromBackward != startSquare && squareToSearchFromBackward != squareToApproach)
            {
                squaresFoundInApproaches.Add(squareToSearchFromBackward);
            }

        }

        //Just for debugging...
        //foreach (Vector2Int approachSquare in squaresFoundInApproaches)
        //{
        //    terrainTileDictionary[approachSquare].HighlightMeInGreen();
        //}
        //StartCoroutine(WaitThenUnHighlight(squaresFoundInApproaches, 3));
        //

        //Now squaresFoundInApproaches contains the boundaries of our critical area. We need to decide what its shape is to know how to work with it.
        if (FindLongestComponent(startSquare, squareToApproach) == new Vector2(0, 0))
        {
            //In this case the line connecting the two squares is perfectly diagonal, in which case we have a direct shot.
            //SquaresFoundInApproaches should then contain all the intervening squares, with the square in the middle counted twice, as the forward != backward condition should have been triggered.
            return squaresFoundInApproaches;
        }
        else if (FindLongestComponent(startSquare, squareToApproach) == new Vector2(1, 0))
        {
            //In this situation the line is oriented east/west.
            //We need to take a series of slices north/south along the line, running from the start square to the end square.
            var vectorToSlideSliceAlong = new Vector2Int();
            var yToStartFrom = startSquare.y;
            var yToGoTo = squareToApproach.y;
            var xToStartFrom = startSquare.x;
            var xToGoTo = squareToApproach.x;
            if (yToGoTo - yToStartFrom == 0)
            {
                //In this case we have a perfectly horizontal trajectory, and we can just take squaresFoundInApproaches again.
                return squaresFoundInApproaches;
            }
            else
            {
                if (xToStartFrom > xToGoTo)
                {
                    //In this case our primary vector points west, and we need to slice vertically while subtracting an x each time.
                    vectorToSlideSliceAlong = new Vector2Int(-1, 0);
                }
                else if (xToStartFrom < xToGoTo)
                {
                    //In this case the vector points east, so we need to slide stepwise east.
                    vectorToSlideSliceAlong = new Vector2Int(1, 0);
                }
            }
            var xToSlice = startSquare.x;
            int timesSliced = 0;
            while (xToSlice != xToGoTo && timesSliced < 10)
            {
                //Again we add a safety valve number-of-loops to prevent an infinite loop in case of a bug.
                timesSliced++;
                //This is the critical loop. For each x value running from start to finish squares, we find all the squares we found in out previous step and run them through a series of conditions.
                //Our goal is to find the squares that are critical for LOS-detection.
                //If there's only one square at that position, it's critical.
                //If there are two squares at that position, we have to count the number of squares in between.
                //If the number in between is odd, the center square is critical.
                //If the number in between is even, the center two squares are critical.
                //If the number in between is zero, both squares are critical.
                var squaresInSlice = new List<Vector2Int>();
                foreach (Vector2Int gridInCollection in squaresFoundInApproaches)
                {
                    //So now we go through our list of boundary squares and find the ones that lie along the slice we're taking.
                    if (gridInCollection.x == xToSlice)
                    {
                        squaresInSlice.Add(gridInCollection);
                    }
                }
                if (squaresInSlice.Count == 1)
                {
                    //In this case the boundaries intersect here and it is a critical square.
                    listToReturn.Add(squaresInSlice[0]);
                }
                else if (squaresInSlice.Count == 2)
                {
                    //Now we need to find the distance between the two boundary squares.
                    var distanceAlongSlice = Mathf.Abs(squaresInSlice[0].y - Mathf.Abs(squaresInSlice[1].y));
                    if (distanceAlongSlice == 1)
                    {
                        //In this case the squares are adjacent and both are critical.
                        listToReturn.Add(squaresInSlice[0]);
                        listToReturn.Add(squaresInSlice[1]);
                    }
                    else if (distanceAlongSlice % 2 == 0)
                    {
                        //In this case the distance along the slice is even.
                        //This actually means that the number of intervening squares is odd.
                        //Consequently we need to take the middle square, which we find by adding half the distance to the start square.
                        //At this stage we don't know which is the start square so we check squaresInSlice for the contained y values, making sure we add upward in y from the bottom y square.
                        if (squaresInSlice[0].y < squaresInSlice[1].y)
                        {
                            var vectorToAdd = new Vector2Int(0, distanceAlongSlice / 2);
                            listToReturn.Add(squaresInSlice[0] + vectorToAdd);
                        }
                        else
                        {
                            var vectorToAdd = new Vector2Int(0, distanceAlongSlice / 2);
                            listToReturn.Add(squaresInSlice[1] + vectorToAdd);
                        }
                    }
                    else if (distanceAlongSlice % 2 != 0)
                    {
                        //In this case the distance along the slice is odd, which means there are two middle squares.
                        //We find those two middle squares by taking distanceAlongSlize / 2 and adding and subtracting 0.5.
                        var vectorToAddOne = new Vector2Int(0, Mathf.RoundToInt((distanceAlongSlice / 2) + 0.5f));
                        var vectorToAddTwo = new Vector2Int(0, Mathf.RoundToInt((distanceAlongSlice / 2) - 0.5f));
                        //Then we repeat the logic from the distance-is-even step.
                        if (squaresInSlice[0].y < squaresInSlice[1].y)
                        {

                            listToReturn.Add(squaresInSlice[0] + vectorToAddOne);
                            listToReturn.Add(squaresInSlice[0] + vectorToAddTwo);
                        }
                        else
                        {
                            listToReturn.Add(squaresInSlice[1] + vectorToAddOne);
                            listToReturn.Add(squaresInSlice[1] + vectorToAddTwo);
                        }
                    }
                }

                //So now we've found the critical squares AT THIS SLICE and we want to take the next slice.
                xToSlice += vectorToSlideSliceAlong.x;

            }
        }

        else if (FindLongestComponent(startSquare, squareToApproach) == new Vector2(0, 1))
        {
            //In this situation the line is oriented north/south.
            //We need to take a series of slices east/west along the line, running from the start square to the end square.
            var vectorToSlideSliceAlong = new Vector2Int();
            var yToStartFrom = startSquare.y;
            var yToGoTo = squareToApproach.y;
            var xToStartFrom = startSquare.x;
            var xToGoTo = squareToApproach.x;
            if (xToGoTo - xToStartFrom == 0)
            {
                //In this case we have a perfectly vertical trajectory, and we can just take squaresFoundInApproaches again.
                return squaresFoundInApproaches;
            }
            else
            {
                if (yToStartFrom > yToGoTo)
                {
                    //In this case our primary vector points south, and we need to slice horizontally while subtracting a y each time.
                    vectorToSlideSliceAlong = new Vector2Int(0, -1);
                }
                else if (yToStartFrom < yToGoTo)
                {
                    //In this case the vector points north, so we need to slide stepwise north.
                    vectorToSlideSliceAlong = new Vector2Int(0, 1);
                }
            }
            var yToSlice = startSquare.y;
            int timesSliced = 0;
            while (yToSlice != yToGoTo && timesSliced < 10)
            {
                //Again we add a safety valve number-of-loops to prevent an infinite loop in case of a bug.
                timesSliced++;
                //This is the critical loop. For each y value running from start to finish squares, we find all the squares we found in out previous step and run them through a series of conditions.
                //Our goal is to find the squares that are critical for LOS-detection.
                //If there's only one square at that position, it's critical.
                //If there are two squares at that position, we have to count the number of squares in between.
                //If the number in between is odd, the center square is critical.
                //If the number in between is even, the center two squares are critical.
                //If the number in between is zero, both squares are critical.
                var squaresInSlice = new List<Vector2Int>();
                foreach (Vector2Int gridInCollection in squaresFoundInApproaches)
                {
                    //So now we go through our list of boundary squares and find the ones that lie along the slice we're taking.
                    if (gridInCollection.y == yToSlice)
                    {
                        squaresInSlice.Add(gridInCollection);
                    }
                }
                if (squaresInSlice.Count == 1)
                {
                    //In this case the boundaries intersect here and it is a critical square.
                    listToReturn.Add(squaresInSlice[0]);
                }
                else if (squaresInSlice.Count == 2)
                {
                    //Now we need to find the distance between the two boundary squares.
                    var distanceAlongSlice = Mathf.Abs(squaresInSlice[0].x - Mathf.Abs(squaresInSlice[1].x));
                    if (distanceAlongSlice == 1)
                    {
                        //In this case the squares are adjacent and both are critical.
                        listToReturn.Add(squaresInSlice[0]);
                        listToReturn.Add(squaresInSlice[1]);
                    }
                    else if (distanceAlongSlice % 2 == 0)
                    {
                        //In this case the distance along the slice is even.
                        //This actually means that the number of intervening squares is odd.
                        //Consequently we need to take the middle square, which we find by adding half the distance to the start square.
                        //At this stage we don't know which is the start square so we check squaresInSlice for the contained x values, making sure we add upward in x from the leftmost x square.
                        if (squaresInSlice[0].x < squaresInSlice[1].x)
                        {
                            var vectorToAdd = new Vector2Int(0, distanceAlongSlice / 2);
                            listToReturn.Add(squaresInSlice[0] + vectorToAdd);
                        }
                        else
                        {
                            var vectorToAdd = new Vector2Int(0, distanceAlongSlice / 2);
                            listToReturn.Add(squaresInSlice[1] + vectorToAdd);
                        }
                    }
                    else if (distanceAlongSlice % 2 != 0)
                    {
                        //In this case the distance along the slice is odd, which means there are two middle squares.
                        //We find those two middle squares by taking distanceAlongSlize / 2 and adding and subtracting 0.5.
                        var vectorToAddOne = new Vector2Int(0, Mathf.RoundToInt((distanceAlongSlice / 2) + 0.5f));
                        var vectorToAddTwo = new Vector2Int(0, Mathf.RoundToInt((distanceAlongSlice / 2) - 0.5f));
                        //Then we repeat the logic from the distance-is-even step.
                        if (squaresInSlice[0].x < squaresInSlice[1].x)
                        {

                            listToReturn.Add(squaresInSlice[0] + vectorToAddOne);
                            listToReturn.Add(squaresInSlice[0] + vectorToAddTwo);
                        }
                        else
                        {
                            listToReturn.Add(squaresInSlice[1] + vectorToAddOne);
                            listToReturn.Add(squaresInSlice[1] + vectorToAddTwo);
                        }
                    }
                }

                //So now we've found the critical squares AT THIS SLICE and we want to take the next slice.
                yToSlice += vectorToSlideSliceAlong.y;

            }
        }

        return listToReturn;
    }

    public Vector2Int FindLongestComponent(Vector2Int start, Vector2Int target)
    {
        //This function simply outputs whether the horizontal component of distance between two grid squares is greater or less than the vertical.
        //If horizontal component is greater, it outputs the unit right vector.
        //If vertical distance is greater, it outputs unit up.
        //If the distances are equal, it outputs (0,0).
        var distanceVector = target - start;
        if (distanceVector.x == distanceVector.y)
        {
            return new Vector2Int(0, 0);
        }
        else if (Mathf.Abs(distanceVector.x) > Mathf.Abs(distanceVector.y))
        {
            return new Vector2Int(1, 0);
        }
        else if (Mathf.Abs(distanceVector.x) < Mathf.Abs(distanceVector.y))
        {
            return new Vector2Int(0, 1);
        }
        else return new Vector2Int(0, 0);
    }

    public List<Vector2Int> ExpandBy90Degrees(List<Vector2Int> listToExpand, string orientation)
    {
        //This function is a building block in range-finding. It takes a set of grid positions and finds the next grid positions out for a 90 degree firing arc, by taking all the grid positions that are forward, left, and right of the existing ones. It is to be used iteratively to find a firing arc.
        List<Vector2Int> listToReturn = new List<Vector2Int>();
        foreach (Vector2Int startingGrid in listToExpand)
        {
            if (orientation == "North")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(0, 1);
                var leftVector = new Vector2Int(-1, 0);
                var rightVector = new Vector2Int(1, 0);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "South")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(0, -1);
                var leftVector = new Vector2Int(1, 0);
                var rightVector = new Vector2Int(-1, 0);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "East")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(1, 0);
                var leftVector = new Vector2Int(0, 1);
                var rightVector = new Vector2Int(0, -1);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "West")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(-1, 0);
                var leftVector = new Vector2Int(0, -1);
                var rightVector = new Vector2Int(0, 1);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
        }
        return listToReturn;
    }

    public List<Vector2Int> ExpandBy180Degrees(List<Vector2Int> listToExpand, string orientation)
    {
        List<Vector2Int> listToReturn = new List<Vector2Int>();
        foreach (Vector2Int startingGrid in listToExpand)
        {
            if (orientation == "North")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(0, 1);
                var leftVector = new Vector2Int(-1, 0);
                var rightVector = new Vector2Int(1, 0);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                var trueLeft = whereIam + leftVector;
                var trueRight = whereIam + rightVector;
                if (!listToReturn.Contains(trueLeft))
                {
                    if (terrainTileDictionary.ContainsKey(trueLeft))
                    {
                        listToReturn.Add(trueLeft);
                    }

                }
                if (!listToReturn.Contains(trueRight))
                {
                    if (terrainTileDictionary.ContainsKey(trueRight))
                    {
                        listToReturn.Add(trueRight);
                    }

                }
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "South")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(0, -1);
                var leftVector = new Vector2Int(1, 0);
                var rightVector = new Vector2Int(-1, 0);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                var trueLeft = whereIam + leftVector;
                var trueRight = whereIam + rightVector;
                if (!listToReturn.Contains(trueLeft))
                {
                    if (terrainTileDictionary.ContainsKey(trueLeft))
                    {
                        listToReturn.Add(trueLeft);
                    }

                }
                if (!listToReturn.Contains(trueRight))
                {
                    if (terrainTileDictionary.ContainsKey(trueRight))
                    {
                        listToReturn.Add(trueRight);
                    }

                }
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "East")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(1, 0);
                var leftVector = new Vector2Int(0, 1);
                var rightVector = new Vector2Int(0, -1);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                var trueLeft = whereIam + leftVector;
                var trueRight = whereIam + rightVector;
                if (!listToReturn.Contains(trueLeft))
                {
                    if (terrainTileDictionary.ContainsKey(trueLeft))
                    {
                        listToReturn.Add(trueLeft);
                    }

                }
                if (!listToReturn.Contains(trueRight))
                {
                    if (terrainTileDictionary.ContainsKey(trueRight))
                    {
                        listToReturn.Add(trueRight);
                    }

                }
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
            else if (orientation == "West")
            {
                var whereIam = startingGrid;
                var forwardVector = new Vector2Int(-1, 0);
                var leftVector = new Vector2Int(0, -1);
                var rightVector = new Vector2Int(0, 1);
                var forwardPosition = whereIam + forwardVector;
                var leftPosition = forwardPosition + leftVector;
                var rightPosition = whereIam + forwardVector + rightVector;
                var trueLeft = whereIam + leftVector;
                var trueRight = whereIam + rightVector;
                if (!listToReturn.Contains(trueLeft))
                {
                    if (terrainTileDictionary.ContainsKey(trueLeft))
                    {
                        listToReturn.Add(trueLeft);
                    }

                }
                if (!listToReturn.Contains(trueRight))
                {
                    if (terrainTileDictionary.ContainsKey(trueRight))
                    {
                        listToReturn.Add(trueRight);
                    }

                }
                if (!listToReturn.Contains(forwardPosition))
                {
                    if (terrainTileDictionary.ContainsKey(forwardPosition))
                    {
                        listToReturn.Add(forwardPosition);
                    }

                }
                if (!listToReturn.Contains(leftPosition))
                {
                    if (terrainTileDictionary.ContainsKey(leftPosition))
                    {
                        listToReturn.Add(leftPosition);
                    }

                }
                if (!listToReturn.Contains(rightPosition))
                {
                    if (terrainTileDictionary.ContainsKey(rightPosition))
                    {
                        listToReturn.Add(rightPosition);
                    }

                }
            }
        }
        return listToReturn;
    }

    public Vector2 FindTheCenterOfTheseGridSquares(List<Vector2Int> gridList)
    {
        //This is a support function used when loading a Protocol. A copy exists in the Battle Anchor script for when it's translating around.
        float summedX = 0;
        float summedY = 0;
        float numberInList = gridList.Count;
        foreach (Vector2Int vector in gridList)
        {
            var worldVector = terrainGridToWorldDictionary[vector];
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

    public int GetTheNumberOfStepsBetweenTwoGrid(Vector2Int start, Vector2Int target)
    {
        int stepsInY = Mathf.Abs(target.y - start.y);
        int stepsInX = Mathf.Abs(target.x - start.x);
        return stepsInY + stepsInX;
    }

    //
    //End Miscellaneous Helper Functions for working with the battle grid
    //





    //
    //Begin Miscellaneous Helper functions for working with the combat system
    //

    public int FindHeightBonusBasedOnMoveType(BattleParticipant participantToAssignBonus)
    {
        string participantMoveType = participantToAssignBonus.moveType;
        if (participantMoveType == "Walk" || participantMoveType == "Biped" || participantMoveType == "Large Beast")
        {
            return 1;
        }
        else if (participantMoveType == "Large Biped" || participantMoveType == "Large Quad")
        {
            return 2;
        }
        else if (participantMoveType == "Fly")
        {
            return 3;
        }
        else
        {
            Debug.Log("Encountered an unrecognized movetype while height bonus finding");
            return 0;
        }
    }

    public string FindCompassDirectionToTarget(ProposedAction actionToTake, BattleParticipant target)
    {
        //
        //This function finds the on-map direction of the line of sight from actor to target.
        //It's used to determine whether or not an attack is a flank or rear attack in the CheckForFlanking() function.
        //This function outputs North, South, East, West, Northeast, Southeast, Southwest, or Southeast.
        //

        //
        //Right now it just checks from the first square in the bp's list of occupied squares, but later we can refine that to make flank attacks easier or harder for large units.
        //

        Vector2Int gridToCheckFor = actionToTake.actor.gridPositionsIAmOver[0];
        Vector2Int gridToTarget = target.gridPositionsIAmOver[0];

        Vector2Int connectingVector = (gridToTarget - gridToCheckFor);

        if (Mathf.Abs(connectingVector.x) == Mathf.Abs(connectingVector.y))
        {
            //This is the case where the vector is perfectly diagonal, and we want to output one of the diagonal directions.
            if (connectingVector.x > 0 && connectingVector.y > 0)
            {
                return "Northeast";
            }
            else if (connectingVector.x > 0 && connectingVector.y < 0)
            {
                return "Southeast";
            }
            else if (connectingVector.x < 0 && connectingVector.y > 0)
            {
                return "Northwest";
            }
            else
            {
                return "Southwest";
            }
        }
        else if (Mathf.Abs(connectingVector.x) > Mathf.Abs(connectingVector.y))
        {
            //This is the case where the east/west component is dominant, and we'll output either East or West depending on signs.
            if (connectingVector.x > 0)
            {
                return "East";
            }
            else
            {
                return "West";
            }
        }
        else
        {
            //This is the case where the north/south component is dominant, and we'll output either North or South depending on signs.
            if (connectingVector.y > 0)
            {
                return "North";
            }
            else if (connectingVector.y < 0)
            {
                return "South";
            }
        }

        Debug.Log("We were unable to determine the compass vector to the target!");
        return null;
    }

    public string FindRelativeFacingToCompassVector(string myFacing, string compassVector)
    {
        //
        //This function outputs "Direct," "Side," or "Rear" based on the "myFacing" variable relative to the compass vector.
        //

        //First the rear-facing case, which is easiest.
        if (myFacing == compassVector)
        {
            return "Rear";
        }
        //Next we go through the facings one by one and scoop up the other 7 cases which sort into direct and side.
        else if (myFacing == "North")
        {
            if (compassVector == "South" || compassVector == "Southeast" || compassVector == "Southwest")
            {
                //I'm facing north, and the attack is coming at me southward, or southeastward, or southwestward, etc.
                return "Direct";
            }
            else if (compassVector == "West" || compassVector == "East" || compassVector == "Northeast" || compassVector == "Northwest")
            {
                return "Side";
            }
        }
        else if (myFacing == "East")
        {
            if (compassVector == "West" || compassVector == "Southwest" || compassVector == "Northwest")
            {
                return "Direct";
            }
            else if (compassVector == "North" || compassVector == "South" || compassVector == "Northeast" || compassVector == "Southeast")
            {
                return "Side";
            }
        }
        else if (myFacing == "South")
        {
            if (compassVector == "North" || compassVector == "Northeast" || compassVector == "Northwest")
            {
                return "Direct";
            }
            else if (compassVector == "East" || compassVector == "West" || compassVector == "Southeast" || compassVector == "Southwest")
            {
                return "Side";
            }
        }
        else if (myFacing == "West")
        {
            if (compassVector == "East" || compassVector == "Northeast" || compassVector == "Southeast")
            {
                return "Direct";
            }
            else if (compassVector == "North" || compassVector == "South" || compassVector == "Southwest" || compassVector == "Northwest")
            {
                return "Side";
            }
        }

        Debug.Log("We couldn't find the relative facing while flank checking! Myfacing is " + myFacing + "and the dominant vector is " + compassVector);
        return null;
    }


    //
    //End combat helper functions
    //

    //
    //Begin Scenario functions
    //
    //These functions are used to work with the Scenario system.
    //They include checking win conditions, triggering cut scenes and events, etc.
    //

    public bool CheckForWinConditions()
    {
        //
        //This function checks whether or not the win conditions for the battle have been satisfied.
        //It should be called after every action and after every major combat event so we know when to end the battle.
        //It returns "True" if so.
        //

        
        if (winConditionsForThisBattle.defeatAllMechs)
        {
            if (!CheckIfAnyCharactersRemainOnATeam(1))
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckForLossConditions()
    {
        if (!CheckIfAnyCharactersRemainOnATeam(0))
        {
            return true;
        }
        return false;
    }

    public bool CheckIfAnyCharactersRemainOnATeam(int teamToCheck)
    {
        //
        //This function returns true if any characters are left on team X, otherwise false.
        //It's used to check for win conditions and battle status.
        //
        foreach (BattleParticipant bp in FindObjectsOfType<BattleParticipant>())
        {
            if (bp.myTeam == teamToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator WinTheBattle()
    {
        //
        //This function contains all the events that should take place after the battle is won.
        //It should end by returning the player to their main hub.
        //

        yield return StartCoroutine(FlashWhoseTurn());
        yield return StartCoroutine(AssignRewards());
        ReturnToBase();
        yield return null;
        

    }

    public IEnumerator LoseTheBattle()
    {
        yield return StartCoroutine(FlashWhoseTurn());
        ReturnToBase();
        yield return null;
    }

    public IEnumerator AssignRewards()
    {
        yield return null;
    }

    public void ReturnToBase()
    {
        SceneManager.LoadScene("Mech Lab");
    }



    //
    //End Scenario functions
    //




    //
    //Initialization functions
    //
    //These functions are called once at the very beginning of a battle and never again.
    //

    public void InitializeProtocolSaveLoadFilepath()
    {
        {
            protocolRootFilepath = Path.Combine(Application.persistentDataPath, "Protocols");
            if (!Directory.Exists(protocolRootFilepath))
            {
                Debug.Log("Creating directory...");
                Directory.CreateDirectory(protocolRootFilepath);
            }
            //Debug.Log("The Protocols directory is " + protocolRootFilepath);
        }
    }

    public void InitializeDefaultPortraitSprite()
    {
        defaultCharacterImage = Resources.Load<Sprite>("DefaultCharacterPortrait");
    }

    public List<TerrainTile> FindAllTerrainTiles()
    {
        var foundTerrainTiles = GameObject.FindObjectsOfType<TerrainTile>();
        var terrainTileList = new List<TerrainTile>();
        foreach (TerrainTile tile in foundTerrainTiles)
        {
            terrainTileList.Add(tile);
        }
        return terrainTileList;
    }

    public void AssignTerrainTileScriptsTheirHeights(List<TerrainTile> tileList)
    {
        //This function takes terrain tiles and finds the parent of their parent, which by convention is the Tile Layer they're implanted in. Those tile layers have a set Y and Z position that determines how they render. We read these positions to assign the tiles their heights.
        foreach (TerrainTile tile in tileList)
        {
            var layerZ = tile.transform.parent.parent.transform.localPosition.z;
            var heightIWant = layerZ / tileHeightToWorldYScaleFactor;
            tile.height = Mathf.RoundToInt(heightIWant);
        }
    }
    public List<Vector2> ReadTheBattleGridSquares(List<TerrainTile> terrainTileScriptsInMap)
    {
        //The goals of this function are as follows: 1) Find all of the TerrainTiles on the map, 2) Assign each of them a GridX and GridY which represents their position... In order to do this, eventually we'll need to adjust for their altitude, but since this map is all flat we can skip that here... 3) Get all of these Terrain Tiles and their corresponding Grid X and Grid Y positions into an easily accessible memory structure. We might even need this to be serializable, since Protocols have to be saved and loaded, but it seems more likely we can just serialize the saved GridX and GridY for the mech in the Protocol, then reconstruct it later onto an imaginary 8x8 when we start a battle.

        var tilePositionList = new List<Vector2>();

        foreach (TerrainTile tile in terrainTileScriptsInMap)
        {
            tile.worldX = tile.transform.position.x;
            tile.worldY = tile.transform.position.y;
            var actualY = tile.worldY - (tile.height * tileHeightToWorldYScaleFactor);
            var tileWorldVector = new Vector2(tile.worldX, actualY);
            tilePositionList.Add(tileWorldVector);
        }

        tilePositionList.Sort(SortWorldVectorsForGrid);
        return tilePositionList;

        //terrainTilePositions = NormalizeTerrainTilePosition(terrainTilePositions);
    }

    public void DeactivateInertTiles(List<TerrainTile> listToClean)
    {
        //This function takes tiles marked as "Inert" in the inspector -- tiles which get painted but can't actually be walked on or interacted with -- and destroys their TerrainTile script.
        //It then sets all remaining tiles to their "inactive" state, with colliders disabled, so they don't eat resources.
        foreach (TerrainTile tile in listToClean)
        {
            if (tile.isInert)
            {
                tile.myCollider.enabled = false;
                Destroy(tile.myCollider);
                tile.enabled = false;
                Destroy(tile);
            }
            tile.SetInactive();
        }
    }

    public void SetUpTheGridPositions(List<Vector2> sortedTerrainTileList, bool workingWithSubGrid)
    {
        //The workingWithSubGrid variable applies when organizing the Protocol Anchors for set-up. We don't want to add those tiles again to the overall world Dictionaries of tiles, so we call that function with "True" and skip those steps.
        Vector2 oneStepEast = new Vector2(0.5f, -0.25f);
        //Debug.Log("One step east is defined as " + oneStepEast);
        Vector2 oneStepNorth = new Vector2(0.5f, 0.25f);
        //Debug.Log("One step north is defined as " + oneStepNorth);
        Vector2 originWorld = sortedTerrainTileList[0];
        int tileListSize = sortedTerrainTileList.Count;

        foreach (Vector2 terrainTileWorldVector in sortedTerrainTileList)
        {
            bool found = false;
            if (terrainTileWorldVector == originWorld)
            {
                Vector2Int gridVector = new Vector2Int(0, 0);
                if (!workingWithSubGrid)
                {
                    //Debug.Log("I don't think I'm working with subgrid.");
                    terrainTileGridPositions.Add(gridVector);
                    terrainWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                    terrainGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
                }
                if (workingWithSubGrid)
                {
                    subgridGridPositions.Add(gridVector);
                    subgridWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                    subgridGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
                }

                found = true;
            }
            else
            {
                Vector2 distanceFromOrigin = terrainTileWorldVector - originWorld;
                for (int gridX = 0; gridX < tileListSize; gridX++)
                {
                    if (found)
                    {
                        break;
                    }
                    for (int gridY = 0; gridY < tileListSize; gridY++)
                    {
                        if (found)
                        {
                            break;
                        }
                        var composedVector = (gridX * oneStepEast) + (gridY * oneStepNorth);
                        //Right here is the moment when the old Normalize() function was needed, so just for this step, in this function, we'll add in the starting offset:
                        composedVector += originWorld;
                        var composedVectorDistance = Vector2.SqrMagnitude(terrainTileWorldVector - composedVector);
                        if (composedVectorDistance < tileToGridFudgeFactor)
                        {
                            //Debug.Log("We found a matching position from our composed vector!");
                            var gridVector = new Vector2Int(gridX, gridY);
                            if (!workingWithSubGrid)
                            {
                                terrainTileGridPositions.Add(gridVector);
                                terrainWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                                terrainGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
                            }
                            if (workingWithSubGrid)
                            {
                                subgridGridPositions.Add(gridVector);
                                subgridWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                                subgridGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
                            }

                            found = true;
                        }
                    }
                }
            }


            if (!found)
            {
                problemTilePositions.Add(terrainTileWorldVector);
            }
        }
        foreach (Vector2 problemPosition in problemTilePositions)
        {
            Debug.Log("We have a problem location at " + problemPosition);
        }
    }


    public void AssignTilesToTheGrid(List<TerrainTile> tilesToAssign)
    {
        foreach (TerrainTile terrainTile in tilesToAssign)
        {
            //The tile finds its own World X and World Y before it knows its in-game height. The List<Vector2> of tile positions includes the adjustment for height. Since we're dipping back to the original Tile Script list, we need to adjust for height again here.
            var actualY = terrainTile.worldY - (terrainTile.height * tileHeightToWorldYScaleFactor);
            var tileWorldPosition = new Vector2(terrainTile.worldX, actualY);
            //Since we previously normalized all the positions in our world position List, then added those to the dictionary, we need to normalize again as we jump from the tile's stored world position to the list.
            //tileWorldPosition -= tileOriginOffset;
            //Debug.Log("My position is " + tileWorldPosition);
            var correspondingGrid = terrainWorldToGridDictionary[tileWorldPosition];
            //Debug.Log("The dictionary contains an entry here which is " + correspondingGrid);
            terrainTileDictionary.Add(correspondingGrid, terrainTile);
            terrainGridFromTileDictionary.Add(terrainTile, correspondingGrid);
            terrainTile.gridX = correspondingGrid.x;
            terrainTile.gridY = correspondingGrid.y;
        }
    }

    private static int SortWorldVectorsForGrid(Vector2 firstVector, Vector2 secondVector)
    //This is the sorting function for the world positions of the terrain tiles. It winds up sorting them going piecewise from left to right, bottom to top. We can assign grid positions algorithmically based on the resulting order.
    {
        if (firstVector.x == secondVector.x)
        {
            if (firstVector.y > secondVector.y)
            {
                return 1;
            }
            if (firstVector.y < secondVector.y)
            {
                return -1;
            }
            if (firstVector.y == secondVector.y)
            {
                return 0;
            }
        }

        if (firstVector.x < secondVector.x)
        {
            return -1;
        }
        if (firstVector.x > secondVector.x)
        {
            return 1;
        }

        else
        {
            return 0;
        }


    }
    public List<List<ProtocolAnchor>> OrganizeTheProtocolAnchorsByTeam(ProtocolAnchor[] anchors)
    {
        var list0 = new List<ProtocolAnchor>();
        var list1 = new List<ProtocolAnchor>();

        foreach (ProtocolAnchor pAnchor in anchors)
        {
            if (pAnchor.myTeam == 0)
            {
                list0.Add(pAnchor);
            }
            if (pAnchor.myTeam == 1)
            {
                list1.Add(pAnchor);
            }
        }

        var listOfLists = new List<List<ProtocolAnchor>>();
        listOfLists.Add(list0);
        listOfLists.Add(list1);
        return listOfLists;

    }

    public void AssignProtocolAnchorsOriginalGridSquares(List<List<ProtocolAnchor>> anchors)
    {
        foreach (List<ProtocolAnchor> team in anchors)
        {
            //First we set up dictionaries and lists to store data.
            var dictionaryAnchorToPosition = new Dictionary<ProtocolAnchor, Vector2>();
            var dictionaryPositionToAnchor = new Dictionary<Vector2, ProtocolAnchor>();
            var listOfPositions = new List<Vector2>();
            //Next we generate a list of positions and associate them with squares.
            foreach (ProtocolAnchor anchorSquare in team)
            {
                var squarePos = new Vector2(anchorSquare.transform.position.x, anchorSquare.transform.position.y);
                listOfPositions.Add(squarePos);
                dictionaryAnchorToPosition.Add(anchorSquare, squarePos);
                dictionaryPositionToAnchor.Add(squarePos, anchorSquare);
            }
            //Next we sort them to find the lower-left one.
            listOfPositions.Sort(SortWorldVectorsForGrid);
            SetUpTheGridPositions(listOfPositions, true);
            foreach (Vector2 position in listOfPositions)
            {
                //This is the point where we adjust the Protocol Anchor's position by its height.
                var protocolAnchorToSet = dictionaryPositionToAnchor[position];
                var protocolAnchorHeightVector = new Vector2(0, protocolAnchorToSet.myCurrentHeight * tileHeightToWorldYScaleFactor);
                protocolAnchorToSet.myOriginalGrid = subgridWorldToGridDictionary[position];
                protocolAnchorToSet.myCurrentGrid = FindTheNearestGridPosition(position - protocolAnchorHeightVector);
            }
            subgridGridPositions.Clear();
            subgridGridToWorldDictionary.Clear();
            subgridWorldToGridDictionary.Clear();

        }
    }

    //These functions are part of initializing protocols to the protocol anchors.

    public ProtocolSaveData FindAnEnemyProtocol(int protocolNumber)
    {
        //If 0, we find a random protocol. The range must be updated any time we add a random enemy to the library.
        if (protocolNumber == 0)
        {
            protocolNumber = Mathf.RoundToInt(Random.Range(0.6f, 5.4f));
        }
        string pathToLoad = Path.Combine(protocolRootFilepath, "Enemy" + protocolNumber + ".dat");
        string protocolJson = File.ReadAllText(pathToLoad);
        var protocol = JsonUtility.FromJson<ProtocolSaveData>(protocolJson);
        return protocol;
    }

    public ProtocolSaveData FindThePlayerProtocol()
    {
        if (MechPlayerManager.playerProtocol != null)
        {
            return MechPlayerManager.playerProtocol;
        }
        else
        {
            //If we don't have player data for some reason, use a random enemy outfit.
            return FindAnEnemyProtocol(0);
        }
    }

    public Scenario LoadTheScenario()
    {
        //This function reads the scenario from the Mech Player Manager, which should be put together on the Expeditions screen.
        //If no scenario exists there it constructs a Default Scenario with random parameters, copying the previous battle-load functionality.
        //
        //Ideally we want the Scenarios screen to build a complete, readable scenario here with what protocols to spawn and where to spawn them.

        if (MechPlayerManager.scenarioToLoad != null)
        {
            return MechPlayerManager.scenarioToLoad;
        }
        else
        {
            return ConstructDefaultScenario();
        }

    }

    public Scenario ConstructDefaultScenario()
    {
        //This function outputs a boilerplate scenario with a random enemy team and an objective of destroying all enemies.

        var scenarioToReturn = new Scenario();
        scenarioToReturn.winCondition = WinCondition.BasicSkirmish();
        scenarioToReturn.playerProtocol = FindThePlayerProtocol();
        var enemyProtocols = new List<ProtocolSaveData>();
        var protocolAnchorDictionary = new Dictionary<ProtocolSaveData, int>();
        foreach (List<ProtocolAnchor> protocolAnchorList in protocolAnchorsByTeam)
        {
            if (protocolAnchorList[0].myTeam != 0)
            {
                var enemyProtocol = FindAnEnemyProtocol(0);
                enemyProtocols.Add(enemyProtocol);
                protocolAnchorDictionary.Add(enemyProtocol, protocolAnchorList[0].myIndex);
            }
        }
        scenarioToReturn.enemyProtocols = enemyProtocols;
        scenarioToReturn.protocolsByProtocolAnchorIndex = protocolAnchorDictionary;
        scenarioToReturn.resourceOneToReward = 0;
        scenarioToReturn.resourceTwoToReward = 0;
        scenarioToReturn.resourceThreeToReward = 0;
        return scenarioToReturn;
    }

    public void PopulateTheBattlefield(List<List<ProtocolAnchor>> protocolAnchors, Scenario scenarioToLoad)
    {
        foreach (List<ProtocolAnchor> anchorsOfTeam in protocolAnchors)
        {
            var team = anchorsOfTeam[0].myTeam;
            if (team == playerTeamNumber)
            {
                //We've located the protocol anchors for the player team and will instantiate the player protocol here.
                if (SceneManager.GetActiveScene().name == "Mech Design Test Battle")
                {
                    string trainingProtocolFilepath = GameManager.mechTrainingProtocolFilepath;
                    string trainingProtocolJSON = File.ReadAllText(trainingProtocolFilepath);
                    ProtocolSaveData trainingProtocol = JsonUtility.FromJson<ProtocolSaveData>(trainingProtocolJSON);
                    InstantiateAProtocol(trainingProtocol, anchorsOfTeam);
                }
                else
                {
                    var playerProtocol = scenarioToLoad.playerProtocol;
                    InstantiateAProtocol(playerProtocol, anchorsOfTeam);
                }

            }
            else
            {
                //We've located the protocol anchors for a non-player team and will instantiate the protocol chosen by the scenario for these squares.
                if (SceneManager.GetActiveScene().name == "Mech Design Test Battle")
                {
                    string trainingDummyProtocolFilepath = GameManager.mechTrainingDummyProtocolFilepath;
                    string trainingDummyProtocolJSON = File.ReadAllText(trainingDummyProtocolFilepath);
                    ProtocolSaveData trainingDummyProtocol = JsonUtility.FromJson<ProtocolSaveData>(trainingDummyProtocolJSON);
                    InstantiateAProtocol(trainingDummyProtocol, anchorsOfTeam);
                }
                else
                {
                    int protocolIndex = anchorsOfTeam[0].myIndex;
                    foreach (ProtocolSaveData protocol in scenarioToLoad.enemyProtocols)
                    {
                        if (scenarioToLoad.protocolsByProtocolAnchorIndex[protocol] == protocolIndex)
                        {
                            InstantiateAProtocol(protocol, anchorsOfTeam);
                        }
                    }
                }
            }
        }
    }

    public void InstantiateAProtocol(ProtocolSaveData protocol, List<ProtocolAnchor> protocolAnchorSet)
    {
        var mechsToInstall = protocol.mechsInProtocol;
        var teamToInstallTo = protocolAnchorSet[0].myTeam;
        var localGridToWorldGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
        foreach (ProtocolAnchor anchorSquare in protocolAnchorSet)
        {
            localGridToWorldGridDictionary.Add(anchorSquare.myOriginalGrid, anchorSquare.myCurrentGrid);
        }
        foreach (ProtocolMechData mechToInstall in mechsToInstall)
        {
            var placeToInstallTo = FindWorldPosToInstall(mechToInstall, localGridToWorldGridDictionary);
            var newMechParent = new GameObject(mechToInstall.mechName);
            var mechUnderConstruction = BuildAMechFromProtocolData(newMechParent, mechToInstall);
            ClearTheDesignAnchors(mechUnderConstruction);
            ClearTheColliders(mechUnderConstruction);
            var bpScriptUnderConstruction = PrepareABattleParticipantScript(mechUnderConstruction, mechToInstall, localGridToWorldGridDictionary);
            newMechParent.transform.position = new Vector3(placeToInstallTo.x, placeToInstallTo.y, 0);
            NudgeTheBattleAnchorIntoPosition(placeToInstallTo, mechUnderConstruction);
            QuietTheSquares(mechToInstall.mechOccupiedGridSquares, mechUnderConstruction, localGridToWorldGridDictionary);
            bpScriptUnderConstruction.myTeam = teamToInstallTo;
        }


    }

    public void ClearTheProtocolAnchors()
    {
        var anchors = FindObjectsOfType<ProtocolAnchor>();
        foreach (ProtocolAnchor anchor in anchors)
        {
            anchor.gameObject.SetActive(false);
            Destroy(anchor.gameObject);

        }
    }
    public List<List<BattleParticipant>> FindAllParticipants()
    {
        var listToReturn = new List<List<BattleParticipant>>();
        var bpsInMap = GameObject.FindObjectsOfType<BattleParticipant>();
        var teamsInMap = new List<int>();
        foreach (BattleParticipant bp in bpsInMap)
        {
            var bpTeamNumber = bp.myTeam;
            if (!teamsInMap.Contains(bpTeamNumber))
            {
                teamsInMap.Add(bpTeamNumber);
            }
        }
        numberOfTeams = teamsInMap.Count;
        foreach (int teamNumber in teamsInMap)
        {
            var teamList = new List<BattleParticipant>();
            foreach (BattleParticipant bp in bpsInMap)
            {
                if (bp.myTeam == teamNumber)
                {
                    teamList.Add(bp);
                }
            }
            listToReturn.Add(teamList);
        }
        return listToReturn;
    }

    public Dictionary<BattleParticipant, List<ActiveAbility>> FindParticipantActiveAbilities()
    {
        var dictionaryToReturn = new Dictionary<BattleParticipant, List<ActiveAbility>>();
        var bpsInMap = GameObject.FindObjectsOfType<BattleParticipant>();
        foreach (BattleParticipant bpInMap in bpsInMap)
        {
            var abilitiesForThisMech = new List<ActiveAbility>();
            var foundAbilities = bpInMap.gameObject.GetComponentsInChildren<ActiveAbility>();
            foreach (ActiveAbility aa in foundAbilities)
            {
                abilitiesForThisMech.Add(aa);
            }
            dictionaryToReturn.Add(bpInMap, abilitiesForThisMech);
        }
        return dictionaryToReturn;
    }

    public Vector2 FindWorldPosToInstall(ProtocolMechData mechInSavedProtocol, Dictionary<Vector2Int, Vector2Int> translationDictionary)
    {
        //Here in the Battle Manager, we have to modify this function so that it translates from the Protocol screen grid to the world grid to find the right square. So we add a second call requirement for the translation dictionary, which can be made elsewhere as the localGridToWorldGridDictionary.
        //Debug.Log("We're trying to install " + mechInSavedProtocol.mechName);
        //Debug.Log("The first entry in its saved list of grid squares is " + mechInSavedProtocol.mechOccupiedGridSquares[0]);
        List<Vector2Int> squaresToInstallTo = mechInSavedProtocol.mechOccupiedGridSquares;
        int numberOfSquaresToInstallTo = squaresToInstallTo.Count;
        //Debug.Log("The list of squares to install to has " + squaresToInstallTo.Count + " entries");
        if (numberOfSquaresToInstallTo == 1)
        {
            var worldGridToInstallTo = translationDictionary[squaresToInstallTo[0]];
            return terrainGridToWorldDictionary[worldGridToInstallTo];
        }
        if (numberOfSquaresToInstallTo == 4)
        {
            var newGridList = new List<Vector2Int>();
            foreach (Vector2Int originalGrid in squaresToInstallTo)
            {
                var newGrid = translationDictionary[originalGrid];
                newGridList.Add(newGrid);
            }
            //Debug.Log("Our list of world grid positions we want to install around is");
            //Debug.Log(newGridList[0]);
            //Debug.Log(newGridList[1]);
            //Debug.Log(newGridList[2]);
            //Debug.Log(newGridList[3]);
            var positionToInstallTo = FindTheCenterOfTheseGridSquares(newGridList);
            //Debug.Log("We want to install " + mechInSavedProtocol.mechName + " to " + positionToInstallTo);
            return positionToInstallTo;
        }
        else
        {
            Debug.Log("Find a WorldPosToInstall failed");
            return new Vector2(0, 0);
        }

    }

    public GameObject BuildAMechFromProtocolData(GameObject parent, ProtocolMechData data)
    {
        //This function takes an empty parent that we built in a previous step of the Instantiate process for Protocols.
        //It fills that parent up with the parts in its saved data, and locates those parts relative to it the way the localPositions and localScales are set in the data.
        foreach (ProtocolPartData partData in data.partsInMech)
        {
            if (partData.partName != "Battle Anchor")
            {
                //This is a part we need to install, not the battle anchor, so we instantiate it from Resources, set its parent, and set its position and scale.
                string pathToLoad = "Mech Parts/" + partData.partName;
                var assetToLoad = Resources.Load<GameObject>(pathToLoad);
                if (assetToLoad != null)
                {
                    var whatWeJustBuilt = Instantiate(assetToLoad, new Vector3(0, 0, 0), assetToLoad.transform.rotation);
                    whatWeJustBuilt.transform.SetParent(parent.transform);
                    whatWeJustBuilt.transform.localPosition = new Vector3(partData.partLocalX, partData.partLocalY, partData.partLocalZ);
                    whatWeJustBuilt.transform.localScale = new Vector3(partData.partScaleX, partData.partScaleY, partData.partScaleZ);
                    whatWeJustBuilt.GetComponent<IndividualPartScript>().enabled = false;
                    Destroy(whatWeJustBuilt.GetComponent<IndividualPartScript>());
                }
                else
                {
                    //This function fails when loading an arm part, as the secondary arm doesn't exist in the Resources folder but may still exist in the mech data.
                    Debug.Log("Attempted and failed to load at " + pathToLoad);
                }
            }
            if (partData.partName == "Battle Anchor")
            {
                var whatWeJustBuilt = Instantiate(battleAnchor, new Vector3(0, 0, 0), battleAnchor.transform.rotation);
                whatWeJustBuilt.transform.SetParent(parent.transform);
                whatWeJustBuilt.transform.localPosition = new Vector3(partData.partLocalX, partData.partLocalY, partData.partLocalZ);
                whatWeJustBuilt.transform.localScale = new Vector3(partData.partScaleX, partData.partScaleY, partData.partScaleZ);
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().unitIAmAttachedTo = parent;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().installedInProtocol = false;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().inBattle = true;
            }
        }
        return parent;
    }

    public void ClearTheDesignAnchors(GameObject mechToClean)
    {
        var anchorsInMech = mechToClean.GetComponentsInChildren<AnchorPointScript>();
        foreach (AnchorPointScript anchor in anchorsInMech)
        {
            anchor.enabled = false;
            Destroy(anchor.gameObject);
        }
    }

    public void ClearTheColliders(GameObject mechToClean)
    {
        //We sweep through the mech and disable then delete any colliders EXCEPT for the one on the Battle Anchor, which we detect by checking for its script.
        var collidersInMech = mechToClean.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in collidersInMech)
        {
            if (collider.gameObject.GetComponent<BattleAnchorScript>() == null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
        }
    }

    public void NudgeTheBattleAnchorIntoPosition(Vector3 positionToReach, GameObject mechToNudge)
    {

        //This is the final move function for placing the mech. Without this function the mech is put together onto a new empty parent that isn't located anywhere in particular. This function finds the distance between the newly-put-together mech's Battle Anchor and where it wants to be, then nudges the entire mech into that position.
        var mechBattleAnchor = mechToNudge.GetComponentInChildren<BattleAnchorScript>().gameObject;
        var distanceToNudge = positionToReach - mechBattleAnchor.transform.position;
        mechToNudge.transform.Translate(distanceToNudge);
    }

    public BattleParticipant PrepareABattleParticipantScript(GameObject mechThatNeedsScript, ProtocolMechData data, Dictionary<Vector2Int, Vector2Int> translationDictionary)
    {
        //Here in the Battle Manager, we have to modify this function so that it translates from the Protocol screen grid to the world grid to find the right square. So we add a second call requirement for the translation dictionary, which can be made elsewhere as the localGridToWorldGridDictionary.
        var scriptUnderConstruction = mechThatNeedsScript.AddComponent<BattleParticipant>();
        scriptUnderConstruction.characterName = data.mechName;
        scriptUnderConstruction.myOrientation = data.mechOrientation;
        scriptUnderConstruction.healthMax = data.healthMax;
        scriptUnderConstruction.powerUsed = data.powerUsed;
        scriptUnderConstruction.powerMax = data.powerMax;
        scriptUnderConstruction.weightUsed = data.weightUsed;
        scriptUnderConstruction.weightMax = data.weightMax;
        scriptUnderConstruction.cpuUsed = data.cpuUsed;
        scriptUnderConstruction.cpuMax = data.cpuMax;
        scriptUnderConstruction.moveType = data.moveType;
        scriptUnderConstruction.movePower = data.movePower;
        scriptUnderConstruction.rotateCost = data.rotateCost;
        scriptUnderConstruction.evade = data.evade;
        scriptUnderConstruction.accuracy = data.accuracy;
        var realWorldGridSquares = new List<Vector2Int>();
        foreach (Vector2Int oldGrid in data.mechOccupiedGridSquares)
        {
            var newGrid = translationDictionary[oldGrid];
            realWorldGridSquares.Add(newGrid);
        }
        scriptUnderConstruction.gridPositionsIAmOver = realWorldGridSquares;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().battleParticipantScriptIAmAttachedTo = scriptUnderConstruction;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().UpdateMyOrientation();
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().overAnotherThing = false;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().gridPositionsIAmOver = realWorldGridSquares;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().unitIAmAttachedTo = mechThatNeedsScript;
        return scriptUnderConstruction;
    }

    public void QuietTheSquares(List<Vector2Int> squaresWeJustInstalledOver, GameObject whatOccupiesTheSquares, Dictionary<Vector2Int, Vector2Int> dictionary)
    {
        //Here in the Battle Manager, we have to modify this function so that it translates from the Protocol screen grid to the world grid to find the right square. So we add a second call requirement for the translation dictionary, which can be made elsewhere as the localGridToWorldGridDictionary.
        foreach (Vector2Int gridSquare in squaresWeJustInstalledOver)
        {
            var realWorldGridSquare = dictionary[gridSquare];
            var tileWeCareAbout = terrainTileDictionary[realWorldGridSquare];
            tileWeCareAbout.searchable = false;
            tileWeCareAbout.objectCanEnter = false;
            tileWeCareAbout.isOccupied = true;
            tileWeCareAbout.whatOccupiesMe = whatOccupiesTheSquares;
        }
    }

    public void InitializeTheBattlePreview()
    {
        battleActionPreviewPosition = battleActionPreview.transform.position;
        battleActionPreview.GetComponent<BattleActionPreviewScript>().initialPosition = battleActionPreviewPosition;
        battleActionPreview.GetComponent<BattleActionPreviewScript>().initializing = false;
        battleActionPreview.SetActive(false);
    }



    //
    //End initialization functions
    //





    // Update is called once per frame
    void Update()
    {
        //
        //We'll use Update() to trigger a desired function on key press for debugging.
        //

        if (Input.GetKeyDown("k"))
        {
            StartCoroutine(WinTheBattle());
        }
        
    }





}
