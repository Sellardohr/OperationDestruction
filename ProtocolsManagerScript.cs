using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class ProtocolsManagerScript : MonoBehaviour
{
    //Variables that help us translate from Design to Battle
    public float designToBattleScaleFactor = 0.33f;
    public float tileHeightToWorldYScaleFactor = 1;

    //Variables that store the performance stats of the current Protocol
    public int currentProtocolWeight = 0;
    public int currentProtocolPower = 0;
    public int currentProtocolCPU = 0;
    public int maxProtocolWeight = 15000;
    public int maxProtocolPower = 1000;
    public int maxProtocolCPU = 250;
    public bool overWeight = false;
    public string currentProtocolName = "Empty protocol";

    //Variables that reference other objects in the Protocols Scene
    public GameObject mainNavButtons;
    public GameObject designsLibrary;
    public GameObject protocolsLibrary;
    public TextMeshProUGUI currentProtocolWeightText;
    public TextMeshProUGUI currentProtocolPowerText;
    public TextMeshProUGUI currentProtocolCpuText;
    public TextMeshProUGUI focusedMechStatsText;
    public GameObject focusedMechPreviewPanel;
    public Vector3 focusedMechPreviewLocation;

    public TextMeshProUGUI protocolNameInputText;
    public TMP_InputField protocolNameInputDisplay;
    public GameObject protocolNameInputBox;
    public TextMeshProUGUI protocolNameDisplay;

    public GameObject mechToDelete;

    //Variables for working with the battlegrid
    public List<TerrainTile> terrainTileScriptsInGrid;
    public List<Vector2> terrainTilePositions;
    public List<Vector2Int> terrainTileGridPositions;
    public Dictionary<Vector2, Vector2Int> terrainWorldToGridDictionary;
    public Dictionary<Vector2Int, Vector2> terrainGridToWorldDictionary;
    public Dictionary<Vector2Int, TerrainTile> terrainTileDictionary;
    public Dictionary<TerrainTile, Vector2Int> terrainGridFromTileDictionary;
    public Vector2 tileOriginOffset;

    //The battle anchor prefab will be set in the inspector:
    public GameObject battleAnchor;

    //Variables that organize save data for Designs
    public string slotToSaveOrLoad = "1";
    public string mechDesignRootFilepath;
    public string mechDesignFilepathToSaveOrLoadToOrFrom;
    public List<MechDesignData> mechDesignDataLookup;
    public DesignSaveSlotScript[] designSaveSlotList;

    //Variables that organize save data for Protocols
    public List<BattleParticipant> battleParticipantsInProtocol;
    public ProtocolSaveSlotScript[] protocolSaveSlotList;
    public string protocolSlotToSaveOrLoad = "1";
    public string protocolRootFilepath;
    public string protocolFilepathToSaveOrLoadToOrFrom;
    public ProtocolSaveData protocolToExecute;


    //Variables that store info on the mech we're focusing on in the current protocol

    public BattleParticipant battleParticipantUnderConstruction;

    //Variables that track the game flow

    public bool loadingProtocol = false;


    private void Awake()
    {

        designSaveSlotList = FindObjectsOfType<DesignSaveSlotScript>();
        protocolSaveSlotList = FindObjectsOfType<ProtocolSaveSlotScript>();
        mainNavButtons = GameObject.Find("Main Navigation Buttons");
        designsLibrary = GameObject.Find("Save/Load Designs Menu");
        protocolsLibrary = GameObject.Find("Save/Load Protocols Menu");
        currentProtocolWeightText = GameObject.Find("Weight Text").GetComponent<TextMeshProUGUI>();
        currentProtocolPowerText = GameObject.Find("Power Text").GetComponent<TextMeshProUGUI>();
        currentProtocolCpuText = GameObject.Find("CPU Text").GetComponent<TextMeshProUGUI>();
        focusedMechStatsText = GameObject.Find("Focused Mech Deploy Reqs Text").GetComponent<TextMeshProUGUI>();
        focusedMechPreviewPanel = GameObject.Find("Focused Mech Preview");
        protocolNameInputBox = GameObject.Find("Protocol Name Input");
        protocolNameInputText = GameObject.Find("Protocol Name Input Text").GetComponent<TextMeshProUGUI>();
        protocolNameInputDisplay = GameObject.Find("Protocol Name Input").GetComponent<TMP_InputField>();
        protocolNameDisplay = GameObject.Find("Protocol Name Display").GetComponent<TextMeshProUGUI>();
        protocolNameDisplay.text = currentProtocolName;
        focusedMechPreviewLocation = focusedMechPreviewPanel.transform.position;
        terrainWorldToGridDictionary = new Dictionary<Vector2, Vector2Int>();
        terrainGridToWorldDictionary = new Dictionary<Vector2Int, Vector2>();
        terrainTileDictionary = new Dictionary<Vector2Int, TerrainTile>();
        terrainGridFromTileDictionary = new Dictionary<TerrainTile, Vector2Int>();
        battleParticipantsInProtocol = new List<BattleParticipant>();
    }

    void Start()
    {
        //First we find where Designs and Protocols are saved and make sure that the designs are assigned to the save slots so we can see them once we open their menus.
        InitializeSaveLoadFilepath();
        InitializeSaveLoadSlots();
        InitializeProtocolSaveLoadFilepath();
        InitializeProtocolSaveLoadSlots();
        InitializeProtocolMaxStats();
        //Next we set unneeded UI elements to inactive.
        designsLibrary.gameObject.SetActive(false);
        protocolNameInputBox.SetActive(false);
        protocolsLibrary.gameObject.SetActive(false);
        //Next we read the battlegrid, populating the Dictionaries that associate terrain tiles with grid positions.
        UpdateCurrentProtocolStats();
        ReadTheBattleGridSquares();
        SetUpTheGridPositions(terrainTilePositions);
        AssignTilesToTheGrid();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    //The following functions find the right filepath to save/load Designs from, then equips the Save Slots with their designs saved on the hard drive.
    public void InitializeSaveLoadFilepath()
    {

        slotToSaveOrLoad = "1";
        mechDesignRootFilepath = Path.Combine(Application.persistentDataPath, "Designs");
        if (!Directory.Exists(mechDesignRootFilepath))
        {
            Directory.CreateDirectory(mechDesignRootFilepath);
        }
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

    public void InitializeProtocolSaveLoadFilepath()
    {
        {

            protocolSlotToSaveOrLoad = "1";
            protocolRootFilepath = Path.Combine(Application.persistentDataPath, "Protocols");
            if (!Directory.Exists(protocolRootFilepath))
            {
                Debug.Log("Creating directory...");
                Directory.CreateDirectory(protocolRootFilepath);
            }
            //Debug.Log("The Protocols directory is " + protocolRootFilepath);
        }
    }

    public void InitializeProtocolSaveLoadSlots()
    {
        //This script runs through each of the Protocol Save Slots and creates dummy files if needed.
        foreach (ProtocolSaveSlotScript protocolSlot in protocolSaveSlotList)
        {
            protocolFilepathToSaveOrLoadToOrFrom = Path.Combine(protocolRootFilepath, "Protocol" + protocolSlot.saveSlotIndex + ".dat");
            if (!File.Exists(protocolFilepathToSaveOrLoadToOrFrom))
            {
                Debug.Log("Making a blank Protocol file in slot " + protocolSlot.saveSlotIndex);
                //File.Create(protocolFilepathToSaveOrLoadToOrFrom);
                var emptyProtocol = new ProtocolSaveData("Empty Slot", 0, 0, 0, new List<ProtocolMechData>());
                var emptyProtocolJson = JsonUtility.ToJson(emptyProtocol);
                File.WriteAllText(protocolFilepathToSaveOrLoadToOrFrom, emptyProtocolJson);
            }
            string jsonToInitialize = File.ReadAllText(protocolFilepathToSaveOrLoadToOrFrom);
            ProtocolSaveData protocolToInitialize = JsonUtility.FromJson<ProtocolSaveData>(jsonToInitialize);
            protocolSlot.saveSlotDescription.text = protocolToInitialize.protocolName;


            protocolSlot.thisProtocol = protocolToInitialize;
        }
    }

    //The following functions navigate between different moments of game flow and tell different components how to behave in each state.

    public void PrepareToInstallDesign()
    {
        foreach (TerrainTile tile in terrainTileScriptsInGrid)
        {
            if (tile.whatOccupiesMe == null)
            {
                tile.objectCanEnter = true;
                tile.isOccupied = false;
            }

        }
    }

    public void AbortInstallingDesign()
    {
        foreach (TerrainTile tile in terrainTileScriptsInGrid)
        {
            tile.objectCanEnter = false;
            tile.UnHighlightMe();
            if (tile.whatOccupiesMe == null)
            {
                tile.ClearMe();
            }
        }
    }


    //The following functions update the stats and stat display of various elements in the scene, to be called any time these stats change and the change needs to be registered.

    public void InitializeProtocolMaxStats()
    {
        maxProtocolWeight = 15000;
        maxProtocolPower = 1000;
        maxProtocolCPU = 250;
    }
    public void UpdateCurrentProtocolStats()
    {
        currentProtocolWeight = 0;
        currentProtocolPower = 0;
        currentProtocolCPU = 0;
        battleParticipantsInProtocol.Clear();
        var foundBattleParticipants = GameObject.FindObjectsOfType<BattleParticipant>();

        foreach (BattleParticipant participant in foundBattleParticipants)
        {
            battleParticipantsInProtocol.Add(participant);
        }
        foreach (BattleParticipant designInProtocol in battleParticipantsInProtocol)
        {
            currentProtocolWeight += designInProtocol.weightUsed;
            currentProtocolPower += designInProtocol.powerUsed;
            currentProtocolCPU += designInProtocol.cpuUsed;
        }
        if (currentProtocolWeight > maxProtocolWeight)
        {
            currentProtocolWeightText.color = new Color(255, 0, 0);
            currentProtocolWeightText.text = "" + currentProtocolWeight;
            overWeight = true;
        }
        else
        {
            currentProtocolWeightText.color = new Color(255, 255, 255);
            currentProtocolWeightText.text = "" + currentProtocolWeight;
        }
        if (currentProtocolPower > maxProtocolPower)
        {
            currentProtocolPowerText.color = new Color(255, 0, 0);
            currentProtocolPowerText.text = "" + currentProtocolPower;
            overWeight = true;
        }
        else
        {
            currentProtocolPowerText.color = new Color(255, 255, 255);
            currentProtocolPowerText.text = "" + currentProtocolPower;
        }
        if (currentProtocolCPU > maxProtocolCPU)
        {
            currentProtocolCpuText.color = new Color(255, 0, 0);
            currentProtocolCpuText.text = "" + currentProtocolCPU;
            overWeight = true;
        }
        else
        {
            currentProtocolCpuText.color = new Color(255, 255, 255);
            currentProtocolCpuText.text = "" + currentProtocolCPU;
        }



    }

    public void DeleteThisMech(GameObject mech)
    {
        mech.SetActive(false);
        //Debug.Log("I just found " + myBattleParticipant);

        //Debug.Log("I think I just destroyed it.");
        UpdateCurrentProtocolStats();
        Destroy(mech);
    }

    public void UpdateCurrentProtocolName()
    {
        string desiredName;
        desiredName = protocolNameInputText.text;
        protocolNameDisplay.text = desiredName;
        currentProtocolName = desiredName;
        protocolNameInputBox.SetActive(false);
    }

    public void UpdateFocusedMechStatDisplay()
    {
        //This function requires a different script to feed it the BattleParticipant script under construction at such time as that script undergoes a change. Once it knows what Battle Participant script to read, we route that to the display text in the GUI.
        var weight = battleParticipantUnderConstruction.weightUsed;
        var power = battleParticipantUnderConstruction.powerUsed;
        var cpu = battleParticipantUnderConstruction.cpuUsed;
        var name = battleParticipantUnderConstruction.characterName;
        //Debug.Log("The stats the Protocol Manager knows are" + weight + power + cpu + name);
        focusedMechStatsText.text = name + "\n" + "Weight: " + weight + "\n" + "Power: " + power + "\n" + "CPU: " + cpu;
        //Debug.Log("We just attempted to set the focused Mech Stats Text to these stats");
    }

    //Functions that navigate between menus of the Protocols screen

    public void SwitchToDesignsLibrary()
    {
        mainNavButtons.gameObject.SetActive(false);
        designsLibrary.gameObject.SetActive(true);
        protocolNameInputBox.SetActive(true);
    }

    public void SwitchFromDesignsLibrary()
    {
        designsLibrary.gameObject.SetActive(false);
        mainNavButtons.gameObject.SetActive(true);
        protocolNameInputBox.SetActive(false);
    }

    public void SwitchToProtocolSaveMenu()
    {
        mainNavButtons.gameObject.SetActive(false);
        protocolsLibrary.gameObject.SetActive(true);
    }

    public void SwitchToProtocolLoadMenu()
    {
        mainNavButtons.gameObject.SetActive(false);
        protocolsLibrary.gameObject.SetActive(true);
        loadingProtocol = true;
    }

    public void SwitchFromProtocolSaveLoadMenu()
    {
        protocolsLibrary.gameObject.SetActive(false);
        mainNavButtons.gameObject.SetActive(true);
        loadingProtocol = false;
    }


    //Functions that initialize data concerning the battle grid

    public void ReadTheBattleGridSquares()
    {
        //The goals of this function are as follows: 1) Find all of the TerrainTiles on the map, 2) Assign each of them a GridX and GridY which represents their position... In order to do this, eventually we'll need to adjust for their altitude, but since this map is all flat we can skip that here... 3) Get all of these Terrain Tiles and their corresponding Grid X and Grid Y positions into an easily accessible memory structure. We might even need this to be serializable, since Protocols have to be saved and loaded, but it seems more likely we can just serialize the saved GridX and GridY for the mech in the Protocol, then reconstruct it later onto an imaginary 8x8 when we start a battle.


        var foundTerrainTiles = GameObject.FindObjectsOfType<TerrainTile>();
        foreach (TerrainTile tile in foundTerrainTiles)
        {
            terrainTileScriptsInGrid.Add(tile);
        }
        foreach (TerrainTile tile in terrainTileScriptsInGrid)
        {
            tile.worldX = tile.transform.position.x;
            tile.worldY = tile.transform.position.y;
            var actualY = tile.worldY - (tile.height * tileHeightToWorldYScaleFactor);
            var tileWorldVector = new Vector2(tile.worldX, actualY);
            terrainTilePositions.Add(tileWorldVector);
        }

        terrainTilePositions.Sort(SortWorldVectorsForGrid);
        //terrainTilePositions = NormalizeTerrainTilePosition(terrainTilePositions);
    }

    public List<Vector2> NormalizeTerrainTilePosition(List<Vector2> listToNormalize)
    {
        //This function rewrites the World vectors so that the far left origin square is defined as 0,0 rather than Unity's origin.
        var listToReturn = new List<Vector2>();
        tileOriginOffset = listToNormalize[0];
        foreach (Vector2 vector in listToNormalize)
        {
            var normalizedVector = vector - tileOriginOffset;
            listToReturn.Add(normalizedVector);
        }
        return listToReturn;
    }

    public List<Vector2> DeNormalizeTerrainTilePosition(List<Vector2> listToNormalize)
    {
        //This function UNrewrites the World vectors so that the far left origin square is defined as 0,0 rather than Unity's origin.
        var listToReturn = new List<Vector2>();

        foreach (Vector2 vector in listToNormalize)
        {
            var denormalizedVector = vector + tileOriginOffset;
            listToReturn.Add(denormalizedVector);
        }
        return listToReturn;
    }

    public void SetUpTheGridPositions(List<Vector2> sortedTerrainTileList)
    {
        Vector2 oneStepEast = sortedTerrainTileList[1] - sortedTerrainTileList[0];
        //Debug.Log("One step east is defined as " + oneStepEast);
        Vector2 oneStepNorth = sortedTerrainTileList[2] - sortedTerrainTileList[0];
        //Debug.Log("One step north is defined as " + oneStepNorth);
        Vector2 originWorld = sortedTerrainTileList[0];
        int tileListSize = sortedTerrainTileList.Count;
        foreach (Vector2 terrainTileWorldVector in sortedTerrainTileList)
        {
            if (terrainTileWorldVector == originWorld)
            {
                Vector2Int gridVector = new Vector2Int(0, 0);
                terrainTileGridPositions.Add(gridVector);
                terrainWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                terrainGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
            }
            else
            {
                Vector2 distanceFromOrigin = terrainTileWorldVector - originWorld;
                for (int gridX = 0; gridX < tileListSize; gridX++)
                {
                    for (int gridY = 0; gridY < tileListSize; gridY++)
                    {
                        var composedVector = (gridX * oneStepEast) + (gridY * oneStepNorth);
                        //Right here is the moment when the old Normalize() function was needed, so just for this step, in this function, we'll add in the starting offset:
                        composedVector += originWorld;
                        if (composedVector == terrainTileWorldVector)
                        {
                            //Debug.Log("We found a matching position from our composed vector!");
                            var gridVector = new Vector2Int(gridX, gridY);
                            terrainTileGridPositions.Add(gridVector);
                            terrainWorldToGridDictionary.Add(terrainTileWorldVector, gridVector);
                            terrainGridToWorldDictionary.Add(gridVector, terrainTileWorldVector);
                        }
                    }
                }
            }





        }
    }


    public void AssignTilesToTheGrid()
    {
        foreach (TerrainTile terrainTile in terrainTileScriptsInGrid)
        {
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

    //This is a supporting math function which simply operates the Projection of U onto V 2D vector equation. It wound up not being useful, but I'm leaving it in case it does. (It wasn't useful because the grid North and East vectors aren't orthogonal.)
    public Vector2 ProjectionOfVectorOneOntoVectorTwo(Vector2 vectorToProject, Vector2 vectorTarget)
    {

        var dotProductNumerator = Vector2.Dot(vectorToProject, vectorTarget);
        var dotProductDenominator = Vector2.Dot(vectorTarget, vectorTarget);
        var projectedVector = (dotProductNumerator / dotProductDenominator) * vectorTarget;
        return projectedVector;
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


    //Below are the functions used to Save Protocol data.

    public void SaveThisProtocol()
    {
        List<ProtocolMechData> mechsInProtocol = new List<ProtocolMechData>();
        var actualMechsInProtocol = GameObject.FindObjectsOfType<BattleParticipant>();
        foreach (BattleParticipant foundBattleParticipant in actualMechsInProtocol)
        {
            //Debug.Log("I found a Battle Participant : " + foundBattleParticipant.gameObject.name);
            //We need to locate all the parts in the mech and generate a ProtocolPartData for them.
            //We then need to make a List<> of those ProtocolPartDatas and add them to a ProtocolMechData.
            //We then need to fill out that ProtocolMechData with the rest of its information.
            //Finally we add the completed ProtocolMechData to a ProtocolSaveData which we'll write to the hard drive and associate with the Save Slot.
            string mechName;
            string thisMechName = "Unknown";
            string mechOrientation;
            int healthMax;
            int powerUsed;
            int powerMax;
            int weightUsed;
            int weightMax;
            int cpuUsed;
            int cpuMax;
            string moveType;
            int movePower;
            float rotateCost;
            float evade;
            float accuracy;
            List<Vector2Int> mechOccupiedGridSquares;

            List<ProtocolPartData> partsInThisMech = new List<ProtocolPartData>();
            var transformsInMech = foundBattleParticipant.transform.GetComponentsInChildren<Transform>();
            foreach (Transform foundTransform in transformsInMech)
            {
                if (foundTransform.parent == null)
                {
                    //Debug.Log("I found a parentless transform, its name is " + foundTransform.gameObject.name);
                    //This is the mech parent and we don't include it as a part, instead we set it as the mech Name.
                    thisMechName = foundTransform.gameObject.name;

                }
                else
                {
                    //For each child transform UNDER THIS BATTLE PARTICIPANT, we scoop up the part data and add it to the list we defined FOR THIS BATTLE PARTICIPANT.
                    //Debug.Log("I found a child transform, its name is " + foundTransform.gameObject.name);
                    var partName = foundTransform.gameObject.name;
                    partName = partName.Replace("(Clone)", "");
                    var partX = foundTransform.localPosition.x;
                    var partY = foundTransform.localPosition.y;
                    var partZ = foundTransform.localPosition.z;
                    var partScaleX = foundTransform.localScale.x;
                    var partScaleY = foundTransform.localScale.y;
                    var partScaleZ = foundTransform.localScale.z;
                    var partData = new ProtocolPartData(partName, partX, partY, partZ, partScaleX, partScaleY, partScaleZ);
                    partsInThisMech.Add(partData);

                    string partDataJson = JsonUtility.ToJson(partData);
                    //Debug.Log(partDataJson);

                }
            }



            //Now we have our list of parts, time to build the ProtocolMechData FOR THIS BATTLE PARTICIPANT. We'll then add it to the master list.
            mechName = thisMechName;
            mechOrientation = foundBattleParticipant.myOrientation;
            healthMax = foundBattleParticipant.healthMax;
            powerUsed = foundBattleParticipant.powerUsed;
            powerMax = foundBattleParticipant.powerMax;
            weightUsed = foundBattleParticipant.weightUsed;
            weightMax = foundBattleParticipant.weightMax;
            cpuUsed = foundBattleParticipant.cpuUsed;
            cpuMax = foundBattleParticipant.cpuMax;
            moveType = foundBattleParticipant.moveType;
            movePower = foundBattleParticipant.movePower;
            rotateCost = foundBattleParticipant.rotateCost;
            evade = foundBattleParticipant.evade;
            accuracy = foundBattleParticipant.accuracy;
            mechOccupiedGridSquares = foundBattleParticipant.gridPositionsIAmOver;

            var mechData = new ProtocolMechData(mechName, mechOrientation, healthMax, powerUsed, powerMax, weightUsed, weightMax, cpuUsed, cpuMax, moveType, movePower, rotateCost, evade, accuracy, mechOccupiedGridSquares, partsInThisMech);
            string mechDataJson = JsonUtility.ToJson(mechData);
            //Debug.Log(mechDataJson);

            //Now we add this participant's data to the master list.
            mechsInProtocol.Add(mechData);


        }

        //Now we know all the mechs, time to build the master file.
        string protocolName = currentProtocolName;
        int protocolWeight = currentProtocolWeight;
        int protocolPower = currentProtocolPower;
        int protocolCPU = currentProtocolCPU;

        var protocolToSave = new ProtocolSaveData(protocolName, protocolWeight, protocolPower, protocolCPU, mechsInProtocol);
        string protocolJson = JsonUtility.ToJson(protocolToSave);
        //Debug.Log(protocolJson);


        //Next we write the design to a file

        protocolFilepathToSaveOrLoadToOrFrom = Path.Combine(protocolRootFilepath, "Protocol" + protocolSlotToSaveOrLoad + ".dat");

        File.WriteAllText(protocolFilepathToSaveOrLoadToOrFrom, protocolJson);

        //Then we update the button displays to match the new name

        InitializeProtocolSaveLoadSlots();



        ////And we're done! Leaving this debug line that outputs the contents of the file to check for success for now.

        //Debug.Log(mechDesignJson);
    }

    public void ClearThisProtocol()
    {
        currentProtocolName = "Empty Protocol";
        protocolNameDisplay.text = currentProtocolName;
        var listOfMechs = GameObject.FindObjectsOfType<BattleParticipant>();
        foreach (BattleParticipant bp in listOfMechs)
        {
            foreach (Vector2Int gridSquare in bp.gridPositionsIAmOver)
            {
                terrainTileDictionary[gridSquare].ClearMe();
            }
            DeleteThisMech(bp.gameObject);
        }
        AbortInstallingDesign();
        UpdateCurrentProtocolStats();
    }

    public void AssignThisProtocolAsActive()
    {
        List<ProtocolMechData> mechsInProtocol = new List<ProtocolMechData>();
        var actualMechsInProtocol = GameObject.FindObjectsOfType<BattleParticipant>();
        foreach (BattleParticipant foundBattleParticipant in actualMechsInProtocol)
        {
            //Debug.Log("I found a Battle Participant : " + foundBattleParticipant.gameObject.name);
            //We need to locate all the parts in the mech and generate a ProtocolPartData for them.
            //We then need to make a List<> of those ProtocolPartDatas and add them to a ProtocolMechData.
            //We then need to fill out that ProtocolMechData with the rest of its information.
            //Finally we add the completed ProtocolMechData to a ProtocolSaveData which we'll write to the hard drive and associate with the Save Slot.
            string mechName;
            string thisMechName = "Unknown";
            string mechOrientation;
            int healthMax;
            int powerUsed;
            int powerMax;
            int weightUsed;
            int weightMax;
            int cpuUsed;
            int cpuMax;
            string moveType;
            int movePower;
            float rotateCost;
            float evade;
            float accuracy;
            List<Vector2Int> mechOccupiedGridSquares;

            List<ProtocolPartData> partsInThisMech = new List<ProtocolPartData>();
            var transformsInMech = foundBattleParticipant.transform.GetComponentsInChildren<Transform>();
            foreach (Transform foundTransform in transformsInMech)
            {
                if (foundTransform.parent == null)
                {
                    //Debug.Log("I found a parentless transform, its name is " + foundTransform.gameObject.name);
                    //This is the mech parent and we don't include it as a part, instead we set it as the mech Name.
                    thisMechName = foundTransform.gameObject.name;

                }
                else
                {
                    //For each child transform UNDER THIS BATTLE PARTICIPANT, we scoop up the part data and add it to the list we defined FOR THIS BATTLE PARTICIPANT.
                    //Debug.Log("I found a child transform, its name is " + foundTransform.gameObject.name);
                    var partName = foundTransform.gameObject.name;
                    partName.Replace("(Clone)", "");
                    var partX = foundTransform.localPosition.x;
                    var partY = foundTransform.localPosition.y;
                    var partZ = foundTransform.localPosition.z;
                    var partScaleX = foundTransform.localScale.x;
                    var partScaleY = foundTransform.localScale.y;
                    var partScaleZ = foundTransform.localScale.z;
                    var partData = new ProtocolPartData(partName, partX, partY, partZ, partScaleX, partScaleY, partScaleZ);
                    partsInThisMech.Add(partData);

                    string partDataJson = JsonUtility.ToJson(partData);
                    //Debug.Log(partDataJson);

                }
            }



            //Now we have our list of parts, time to build the ProtocolMechData FOR THIS BATTLE PARTICIPANT. We'll then add it to the master list.
            mechName = thisMechName;
            mechOrientation = foundBattleParticipant.myOrientation;
            healthMax = foundBattleParticipant.healthMax;
            powerUsed = foundBattleParticipant.powerUsed;
            powerMax = foundBattleParticipant.powerMax;
            weightUsed = foundBattleParticipant.weightUsed;
            weightMax = foundBattleParticipant.weightMax;
            cpuUsed = foundBattleParticipant.cpuUsed;
            cpuMax = foundBattleParticipant.cpuMax;
            moveType = foundBattleParticipant.moveType;
            movePower = foundBattleParticipant.movePower;
            rotateCost = foundBattleParticipant.rotateCost;
            evade = foundBattleParticipant.evade;
            accuracy = foundBattleParticipant.accuracy;
            mechOccupiedGridSquares = foundBattleParticipant.gridPositionsIAmOver;

            var mechData = new ProtocolMechData(mechName, mechOrientation, healthMax, powerUsed, powerMax, weightUsed, weightMax, cpuUsed, cpuMax, moveType, movePower, rotateCost, evade, accuracy, mechOccupiedGridSquares, partsInThisMech);
            string mechDataJson = JsonUtility.ToJson(mechData);
            //Debug.Log(mechDataJson);

            //Now we add this participant's data to the master list.
            mechsInProtocol.Add(mechData);


        }

        //Now we know all the mechs, time to build the master file.
        string protocolName = currentProtocolName;
        int protocolWeight = currentProtocolWeight;
        int protocolPower = currentProtocolPower;
        int protocolCPU = currentProtocolCPU;

        var protocolToSave = new ProtocolSaveData(protocolName, protocolWeight, protocolPower, protocolCPU, mechsInProtocol);
        MechPlayerManager.playerProtocol = protocolToSave;
    }

}

//The following functions provide the structure of Save Data for Protocols.

[System.Serializable]
public class ProtocolPartData
{
    public string partName;
    public float partLocalX;
    public float partLocalY;
    public float partLocalZ;
    public float partScaleX;
    public float partScaleY;
    public float partScaleZ;

    public ProtocolPartData(string partName, float partLocalX, float partLocalY, float partLocalZ, float partScaleX, float partScaleY, float partScaleZ)
    {
        this.partName = partName;
        this.partLocalX = partLocalX;
        this.partLocalY = partLocalY;
        this.partLocalZ = partLocalZ;
        this.partScaleX = partScaleX;
        this.partScaleY = partScaleY;
        this.partScaleZ = partScaleZ;
    }
}

[System.Serializable]
public class ProtocolMechData
{
    public string mechName;
    public string mechOrientation;
    public int healthMax;
    public int powerUsed;
    public int powerMax;
    public int weightUsed;
    public int weightMax;
    public int cpuUsed;
    public int cpuMax;
    public string moveType;
    public int movePower;
    public float rotateCost;
    public float evade;
    public float accuracy;
    public List<Vector2Int> mechOccupiedGridSquares;
    public List<ProtocolPartData> partsInMech;

    public ProtocolMechData(string name, string orientation, int healthMax, int powerUsed, int powerMax, int weightUsed, int weightMax, int cpuUsed, int cpuMax, string moveType, int movePower, float rotateCost, float evade, float accuracy, List<Vector2Int> gridSquares, List<ProtocolPartData> parts)
    {
        this.mechName = name;
        this.mechOrientation = orientation;
        this.healthMax = healthMax;
        this.powerUsed = powerUsed;
        this.powerMax = powerMax;
        this.weightUsed = weightUsed;
        this.weightMax = weightMax;
        this.cpuUsed = cpuUsed;
        this.cpuMax = cpuMax;
        this.moveType = moveType;
        this.movePower = movePower;
        this.rotateCost = rotateCost;
        this.evade = evade;
        this.accuracy = accuracy;
        this.mechOccupiedGridSquares = gridSquares;
        this.partsInMech = parts;
    }
}


[System.Serializable]
public class ProtocolSaveData
{
    public string protocolName;
    public int protocolWeight;
    public int protocolPower;
    public int protocolCPU;
    public List<ProtocolMechData> mechsInProtocol;

    public ProtocolSaveData(string name, int weight, int power, int cpu, List<ProtocolMechData> mechs)
    {
        this.protocolName = name;
        this.protocolWeight = weight;
        this.protocolPower = power;
        this.protocolCPU = cpu;
        this.mechsInProtocol = mechs;
    }
}






