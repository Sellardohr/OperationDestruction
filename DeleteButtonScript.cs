using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteButtonScript : MonoBehaviour
{
    private Button thisButton;
    public GameObject focusedPart;
    public IndividualPartScript focusedPartScript;
    public GameObject anchorToFree;
    public AnchorPointScript anchorToFreeScript;
    private DesignsManagerScript designsManagerScript;
    


    // Start is called before the first frame update
    void Start()
    {
        thisButton = GetComponent<Button>();
        designsManagerScript = GameObject.Find("DesignsManager").GetComponent<DesignsManagerScript>();
        thisButton.onClick.AddListener(DeleteThisPart);
    }

    public void DeleteThisPart()
    {
        focusedPart = designsManagerScript.focusedPart;
        //We need to find the anchor points that had their matches made by the deleted part and reset them so their match can be remade:
        if (focusedPart != null)
        {
            focusedPartScript = focusedPart.GetComponent<IndividualPartScript>();
            anchorToFree = focusedPartScript.attachablePoint;
            //If this is the 1st leg part, the parent has no Individual Part Script, so we need to set the freed anchor to the ground anchor, as it won't find anything in the previous step.
            if (anchorToFree == null && focusedPartScript.installed == true)
            {
                anchorToFree = designsManagerScript.legGroundAnchor;
            }
            //For a part that has never been installed, anchorToFree will still be empty becaused "installed" is false, so we skip the following free-up action.
            if (anchorToFree != null)
            {
                anchorToFreeScript = anchorToFree.GetComponent<AnchorPointScript>();
                anchorToFreeScript.matchMade = false;
            }
            //Finally we destroy the part, as long as we haven't already deleted it.
            Destroy(focusedPart);
            focusedPart.SetActive(false);
        }
        designsManagerScript.partJustChanged = true;

    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
