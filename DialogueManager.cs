using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static GameObject dialogueBackground;
    public static Sprite dialoguePortraitSprite;
    public static TextMeshProUGUI dialogueText;

    public static Dialogue dialogueToRead;
    public static bool displayingDialogue;

    public float defaultScale = 0.011f;
    public Vector3 defaultPositionRelativeToCamera = new Vector3(0f, -3f, 100f);

    // Start is called before the first frame update
    void Start()
    {
        dialogueBackground = GameObject.Find("Dialogue Background");
        dialoguePortraitSprite = GameObject.Find("Dialogue Portrait").GetComponent<Image>().sprite;
        dialogueText = GameObject.Find("Dialogue Text").GetComponent<TextMeshProUGUI>();
        dialogueBackground.SetActive(false);

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        SizeAndPositionMeForTheCamera();
    }

    public static void AbortDialogue()
    {
        dialoguePortraitSprite = null;
        dialogueText.text = "";
        dialogueToRead = null;
        dialogueBackground.SetActive(false);
    }

    public static IEnumerator PlayDialogueEntranceAnimation()
    {
        //
        //Eventually we'll program a snappy animation here for swooping in the dialogue background.
        //
        dialogueBackground.SetActive(true);
        yield return null;
    }

    public static IEnumerator DisplayDialogue()
    {
        //
        //This function reads out the loaded dialogue letter by letter at a predetermined speed.
        //In the future we'll want to implement a keystroke to speed it up, as well as a keystroke to display the whole dialogue.
        //
        if (!dialogueBackground.activeInHierarchy)
        {
            dialogueBackground.SetActive(true);
        }
        else if (dialogueBackground.activeInHierarchy)
        {
            dialoguePortraitSprite = null;
            dialogueText.text = "";
        }

        dialoguePortraitSprite = dialogueToRead.portraitSprite;
        GameObject.Find("Dialogue Portrait").GetComponent<Image>().sprite = dialoguePortraitSprite;
        string stringToDisplay = "";
        dialogueText.text = stringToDisplay;
        Dialogue dialogueThisCoroutineIsReading = dialogueToRead;
        for (int i = 0; i < dialogueToRead.textToDisplay.Length; i++)
        {
            if (dialogueToRead != dialogueThisCoroutineIsReading)
            {
                break;
            }
            stringToDisplay = dialogueToRead.textToDisplay.Substring(0, i);
            dialogueText.text = stringToDisplay;
            yield return new WaitForSeconds(dialogueToRead.secondsBetweenLetters);
        }
        yield return null;
    }

    public IEnumerator DisplayAListOfDialogues(List<Dialogue> dialogueList)
    {
        //
        //This function reads out a series of dialogue blocks in a row.
        //At the moment it simply waits 1 second in between, in the future we'll want to trigger a waiting segment where the user can spend time reading if they want, then on keypress advance.
        //

        foreach (Dialogue dialogue in dialogueList)
        {
            yield return StartCoroutine(DisplayDialogue());
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }

    public void SizeAndPositionMeForTheCamera()
    {
        float scaleFactor = Camera.main.orthographicSize / 5f;
        transform.position = Camera.main.transform.position + (defaultPositionRelativeToCamera * scaleFactor);
        transform.localScale = new Vector3(defaultScale * scaleFactor, defaultScale * scaleFactor, 1);
    }

    public static void QuietMe()
    {
        dialogueToRead = null;
        dialogueBackground.SetActive(false);
    }

}

public class Dialogue
{
    public string textToDisplay;
    public Sprite portraitSprite;
    public float secondsBetweenLetters;

    public Dialogue(string textToDisplay, Sprite portraitSprite, float secondsBetweenLetters)
    {
        this.textToDisplay = textToDisplay;
        this.portraitSprite = portraitSprite;
        this.secondsBetweenLetters = secondsBetweenLetters;
    }
}
