using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

//This script tracks persistent data for the Mech player that follows them around from screen to screen.
public class MechPlayerManager : MonoBehaviour
{
    public static MechPlayerManager theOneMechPlayerManager;
    //Protocol the player will enter battle with
    public static ProtocolSaveData playerProtocol;
    //Variables that track overall game progress
    public static int battlesWon = 0;
    public static int battlesLost = 0;
    public static int currentCampaignMission = 0;
    //Variables that track the next hub phases random scenarios as well as what scenario to walk into battle with
    public static Scenario scenarioToLoad;
    public static Dictionary<int, Scenario> regionScenarioDictionary;
    //
    //Variables that track tech tree progress
    //
    public static int deploymentWeightLimit;
    public static int deploymentEnergyLimit;
    public static int deploymentCpuLimit;
    //
    //Variables that track accumulated resources
    public static int dataOwned = 1000;
    public static int hydrocarbonsOwned = 300;
    public static int titaniumOwned = 300;
    public static int electrumOwned = 150;
    //
    //Variables that track game events and game flow
    public static bool returningFromMechTest;
    public static bool hubPhaseInitialized;
    //
    //Variables that govern mathematical formulas, e.g. the exchange rate between resources and expansion limits
    public static int titaniumPerWeight = 1;
    public static int weightSoftCap = 100000;
    public static float weightUncappedGrowth = 1.01f;
    public static int hydrocarbonsPerEnergy = 2;
    public static int energySoftCap = 25000;
    public static float energyUncappedGrowth = 1.01f;
    public static int electrumPerCpu = 5;
    public static int cpuSoftCap = 10000;
    public static float cpuUncappedGrowth = 1.01f;


    public static void InitializeHubPhase()
    {
        InitializeCampaignMission();
        InitializeRegionRandomScenarios();
        hubPhaseInitialized = true;
    }

    public static void InitializeCampaignMission()
    {
        //
        //The goal of this function is to read the current campaign mission and get the game ready to implement it.
        //It should first access a database of campaign mission programming based on the current campaign mission index. (We'll need to make this database some way.)
        //Then, based on what it finds in the database, it should
        //A) Prepare hub-based dialogue and cut scenes
        //B) Set up the campaign mission battle if appropriate in the Expeditions map
        //

    }

    public static void InitializeRegionRandomScenarios()
    {
        //
        //This function assigns a random scenario to each region as long as it's not the location of the next campaign mission.
        //
        for (int i = 1; i <= 5; i++)
        {
            //TO IMPLEMENT a check for whether this is the location of the next campaign mission.
            Scenario randomScenario = ScenarioManager.GenerateRandomScenario(i);
            regionScenarioDictionary.Add(i, randomScenario);
        }
    }


    void Awake()
    {
        if (theOneMechPlayerManager == null)
        {
            theOneMechPlayerManager = this;
        }
        if (theOneMechPlayerManager != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this);

        if (regionScenarioDictionary == null)
        {
            regionScenarioDictionary = new Dictionary<int, Scenario>();
        }

        if (playerProtocol == null)
        {
            GenerateInitialPlayerProtocol();
        }

        InitializeDeploymentLimits();



    }

    private void GenerateInitialPlayerProtocol()
    {
        //
        //This function is for debugging for now. In situations where a playerProtocol is needed but doesn't exist, we initialize a random enemy protocol.
        //This function can be called in the Awake() phase but only after loading the player's persistent player data occurs. The player's persistent data should contain their protocol, and this function should check if the playerProtocol is null.
        //

        int enemyProtocolRoll = Random.Range(1, 5);
        string enemyProtocolFileName = "Enemy" + enemyProtocolRoll + ".dat";
        string enemyProtocolFilepath = Path.Combine(Application.dataPath, "Resources", "Enemy Protocols", enemyProtocolFileName);
        string enemyProtocolDataString = File.ReadAllText(enemyProtocolFilepath);
        ProtocolSaveData enemyProtocol = JsonUtility.FromJson<ProtocolSaveData>(enemyProtocolDataString);
        playerProtocol = enemyProtocol;

    }
        
    void Start()
    {

    }

    private void InitializeDeploymentLimits()
    {
        //This function sets the deployment limits to defaults if no limits exist when the Manager is Loaded.
        //Normally the limits will be loaded out of the player data, so this function is mainly for working in the Editor.
        if (deploymentWeightLimit < 10000)
        {
            deploymentWeightLimit = 10000;
        }
        if (deploymentEnergyLimit < 5000)
        {
            deploymentEnergyLimit = 5000;
        }
        if (deploymentCpuLimit < 1000)
        {
            deploymentCpuLimit = 1000;
        }
    }

    public static int GrowWeightLimit(int titaniumToInput)
    {
        //
        //This function takes a certain amount of titanium as the input, then reads the current amount of weight capacity for deployment, and calculates how much weight capacity we would add if we used up this much titanium.
        //
        //This function is trifurcated into the 3 resource types. The code is copy / pasted but name-adjusted for the resource types and the separate soft caps and growth rates. When adjusting one function, remember to adjust all three.
        //
        //The basic structure of the function is to separate into cases -- 100% uncapped growth, 100% softcapped growth, and split growth, depending on our proximity to the limit and the amount of spent resource. The third case ought to be quite uncommon -- in fact it should only trigger once per resource per game.
        //

        int kgToGrowBy = 0;
        int distanceToSoftcap = weightSoftCap - deploymentWeightLimit;
        int uncappedGrowth = Mathf.FloorToInt(titaniumToInput / titaniumPerWeight);
        if (uncappedGrowth < distanceToSoftcap)
        {
            //If we're not going to reach the softcap with this growth, the growth is linear and we're done.
            kgToGrowBy = uncappedGrowth;
            return kgToGrowBy;
        }

        else
        {
            //Otherwise we split the growth into an uncapped and a capped portion and calculate them separately.
            //First the case where all growth is capped:
            if (distanceToSoftcap < 0)
            {
                //In this case the entire growth is capped.
                uncappedGrowth = 0;
                //Each kg of weight limit will cost the growth factor more than the previous.

                //First we find the cost of the next kg and see if it's payable. We also estimate how many times to iterate using this number.

                int costForVeryNextkg = Mathf.FloorToInt(titaniumPerWeight * Mathf.Pow(weightUncappedGrowth, (deploymentWeightLimit - weightSoftCap)));
                float numberOfTimesWeCanPayThisCost = costForVeryNextkg / titaniumToInput;

                if (numberOfTimesWeCanPayThisCost < 1)
                {
                    return 0;
                }
                else
                {
                    int titaniumAvailable = titaniumToInput;
                    for (int i = 0; i < Mathf.FloorToInt(numberOfTimesWeCanPayThisCost); i++)
                    {
                        int costOfNextKg = Mathf.FloorToInt(costForVeryNextkg * Mathf.Pow(weightUncappedGrowth, i));
                        if (costOfNextKg < titaniumAvailable)
                        {
                            kgToGrowBy += 1;
                            titaniumAvailable -= costOfNextKg;
                        }
                        else
                        {
                            return kgToGrowBy;
                        }

                    }
                    return kgToGrowBy;
                }

            }

            //Finally the case where some growth is capped, and some isn't:
            else
            {
                int kgToGrowByUncapped = distanceToSoftcap;
                int titaniumSpentUncapped = kgToGrowByUncapped * titaniumPerWeight;
                int titaniumToSpendCapped = titaniumToInput - titaniumSpentUncapped;

                kgToGrowBy = kgToGrowByUncapped;

                int numberOfTimesToTrySpending = Mathf.FloorToInt(titaniumToSpendCapped / titaniumPerWeight);

                if (numberOfTimesToTrySpending == 0)
                {
                    return kgToGrowBy;
                }

                int titaniumAvailable = titaniumToSpendCapped;
                for (int i = 0; i < numberOfTimesToTrySpending; i++)
                {
                    int costOfNextKg = Mathf.FloorToInt(titaniumPerWeight * Mathf.Pow(weightUncappedGrowth, i));
                    if (costOfNextKg < titaniumAvailable)
                    {
                        kgToGrowBy += 1;
                        titaniumAvailable -= costOfNextKg;
                    }
                    else
                    {
                        return kgToGrowBy;
                    }

                }
                return kgToGrowBy;
            }
        }
    }

    public static int GrowCPULimit(int electrumToInput)
    {
        //This function takes a certain amount of electrum as the input, then reads the current amount of CPU capacity for deployment, and calculates how much CPU capacity we would add if we used up this much electrum.
        int gHzToGrowBy = 0;
        int distanceToSoftcap = cpuSoftCap - deploymentCpuLimit;
        int uncappedGrowth = Mathf.FloorToInt(electrumToInput / electrumPerCpu);
        if (uncappedGrowth < distanceToSoftcap)
        {
            //If we're not going to reach the softcap with this growth, the growth is linear and we're done.
            gHzToGrowBy = uncappedGrowth;
            return gHzToGrowBy;
        }

        else
        {
            //Otherwise we split the growth into an uncapped and a capped portion and calculate them separately.
            //First the case where all growth is capped:
            if (distanceToSoftcap < 0)
            {
                //In this case the entire growth is capped.
                uncappedGrowth = 0;
                //Each kg of weight limit will cost the growth factor more than the previous.

                //First we find the cost of the next kg and see if it's payable. We also estimate how many times to iterate using this number.

                int costForVeryNextGHz = Mathf.FloorToInt(electrumPerCpu * Mathf.Pow(cpuUncappedGrowth, (deploymentCpuLimit - cpuSoftCap)));
                float numberOfTimesWeCanPayThisCost = costForVeryNextGHz / electrumToInput;

                if (numberOfTimesWeCanPayThisCost < 1)
                {
                    return 0;
                }
                else
                {
                    int electrumAvailable = electrumToInput;
                    for (int i = 0; i < Mathf.FloorToInt(numberOfTimesWeCanPayThisCost); i++)
                    {
                        int costOfNextGHz = Mathf.FloorToInt(costForVeryNextGHz * Mathf.Pow(cpuUncappedGrowth, i));
                        if (costOfNextGHz < electrumAvailable)
                        {
                            gHzToGrowBy += 1;
                            electrumAvailable -= costOfNextGHz;
                        }
                        else
                        {
                            return gHzToGrowBy;
                        }

                    }
                    return gHzToGrowBy;
                }

            }

            //Finally the case where some growth is capped, and some isn't:
            else
            {
                int gHzToGrowByUncapped = distanceToSoftcap;
                int electrumSpentUncapped = gHzToGrowByUncapped * electrumPerCpu;
                int electrumToSpendCapped = electrumToInput - electrumSpentUncapped;

                gHzToGrowBy = gHzToGrowByUncapped;

                int numberOfTimesToTrySpending = Mathf.FloorToInt(electrumToSpendCapped / electrumPerCpu);

                if (numberOfTimesToTrySpending == 0)
                {
                    return gHzToGrowBy;
                }

                int electrumAvailable = electrumToSpendCapped;
                for (int i = 0; i < numberOfTimesToTrySpending; i++)
                {
                    int costOfNextGHz = Mathf.FloorToInt(electrumPerCpu * Mathf.Pow(cpuUncappedGrowth, i));
                    if (costOfNextGHz < electrumAvailable)
                    {
                        gHzToGrowBy += 1;
                        electrumAvailable -= costOfNextGHz;
                    }
                    else
                    {
                        return gHzToGrowBy;
                    }

                }
                return gHzToGrowBy;
            }
        }
        
    }

    public static int GrowEnergyLimit(int hydrocarbonsToInput)
    {
        //This function takes a certain amount of hydrocarbons as the input, then reads the current amount of energy capacity for deployment, and calculates how much energy capacity we would add if we used up this much electrum.
        int mWToGrowBy = 0;
        int distanceToSoftcap = cpuSoftCap - deploymentEnergyLimit;
        int uncappedGrowth = Mathf.FloorToInt(hydrocarbonsToInput / hydrocarbonsPerEnergy);
        if (uncappedGrowth < distanceToSoftcap)
        {
            //If we're not going to reach the softcap with this growth, the growth is linear and we're done.
            mWToGrowBy = uncappedGrowth;
            return mWToGrowBy;
        }

        else
        {
            //Otherwise we split the growth into an uncapped and a capped portion and calculate them separately.
            //First the case where all growth is capped:
            if (distanceToSoftcap < 0)
            {
                //In this case the entire growth is capped.
                uncappedGrowth = 0;
                //Each kg of weight limit will cost the growth factor more than the previous.

                //First we find the cost of the next kg and see if it's payable. We also estimate how many times to iterate using this number.

                int costForVeryNextMW = Mathf.FloorToInt(hydrocarbonsPerEnergy * Mathf.Pow(energyUncappedGrowth, (deploymentEnergyLimit - energySoftCap)));
                float numberOfTimesWeCanPayThisCost = costForVeryNextMW / hydrocarbonsToInput;

                if (numberOfTimesWeCanPayThisCost < 1)
                {
                    return 0;
                }
                else
                {
                    int hydrocarbonsAvailable = hydrocarbonsToInput;
                    for (int i = 0; i < Mathf.FloorToInt(numberOfTimesWeCanPayThisCost); i++)
                    {
                        int costOfNextMW = Mathf.FloorToInt(costForVeryNextMW * Mathf.Pow(energyUncappedGrowth, i));
                        if (costOfNextMW < hydrocarbonsAvailable)
                        {
                            mWToGrowBy += 1;
                            hydrocarbonsAvailable -= costOfNextMW;
                        }
                        else
                        {
                            return mWToGrowBy;
                        }

                    }
                    return mWToGrowBy;
                }

            }

            //Finally the case where some growth is capped, and some isn't:
            else
            {
                int mWToGrowByUncapped = distanceToSoftcap;
                int hydrocarbonsSpentUncapped = mWToGrowByUncapped * hydrocarbonsPerEnergy;
                int hydrocarbonsToSpendCapped = hydrocarbonsToInput - hydrocarbonsSpentUncapped;

                mWToGrowBy = mWToGrowByUncapped;

                int numberOfTimesToTrySpending = Mathf.FloorToInt(hydrocarbonsToSpendCapped / hydrocarbonsPerEnergy);

                if (numberOfTimesToTrySpending == 0)
                {
                    return mWToGrowBy;
                }

                int hydrocarbonsAvailable = hydrocarbonsToSpendCapped;
                for (int i = 0; i < numberOfTimesToTrySpending; i++)
                {
                    int costOfNextMW = Mathf.FloorToInt(hydrocarbonsPerEnergy * Mathf.Pow(energyUncappedGrowth, i));
                    if (costOfNextMW < hydrocarbonsAvailable)
                    {
                        mWToGrowBy += 1;
                        hydrocarbonsAvailable -= costOfNextMW;
                    }
                    else
                    {
                        return mWToGrowBy;
                    }

                }
                return mWToGrowBy;
            }
        }

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    

    public void ReturnFromTestMode()
    {
        SceneManager.LoadScene("Designs Screen");
        returningFromMechTest = true;
    }
}
