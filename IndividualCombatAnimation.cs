using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndividualCombatAnimation : MonoBehaviour
{
    public string nameOfAbilityIRepresent;
    public GameObject battleCopyOfThePartIAmOn;

    public ActiveAbility myActiveAbility;
    public Dictionary<int, string> abilityHitMissDictionary;
    public List<float> myShotSchedule;
    public Dictionary<float, string> myScheduledHitMissDictionary;
    public Dictionary<List<CombatAnimationEvent>, float> animationHitSchedule;
    public Dictionary<List<CombatAnimationEvent>, float> animationMissSchedule;
    public List<CombatAnimationEvent> animationsForOneHit;
    public List<CombatAnimationEvent> animationsForOneMiss;

    public float totalDuration = 3;
    public float attackLaunchHitDelta;

    public float hitEffectKnockbackDistance = 0.25f;
    public float hitEffectKnockbackDuration = 1;

    public GameObject damageReadoutCanvas;
    public GameObject damageReadoutPrefab;

    public CombatAnimationManager combatAnimationManager;

    //Individual combat animations that get attached to parts inherit from this class.
    //All individual combat animations have a series of events defined as One Hit and One Miss, as well as a burst definition.
    //The definition of One Hit and One Miss gets set at the individual level, but then the basic routine of playing the Hits and Misses in time with the Burst gets done in this parent class.
    //The overal ExecuteThisAnimation() function is also virtual, so that anything specific we might want to include in its coroutines can go with the individual animation.
    public class CombatAnimationEvent
    {
        public GameObject objectToSpawn;
        public Vector3 positionToSpawn;
        public float timeToWaitForSpawn;
        public float duration;
    }

    public virtual IEnumerator ExecuteMyAnimation()
    {
        Debug.Log("Executing the base version of ExecuteMyAnimation");
        StartCoroutine(ExecuteMyHitSchedule());
        StartCoroutine(ExecuteMyMissSchedule());
        StartCoroutine(ExecuteMyDamageDisplayNumbers());
        StartCoroutine(ExecuteMyHitResponses());
        yield return new WaitForSeconds(totalDuration);
        yield return null;
    }

    public virtual List<CombatAnimationEvent> DefineOneHit()
    {
        return null;
    }

    public virtual List<CombatAnimationEvent> DefineOneMiss()
    {
        return null;
    }

    public ActiveAbility FindMyActiveAbility()
    {
        var activesOnMyPart = gameObject.GetComponents<ActiveAbility>();
        foreach (ActiveAbility active in activesOnMyPart)
        {
            if (active.abilityName == nameOfAbilityIRepresent)
            {
                //Debug.Log("Linked up the active ability with its animation for " + nameOfAbilityIRepresent);
                return active;
            }
        }
        Debug.Log("Couldn't find the active ability for combat animation " + this.name);
        return null;
    }

    public List<float> FindMyShotSchedule(ActiveAbility abilityToSchedule)
    {
        List<float> hitSchedule = new List<float>();
        int numberOfBursts = abilityToSchedule.numberOfBursts;
        Debug.Log("Number of bursts for " + abilityToSchedule.abilityName + " is " + numberOfBursts);
        int roundsPerBurst = abilityToSchedule.roundsPerBurst;
        float betweenBurstDelay = abilityToSchedule.betweenBurstDelay;
        float inBurstDelay = abilityToSchedule.burstDelay;

        float secondsToWait = 0;

        for (int burstIndex = 0; burstIndex < numberOfBursts; burstIndex++)
        {
            secondsToWait += betweenBurstDelay;

            for (int roundIndex = 0; roundIndex < roundsPerBurst; roundIndex++)
            {
                hitSchedule.Add(secondsToWait);
                Debug.Log("Adding an entry to the hit schedule with a wait time of " + secondsToWait);
                secondsToWait += inBurstDelay;

            }

        }

        Debug.Log("" + this.gameObject.name + "has set up a shot schedule of length " + hitSchedule.Count);

        totalDuration = hitSchedule[hitSchedule.Count - 1] + 3;


        return hitSchedule;
    }

    public Dictionary<float, string> FindMyHitMissDictionary()
    {
        var dictToReturn = new Dictionary<float, string>();
        for (int i = 0; i < myShotSchedule.Count; i++)
        {
            float timeToShot = myShotSchedule[i];
            string shotStatus = abilityHitMissDictionary[i];
            dictToReturn.Add(timeToShot, shotStatus);
        }
        return dictToReturn;
    }

    public Dictionary<List<CombatAnimationEvent>, float> SetUpMyHitSchedule()
    {
        var outputDict = new Dictionary<List<CombatAnimationEvent>, float>();
        foreach (float hitTime in myScheduledHitMissDictionary.Keys)
        {
            if (myScheduledHitMissDictionary[hitTime] != "Miss" && myScheduledHitMissDictionary[hitTime] != "Evade")
            {
                outputDict.Add(DefineOneHit(), hitTime);
            }
        }

        return outputDict;
    }

    public Dictionary<List<CombatAnimationEvent>, float> SetUpMyMissSchedule()
    {
        var outputDict = new Dictionary<List<CombatAnimationEvent>, float>();
        foreach (float hitTime in myScheduledHitMissDictionary.Keys)
        {
            if (myScheduledHitMissDictionary[hitTime] == "Miss" || myScheduledHitMissDictionary[hitTime] == "Evade")
            {
                outputDict.Add(DefineOneMiss(), hitTime);
            }
        }

        return outputDict;
    }

    public IEnumerator ExecuteMyHitSchedule()
    {
        Debug.Log("Executing Hit schedule " + animationHitSchedule.Count + " entries");
        foreach (List<CombatAnimationEvent> hitEventList in animationHitSchedule.Keys)
        {
            Debug.Log("Scheduling a hit event in " + animationHitSchedule[hitEventList] + " from " + Time.time);
            StartCoroutine(WaitThenTriggerAListOfAnimationEvents(hitEventList, animationHitSchedule[hitEventList]));
        }
        yield return null;
    }

    public IEnumerator ExecuteMyMissSchedule()
    {
        Debug.Log("Executing Miss schedule with " + animationMissSchedule.Count + " entries");
        foreach (List<CombatAnimationEvent> missEventList in animationMissSchedule.Keys)
        {
            Debug.Log("Scheduling a miss event in " + animationMissSchedule[missEventList] + " from " + Time.time);
            StartCoroutine(WaitThenTriggerAListOfAnimationEvents(missEventList, animationMissSchedule[missEventList]));
        }
        yield return null;
    }

    public IEnumerator ExecuteMyDamageDisplayNumbers()
    {
        foreach (float scheduledHit in myScheduledHitMissDictionary.Keys)
        {
            Vector3 targetLocation = PickARandomPositionWithinACircle(combatAnimationManager.rightHandAnchorStandin.transform.position + new Vector3(0, 0.35f, 0), 0.35f);
            if (myScheduledHitMissDictionary[scheduledHit] != "Miss" && myScheduledHitMissDictionary[scheduledHit] != "Evade")
            {
                foreach (List<CombatAnimationEvent> combatAnimationList in animationHitSchedule.Keys)
                {
                    if (animationHitSchedule[combatAnimationList] == scheduledHit)
                    {
                        targetLocation = combatAnimationList[1].positionToSpawn;
                        break;
                    }
                }
            }

            StartCoroutine(WaitThenDisplayADamageNumber(scheduledHit + attackLaunchHitDelta, myScheduledHitMissDictionary[scheduledHit], targetLocation));
            
        }
        yield return null;
    }

    public IEnumerator ExecuteMyHitResponses()
    {
        foreach (List<CombatAnimationEvent> eventList in animationHitSchedule.Keys)
        {
            float timeToWait = animationHitSchedule[eventList];
            Debug.Log("Executing a hit response with supposed duration of " + hitEffectKnockbackDuration);
            StartCoroutine(WaitThenExecuteAHitResponse(timeToWait + attackLaunchHitDelta, combatAnimationManager.rightHandAnchorStandin, hitEffectKnockbackDistance, hitEffectKnockbackDuration));
        }
        yield return null;
    }

    public void Start()
    {
        myActiveAbility = FindMyActiveAbility();
        Debug.Log("" + this.gameObject.name + " has found an active ability of " + myActiveAbility.abilityName);
        myShotSchedule = FindMyShotSchedule(myActiveAbility);


    }

    public virtual void InitializeThisAnimation()
    {
        combatAnimationManager = FindObjectOfType<CombatAnimationManager>();
        damageReadoutCanvas = combatAnimationManager.damageReadoutCanvas;
        damageReadoutPrefab = combatAnimationManager.damageReadoutPrefab;
        myScheduledHitMissDictionary = FindMyHitMissDictionary();
        animationsForOneHit = DefineOneHit();
        animationsForOneMiss = DefineOneMiss();
        animationHitSchedule = SetUpMyHitSchedule();
        animationMissSchedule = SetUpMyMissSchedule();
    }

    public IEnumerator TriggerAnAnimationEvent(CombatAnimationEvent eventToTrigger)
    {
        Debug.Log("Triggering an animation event, " + eventToTrigger.objectToSpawn.name + " at time " + Time.time);
        yield return new WaitForSeconds(eventToTrigger.timeToWaitForSpawn);
        GameObject animationObject = Instantiate(eventToTrigger.objectToSpawn, eventToTrigger.positionToSpawn, eventToTrigger.objectToSpawn.transform.rotation);
        yield return new WaitForSeconds(eventToTrigger.duration);
        animationObject.SetActive(false);
        Destroy(animationObject);
        yield return null;
    }

    public IEnumerator TriggerAListOfAnimationEvents(List<CombatAnimationEvent> eventsList)
    {
        foreach (CombatAnimationEvent combatEvent in eventsList)
        {
            StartCoroutine(TriggerAnAnimationEvent(combatEvent));
        }
        yield return null;
    }

    public IEnumerator WaitThenTriggerAListOfAnimationEvents(List<CombatAnimationEvent> eventsList, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        StartCoroutine(TriggerAListOfAnimationEvents(eventsList));
        yield return null;
    }

    public Vector3 PickARandomPositionWithinACircle(Vector3 circleCenter, float circleRadius)
    {
        float randomRadius = Random.Range(0f, circleRadius);
        float randomAngle = Random.Range(0f, 2 * Mathf.PI);

        float randomX = Mathf.Cos(randomAngle) * randomRadius;
        float randomY = Mathf.Sin(randomAngle) * randomRadius;
        return new Vector3(circleCenter.x + randomX, circleCenter.y + randomY, circleCenter.z);
    }

    public Vector3 PickARandomPositionForHitAnimation()
    {
        var spritesInTarget = combatAnimationManager.rightHandAnchorStandin.GetComponentsInChildren<SpriteRenderer>();
        int randomIndex = Random.Range(0, spritesInTarget.Length);
        SpriteRenderer spriteToTarget = spritesInTarget[randomIndex];
        Vector3 targetTransform = spriteToTarget.transform.position;
        Vector3 targetLocation = PickARandomPositionWithinACircle(targetTransform, 0.25f);
        return targetLocation;
    }

    public IEnumerator ExecuteRecoil(GameObject objectToRecoil, Vector2 directionOfRecoil, float distanceToRecoil, float totalTimeOfRecoil)
    {
        Debug.Log("Executing recoil for " + objectToRecoil.name + " at time " + Time.time);
        float timeToGoBack = 0.2f * totalTimeOfRecoil;
        float timeToReturn = totalTimeOfRecoil - timeToGoBack;

        Vector2 goBackVector = Vector3.Normalize(directionOfRecoil) * distanceToRecoil;

        Debug.Log("Starting the first leg of recoil travel at time " + Time.time);
        yield return StartCoroutine(AnimationManager.TranslateLinearlyOverTime(objectToRecoil, goBackVector, timeToGoBack));
        //yield return new WaitForSeconds(timeToGoBack);

        Debug.Log("Starting the second leg of recoil travel at " + Time.time + " whereas we expected to wait " + timeToGoBack);
        yield return StartCoroutine(AnimationManager.TranslateLinearlyOverTime(objectToRecoil, -goBackVector, timeToReturn));


        yield return null;
    }



    public IEnumerator WaitThenRecoil(float timeToWait, GameObject objectToRecoil, float recoilMagnitude, Vector2 recoilDirection, float recoilDuration)
    {
        yield return new WaitForSeconds(timeToWait);
        yield return StartCoroutine(ExecuteRecoil(objectToRecoil, recoilDirection, recoilMagnitude, recoilDuration));
        yield return null;
    }

    public IEnumerator DisplayADamageNumber(string infoToDisplay, Vector3 locationToDisplay)
    {
        var instantiatedNumber = Instantiate(damageReadoutPrefab, damageReadoutCanvas.transform);
        instantiatedNumber.transform.position = locationToDisplay;
        instantiatedNumber.GetComponent<TextMeshProUGUI>().text = infoToDisplay;
        StartCoroutine(AnimationManager.TranslateLinearlyOverTime(instantiatedNumber, new Vector2(0, 1f), 1f));
        Debug.Log("Instantiated a damage number which shows " + infoToDisplay + " at " + locationToDisplay);
        yield return new WaitForSeconds(2);
        instantiatedNumber.SetActive(false);
        Destroy(instantiatedNumber);
        yield return null;
    }

    public IEnumerator WaitThenDisplayADamageNumber(float timeToWait, string infoToDisplay, Vector3 locationToDisplay)
    {
        yield return new WaitForSeconds(timeToWait);
        StartCoroutine(DisplayADamageNumber(infoToDisplay, locationToDisplay));
        yield return null;
    }

    public IEnumerator ExecuteAHitResponse(GameObject objectToHit, float magnitudeOfHit, float durationOfHit)
    {
        Debug.Log("Duration of hit for this hitresponse is " + durationOfHit);
        StartCoroutine(ExecuteRecoil(objectToHit, Vector2.right, magnitudeOfHit, durationOfHit));
        foreach (SpriteRenderer sprite in objectToHit.GetComponentsInChildren<SpriteRenderer>())
        {
            StartCoroutine(AnimationManager.DarkenOrLightenASprite(sprite.gameObject, 0.2f, 0.2f, 0.2f, 1f, durationOfHit * 0.2f));
        }
        yield return new WaitForSeconds(durationOfHit * 0.2f);
        foreach (SpriteRenderer sprite in objectToHit.GetComponentsInChildren<SpriteRenderer>())
        {
            StartCoroutine(AnimationManager.DarkenOrLightenASprite(sprite.gameObject, 1, 1, 1, 1, durationOfHit * 0.8f));
        }
        yield return null;
    }

    public IEnumerator WaitThenExecuteAHitResponse(float timeToWait, GameObject objectToHit, float magnitudeOfHit, float durationOfHit)
    {
        yield return new WaitForSeconds(timeToWait);
        StartCoroutine(ExecuteAHitResponse(objectToHit, magnitudeOfHit, durationOfHit));
        yield return null;
    }

}
