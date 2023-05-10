using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEdgeScript : MonoBehaviour
{
    // Start is called before the first frame update

    public Camera mainCamera;
    public int scrollSpeed = 2;
    public bool limitLeft = false;
    public bool limitRight = false;
    public bool limitUp = false;
    public bool limitDown = false;
    //The same Script can be used for the edges of the screen and the world.
    //isEdge should be set for specific spots in the inspector.
    //The directional settings are also set in the inspector and allow specific boxes to cause motion in specific directions.
    public bool isEdge;
    public bool isLeft;
    public bool isRight;
    public bool isUp;
    public bool isDown;

    public BattleManager battleManagerScript;
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        battleManagerScript = GameObject.FindObjectOfType<BattleManager>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnMouseOver()
    {
        if (isLeft && !limitLeft && !isEdge && CheckForPlayerScrollControl())
        {
            mainCamera.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        }
        if (isRight && !limitRight && !isEdge && CheckForPlayerScrollControl())
        {
            mainCamera.transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
        }
        if (isUp && !limitUp && !isEdge && CheckForPlayerScrollControl())
        {
            mainCamera.transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
        }
        if (isDown && !limitDown && !isEdge && CheckForPlayerScrollControl())
        {
            mainCamera.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
        {
            if (isLeft && !limitLeft && collision.gameObject.GetComponent<ScreenEdgeScript>().isLeft)
            {
                //Debug.Log("I am left, limit left is not active, and my encounter is with a left thing!");
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    //Debug.Log("Seems we encountered a collider!");
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitLeft = true;
                    this.limitLeft = true;
                    //Debug.Log("Tried to set that collider to think it's at the limit.");
                }

            }
            if (isRight && !limitRight && collision.gameObject.GetComponent<ScreenEdgeScript>().isRight)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitRight = true;
                    this.limitRight = true;
                }
            }
            if (isUp && !limitUp && collision.gameObject.GetComponent<ScreenEdgeScript>().isUp)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitUp = true;
                    this.limitUp = true;
                }
            }
            if (isDown && !limitDown && collision.gameObject.GetComponent<ScreenEdgeScript>().isDown)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitDown = true;
                    this.limitDown = true;
                }
            }
        }
        //Debug.Log("Collision between " + this.gameObject.name + " and other object " + collision.gameObject.name);
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
        {
            if (isLeft && limitLeft && collision.gameObject.GetComponent<ScreenEdgeScript>().isLeft)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitLeft = false;
                    this.limitLeft = false;
                }

            }
            if (isRight && limitRight && collision.gameObject.GetComponent<ScreenEdgeScript>().isRight)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitRight = false;
                    this.limitRight = false;
                }
            }
            if (isUp && limitUp && collision.gameObject.GetComponent<ScreenEdgeScript>().isUp)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitUp = false;
                    this.limitUp = false;
                }
            }
            if (isDown && limitDown && collision.gameObject.GetComponent<ScreenEdgeScript>().isDown)
            {
                if (collision.gameObject.GetComponent<ScreenEdgeScript>() != null)
                {
                    collision.gameObject.GetComponent<ScreenEdgeScript>().limitDown = false;
                    this.limitDown = false;
                }
            }
        }
        //Debug.Log("Collision over between " + this.gameObject.name + " and other object " + collision.gameObject.name);

    }

    private bool CheckForPlayerScrollControl()
    {
        //This function looks at various game-state variables in the Battle Manager and returns false, disabling scrolling, if any of them are true.
        if (battleManagerScript.consideringAProposedAbility || battleManagerScript.shoppingForAnActiveAbility || battleManagerScript.readyToMove)
        {
            return false;
        }
        return true;
    }

}
