using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : MonoBehaviour
{
    //Active ability parameters are set in the Inspector.

    //Top level ability descriptors
    public string abilityName;
    public Sprite abilitySprite;
    public string abilityDescription;
    public BattleParticipant participantIAmAttachedTo;
    public List<BattleParticipant> participantsIAffect;

    public bool isTemporary;
    public int turnsRemaining;


    //Low level ability parameters
    public float powerModifier;
    public float powerBonus;

    public float healthModifier;
    public int healthBonus;
    public int healthOverTime;
    public float cpuModifier;
    public float cpuBonus;
    public int maxRangeBonus;

    public int abilityAreaOfEffect;

    public int firingArcOverride;

    public bool affectsEnemies;
    public bool affectsAllies;
    public bool affectsSelf;

    public float accuracyModifier;
    //Accuracy modifier is multiplicative, so if we want it to stay neutral, set it to 1.
    public float accuracyBonus;
    //Accuracy bonus is additive, so if we want it to stay neutral, set it to 0.

    public float evadeBonus;
    //Evade is only given in chunks, not scaling.

    public void Start()
    {
        participantIAmAttachedTo = gameObject.GetComponentInParent<BattleParticipant>();
        participantsIAffect = new List<BattleParticipant>();

    }

}
