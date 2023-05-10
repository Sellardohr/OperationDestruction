using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAnimationManager : MonoBehaviour
{

    public BattleManager battleManager;

    public GameObject leftHandBorder;
    public GameObject leftHandBackground;
    public GameObject leftHandAnchorStandin;
    public GameObject leftHandTargetPosition;
    public Vector2 leftHandBorderInitialPosition;
    public Vector2 leftHandAnchorStandinInitialPosition;

    public GameObject rightHandBorder;
    public GameObject rightHandBackground;
    public GameObject rightHandAnchorStandin;
    public GameObject rightHandTargetPosition;
    public Vector2 rightHandBorderInitialPosition;
    public Vector2 rightHandAnchorStandinInitialPosition;

    public BattleParticipant leftHandBattleParticipant;
    public BattleParticipant rightHandBattleParticipant;

    public BattleManager.ActionOutcome actionToRun;
    public IndividualCombatAnimation combatAnimationToExecute;

    public GameObject damageReadoutCanvas;
    public GameObject damageReadoutPrefab;

    public float secondsToSlideIn = 2f;
    public float secondsToPauseBeforeCharactersEnter = 0.1f;
    public float secondsForCharactersToWalkIn = 0.6f;
    public float secondsToPauseBeforeAttack = 0.1f;
    public float secondsToPauseBeforeExit = 1f;

    private void Awake()
    {
        battleManager = FindObjectOfType<BattleManager>();
        leftHandBorder = GameObject.Find("Combat Animation Border Left");
        rightHandBorder = GameObject.Find("Combat Animation Border Right");
        leftHandBackground = GameObject.Find("Combat Animation BG Left");
        rightHandBackground = GameObject.Find("Combat Animation BG Right");
        leftHandAnchorStandin = GameObject.Find("Anchor Standin Left");
        rightHandAnchorStandin = GameObject.Find("Anchor Standin Right");
        leftHandTargetPosition = GameObject.Find("Attacker Standpoint Left");
        rightHandTargetPosition = GameObject.Find("Attacker Standpoint Right");
        damageReadoutCanvas = GameObject.Find("Hit Damage Canvas");
        

        //We'd like to be able to set the initial border positions as whatever they are, instead of hard coding these numbers.
        //However, when we slide out, we wind up in the wrong position and can't achieve the correct positions without flashing them for a frame (I don't think).
        //Therefore we're hard coding them for now.
        leftHandBorderInitialPosition = new Vector3(-4.42f, -0.13f, -9f);
        rightHandBorderInitialPosition = new Vector3(5.31f, -0.07f, -9f);

        leftHandAnchorStandinInitialPosition = new Vector3(-6.91f, -2.76f, 0);
        rightHandAnchorStandinInitialPosition = new Vector3(-6.17f, 2.99f, 0);

        
    }

    public void InitializeTheAnimationOverlay()
    {
        foreach (Transform child in leftHandAnchorStandin.GetComponentsInChildren<Transform>())
        {
            if (child != leftHandAnchorStandin.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in rightHandAnchorStandin.GetComponentsInChildren<Transform>())
        {
            if (child != rightHandAnchorStandin.transform)
            {
                Destroy(child.gameObject);
            }
        }

        leftHandAnchorStandin.transform.localPosition = leftHandAnchorStandinInitialPosition;
        rightHandAnchorStandin.transform.localPosition = rightHandAnchorStandinInitialPosition;

        leftHandBorder.transform.localPosition = leftHandBorderInitialPosition;
        rightHandBorder.transform.localPosition = rightHandBorderInitialPosition;

    }
    public void InitializeTheCombatAnimation(BattleManager.ActionOutcome actionToInitializeWith)
    {
        actionToRun = actionToInitializeWith;
        LoadTheBackgroundImages(actionToInitializeWith);
        DefineTheBattleParticipants();

        leftHandBorder.transform.localPosition = leftHandBorderInitialPosition;
        rightHandBorder.transform.localPosition = rightHandBorderInitialPosition;
    }

    public void LoadTheBackgroundImages(BattleManager.ActionOutcome actionToDisplay)
    {
        Sprite attackerBG = battleManager.terrainTileDictionary[actionToDisplay.actionTaken.actionToRefine.actor.gridPositionsIAmOver[0]].combatBackground;
        Sprite defenderBG = battleManager.terrainTileDictionary[actionToDisplay.actionTaken.actionToRefine.targetList[0].gridPositionsIAmOver[0]].combatBackground;
        if (attackerBG != null)
        {
            leftHandBackground.GetComponent<SpriteRenderer>().sprite = attackerBG;
        }
        if (defenderBG != null)
        {
            rightHandBackground.GetComponent<SpriteRenderer>().sprite = defenderBG;
        }
    }

    public void DefineTheBattleParticipants()
    {
        leftHandBattleParticipant = actionToRun.actionTaken.actionToRefine.actor;
        rightHandBattleParticipant = actionToRun.actionTaken.actionToRefine.targetList[0];
    }

    public void DefineTheAnimationToRun()
    {
        //Needs written, critical
        combatAnimationToExecute = actionToRun.actionTaken.actionToRefine.abilityToUse.myCombatAnimation;
        combatAnimationToExecute.abilityHitMissDictionary = actionToRun.actionAnimationHitOutcomeDictionary;
        Debug.Log("DefineTheAnimationToRun has executed, the combat animation we're working with is " + combatAnimationToExecute.myActiveAbility);
    }

    public void SwitchToTheBattleCopyAnimationToRun()
    {
        string combatAnimationToExecuteName = combatAnimationToExecute.nameOfAbilityIRepresent;
        var combatAnimationsInAttacker = leftHandAnchorStandin.GetComponentsInChildren<IndividualCombatAnimation>();
        foreach (IndividualCombatAnimation indivAnim in combatAnimationsInAttacker)
        {
            if (indivAnim.nameOfAbilityIRepresent == combatAnimationToExecuteName)
            {
                combatAnimationToExecute = indivAnim;
                combatAnimationToExecute.abilityHitMissDictionary = actionToRun.actionAnimationHitOutcomeDictionary;
                break;
            }
        }
    }

    public IEnumerator RunTheCombatAnimation()
    {
        yield return StartCoroutine(SlideInTheCombatBackground());
        yield return new WaitForSeconds(secondsToPauseBeforeCharactersEnter);

        GameObject attacker = CreateABattleSprite(actionToRun.actionTaken.actionToRefine.actor, leftHandAnchorStandin);
        GameObject defender = CreateABattleSprite(actionToRun.actionTaken.actionToRefine.targetList[0], rightHandAnchorStandin);
        MirrorAndNudgeTheDefenderSprite(defender);





        yield return StartCoroutine(EnterTheCombatants());

        //We need to Initialize the animation after the combatants are created, as the FindPlaceToHit() process requires there to be sprites in play in order to pick on to hit.
        DefineTheAnimationToRun();
        SwitchToTheBattleCopyAnimationToRun();
        combatAnimationToExecute.InitializeThisAnimation();

        yield return new WaitForSeconds(secondsToPauseBeforeAttack);

        Debug.Log("Starting the combat animation, time is " + Time.time);
        yield return StartCoroutine(combatAnimationToExecute.ExecuteMyAnimation());
        Debug.Log("Combat animation finished, beginning slide out, time is " + Time.time);

        yield return StartCoroutine(SlideOutTheCombatBackground());
        Debug.Log("RunTheCombatAnimation in the Combat Animation Manager is finished at time " + Time.time);
    }

    public IEnumerator SlideInTheCombatBackground()
    {
        InitializeTheAnimationOverlay();
        float initialDistanceToSlideOut = 11f;
        leftHandBorder.transform.Translate(new Vector2(-initialDistanceToSlideOut, 0));
        rightHandBorder.transform.Translate(new Vector2(initialDistanceToSlideOut, 0));
        float timePassed = 0;
        while (timePassed < secondsToSlideIn)
        {
            float distanceToTranslate = Time.deltaTime * initialDistanceToSlideOut / secondsToSlideIn;
            leftHandBorder.transform.Translate(new Vector2(distanceToTranslate, 0));
            rightHandBorder.transform.Translate(new Vector2(-distanceToTranslate, 0));
            timePassed += Time.deltaTime;
            yield return null;
        }
        leftHandBorder.transform.localPosition = leftHandBorderInitialPosition;
        rightHandBorder.transform.localPosition = rightHandBorderInitialPosition;
        yield return null;
    }

    public IEnumerator SlideOutTheCombatBackground()
    {
        float timePassed = 0;
        while (timePassed < secondsToSlideIn)
        {
            float distanceToTranslate = Time.deltaTime * 11 / secondsToSlideIn;
            leftHandBorder.transform.Translate(new Vector2(-distanceToTranslate, 0));
            rightHandBorder.transform.Translate(new Vector2(distanceToTranslate, 0));
            timePassed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public GameObject CreateABattleSprite(BattleParticipant bpToCopy, GameObject battleAnchorStandInToBuildAround)
    {
        //This function spawns a non-functional copy of a battle participant into one half of the combat animation overlay.
        //It sets this object as a child of the Battle Anchor Standin and then nudges it into such a position as its anchor lies right over the stand-in.

        var gameObjectToCopy = bpToCopy.gameObject;
        GameObject battleCopy = Instantiate(gameObjectToCopy, battleAnchorStandInToBuildAround.transform);

        //This process neuters the clone so it doesn't run BP functions.
        battleCopy.GetComponent<BattleParticipant>().enabled = false;
        GameObject battleCopyBattleAnchor = battleCopy.GetComponentInChildren<BattleAnchorScript>().gameObject;
        battleCopy.GetComponentInChildren<BattleAnchorScript>().gameObject.SetActive(false);
        //


        //Here we nudge into position.
        battleCopy.transform.localPosition = Vector3.zero;
        Vector3 distanceToTranslateIntoPosition = battleAnchorStandInToBuildAround.transform.position - battleCopyBattleAnchor.transform.position;
        //Debug.Log("It seems we need to translate the battle copy by " + distanceToTranslateIntoPosition);
        //
        //I don't know why this line needs the translation vector to be scaled up by the design/battle scale factor, but it does.
        //Without the scale factor the left-hand mech winds up below the border.
        //
        battleCopy.transform.Translate(distanceToTranslateIntoPosition / GameManager.mechDesignsToBattleScaleFactor);
        //

        //Here we grow the size of the clone back to real size and place it in a visible set of layers.
        battleCopy.transform.localScale /= GameManager.mechDesignsToBattleScaleFactor;
        foreach (SpriteRenderer partSpriteRenderer in battleCopy.GetComponentsInChildren<SpriteRenderer>())
        {
            partSpriteRenderer.sortingLayerName = "Props & Overlays";
            partSpriteRenderer.sortingOrder += 1;
        }
        //
        return battleCopy;
    }

    public void MirrorAndNudgeTheDefenderSprite(GameObject defender)
    {
        //Since the right hand combat animation has negative scale numbers in order to mirror it, this correction is needed to get the defender in place before it starts moving.
        Vector3 scale = defender.transform.localScale;
        defender.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        defender.transform.localPosition = Vector3.zero;
        var defenderAnchor = defender.GetComponentInChildren<BattleAnchorScript>(true).gameObject;
        Vector3 nudgeVector = rightHandAnchorStandin.transform.position - defenderAnchor.transform.position;
        //Debug.Log("It seems we want to nudge the defender by " + nudgeVector);
        defender.transform.Translate(-nudgeVector);
    }

    public IEnumerator EnterTheCombatants()
    {
        var attackerTargetPosition = leftHandTargetPosition.transform.position;
        var defenderTargetPosition = rightHandTargetPosition.transform.position;
        StartCoroutine(AnimationManager.SmoothlyTranslateIn(leftHandAnchorStandin, attackerTargetPosition, secondsForCharactersToWalkIn, 0.3f, 0.3f));
        StartCoroutine(AnimationManager.SmoothlyTranslateInBackwards(rightHandAnchorStandin, defenderTargetPosition, secondsForCharactersToWalkIn, 0.3f, 0.3f));
        yield return new WaitForSeconds(secondsForCharactersToWalkIn);
        Debug.Log("EnterTheCombatants() is complete, time is " + Time.time);
        yield return null;
    }




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //
    //Begin Helper functions
    //
    //These functions are generalized calls that help other combat animation functions do their jobs.
    //





}