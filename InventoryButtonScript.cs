using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class InventoryButtonScript : MonoBehaviour
{
    //Core variables
    public GameObject associatedPart;
    private IndividualPartScript associatedPartScript;
    public GameObject instantiatedPart;
    private Button thisButton;
    public Sprite partSprite;
    private DesignsManagerScript designsManagerScript;

    //Variables to lookup the Focused Part components of the GUI
    private Image focusViewport;
    private TextMeshProUGUI focusDescriptionText;
    private TextMeshProUGUI focusPartStats;
    
    
    //Start is called before the first frame update
    void Start()
    {
        thisButton = GetComponent<Button>();
        partSprite = GetComponent<Image>().sprite;
        associatedPartScript = associatedPart.GetComponent<IndividualPartScript>();
        focusViewport = GameObject.Find("Focus Viewport Image").GetComponent<Image>();
        focusDescriptionText = GameObject.Find("Focus Description Text").GetComponent<TextMeshProUGUI>();
        focusPartStats = GameObject.Find("Focus Stats Text").GetComponent<TextMeshProUGUI>();
        thisButton.onClick.AddListener(FocusPart);
        designsManagerScript = GameObject.Find("DesignsManager").GetComponent<DesignsManagerScript>();
    }

    public void FocusPart()
    {
        designsManagerScript.FocusOnPart(associatedPart);
        designsManagerScript.SwitchToInstallButton();
        //Old code which placed the focus update in this script; less useful when, e.g., we want to focus on a part by clicking on it in the current build
        //focusViewport.sprite = partSprite;
        //focusDescriptionText.text = associatedPartScript.descriptionText;
        //focusPartStats.text = associatedPartScript.health + "\n" + associatedPartScript.weightUsed + "/" + associatedPartScript.weightMax + "\n" + associatedPartScript.cpuUsed + "/" + associatedPartScript.cpuMax + "\n" + associatedPartScript.move + " Move type: " + associatedPartScript.moveType + "\n" + associatedPartScript.evade + "%...." + associatedPartScript.accuracy + "%";
        //designsManagerScript.focusedPart = associatedPart;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    

}
