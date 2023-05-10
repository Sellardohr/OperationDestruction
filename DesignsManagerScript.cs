using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;



public class DesignsManagerScript : MonoBehaviour
{
    //Variables that reference other objects in the Scene
    private GameObject navMenuButtons;
    private GameObject editMenuButtons;
    private GameObject partsMenuButtons;
    private GameObject nameInputBox;
    public GameObject currentlyEditedMech;
    public GameObject designsBG;
    private CurrentlyEditedMechContainer currentlyEditedMechContainer;
    public TextMeshProUGUI mechNameDisplay;
    private TextMeshProUGUI mechStatDisplay;
    private TextMeshProUGUI partStatDisplay;
    private TMP_InputField nameInputText;
    public GameObject legGroundAnchor;
    private GameObject legsInventory;
    private GameObject armsInventory;
    private GameObject torsosInventory;
    private GameObject headsInventory;
    private GameObject handWeaponsInventory;
    public GameObject focusedPart;
    private GameObject installButton;
    private GameObject deleteButton;
    private GameObject saveLoadDesignsMenu;
    private GameObject partDoorsLeft;
    private GameObject partDoorsRight;

    //Variables to lookup the Focused Part components of the GUI
    private Image focusViewport;
    private TextMeshProUGUI focusDescriptionText;
    private TextMeshProUGUI focusPartStats;

    private TextMeshProUGUI focusLCDPartStats;
    public Vector2 focusLCDPartStatsOriginalLocation;
    public Coroutine focusLCDPartStatsScrollRoutine;

    private TextMeshProUGUI focusLCDMechStats;
    public Vector2 focusLCDMechStatsOriginalLocation;
    public Coroutine focusLCDMechStatsScrollRoutine;

    //Variables to store parameters of a given part while saving mech design data
    string partName;
    string parentName;
    bool isAnchor;
    bool anchorMatchMade;
    float partX;
    float partY;
    float partZ;
    float scaleX;
    float scaleY;
    float scaleZ;

    //Variables that organize save data
    public string slotToSaveOrLoad = "1";
    public string mechDesignRootFilepath;
    public string mechDesignFilepathToSaveOrLoadToOrFrom;
    public List<MechDesignData> mechDesignDataLookup;
    public DesignSaveSlotScript[] designSaveSlotList;


    //Variables that reference the game flow
    public bool installingPart = false;
    public bool partJustChanged = false;
    public bool loadingDesign = false;
    public bool initializingSaveLoadSlots = true;
    public int levelOfZoom = 1;
    public bool doorsOpen = false;

    //Variables for managing mech data when activating a Test Battle
    public GameObject battleAnchor;



    // Start is called before the first frame update
    void Start()
    {
        //Locates needed references
        navMenuButtons = GameObject.Find("Main Nav Buttons");
        editMenuButtons = GameObject.Find("Edit Menu Buttons");
        partsMenuButtons = GameObject.Find("Parts Menu Buttons");
        nameInputBox = GameObject.Find("Mech Name Input");
        legGroundAnchor = GameObject.Find("Floor-Leg Anchor");
        saveLoadDesignsMenu = GameObject.Find("Save/Load Designs Menu");
        designsBG = GameObject.Find("Designs BG");
        nameInputText = GameObject.Find("Mech Name Input").GetComponent<TMP_InputField>();
        mechNameDisplay = GameObject.Find("Mech Name Text").GetComponent<TextMeshProUGUI>();
        partStatDisplay = GameObject.Find("Focus Stats Text").GetComponent<TextMeshProUGUI>();
        mechStatDisplay = GameObject.Find("Stats Text").GetComponent<TextMeshProUGUI>();
        legsInventory = GameObject.Find("Legs Inventory");
        armsInventory = GameObject.Find("Arms Inventory");
        torsosInventory = GameObject.Find("Torsos Inventory");
        headsInventory = GameObject.Find("Heads Inventory");
        handWeaponsInventory = GameObject.Find("Hand Weapons Inventory");
        currentlyEditedMech = GameObject.FindGameObjectWithTag("CurrentlyEditedMech");
        installButton = GameObject.Find("Install Button");
        deleteButton = GameObject.Find("Delete Button");
        currentlyEditedMechContainer = currentlyEditedMech.GetComponent<CurrentlyEditedMechContainer>();
        mechNameDisplay.text = currentlyEditedMechContainer.mechName;
        focusViewport = GameObject.Find("Focus Viewport Image").GetComponent<Image>();
        focusDescriptionText = GameObject.Find("Focus Description Text").GetComponent<TextMeshProUGUI>();
        focusPartStats = GameObject.Find("Focus Stats Text").GetComponent<TextMeshProUGUI>();
        focusLCDPartStats = GameObject.Find("Part Info LCD Text").GetComponent<TextMeshProUGUI>();
        focusLCDPartStatsOriginalLocation = focusLCDPartStats.transform.position;
        focusLCDMechStats = GameObject.Find("Mech Info LCD Text").GetComponent<TextMeshProUGUI>();
        focusLCDMechStatsOriginalLocation = focusLCDPartStats.transform.position;
        partDoorsLeft = GameObject.Find("Designs Screen Part Door Left");
        partDoorsRight = GameObject.Find("Designs Screen Part Door Right");

        //This code looks up all the Design Save Slots and places them in an array for later use
        designSaveSlotList = FindObjectsOfType<DesignSaveSlotScript>();

        //Initializes the MechStatDisplay
        UpdateMechStatDisplay();
        UpdateMechName();
        ScrollTheInfoText();

        //Sets unneeded UI components to inactive at startup
        editMenuButtons.gameObject.SetActive(false);
        nameInputBox.gameObject.SetActive(false);
        partsMenuButtons.gameObject.SetActive(false);
        legsInventory.gameObject.SetActive(false);
        torsosInventory.gameObject.SetActive(false);
        armsInventory.gameObject.SetActive(false);
        headsInventory.gameObject.SetActive(false);
        handWeaponsInventory.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        saveLoadDesignsMenu.gameObject.SetActive(false);

        //Sets the filepath for saving and loading designs and initializes the directory and creates dummy save slots
        InitializeSaveLoadFilepath();
    }

    // Update is called once per frame
    void Update()
    {
        if (installingPart == true)
        {
            TurnOnAnchorPoints();
            SwitchToDeleteButton();
            StartCoroutine(OpenAndCloseDoors());
            StartCoroutine(RollInThePartToInstall(focusedPart));
            //The following line simply stops the IF call from being an endless loop and should only happen once everything that needs to happen in the "installing Part" phase happens.
            installingPart = false;
        }
        if (partJustChanged == true)
        {
            UpdateMechStats();
            UpdateMechStatDisplay();
            partJustChanged = false;
        }
    }

    //Navigation Functions
    public void SwitchToEditMenu()
    {
        navMenuButtons.gameObject.SetActive(false);
        partsMenuButtons.gameObject.SetActive(false);
        editMenuButtons.gameObject.SetActive(true);
    }

    public void SwitchToNavMenu()
    {
        editMenuButtons.gameObject.SetActive(false);
        navMenuButtons.gameObject.SetActive(true);
    }

    public void SwitchToPartsMenu()
    {
        editMenuButtons.gameObject.SetActive(false);
        partsMenuButtons.gameObject.SetActive(true);
    }

    public void SwitchToNameInput()
    {
        nameInputBox.gameObject.SetActive(true);
    }

    public void SwitchFromNameInput()
    {
        nameInputBox.gameObject.SetActive(false);
    }

    public void SwitchToSaveDesignsMenu()
    {
        editMenuButtons.SetActive(false);
        navMenuButtons.SetActive(false);
        saveLoadDesignsMenu.SetActive(true);
        InitializeSaveLoadSlots();
    }

    public void SwitchToLoadDesignsMenu()
    {
        editMenuButtons.SetActive(false);
        navMenuButtons.SetActive(false);
        saveLoadDesignsMenu.SetActive(true);
        InitializeSaveLoadSlots();
        loadingDesign = true;
    }

    public void SwitchFromSaveDesignsMenu()
    {
        saveLoadDesignsMenu.SetActive(false);
        editMenuButtons.SetActive(true);
        loadingDesign = false;

    }

    public void SwitchToLegsInventory()
    {
        partsMenuButtons.gameObject.SetActive(false);
        legsInventory.gameObject.SetActive(true);
    }

    public void SwitchFromLegsInventory()
    {
        partsMenuButtons.gameObject.SetActive(true);
        legsInventory.gameObject.SetActive(false);
        TurnOffAnchorPoints();
    }

    public void SwitchToTorsosInventory()
    {
        partsMenuButtons.gameObject.SetActive(false);
        torsosInventory.gameObject.SetActive(true);
    }

    public void SwitchFromTorsosInventory()
    {
        partsMenuButtons.gameObject.SetActive(true);
        torsosInventory.gameObject.SetActive(false);
        TurnOffAnchorPoints();
    }

    public void SwitchToHeadsInventory()
    {
        partsMenuButtons.gameObject.SetActive(false);
        headsInventory.gameObject.SetActive(true);
    }

    public void SwitchFromHeadsInventory()
    {
        partsMenuButtons.gameObject.SetActive(true);
        headsInventory.gameObject.SetActive(false);
        TurnOffAnchorPoints();
    }

    public void SwitchToArmsInventory()
    {
        partsMenuButtons.gameObject.SetActive(false);
        armsInventory.gameObject.SetActive(true);
    }

    public void SwitchFromArmsInventory()
    {
        partsMenuButtons.gameObject.SetActive(true);
        armsInventory.gameObject.SetActive(false);
        TurnOffAnchorPoints();
    }

    public void SwitchToHandWeaponsInventory()
    {
        partsMenuButtons.gameObject.SetActive(false);
        handWeaponsInventory.gameObject.SetActive(true);
    }

    public void SwitchFromHandWeaponsInventory()
    {
        partsMenuButtons.gameObject.SetActive(true);
        handWeaponsInventory.gameObject.SetActive(false);
        TurnOffAnchorPoints();
    }

    //End Navigation Functions

    //Mech Edit Functions

    public void UpdateMechName()
    {
        currentlyEditedMechContainer.mechName = nameInputText.text.ToString();
        if (currentlyEditedMechContainer.mechName == "")
        {
            currentlyEditedMechContainer.mechName = "Empty Mech";
        }
        focusLCDMechStats.text = ".........." + ".........." + ".........." + ".........." + ".........." + currentlyEditedMechContainer.name + ".........." + currentlyEditedMechContainer.weightUsed + "kg" + ".........." + currentlyEditedMechContainer.powerUsed + "MW" + ".........." + currentlyEditedMechContainer.cpuUsed + "GHz" + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + "..........";
        mechNameDisplay.text = currentlyEditedMechContainer.mechName;
        currentlyEditedMech.name = currentlyEditedMechContainer.mechName;
    }

    public void UpdateMechStats()
    {
        //First we initialize the mech to base stats:
        currentlyEditedMechContainer.health = 0;
        currentlyEditedMechContainer.powerUsed = 0;
        currentlyEditedMechContainer.powerMax = 0;
        currentlyEditedMechContainer.weightUsed = 0;
        currentlyEditedMechContainer.weightMax = 0;
        currentlyEditedMechContainer.cpuUsed = 0;
        currentlyEditedMechContainer.cpuMax = 0;
        currentlyEditedMechContainer.moveType = "Walk";
        currentlyEditedMechContainer.move = 0;
        currentlyEditedMechContainer.rotateCost = 0;
        currentlyEditedMechContainer.evade = 0;
        currentlyEditedMechContainer.accuracy = 1;

        //Then we add up all the stats, one part at a time
        var foundPartScripts = currentlyEditedMech.GetComponentsInChildren<IndividualPartScript>();
        int cycles = foundPartScripts.Length;
        for (int i = 0; i < cycles; i++)
        {
            currentlyEditedMechContainer.health += foundPartScripts[i].health;
            currentlyEditedMechContainer.powerUsed += foundPartScripts[i].powerUsed;
            currentlyEditedMechContainer.powerMax += foundPartScripts[i].powerMax;
            currentlyEditedMechContainer.weightUsed += foundPartScripts[i].weightUsed;
            currentlyEditedMechContainer.weightMax += foundPartScripts[i].weightMax;
            currentlyEditedMechContainer.cpuUsed += foundPartScripts[i].cpuUsed;
            currentlyEditedMechContainer.cpuMax += foundPartScripts[i].cpuMax;
            if (foundPartScripts[i].moveType != null && foundPartScripts[i].moveType != "")
            {
                currentlyEditedMechContainer.moveType = foundPartScripts[i].moveType;
            }
            currentlyEditedMechContainer.move += foundPartScripts[i].move;
            currentlyEditedMechContainer.rotateCost += foundPartScripts[i].rotateCost;
            currentlyEditedMechContainer.evade += foundPartScripts[i].evade;
            currentlyEditedMechContainer.accuracy *= foundPartScripts[i].accuracy;
        }


    }
    public void UpdateMechStatDisplay()
    {
        mechStatDisplay.text = currentlyEditedMechContainer.health + "\n" + currentlyEditedMechContainer.weightUsed + "/" + currentlyEditedMechContainer.weightMax + "\n" + currentlyEditedMechContainer.cpuUsed + "/" + currentlyEditedMechContainer.cpuMax + "\n" + currentlyEditedMechContainer.move + " Move type: " + currentlyEditedMechContainer.moveType + "\n" + currentlyEditedMechContainer.evade + "%...." + currentlyEditedMechContainer.accuracy + "%";
        focusLCDMechStats.text = ".........." + ".........." + ".........." + ".........." + ".........." + currentlyEditedMechContainer.name + ".........." + currentlyEditedMechContainer.weightUsed + "kg" + ".........." + currentlyEditedMechContainer.powerUsed + "MW" + ".........." + currentlyEditedMechContainer.cpuUsed + "GHz" + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + "..........";
    }

    public void TurnOffAnchorPoints()
    {
        var foundAnchorPoints = FindObjectsOfType<AnchorPointScript>();
        int cycles = foundAnchorPoints.Length;
        for (int i = 0; i < cycles; i++)
        {
            SpriteRenderer foundAnchorSpritesToTurnOff = foundAnchorPoints[i].GetComponent<SpriteRenderer>();
            foundAnchorSpritesToTurnOff.enabled = false;
            CircleCollider2D foundAnchorCollidersToTurnOff = foundAnchorPoints[i].GetComponent<CircleCollider2D>();
            foundAnchorCollidersToTurnOff.enabled = false;
        }
    }

    public void TurnOnAnchorPoints()
    {
        //The following code activates the LegGround anchor if this is the first part to be installed. From here on out the anchors stay active but the renderer & collider get turned off when a match is made.
        if (legGroundAnchor.GetComponent<AnchorPointScript>().matchMade == false)
        {
            legGroundAnchor.SetActive(true);
        }
        var foundAnchorPoints = FindObjectsOfType<AnchorPointScript>(true);
        int cycles = foundAnchorPoints.Length;
        for (int i = 0; i < cycles; i++)
        {
            if (foundAnchorPoints[i].matchMade == false)
            {
                SpriteRenderer foundAnchorSpritesToTurnOff = foundAnchorPoints[i].GetComponent<SpriteRenderer>();
                foundAnchorSpritesToTurnOff.enabled = true;
                CircleCollider2D foundAnchorCollidersToTurnOff = foundAnchorPoints[i].GetComponent<CircleCollider2D>();
                foundAnchorCollidersToTurnOff.enabled = true;
            }

        }
    }


    public void FocusOnPart(GameObject partToFocus)
    //The goal of this function is to apply the focused part's stats to the focus window.
    {
        focusedPart = partToFocus;
        IndividualPartScript focusedPartScript = partToFocus.GetComponent<IndividualPartScript>();
        focusViewport.sprite = partToFocus.GetComponent<SpriteRenderer>().sprite;
        focusDescriptionText.text = focusedPartScript.descriptionText;
        focusPartStats.text = focusedPartScript.health + "\n" + focusedPartScript.weightUsed + "/" + focusedPartScript.weightMax + "\n" + focusedPartScript.cpuUsed + "/" + focusedPartScript.cpuMax + "\n" + focusedPartScript.move + " Move type: " + focusedPartScript.moveType + "\n" + focusedPartScript.evade + "%...." + focusedPartScript.accuracy + "%";
        focusLCDPartStats.text = ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + focusedPartScript.partName + ".........." + focusedPartScript.health + "Hull" + ".........." + focusedPartScript.weightUsed + "kg" + ".........." + focusedPartScript.powerUsed + "MW" + ".........." + focusedPartScript.cpuUsed + "GHz" + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + ".........." + "..........";
    }

    public void SwitchToDeleteButton()
    {

        installButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(true);
    }
    public void SwitchToInstallButton()
    {
        deleteButton.gameObject.SetActive(false);
        installButton.gameObject.SetActive(true);
    }

    public void InitializeSaveLoadFilepath()
    {
        initializingSaveLoadSlots = true;
        slotToSaveOrLoad = "1";
        mechDesignRootFilepath = Path.Combine(Application.persistentDataPath, "Designs");
        if (!Directory.Exists(mechDesignRootFilepath))
        {
            Directory.CreateDirectory(mechDesignRootFilepath);
        }

        foreach (DesignSaveSlotScript designSaveSlotToMakeDummyFile in designSaveSlotList)
        {
            slotToSaveOrLoad = designSaveSlotToMakeDummyFile.saveSlotIndex;
            mechDesignFilepathToSaveOrLoadToOrFrom = Path.Combine(mechDesignRootFilepath, "MechDesign" + slotToSaveOrLoad + ".dat");
            if (!File.Exists(mechDesignFilepathToSaveOrLoadToOrFrom))
            {
                SaveThisMech();
            }
        }
        //This variable initializingSaveLoadSlots prevents an infinite loop. The function SaveThisMech() calls this function, which itself calls SaveThisMech(). So if we call SaveThisMech() while initializing... is true, we skip the Initialize call and can proceed.
        initializingSaveLoadSlots = false;
    }
    public void InitializeSaveLoadSlots()
    {

        //This code runs through all the save slots and equips them with the appropriate name and mech design from saved files.
        foreach (DesignSaveSlotScript designSaveSlot in designSaveSlotList)
        {
            mechDesignFilepathToSaveOrLoadToOrFrom = Path.Combine(mechDesignRootFilepath, "MechDesign" + designSaveSlot.saveSlotIndex + ".dat");
            string jsonToInitialize = File.ReadAllText(mechDesignFilepathToSaveOrLoadToOrFrom);
            MechDesignData mechDesignToInitialize = JsonUtility.FromJson<MechDesignData>(jsonToInitialize);
            if (mechDesignToInitialize.mechDesignName == null)
            {
                mechDesignToInitialize.mechDesignName = "Empty Slot";
            }
            else
            {
                designSaveSlot.saveSlotDescription.text = mechDesignToInitialize.mechDesignName;
            }
            
            designSaveSlot.thisMechDesign = mechDesignToInitialize;

        }
    }

    public void SaveThisMech()
    {
        //This function creates a List of individual MechPartData pieces, by first creating an Array of parts in the mech and then reading off their datasets one by one. It then serializes this List to a JSON. This function is designed to be called from the Save Design buttons, which will first supply this script with the correct Save Slot to save to.

        List<MechPartData> MechPartsInDesign = new List<MechPartData>();
        var actualPartTransformsInMech = currentlyEditedMech.GetComponentsInChildren<Transform>();
        int numberOfPartsInMech = actualPartTransformsInMech.Length;
        for (int i = 0; i < numberOfPartsInMech; i++)
        {
            partName = actualPartTransformsInMech[i].gameObject.name;
            partName = partName.Replace("(Clone)", "");
            if (actualPartTransformsInMech[i].parent == null)
            {
                parentName = partName;
            }
            else
            {
                parentName = actualPartTransformsInMech[i].parent.gameObject.name;
                parentName = parentName.Replace("(Clone)", "");
            }


            if (actualPartTransformsInMech[i].GetComponent<AnchorPointScript>() == null)
            {
                isAnchor = false;
            }
            else
            {
                isAnchor = true;
            }
            if (isAnchor == true)
            {
                anchorMatchMade = actualPartTransformsInMech[i].GetComponent<AnchorPointScript>().matchMade;
            }
            else
            {
                anchorMatchMade = false;
            }

            partX = actualPartTransformsInMech[i].transform.position.x;
            partY = actualPartTransformsInMech[i].transform.position.y;
            partZ = actualPartTransformsInMech[i].transform.position.z;
            scaleX = actualPartTransformsInMech[i].transform.localScale.x;
            scaleY = actualPartTransformsInMech[i].transform.localScale.y;
            scaleZ = actualPartTransformsInMech[i].transform.localScale.z;

            MechPartsInDesign.Add(new MechPartData(partName, parentName, isAnchor, anchorMatchMade, partX, partY, partZ, scaleX, scaleY, scaleZ));

        }

        var mechDesign = new MechDesignData();
        mechDesign.mechDesign = MechPartsInDesign;
        mechDesign.mechDesignName = currentlyEditedMechContainer.name;
        string mechDesignJson = JsonUtility.ToJson(mechDesign);

        //Next we write the design to a file

        mechDesignFilepathToSaveOrLoadToOrFrom = Path.Combine(mechDesignRootFilepath, "MechDesign" + slotToSaveOrLoad + ".dat");

        File.WriteAllText(mechDesignFilepathToSaveOrLoadToOrFrom, mechDesignJson);

        //Then we update the button displays to match the new name
        if (initializingSaveLoadSlots == false)
        {
            InitializeSaveLoadSlots();
        }


        //And we're done! Leaving this debug line that outputs the contents of the file to check for success for now.

        //Debug.Log(mechDesignJson);
    }

    public void NewMech()
    {
        var partsInCurrentMech = currentlyEditedMech.GetComponentsInChildren<Transform>();
        foreach (Transform partTransform in partsInCurrentMech)
        {
            if (partTransform.gameObject != currentlyEditedMech & partTransform.gameObject.name != "Floor-Leg Anchor")
            {
                Destroy(partTransform.gameObject);
            }
        }
        //The following code resets the mech name in both script and display, and ought to be standardized to a function later.
        currentlyEditedMechContainer.mechName = "Empty Mech";
        mechNameDisplay.text = currentlyEditedMechContainer.mechName;
        currentlyEditedMech.name = currentlyEditedMechContainer.mechName;
        legGroundAnchor.GetComponent<AnchorPointScript>().matchMade = false;
    }



    public void TestButton()
    {
        InitializeTheTrainingDummyProtocol();
        SaveTheCurrentMechAsIs();
        var trainingProtocol = GenerateADummyProtocolForTraining();
        SaveTheTrainingProtocolToHD(trainingProtocol);
        SceneManager.LoadScene("Mech Design Test Battle");

    }

    public void InitializeTheTrainingDummyProtocol()
    {
        //This function creates the Training Dummy Protocol if it doesn't already exists.
        //Not entirely sure how to do this at the moment...
        if (!File.Exists(GameManager.mechTrainingDummyProtocolFilepath))
        {

        }
    }

    public ProtocolSaveData GenerateADummyProtocolForTraining()
    {
        //This function creates a ProtocolSaveData which has the currently edited mech at the center, and nothing else.
        //First it makes a list of parts for ProtocolPartData instances.
        //Next it adds a Battle Anchor where the Floor-Leg Anchor is.
        var mechPartList = new List<ProtocolPartData>();
        var partScriptsInMech = GameObject.FindObjectsOfType<IndividualPartScript>();

        //We have a scale factor to apply. When applying it at the level of each part, I wound up with a bizarre gap with the Battle Anchor, since it doesn't need the scale factor. So let's try applying it at the beginning.
        //I tried and tried, and the only thing that made this gap go away was simply setting the Battle Anchor's offset in the Protocol Data to 0, so we're running with it.

        var floorLegAnchor = GameObject.Find("Floor-Leg Anchor");
        var battleAnchorForProtocol = Instantiate(battleAnchor, floorLegAnchor.transform.position, battleAnchor.transform.rotation);
        battleAnchorForProtocol.transform.SetParent(currentlyEditedMech.transform);
        battleAnchorForProtocol.GetComponent<BattleAnchorScript>().unitIAmAttachedTo = currentlyEditedMech;
        battleAnchorForProtocol.GetComponent<BattleAnchorScript>().SizeMe();
        battleAnchorForProtocol.name = battleAnchorForProtocol.name.Replace("(Clone)", "");
        var battleAnchorData = new ProtocolPartData(battleAnchorForProtocol.name, battleAnchorForProtocol.transform.localPosition.x * GameManager.mechDesignsToBattleScaleFactor * 0, battleAnchorForProtocol.transform.localPosition.y * GameManager.mechDesignsToBattleScaleFactor * 0, battleAnchorForProtocol.transform.localPosition.z * GameManager.mechDesignsToBattleScaleFactor * 0, battleAnchorForProtocol.transform.localScale.x, battleAnchorForProtocol.transform.localScale.y, battleAnchorForProtocol.transform.localScale.z);
        mechPartList.Add(battleAnchorData);
        battleAnchorForProtocol.SetActive(false);
        Destroy(battleAnchorForProtocol);

        currentlyEditedMech.transform.localScale *= GameManager.mechDesignsToBattleScaleFactor;

        //Here is where we generate, save, then delete a Battle Anchor.


        foreach (IndividualPartScript partScript in partScriptsInMech)
        {
            partScript.transform.SetParent(currentlyEditedMech.transform);
            string partName = partScript.gameObject.name;
            //Debug.Log("The part name I found is " + partName);
            string realPartName = partName.Replace("(Clone)", "");
            //Debug.Log("After attempting to cut out Clone the partname is " + realPartName);
            var partX = partScript.transform.position.x - floorLegAnchor.transform.position.x;
            //partX *= GameManager.mechDesignsToBattleScaleFactor;
            var partY = partScript.transform.position.y - floorLegAnchor.transform.position.y;
            //partY *= GameManager.mechDesignsToBattleScaleFactor;
            var partZ = partScript.transform.position.z - floorLegAnchor.transform.position.z;
            //partZ *= GameManager.mechDesignsToBattleScaleFactor;
            var partScaleX = partScript.transform.localScale.x;
            partScaleX *= GameManager.mechDesignsToBattleScaleFactor;
            var partScaleY = partScript.transform.localScale.y;
            partScaleY *= GameManager.mechDesignsToBattleScaleFactor;
            var partScaleZ = partScript.transform.localScale.z;
            partScaleZ *= GameManager.mechDesignsToBattleScaleFactor;
            var protocolPartData = new ProtocolPartData(realPartName, partX, partY, partZ, partScaleX, partScaleY, partScaleZ);
            mechPartList.Add(protocolPartData);
        }



        //The mech data is constructed out of data from the Currently Edited Mech Container which gets updates on the fly as we add and subtract parts.
        //Squares are chosen by a support function, and the parts list is given by the previous step.
        var mechDataForProtocol = new ProtocolMechData(currentlyEditedMechContainer.mechName, "North", currentlyEditedMechContainer.health, currentlyEditedMechContainer.powerUsed, currentlyEditedMechContainer.powerMax, currentlyEditedMechContainer.weightUsed, currentlyEditedMechContainer.weightMax, currentlyEditedMechContainer.cpuUsed, currentlyEditedMechContainer.cpuMax, currentlyEditedMechContainer.moveType, currentlyEditedMechContainer.move, currentlyEditedMechContainer.rotateCost, currentlyEditedMechContainer.evade, currentlyEditedMechContainer.accuracy, PickTrainingSquaresForMech(), mechPartList);
        var mechDataListForProtocol = new List<ProtocolMechData>();
        mechDataListForProtocol.Add(mechDataForProtocol);
        var trainingProtocol = new ProtocolSaveData("Test Protocol", currentlyEditedMechContainer.weightUsed, currentlyEditedMechContainer.powerUsed, currentlyEditedMechContainer.cpuUsed, mechDataListForProtocol);
        return trainingProtocol;
    }

    public List<Vector2Int> PickTrainingSquaresForMech()
    {
        var listToReturn = new List<Vector2Int>();
        if (currentlyEditedMechContainer.moveType == "Biped" || currentlyEditedMechContainer.moveType == "Fly" || currentlyEditedMechContainer.moveType == "Walk")
        {
            listToReturn.Add(new Vector2Int(3, 3));
        }
        else if (currentlyEditedMechContainer.moveType == "Large Quad" || currentlyEditedMechContainer.moveType == "Large Biped")
        {
            listToReturn.Add(new Vector2Int(3, 3));
            listToReturn.Add(new Vector2Int(3, 4));
            listToReturn.Add(new Vector2Int(2, 3));
            listToReturn.Add(new Vector2Int(2, 4));
        }
        else
        {

        }
        return listToReturn;
    }

    public void SaveTheTrainingProtocolToHD(ProtocolSaveData protocolToTest)
    {
        string protocolJson = JsonUtility.ToJson(protocolToTest);
        //Debug.Log(protocolJson);


        //Next we write the design to a file

        var protocolFilepath = Path.Combine(GameManager.mechProtocolRootFilepath, "TrainingProtocol.dat");

        File.WriteAllText(protocolFilepath, protocolJson);
    }

    public void SaveTheCurrentMechAsIs()
    {
        //This function saves mech data to a MechDesign999.dat file, to be restored when we load back in. This slot isn't associated with a Save/Load slot or SaveSlotScript and is only used for this purpose.
        slotToSaveOrLoad = "999";
        SaveThisMech();
    }

    public void LoadThePreviousMechAsWas()
    {

    }

    //These functions concern animations inside the Designs Screen.

    public void ToggleAllAnimations()
    {
        var animationScripts = FindObjectsOfType<PartAnimationController>();
        foreach (PartAnimationController partAnimator in animationScripts)
        {
            if (partAnimator.isFlyer)
            {
                if (!partAnimator.flying)
                {
                    partAnimator.Fly();
                }
                else
                {
                    partAnimator.StopFlying();
                }
            }

        }
    }

    public void DesignsZoomIn()
    {
        if (levelOfZoom == 2)
        {
            return;
        }
        else
        {
            designsBG.transform.localScale *= 1.5f;
            currentlyEditedMech.transform.localScale *= 1.5f;
            levelOfZoom++;
        }
    }

    public void DesignsZoomOut()
    {
        if (levelOfZoom == 0)
        {
            return;
        }
        else
        {
            designsBG.transform.localScale /= 1.5f;
            currentlyEditedMech.transform.localScale /= 1.5f;
            levelOfZoom--;
        }
    }

    public IEnumerator OpenPartDoors()
    {
        //Debug.Log("Open doors was called");
        if (doorsOpen)
        {
            //Debug.Log("Trying to open doors but they're already open, returning");
            yield return null;
        }
        else
        {
            StartCoroutine(AnimationManager.TranslateLinearlyOverTime(partDoorsLeft, new Vector2(-2.5f, 0), 2));
            StartCoroutine(AnimationManager.TranslateLinearlyOverTime(partDoorsRight, new Vector2(2.5f, 0), 2));
            yield return new WaitForSeconds(2);
            doorsOpen = true;
            yield return null;
        }
    }

    public IEnumerator ClosePartDoors()
    {
        //Debug.Log("Close doors was called");
        if (!doorsOpen)
        {
            //Debug.Log("Trying to close doors but they're already closed");
            yield return null;
        }
        else
        {
            StartCoroutine(AnimationManager.TranslateLinearlyOverTime(partDoorsLeft, new Vector2(2.5f, 0), 2));
            StartCoroutine(AnimationManager.TranslateLinearlyOverTime(partDoorsRight, new Vector2(-2.5f, 0), 2));
            yield return new WaitForSeconds(2);
            doorsOpen = false;
            yield return null;
        }
    }

    public void ToggleDoors()
    {
        StartCoroutine(OpenAndCloseDoors());
        //if (doorsOpen)
        //{
        //    //Debug.Log("Doors open, closing them");
        //    StartCoroutine(ClosePartDoors());
        //}
        //else if (!doorsOpen)
        //{
        //    //Debug.Log("Doors closed, opening them");
        //    StartCoroutine(OpenPartDoors());
        //}
        ToggleAllAnimations();
    }

    public IEnumerator OpenAndCloseDoors()
    {
        Debug.Log("Opening and closing doors...");
        yield return StartCoroutine(OpenPartDoors());
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Done waiting after opening doors, now closing them...");
        yield return StartCoroutine(ClosePartDoors());
    }

    public void ScrollTheInfoText()
    {

        if (focusLCDPartStatsScrollRoutine != null)
        {
            StopCoroutine(focusLCDPartStatsScrollRoutine);
        }
        if (focusLCDMechStatsScrollRoutine != null)
        {
            StopCoroutine(focusLCDMechStatsScrollRoutine);
        }
        var partStatsText = focusLCDPartStats.gameObject;
        var mechStatsText = focusLCDMechStats.gameObject;
        focusLCDPartStatsScrollRoutine = StartCoroutine(AnimationManager.ScrollLinearlyAndRepeat(partStatsText, Vector2.left, 1.33f, 15));
        focusLCDMechStatsScrollRoutine = StartCoroutine(AnimationManager.ScrollLinearlyAndRepeat(mechStatsText, Vector2.left, 1.33f, 20));
    }

    public IEnumerator RollInThePartToInstall(GameObject partToRollIn)
    {
        //
        //This is an aesthetic helper function for the old part install function.
        //It instantiates the part, but at a render layer that puts it behind the Mech Lab doors.
        //It begins opening the doors, and then once they're open, it places the part at its usual render layer.
        //Then it closes the doors.
        //

        float shrinkFactor = 0.5f;
        float timeToRollIn = 3f;
        float timeToWaitFromDoorsOpening = 0.5f;

        //First we set parts in the background and shrink them into the BG.
        SpriteRenderer[] partSpriteRenderers = partToRollIn.GetComponentsInChildren<SpriteRenderer>();
        int[] partDisplayLayers = new int[partSpriteRenderers.Length];
        for (int i = 0; i < partSpriteRenderers.Length; i++)
        {
            partDisplayLayers[i] = partSpriteRenderers[i].sortingOrder;
            partSpriteRenderers[i].sortingOrder -= 10;
        }

        partToRollIn.transform.localScale *= shrinkFactor;

        Debug.Log("About to wait for the doors to open..");
        yield return new WaitForSeconds(timeToWaitFromDoorsOpening);
        Debug.Log("Done waiting, about to execute the grow-an-object script");
        StartCoroutine(AnimationManager.TranslateLinearlyOverTime(focusedPart, new Vector2(-1.25f, -1.15f), timeToRollIn));
        yield return StartCoroutine(AnimationManager.GrowAnObjectLinearly(partToRollIn, (1 / shrinkFactor), timeToRollIn));
        Debug.Log("Control returned to the Install Button from the Animation Manager");

        for (int i = 0; i < partSpriteRenderers.Length; i++)
        {
            partSpriteRenderers[i].sortingOrder = partDisplayLayers[i];
        }



        yield return null;
    }


}





//This class stores all the pertinent variables for a single part inside the currently edited mech. When saving a mech design, we'll generate a List of these data sets to write to JSON.

[System.Serializable]
public class MechPartData
{
    public string partName;
    public string parentName;
    public bool isAnchor;
    public bool anchorMatchMade;
    public float partX;
    public float partY;
    public float partZ;
    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public MechPartData(string partName, string parentName, bool isAnchor, bool anchorMatchMade, float partX, float partY, float partZ, float scaleX, float scaleY, float scaleZ)
    {
        this.partName = partName;
        this.parentName = parentName;
        this.isAnchor = isAnchor;
        this.anchorMatchMade = anchorMatchMade;
        this.partX = partX;
        this.partY = partY;
        this.partZ = partZ;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.scaleZ = scaleZ;
    }
}

//The following class is what we'll actually be writing to JSON, composed of the data from all the individual parts in the design.
[System.Serializable]
public class MechDesignData
{
    public string mechDesignName;
    public List<MechPartData> mechDesign;
}




