using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPointScript : MonoBehaviour
{
    private ParticleSystem particleEmitter;
    private ParticleSystem otherParticleEmitter;
    public GameObject parentPart;
    public GameObject attachingPoint;
    public GameObject attachablePoint;
    public IndividualPartScript parentPartScript;
    private AnchorPointScript otherAnchorPointScript;


    public int anchorType;
    public bool canAttach = false;
    public bool attachmentMade = false;
    public Vector3 anchorPosition;
    public Vector3 availableAnchorPosition;
    public bool matchMade = false;

    public int orderInLayerToSet;
    public float scaleFactorToApply;
    //This variable causes the attaching part to enter the marked order in layer, if it's not null.
    //It can also cause the part to scale, e.g. if we want to appear like we're in the background, or if a part is somehow huge or tiny and we want to smudge sizes.
    

    //Anchor types: first is what you have, second is what you're seeking
    //0: Ground-Leg
    //1: Leg-Ground

    //3: Leg-Torso
    //4: Torso-leg

    //6: Torso-arm
    //7: Arm-torso

    //8: Hang a torso from me
    //9: Torso-head
    //10: Head-torso

    //11 Arm-ArmWeapon
    //12 ArmWeapon - Arm

    // Start is called before the first frame update
    void Start()
    {
        parentPart = transform.parent.gameObject;
        parentPartScript = parentPart.GetComponent<IndividualPartScript>();
        particleEmitter = GetComponent<ParticleSystem>();
        particleEmitter.Stop();
    }

    

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //This code looks up the AnchorPoint script on the nearby anchor. It gets its type, and if the types match, it triggers the matchable state. This can be re-written into a custom function for clean-up later.
        GameObject other = collision.gameObject;
        otherAnchorPointScript = other.GetComponent<AnchorPointScript>();
        if (otherAnchorPointScript != null)
        {
            
            int otherType;
            otherType = otherAnchorPointScript.anchorType;
            if (otherType == anchorType - 1)
            {
                //This condition needs to be checked to filter out "collisions" with non-part elements of the scene.
                if (otherAnchorPointScript != null)
                {
                    otherParticleEmitter = other.gameObject.GetComponent<ParticleSystem>();
                    otherParticleEmitter.Play();
                    particleEmitter.Play();
                    attachablePoint = other.gameObject;
                    canAttach = true;
                    //This condition needs to be checked because the floor anchor's parent doesn't have an Individual Part Script and will bark out a null exception if it's not filtered out.
                    if (parentPartScript != null)
                    {
                        parentPartScript.attachablePoint = attachablePoint;
                        parentPartScript.attachingPoint = gameObject;
                        anchorPosition = gameObject.transform.position;
                        availableAnchorPosition = other.gameObject.transform.position;
                        parentPartScript.attachableAnchorPosition = anchorPosition;
                        parentPartScript.availableAttachableAnchorPosition = availableAnchorPosition;
                    }
                                  
                }
            }
        }        
    }

    // private void OnTriggerStay2D(Collider2D collision)
    // {
        //Every frame we're in contact we export the anchors' position to the Individual Part Script so it knows how to translate in order to click into place
    //    if (otherAnchorPointScript != null && parentPartScript != null && attachablePoint != null)
    //    {
    //           
    //                    parentPartScript.attachablePoint = attachablePoint;
    //                    anchorPosition = gameObject.transform.position;
    //                    availableAnchorPosition = attachablePoint.transform.position;
    //                    parentPartScript.attachableAnchorPosition = anchorPosition;
    //                    parentPartScript.availableAttachableAnchorPosition = availableAnchorPosition;
    //    }
    // }

    private void OnTriggerExit2D(Collider2D collision)
    {
        canAttach = false;
        particleEmitter.Stop();
    }

    // Update is called once per frame
    //void Update()
    //{
    //   if (attachmentMade == true)
    //    {
    //        gameObject.SetActive(false)
    //    }
    //}
}
