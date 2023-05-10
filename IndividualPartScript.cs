using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IndividualPartScript : MonoBehaviour
{
    //Backstage variables
    private CurrentlyEditedMechContainer currentlyEditedMechContainerScript;
    private GameObject currentlyEditedMech;
    public DesignsManagerScript designsManagerScript;
    public bool readyToInstall = false;
    private bool installing = false;
    public bool installed = false;
    

    
    //Core mech parameters
    public string partName = "Part Name";
    public int health = 0;
    public int powerUsed = 0;
    public int powerMax = 0;
    public int weightUsed = 0;
    public int weightMax = 0;
    public int cpuUsed = 0;
    public int cpuMax = 0;
    public string moveType = "Walk";
    public int move = 0;
    public float rotateCost = 0;
    public float evade = 0;
    public float accuracy = 0;
    public string descriptionText;

    //Mech components to reference in-script
    public Sprite partSprite;
    public Color partColor;
    public Collider2D partCollider;
    private Vector3 mouseOffset;
    public GameObject attachablePoint;
    public GameObject attachingPoint;
    public Vector3 attachableAnchorPosition;
    public Vector3 availableAttachableAnchorPosition;
    private Vector3 posDelta;
    
    

    //Active abilities
    public bool attackPlasmaBallLauncher = false;
       
    //Start is called before the first frame update
    void Start()
    {
        partSprite = GetComponent<SpriteRenderer>().sprite;
        partColor = GetComponent<SpriteRenderer>().color;
        partCollider = GetComponent<Collider2D>();
        if (SceneManager.GetActiveScene().name == "Designs Screen")
        {
            currentlyEditedMech = GameObject.FindWithTag("CurrentlyEditedMech");
            currentlyEditedMechContainerScript = currentlyEditedMech.GetComponent<CurrentlyEditedMechContainer>();
            designsManagerScript = GameObject.Find("DesignsManager").GetComponent<DesignsManagerScript>();
        }

    }

    //Update is called once per frame
    void Update()
    {
        if (readyToInstall == true)
        {
            MakeMeTransparent();
            installing = true;
            readyToInstall = false;
        }
        if (installed == true)
        {
            
        }
    }

    public void MakeMeTransparent()
    {
        var mySpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in mySpriteRenderers)
        {
            var rendererColor = renderer.color;
            rendererColor.a = 0.5f;
            renderer.color = rendererColor;
        }
    }

    public void MakeMeSolidColored()
    {
        var mySpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in mySpriteRenderers)
        {
            var rendererColor = renderer.color;
            rendererColor.a = 1f;
            renderer.color = rendererColor;
        }
    }

    

    private void OnMouseDown()
    {
        if (installing == true)
        {
            mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (installed == true)
        {
            designsManagerScript.FocusOnPart(gameObject);
            designsManagerScript.SwitchToDeleteButton();
        }
    }
    private void OnMouseDrag()
    {
        if (installing == true)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + mouseOffset;
            
            
        }
    }

    private void OnMouseUp()
    {
        if (installing == true)
        {
            if (attachablePoint != null && attachingPoint.GetComponent<AnchorPointScript>().canAttach == true)
            {
                AttachThisPart();
            }
            else
            {
                Destroy(gameObject);
            }    
        }
    }

    private void AttachThisPart()
    {
        //attaches the part to the relevant attachablePoint and adds it to the currently Edited mech, then restores it to full color
        attachableAnchorPosition = attachingPoint.transform.position;
        availableAttachableAnchorPosition = attachablePoint.transform.position;
        posDelta = availableAttachableAnchorPosition - attachableAnchorPosition;
        gameObject.transform.position += posDelta;
        MakeMeSolidColored();
        //The next code sets the object's parent to the parent of the point we attached to
        if (attachablePoint.GetComponentInParent<IndividualPartScript>() != null)
        {
            gameObject.transform.SetParent(attachablePoint.GetComponentInParent<IndividualPartScript>().gameObject.transform);
            AdjustMyPositionForAnAnchor(attachablePoint.GetComponent<AnchorPointScript>());
        }
        else
        {
            gameObject.transform.SetParent(currentlyEditedMech.transform);
        }

        //The next code adds the part's stats to the Currently Edited Mech Container
        designsManagerScript.partJustChanged = true;
        //Next we need code that will turn off all the anchor points
        attachingPoint.GetComponent<AnchorPointScript>().matchMade = true;
        attachablePoint.GetComponent<AnchorPointScript>().matchMade = true;
        designsManagerScript.TurnOffAnchorPoints();
        
        //Finally we exit installing mode
        installing = false;
        installed = true;
    }

    public void AdjustMyPositionForAnAnchor(AnchorPointScript anchorToAttachTo)
    {
        if (anchorToAttachTo.scaleFactorToApply != 0)
        {
            float anchorScaleFactor = anchorToAttachTo.scaleFactorToApply;
            transform.localScale *= anchorScaleFactor;
        }

        if (anchorToAttachTo.orderInLayerToSet != 0)
        {
            var mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            mySpriteRenderer.sortingOrder = anchorToAttachTo.orderInLayerToSet;
        }

    }

    //private void UnequipThisPart()
    //{
        //In order for this process to work we have to find which parts need to be deleted if we delete a certain part, e.g. which parts are above in the hierarchy.
        //Could we use the actual Unity hierarchy and apply parts as children..? e.g. Once anchored to a part, you're a child of that part, so when you go all your children go? That doesn't work because the anchors get disabled...
        //If instead we controlled the anchors' alpha...? But that could have unintended side effects.
        //So the best solution is to set the parent of the equipped part to the PARENT of the point we attached to. So we need to lookup its parent.
        
    //}

    //public void AddPartStatsToWhole()
    //{
    //    currentlyEditedMechContainerScript.health += health;
    //    currentlyEditedMechContainerScript.powerUsed += powerUsed;
    //    currentlyEditedMechContainerScript.powerMax += powerMax;
    //    currentlyEditedMechContainerScript.weightUsed += weightUsed;
    //    currentlyEditedMechContainerScript.weightMax += weightMax;
    //    currentlyEditedMechContainerScript.cpuUsed += cpuUsed;
    //    currentlyEditedMechContainerScript.cpuMax += cpuMax;
    //    if (moveType != null)
    //        {
    //        currentlyEditedMechContainerScript.moveType = moveType;
    //        }
    //    currentlyEditedMechContainerScript.move += move;
    //    currentlyEditedMechContainerScript.rotateCost += rotateCost;
    //    currentlyEditedMechContainerScript.evade += evade;
    //    currentlyEditedMechContainerScript.accuracy *= accuracy;
    //    designsManagerScript.UpdateMechStatDisplay();
    //}
        
   

}
