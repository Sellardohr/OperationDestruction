using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleActionPreviewScript : MonoBehaviour
{

    GameObject battlePreviewContent;
    Image leftCharacterImage;
    TextMeshProUGUI leftCharacterText;
    Image rightCharacterImage;
    TextMeshProUGUI rightCharacterText;
    GameObject redArrowStreakSpawner;
    public GameObject redArrowStreak;
    //The red arrow streak and any other action-graphics are set in the Inspector.
    TextMeshProUGUI abilityPreviewText;

    public static float secondsToSlideIn = 0.65f;

    public bool initializing = true;

    public Vector3 initialPosition;

    public static int arrowCount = 0;
    public static bool exiting = false;



    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        battlePreviewContent = GameObject.Find("Battle Preview Content");
        redArrowStreakSpawner = GameObject.Find("Red Arrow Streak Spawner");
        leftCharacterText = GameObject.Find("Left Character Name").GetComponent<TextMeshProUGUI>();
        leftCharacterImage = GameObject.Find("Battle Preview Left Portrait").GetComponent<Image>();
        rightCharacterText = GameObject.Find("Right Character Name").GetComponent<TextMeshProUGUI>();
        rightCharacterImage = GameObject.Find("Battle Preview Right Portrait").GetComponent<Image>();
        abilityPreviewText = GameObject.Find("Ability Preview Text").GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnable()
    {

    }

    public void InitializeTheActionPreview(BattleManager.RefinedProposedAction actionToPreview)
    {

        arrowCount = 0;

        leftCharacterText.text = actionToPreview.actionToRefine.actor.characterName;
        leftCharacterImage.sprite = actionToPreview.actionToRefine.actor.characterPortrait;
        rightCharacterText.text = actionToPreview.actionToRefine.targetList[0].characterName;
        rightCharacterImage.sprite = actionToPreview.actionToRefine.targetList[0].characterPortrait;

        FillTheActionPreviewText(actionToPreview);

        StartCoroutine(SlideInThePreviewContent());
    }

    public void FillTheActionPreviewText(BattleManager.RefinedProposedAction actionToPreview)
    {
        abilityPreviewText.text = "" + actionToPreview.actionToRefine.abilityToUse.abilityName + "\n" + "\n" + "Expected Dmg: " + (actionToPreview.meanRawDmg * actionToPreview.actionToRefine.abilityToUse.roundsPerBurst * actionToPreview.actionToRefine.abilityToUse.numberOfBursts) + "\n" + "\n" + "Expected Acc: " + actionToPreview.chanceToHit[actionToPreview.actionToRefine.targetList[0]];
    }

    public void LaunchARedArrow(float alpha, float scaleX, float scaleY, float speed, float positionY)
    {

        //This function launches an individual red arrow under random parameters.
        //
        //While the arrow is flying, if "exiting" gets flagged, it destroys it immediately, otherwise it waits 2 seconds.

        var spawnerPos = redArrowStreakSpawner.transform.position;
        var placeToSpawn = new Vector3(spawnerPos.x, spawnerPos.y + positionY, spawnerPos.z);
        var newArrow = Instantiate(redArrowStreak, redArrowStreakSpawner.transform);
        newArrow.transform.position = placeToSpawn;

        Color newArrowColor = newArrow.GetComponent<Image>().color;
        newArrowColor.a = alpha;
        newArrow.GetComponent<Image>().color = newArrowColor;

        newArrow.transform.localScale = new Vector3(newArrow.transform.localScale.x * scaleX, newArrow.transform.localScale.y * scaleY, newArrow.transform.localScale.z);

        newArrow.GetComponent<DieInXSecondsScript>().speed = speed;
    }

    public IEnumerator LaunchTheRedArrows()
    {
        //This function continually spawns red arrows up to a max of 25 as long as "exiting" is not flagged.
        while (!exiting)
        {
            if (arrowCount > 25)
            {
                yield return new WaitForSeconds(1);
            }
            float timeToWait = Random.Range(0.05f, 0.2f);
            float alpha = Random.Range(0.33f, 1);
            float scaleX = Random.Range(0.8f, 2);
            float scaleY = Random.Range(0.8f, 1.2f);
            float positionY = Random.Range(-1.5f, 1.5f);
            float speed = Random.Range(1f, 5f);
            LaunchARedArrow(alpha, scaleX, scaleY, speed, positionY);
            arrowCount += 1;
            yield return new WaitForSeconds(timeToWait);
        }
        yield return null;
    }

    public void DestroyAllRedArrows()
    {
        var arrows = FindObjectsOfType<DieInXSecondsScript>();
        foreach (DieInXSecondsScript arrow in arrows)
        {
            arrow.gameObject.SetActive(false);
            Destroy(arrow.gameObject);
        }
        arrowCount = 0;
    }

    public IEnumerator SlideInThePreviewContent()
    {
        exiting = false;
        var thisRect = gameObject.GetComponent<RectTransform>();

        //Vector3 startPosition = thisRect.localPosition;
        //Debug.Log("We think the preview's start position is " + startPosition);

        float aspect = Screen.width / Screen.height;
        float worldHeight = Camera.main.orthographicSize * 2;
        float screenUnityWidth = aspect * worldHeight;
        //Debug.Log("We think the screen's width in unity units is " + screenUnityWidth);


        thisRect.Translate(new Vector3(-screenUnityWidth, 0, 0));
        float timePassed = 0;

        while (timePassed < secondsToSlideIn)
        {
            timePassed += Time.deltaTime;
            var distanceToTranslate = Time.deltaTime * (screenUnityWidth / secondsToSlideIn);
            thisRect.Translate(new Vector3(distanceToTranslate, 0, 0));
            yield return null;
        }
        thisRect.position = Camera.main.transform.position + new Vector3(0, 0, 10);
        StartCoroutine(LaunchTheRedArrows());

        //Debug.Log("Returning the preview to " + startPosition);
        yield return null;
    }

    public IEnumerator SlideOutThePreviewContent()
    {
        exiting = true;
        var thisRect = gameObject.GetComponent<RectTransform>();

        //Vector3 startPosition = thisRect.localPosition;
        //Debug.Log("We think the preview's start position is " + startPosition);

        float aspect = Screen.width / Screen.height;
        float worldHeight = Camera.main.orthographicSize * 2;
        float screenUnityWidth = aspect * worldHeight;
        //Debug.Log("We think the screen's width in unity units is " + screenUnityWidth);

        float timePassed = 0;
        while (timePassed < secondsToSlideIn)
        {
            timePassed += Time.deltaTime;
            var distanceToTranslate = 1.5f * Time.deltaTime * (screenUnityWidth / secondsToSlideIn);
            thisRect.Translate(new Vector3(-distanceToTranslate, 0, 0));
            yield return null;
        }
        exiting = false;
        gameObject.SetActive(false);
        //Debug.Log("Returning the preview to " + startPosition);
        yield return null;
    }
}
