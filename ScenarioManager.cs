using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{

    private void Start()
    {

    }
    public static Scenario GenerateRandomScenario(int regionIndex)
    {
        //
        //This function generates a random scenario.
        //At time of writing only the Basic Skirmish is implemented, eventually we'll implement other battle objectives.
        //When we do so we'll update this function.
        //

        var scenarioToReturn = new Scenario();

        //
        //Here we roll for a win condition.
        //

        int winConditionRoll = Random.Range(0, 0);
        if (winConditionRoll == 0)
        {
            scenarioToReturn.winCondition = WinCondition.BasicSkirmish();
        }
        else if (winConditionRoll == 1)
        {
            //other objectives, etc.
        }

        //
        //The player protocol for now is their chosen protocol, but for other purposes we might want to set this to pre-made protocols.
        //

        scenarioToReturn.playerProtocol = MechPlayerManager.playerProtocol;

        //
        //Here we add enemy protocols.
        //
        //For now we only add one, in the future we may want to rearrange this function so that multiples can be selected when such a thing is appropriate for the map and objectives.
        //

        var enemyProtocolList = new List<ProtocolSaveData>();
        int enemyProtocolRoll = Random.Range(1, 5);
        string enemyProtocolFileName = "Enemy" + enemyProtocolRoll + ".dat";
        string enemyProtocolFilepath = Path.Combine(Application.dataPath, "Resources", "Enemy Protocols", enemyProtocolFileName);
        string enemyProtocolDataString = File.ReadAllText(enemyProtocolFilepath);
        ProtocolSaveData enemyProtocol = JsonUtility.FromJson<ProtocolSaveData>(enemyProtocolDataString);
        enemyProtocolList.Add(enemyProtocol);

        
        //With the enemey protocol list constructed, we add it to the scenario under construction.
        scenarioToReturn.enemyProtocols = enemyProtocolList;
        //

        //
        //Likewise for building what protocols spawn where -- in the future this functionality will require more developing, for now it's just 0 and 1.
        //
        var protocolAnchorDict = new Dictionary<ProtocolSaveData, int>();
        protocolAnchorDict.Add(MechPlayerManager.playerProtocol, 0);
        protocolAnchorDict.Add(enemyProtocol, 1);
        scenarioToReturn.protocolsByProtocolAnchorIndex = protocolAnchorDict;
        //

        //Now to find a map:
        int sceneNumber = Random.Range(1, 1);
        string sceneName = ConvertRegionIndexToRegionName(regionIndex) + "Random" + sceneNumber;
        scenarioToReturn.sceneToLoad = sceneName;
        //

        //Finally we determine resource rewards. Eventually this will be algorithmic based on the objectives and enemy protocols, for now just 100, 10, 1.
        scenarioToReturn.resourceOneToReward = 100;
        scenarioToReturn.resourceTwoToReward = 10;
        scenarioToReturn.resourceThreeToReward = 1;
        //






        return scenarioToReturn;
    }

    public static string ConvertRegionIndexToRegionName(int regionIndex)
    {
        //Region numbers:
        //1 = Lowlands
        //2 = Desert
        //3 = Taiga
        //4 = Highlands
        //5 = Fortress
        string outputString = "";
        if (regionIndex == 1)
        {
            return "Lowlands";
        }
        else if (regionIndex == 2)
        {
            return "Desert";
        }
        else if (regionIndex == 3)
        {
            return "Taiga";
        }
        else if (regionIndex == 4)
        {
            return "Highlands";
        }
        else if (regionIndex == 5)
        {
            return "Fortress";
        }
        return outputString;
    }

    

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}

public class Scenario
{
    public WinCondition winCondition;
    public ProtocolSaveData playerProtocol;
    public List<ProtocolSaveData> enemyProtocols;
    public Dictionary<ProtocolSaveData, int> protocolsByProtocolAnchorIndex;
    public string sceneToLoad;
    public int resourceOneToReward;
    public int resourceTwoToReward;
    public int resourceThreeToReward;

}

public class WinCondition
{
    public bool defeatAllMechs;
    public bool defeatOneMech;
    public bool protectAMech;
    public bool reachASquare;
    public List<Vector2Int> squaresToReach;
    public bool defendASquare;
    public List<Vector2Int> squaresToDefend;
    public bool surviveXTurns;
    public int turnsToSurvive;

    public static WinCondition BasicSkirmish()
    {
        var conditionToReturn = new WinCondition();
        conditionToReturn.defeatAllMechs = true;
        conditionToReturn.defeatOneMech = false;
        conditionToReturn.protectAMech = false;
        conditionToReturn.reachASquare = false;
        conditionToReturn.defendASquare = false;
        conditionToReturn.surviveXTurns = false;
        return conditionToReturn;
    }

    public static WinCondition DefeatTheLeader()
    {
        var conditionToReturn = new WinCondition();
        conditionToReturn.defeatAllMechs = false;
        conditionToReturn.defeatOneMech = true;
        conditionToReturn.protectAMech = false;
        conditionToReturn.reachASquare = false;
        conditionToReturn.defendASquare = false;
        conditionToReturn.surviveXTurns = false;
        return conditionToReturn;
    }

    public static WinCondition SurviveTenTurns()
    {
        var conditionToReturn = new WinCondition();
        conditionToReturn.defeatAllMechs = false;
        conditionToReturn.defeatOneMech = false;
        conditionToReturn.protectAMech = false;
        conditionToReturn.reachASquare = false;
        conditionToReturn.defendASquare = false;
        conditionToReturn.surviveXTurns = true;
        conditionToReturn.turnsToSurvive = 10;
        return conditionToReturn;
    }

}
   
