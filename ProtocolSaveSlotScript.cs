using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProtocolSaveSlotScript : MonoBehaviour
{

    //General variables that reference this save slot
    //The save slot index is set in the Inspector.
    public string saveSlotIndex;
    public TextMeshProUGUI saveSlotDescription;
    private Button thisButton;
    public ProtocolSaveData thisProtocol;
    public List<ProtocolMechData> mechsInThisProtocol;
    public List<GameObject> partsInUnderConstructionProtocol;

    //Variables that reference things in the Designs screen
    public ProtocolsManagerScript protocolsManagerScript;

    public void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            protocolsManagerScript = GameObject.Find("Protocols Manager").GetComponent<ProtocolsManagerScript>();
        }

        saveSlotDescription = GetComponentInChildren<TextMeshProUGUI>();
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(SaveOrLoadProtocol);
    }
    public void SaveOrLoadProtocol()
    //This is a mirror of the SaveOrLoadDesign() function for Designs. It splits based on the current situation -- what screen am I in, and am I saving or loading?
    //If saving in the Protocols screen, it feeds the right saveSlotIndex to the Protocols Manager and then executes the Protocols Manager's save function.
    //If loading in the Protocols screen, it clears the current Protocol and proceeds to reconstruct the saved Protocol using a series of functions written to that purpose. When complete, the loaded protocol should be able to be continued to be edited, just like as functions on the Designs screen.
    //If this function is accessed inside a Battle, it reconstructs the protocol onto the battlefield in a desired location.
    {
        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            if (protocolsManagerScript.loadingProtocol == false)
            {
                protocolsManagerScript.protocolSlotToSaveOrLoad = saveSlotIndex;
                protocolsManagerScript.SaveThisProtocol();
            }
            if (protocolsManagerScript.loadingProtocol == true)
            {
                //All of the Load Protocol functions here...
                protocolsManagerScript.ClearThisProtocol();
                protocolsManagerScript.InitializeProtocolSaveLoadSlots();
                protocolsManagerScript.currentProtocolName = thisProtocol.protocolName;
                protocolsManagerScript.protocolNameDisplay.text = thisProtocol.protocolName;
                var mechsToInstall = thisProtocol.mechsInProtocol;
                foreach (ProtocolMechData mechToInstall in mechsToInstall)
                {
                    var placeToInstallTo = FindWorldPosToInstall(mechToInstall);
                    var newMechParent = new GameObject(mechToInstall.mechName);
                    var mechUnderConstruction = BuildAMechFromProtocolData(newMechParent, mechToInstall);
                    ClearTheDesignAnchors(mechUnderConstruction);
                    ClearTheColliders(mechUnderConstruction);
                    var bpScriptUnderConstruction = PrepareABattleParticipantScript(mechUnderConstruction, mechToInstall);
                    newMechParent.transform.position = new Vector3(placeToInstallTo.x, placeToInstallTo.y, 0);
                    NudgeTheBattleAnchorIntoPosition(placeToInstallTo, mechUnderConstruction);
                    QuietTheSquares(mechToInstall.mechOccupiedGridSquares, mechUnderConstruction);
                }
                protocolsManagerScript.UpdateCurrentProtocolStats();
                protocolsManagerScript.InitializeProtocolSaveLoadSlots();
            }

        }

        //If we're not in the Protocols screen, it's assumed we're in a battle. There will be many different Scenes for many different battles, so if it turns out we need Protocol save data elsewhere, we'll have to find a way to detect a battle.
        else
        {

        }
    }

    public Vector2 FindWorldPosToInstall(ProtocolMechData mechInSavedProtocol)
    {
        Debug.Log("We're trying to install " + mechInSavedProtocol.mechName);
        Debug.Log("The first entry in its saved list of grid squares is " + mechInSavedProtocol.mechOccupiedGridSquares[0]);
        List<Vector2Int> squaresToInstallTo = mechInSavedProtocol.mechOccupiedGridSquares;
        int numberOfSquaresToInstallTo = squaresToInstallTo.Count;
        Debug.Log("The list of squares to install to has " + squaresToInstallTo.Count + " entries");
        if (numberOfSquaresToInstallTo == 1)
        {
            return protocolsManagerScript.terrainGridToWorldDictionary[squaresToInstallTo[0]];
        }
        if (numberOfSquaresToInstallTo == 4)
        {
            return protocolsManagerScript.FindTheCenterOfTheseGridSquares(squaresToInstallTo);
        }
        else
        {
            return new Vector2(0, 0);
        }

    }

    public GameObject BuildAMechFromProtocolData(GameObject parent, ProtocolMechData data)
    {
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
            }
            if (partData.partName == "Battle Anchor")
            {
                var whatWeJustBuilt = Instantiate(protocolsManagerScript.battleAnchor, new Vector3(0, 0, 0), protocolsManagerScript.battleAnchor.transform.rotation);
                whatWeJustBuilt.transform.SetParent(parent.transform);
                whatWeJustBuilt.transform.localPosition = new Vector3(partData.partLocalX, partData.partLocalY, partData.partLocalZ);
                whatWeJustBuilt.transform.localScale = new Vector3(partData.partScaleX, partData.partScaleY, partData.partScaleZ);
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().unitIAmAttachedTo = parent;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().installedInProtocol = true;
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
            Destroy(anchor);
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
        var mechBattleAnchor = mechToNudge.GetComponentInChildren<BattleAnchorScript>().gameObject;
        var distanceToNudge = positionToReach - mechBattleAnchor.transform.position;
        mechToNudge.transform.Translate(distanceToNudge);
    }
    public BattleParticipant PrepareABattleParticipantScript(GameObject mechThatNeedsScript, ProtocolMechData data)
    {
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
        scriptUnderConstruction.gridPositionsIAmOver = data.mechOccupiedGridSquares;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().battleParticipantScriptIAmAttachedTo = scriptUnderConstruction;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().UpdateMyOrientation();
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().overAnotherThing = false;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().gridPositionsIAmOver = data.mechOccupiedGridSquares;
        mechThatNeedsScript.GetComponentInChildren<BattleAnchorScript>().unitIAmAttachedTo = mechThatNeedsScript;
        return scriptUnderConstruction;
    }

    public void QuietTheSquares(List<Vector2Int> squaresWeJustInstalledOver, GameObject whatOccupiesTheSquares)
    {
        foreach (Vector2Int gridSquare in squaresWeJustInstalledOver)
        {
            var tileWeCareAbout = protocolsManagerScript.terrainTileDictionary[gridSquare];
            tileWeCareAbout.searchable = false;
            tileWeCareAbout.objectCanEnter = false;
            tileWeCareAbout.isOccupied = true;
            tileWeCareAbout.whatOccupiesMe = whatOccupiesTheSquares;
        }
    }


    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
