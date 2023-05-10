using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DesignSaveSlotScript : MonoBehaviour
{
    //General variables that reference this save slot
    public string saveSlotIndex;
    public TextMeshProUGUI saveSlotDescription;
    private Button thisButton;
    public MechDesignData thisMechDesign;
    public List<MechPartData> partsInThisDesign;
    public List<GameObject> partsInUnderConstructionMech;

    //Variables that reference things in the Designs screen
    public DesignsManagerScript designsManagerScript;
    public ThumbnailCameraScript thumbnailCameraScript;

    //Variables that reference things in the Protocols screen
    public GameObject emptyParentForProtocols;
    public ProtocolsManagerScript protocolsManagerScript;
    public Vector3 designDefaultPosition;
    public BattleParticipant battleParticipantScriptUnderConstruction;

    //Variable that triggers the rebuild process after returning from Test mode
    public bool returningFromMechTest = false;






    //Start is called before the first frame update
    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Designs Screen")
        {
            designsManagerScript = GameObject.Find("DesignsManager").GetComponent<DesignsManagerScript>();
        }
        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            protocolsManagerScript = GameObject.Find("Protocols Manager").GetComponent<ProtocolsManagerScript>();
        }

        saveSlotDescription = GetComponentInChildren<TextMeshProUGUI>();
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(SaveOrLoadDesign);

        //The following code initializes the names of the Designs.

    }

    public void Start()
    {
        if (saveSlotIndex == "1" && MechPlayerManager.returningFromMechTest)
        {
            Debug.Log("The first save slot is attempting to return from Test mode...");
            MechPlayerManager.returningFromMechTest = false;
            LoadTheMechWeJustTested();
        }
        thumbnailCameraScript = FindObjectOfType<ThumbnailCameraScript>();
    }

    public void LoadTheMechWeJustTested()
    {
        designsManagerScript.loadingDesign = true;
        designsManagerScript.slotToSaveOrLoad = "999";
        SaveOrLoadDesign();
        designsManagerScript.loadingDesign = false;
        designsManagerScript.slotToSaveOrLoad = null;
    }



    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void SaveOrLoadDesign()
    {
        //This function gets split in two based on whether we're looking at this Design in the Designs screen or the Protocols screen. Within the Designs screen, it reads the state of flow from the Designs Manager to determine whether we're Saving or Loading by clicking on the button.

        if (SceneManager.GetActiveScene().name == "Designs Screen")
        {
            if (designsManagerScript.loadingDesign == false)
            {
                TakeAThumbnailScreenshot();
                designsManagerScript.slotToSaveOrLoad = saveSlotIndex;
                designsManagerScript.SaveThisMech();
            }

            //If we are in the loading state, we execute the code to reconstruct the design.. In order to test this we'll insert the sub-functions one at a time and check the result
            if (designsManagerScript.loadingDesign == true)
            {
                MakeAListOfParts();
                ClearTheCurrentMech();
                ConstructPhysicalParts();
                PutPartsInHierarchy();
                SetUpTheAnchorPoints();
                designsManagerScript.TurnOffAnchorPoints();
                designsManagerScript.partJustChanged = true;
            }
        }

        if (SceneManager.GetActiveScene().name == "Protocols Screen")
        {
            MakeAListOfParts();
            MakeAParent();
            ConstructPhysicalPartsForProtocols();
            ClearTheAnchorPoints();
            TurnOffTheColliders();
            PackageThePartStats();
            SetTheBattleAnchorSize();
            UpdateTheFocusedMechStats();
            protocolsManagerScript.PrepareToInstallDesign();
        }

        //If we're not in the Loading state, we save the design

    }

    public void MakeAListOfParts()
    {
        partsInThisDesign = thisMechDesign.mechDesign;
    }

    //The following functions are called on the Designs screen when rebuilding the mech to continue editing it.
    public void ClearTheCurrentMech()
    {
        //This function gets all the parts in the currently edited mech and deletes all except the parent, then sets the Floor-Leg Anchor to free.
        var currentlyEditedMech = designsManagerScript.currentlyEditedMech;
        var partsInCurrentMech = currentlyEditedMech.GetComponentsInChildren<Transform>();
        foreach (Transform partTransform in partsInCurrentMech)
        {
            if (partTransform.gameObject != currentlyEditedMech & partTransform.gameObject.name != "Floor-Leg Anchor")
            {
                Destroy(partTransform.gameObject);
            }

        }
        if (thisMechDesign == null)
        {
            currentlyEditedMech.name = "Empty Mech";
        }
        else
        {
            currentlyEditedMech.name = thisMechDesign.mechDesignName;
        }
        //The following code updates the mech name in both script and display, and ought to be standardized to a function later.
        designsManagerScript.currentlyEditedMech.GetComponent<CurrentlyEditedMechContainer>().mechName = thisMechDesign.mechDesignName;
        designsManagerScript.mechNameDisplay.text = thisMechDesign.mechDesignName;

        partsInUnderConstructionMech.Clear();
        var floorLegAnchorScript = GameObject.Find("Floor-Leg Anchor").GetComponent<AnchorPointScript>().matchMade = false;
    }
    public void ConstructPhysicalParts()
    {

        //For each part in the parts list, if it's not an Anchor, we look up the part in the Resources folder and instantiate it in its desired location. We also need to instantiate the Leg-Ground Anchor.
        foreach (MechPartData mechPartToConstruct in partsInThisDesign)
        {
            //So we sweep through every part in the design. First we construct the physical parts by finding the relevant asset and instantiating it. Then as a first pass we set everything as the child of the currently edited mech container.
            if (mechPartToConstruct.isAnchor == false & mechPartToConstruct.partName != mechPartToConstruct.parentName)
            {
                string pathToLoad = "Mech Parts/" + mechPartToConstruct.partName;
                //Debug.Log(pathToLoad);
                var assetToLoad = Resources.Load<GameObject>(pathToLoad);
                //Debug.Log(assetToLoad.name);
                var locationToLoadTo = new Vector3(mechPartToConstruct.partX, mechPartToConstruct.partY, mechPartToConstruct.partZ);
                if (assetToLoad != null)
                {
                    var whatWeJustBuilt = Instantiate(assetToLoad, locationToLoadTo, assetToLoad.transform.rotation);
                    whatWeJustBuilt.transform.SetParent(designsManagerScript.currentlyEditedMech.transform);
                    whatWeJustBuilt.transform.position = locationToLoadTo;
                    whatWeJustBuilt.GetComponent<IndividualPartScript>().installed = true;
                }

            }

        }

    }

    public void PutPartsInHierarchy()
    {
        //First we construct a List of the parts in our mech we just put together
        var partScriptsInUnderConstructionMech = designsManagerScript.currentlyEditedMech.GetComponentsInChildren<IndividualPartScript>();
        foreach (IndividualPartScript individualPartScript in partScriptsInUnderConstructionMech)
        {
            partsInUnderConstructionMech.Add(individualPartScript.gameObject);
        }
        //Next we grab the name of each part in our under-construction mech and find the part in our Saved Design parts list that matches it -- note it must also match the X and Y coords to weed out false matches
        foreach (GameObject partToExamine in partsInUnderConstructionMech)
        {
            //Debug.Log("The pre-adjustment part name we're working on is" + partToExamine.name);
            string realPartName = partToExamine.name.Replace("(Clone)", "");
            //Debug.Log("The real part name we're working on is " + realPartName);
            foreach (MechPartData mechPartToMatch in partsInThisDesign)
            {
                string realMechPartToMatchName = mechPartToMatch.partName;
                realMechPartToMatchName = realMechPartToMatchName.Replace("(Clone)", "");
                //Debug.Log("We are about to compare " + realMechPartToMatchName + " to " + realPartName);

                if (realMechPartToMatchName == realPartName)
                {
                    //Debug.Log("The names match!");
                }

                if (mechPartToMatch.partX == partToExamine.transform.position.x)
                {

                    //Debug.Log("The Xs match!");
                }

                if (mechPartToMatch.partY == partToExamine.transform.position.y)
                {
                    //Debug.Log("The Ys match!");
                }

                if (realMechPartToMatchName == realPartName && mechPartToMatch.partX == partToExamine.transform.position.x && mechPartToMatch.partY == partToExamine.transform.position.y)
                {
                    //Debug.Log("We found a match!");
                    //Now we've found the matching part, so we need to find the target parent and then get the parent Game Object that matches
                    string targetParent = mechPartToMatch.parentName;
                    targetParent = targetParent.Replace("(Clone)", "");
                    //Debug.Log("The target parent of " + realPartName + "is " + targetParent);
                    foreach (GameObject mechPartToParentMatch in partsInUnderConstructionMech)
                    {
                        string realParentName = mechPartToParentMatch.name.Replace("(Clone)", "");
                        if (realParentName == targetParent)
                        {
                            //So now we have the Game Object of the target parent, and we set that game object as the parent of the part we're examining from 3 nests above

                            partToExamine.transform.SetParent(mechPartToParentMatch.transform);
                            //Debug.Log("We should have just set the parent of " + partToExamine.name + "to " + mechPartToParentMatch.name);
                        }
                    }
                }
            }
        }
    }

    public void SetUpTheAnchorPoints()
    {
        //First we construct a list of the Anchor Point game objects we just put together. The logic here mimics what we just did for the physical parts. We'll recycle the same list, in fact, we just used by emptying it out first.
        var anchorPointsInTheBeingBuiltMech = FindObjectsOfType<AnchorPointScript>();
        partsInUnderConstructionMech.Clear();
        foreach (AnchorPointScript anchorPointScript in anchorPointsInTheBeingBuiltMech)
        {
            partsInUnderConstructionMech.Add(anchorPointScript.gameObject);
        }
        //Since the anchor points are instantiated as children, they don't wind up with the (Clone) tag so we don't have to worry about filtering it. Now -- for each anchor point GameObject we're working with, scan the list of MechPartDatas in our save file and look for the match of name, X, and Y. Once you get that match, set the matchMade variable equivalent to the save data. This requires rounding X and Y a bit because Unity is spitting out E-9 order position errors.
        foreach (GameObject anchorPointToExamine in partsInUnderConstructionMech)
        {
            string anchorPointName = anchorPointToExamine.name;
            foreach (MechPartData mechPartData in partsInThisDesign)
            {
                if (anchorPointName == mechPartData.partName && RoundToThreeDigits(anchorPointToExamine.transform.position.x) == RoundToThreeDigits(mechPartData.partX) && RoundToThreeDigits(anchorPointToExamine.transform.position.y) == RoundToThreeDigits(mechPartData.partY))
                {
                    anchorPointToExamine.GetComponent<AnchorPointScript>().matchMade = mechPartData.anchorMatchMade;
                    //Debug.Log("We just set the matchMade of " + anchorPointToExamine.name + "equal to what's in the save file.");
                }
            }
        }
    }

    //The following functions are called on the Protocols screen when rebuilding the mech to turn it into a Battle Participant.

    //The first function prepares a placeholder with the Focused Mech's name. The only part in the save data for which the parent is itself is the mech's name.
    public void MakeAParent()
    {
        emptyParentForProtocols = new GameObject("Focused Mech");
        emptyParentForProtocols.transform.position = protocolsManagerScript.focusedMechPreviewLocation;
        foreach (MechPartData mechPartToConstruct in partsInThisDesign)
        {
            if (mechPartToConstruct.partName == mechPartToConstruct.parentName)
            {
                emptyParentForProtocols.name = mechPartToConstruct.partName;
                designDefaultPosition.x = mechPartToConstruct.partX;
                designDefaultPosition.y = mechPartToConstruct.partY;
                designDefaultPosition.z = mechPartToConstruct.partZ;
                
                
            }
        }
        battleParticipantScriptUnderConstruction = emptyParentForProtocols.AddComponent<BattleParticipant>();
        battleParticipantScriptUnderConstruction.characterName = emptyParentForProtocols.name;
    }
    public void ConstructPhysicalPartsForProtocols()
    {
        partsInUnderConstructionMech.Clear();
        //For each part in the parts list, if it's not an Anchor, we look up the part in the Resources folder and instantiate it in its desired location. We also need to instantiate the Battle Anchor.
        foreach (MechPartData mechPartToConstruct in partsInThisDesign)
        {
            //So we sweep through every part in the design. First we construct the physical parts by finding the relevant asset and instantiating it.
            if (mechPartToConstruct.isAnchor == false & mechPartToConstruct.partName != mechPartToConstruct.parentName)
            {
                string pathToLoad = "Mech Parts/" + mechPartToConstruct.partName;
                //Debug.Log(pathToLoad);
                var assetToLoad = Resources.Load<GameObject>(pathToLoad);
                //Debug.Log(assetToLoad.name);
                //To find where to instantiate it, we need to find its original offset from the original parent, then transform that down to the preview location in the Protocols screen. The original offsets have to get scaled since we're scaling the sprite for battle.
                var locationOriginallyLoadedTo = new Vector3(mechPartToConstruct.partX, mechPartToConstruct.partY, mechPartToConstruct.partZ);
                var originalOffset = designDefaultPosition - locationOriginallyLoadedTo;
                var scaledOffset = originalOffset * GameManager.mechDesignsToBattleScaleFactor;
                var locationToLoadTo = protocolsManagerScript.focusedMechPreviewLocation - scaledOffset;
                //We need to make sure assetToLoad isn't null, because some parts e.g. arms have children that get scooped up in the data but aren't in the Resources folder.
                if (assetToLoad != null)
                {
                    var whatWeJustBuilt = Instantiate(assetToLoad, locationToLoadTo, assetToLoad.transform.rotation);
                    whatWeJustBuilt.transform.SetParent(emptyParentForProtocols.transform);
                    whatWeJustBuilt.transform.position = locationToLoadTo;
                    var originalScale = whatWeJustBuilt.transform.localScale;
                    whatWeJustBuilt.transform.localScale = originalScale * GameManager.mechDesignsToBattleScaleFactor;
                    whatWeJustBuilt.GetComponent<IndividualPartScript>().installed = true;
                    //Finally we add all the parts we just built to a list we can work with.
                    partsInUnderConstructionMech.Add(whatWeJustBuilt);
                }

            }
            //Here we add in the Battle Anchor that places the unit on the grid. It gets placed right at the center of the old Floor-Leg Anchor. It then gets told who its attached unit is, and gets its variable flicked that allows it to be dragged onto the grid.
            if (mechPartToConstruct.partName == "Floor-Leg Anchor")
            {
                var locationOriginallyLoadedTo = new Vector3(mechPartToConstruct.partX, mechPartToConstruct.partY, mechPartToConstruct.partZ);
                var originalOffset = designDefaultPosition - locationOriginallyLoadedTo;
                var scaledOffset = originalOffset * GameManager.mechDesignsToBattleScaleFactor;
                var locationToLoadTo = protocolsManagerScript.focusedMechPreviewLocation - scaledOffset;
                var whatWeJustBuilt = Instantiate(protocolsManagerScript.battleAnchor, locationToLoadTo, protocolsManagerScript.battleAnchor.transform.rotation);
                whatWeJustBuilt.transform.SetParent(emptyParentForProtocols.transform);
                whatWeJustBuilt.transform.position = locationToLoadTo;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().unitIAmAttachedTo = emptyParentForProtocols;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().battleParticipantScriptIAmAttachedTo = battleParticipantScriptUnderConstruction;
                whatWeJustBuilt.GetComponent<BattleAnchorScript>().readyToInstallInProtocol = true;
            }

        }

    }

    public void ClearTheAnchorPoints()
    {
        foreach (GameObject mechPart in partsInUnderConstructionMech)
        {
            var anchorScripts = mechPart.GetComponentsInChildren<AnchorPointScript>();
            foreach (AnchorPointScript anchorScript in anchorScripts)
            {
                Destroy(anchorScript.gameObject);
            }
        }
    }

    //This function turns off all the individual part colliders, since they seem to be interfering with the Battle Anchor collider which becomes the main character controller in a battle. May need to revisit this later if we need to use the individual part colliders, as after we turn them off it can be hard to find them later to turn them on.
    public void TurnOffTheColliders()
    {
        foreach (GameObject mechPart in partsInUnderConstructionMech)
        {
            var colliders = mechPart.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.enabled = false;
            }
        }
    }

    public void PackageThePartStats()
    {
        foreach (GameObject mechPart in partsInUnderConstructionMech)
        {
            var partScript = mechPart.GetComponent<IndividualPartScript>();
            battleParticipantScriptUnderConstruction.characterName = emptyParentForProtocols.name;
            battleParticipantScriptUnderConstruction.healthMax += partScript.health;
            battleParticipantScriptUnderConstruction.powerUsed += partScript.powerUsed;
            battleParticipantScriptUnderConstruction.powerMax += partScript.powerMax;
            battleParticipantScriptUnderConstruction.weightUsed += partScript.weightUsed;
            battleParticipantScriptUnderConstruction.weightMax += partScript.weightMax;
            battleParticipantScriptUnderConstruction.cpuUsed += partScript.cpuUsed;
            battleParticipantScriptUnderConstruction.cpuMax += partScript.cpuMax;
            if (partScript.moveType != null && partScript.moveType != "")
            {
                //Debug.Log("We found a moveType in this part and it is " + partScript.moveType);
                battleParticipantScriptUnderConstruction.moveType = partScript.moveType;
            }
            battleParticipantScriptUnderConstruction.movePower += partScript.move;
            battleParticipantScriptUnderConstruction.rotateCost += partScript.rotateCost;
            battleParticipantScriptUnderConstruction.evade *= partScript.evade;
            battleParticipantScriptUnderConstruction.accuracy *= partScript.evade;
            Destroy(partScript);
        }

    }

    public void SetTheBattleAnchorSize()
    {
        var battleAnchor = emptyParentForProtocols.GetComponentInChildren<BattleAnchorScript>();
        battleAnchor.SizeMe();
    }

    public void UpdateTheFocusedMechStats()
    {
        protocolsManagerScript.battleParticipantUnderConstruction = battleParticipantScriptUnderConstruction;
        //Debug.Log("We just assigned the Battle Participant to the Protocol Manager");
        protocolsManagerScript.UpdateFocusedMechStatDisplay();
        //Debug.Log("We just told the Protocol Manager to update the focused mech stat display");

    }

    public void TakeAThumbnailScreenshot()
    {
        thumbnailCameraScript.designSlot = saveSlotIndex;
        thumbnailCameraScript.SaveMechThumbnail();
    }

    //The following is a support function for rounding to 3 digits, called to eliminate slightly inaccurate float values.
    public float RoundToThreeDigits(float floatToRound)
    {
        float roundedFloat = Mathf.Round(floatToRound * 1000.0f) * 0.001f;
        return roundedFloat;
    }






}
