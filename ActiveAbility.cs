using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveAbility : MonoBehaviour
{
    //Active ability parameters are set in the Inspector.

    //Top level ability descriptors
    public string abilityName;
    public Sprite abilitySprite;
    public string abilityDescription;
    public ActiveAbilityButtonScript myButton;
    public IndividualCombatAnimation myCombatAnimation;
    


    //Low level ability parameters
    public float power;
    //Power is the power per hit, modified by the burst size and number of bursts.
    public float powerVariance;
    //powerVariance is the % the damage can wobble by.
    public int cpuCost;

    public int minRange;
    public int maxRange;
    public string trajectory;
    //Trajectories are as follows:
    //"Direct" requires line of sight and is unaffected by height.
    //"Homing" does not require line of sight and is unaffected by height.
    //"Ballistic" does not require line of sight. It is affected by height -- 2 height units = 1 unit of range.
    //"Melee" requires line of sight, and it is affected by height but only negatively.
    public int firingArc;

    public string effectArea;
    //Effect areas are as follows:
    //"Single" is a single square, for the average single-target attack
    //"Plus" is a plus-shape
    //"Ninesquare" is a 3x3 square
    //"All" affects all squares in the range
    //"Diamond" is a diamond centered on the target square, e.g. +2 in X and Y
    //"Random" affects random squares in the range

    public int randomStrikes;
    //The number of times the ability strikes within the effect area.
    public string randomEffectArea;
    //The area within which the random strikes occur. Uses the same list of effect areas.


    public bool affectsEnemies;
    public bool affectsAllies;

    public float accuracyMean;
    public float accuracyStandardDeviation;
    public string accuracyType;
    //Accuracy types are as follows:
    //"Hitscan" means attack is either hit or miss, determined only by accuracyMean and modifiers. Appropriate for kinetic projectiles.
    //"Gaussian" means attack damage is determined by the accuracy roll, with the max at accuracyMean but the damage reduced proportionately by roll / mean. Appropriate for explosions and energy bursts.
    //"Hybrid" means attack damage is 100% within half a stdDev of mean, 50% between half and 1.5, and zero otherwise. Appropriate for small-burst projectiles like HEAT rounds.

    public int roundsPerBurst;
    public int numberOfBursts;
    public float burstDelay;
    public float betweenBurstDelay;
    public bool rangeAttenuated;
    public float rangeAttenuationFactor;

    //Some abilities may move the actor or the target, so these variables record that.
    public bool movesTheUser;
    public bool movesTheTargets;
    public bool isTeleport;
    public bool isPush;
    //Note squaresToPushBy can be negative in the case of a pull.
    public int squaresToPushBy;
    public BattleManager.CharacterPosition actorResultingCharacterPosition;
    public List<BattleManager.CharacterPosition> targetResultingCharacterPositions;

    //Some abilities may create or remove Passive Abilities, so these variables record that.
    public bool givesAPassiveAbility;
    public bool removesAPassiveAbility;
    public List<PassiveAbility> abilitiesToGive;
    public List<PassiveAbility> abilitiesToRemove;
    public Dictionary<PassiveAbility, float> abilityChangeChancePerHitDictionary;
    public string abilityNameToGive;
    public string[] abilityNamesToRemove;

    public List<Vector2Int> FindMyTargetRange()
    {
        return new List<Vector2Int>();
    }



    public void sizeTheCursorAnchorForMe()
    {
        if (effectArea == "Single")
        {

        }
    }

    public bool RollToHit()
    {
        //True if hit, false if miss
        return false;
    }

    public int DetermineDamage()
    {
        return 0;
    }

    public IndividualCombatAnimation FindMyCombatAnimation()
    {
        var combatAnimationsInMyPart = gameObject.GetComponents<IndividualCombatAnimation>();
        foreach (IndividualCombatAnimation combatAnim in combatAnimationsInMyPart)
        {
            if (combatAnim.nameOfAbilityIRepresent == abilityName)
            {
                return combatAnim;
            }
        }
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        myCombatAnimation = FindMyCombatAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
