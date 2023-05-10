using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHubNavUnitScript : MonoBehaviour
{
    //
    //This script goes on the Sprites which represent the main branches of the Mech Hub.
    //It darkens those sprites but causes them to brighten on mouse-over.
    //When clicked it takes you to that particular branch of the mech hub.
    //

    public SpriteRenderer mySpriteRenderer;
    public string myScene;
    public Color initialColor;
    public float darkenedColor = 0.55f;

    public GameObject[] objectsToDarkenWithMe;

    public float zoomedX;
    public float zoomedY;
    public float zoomedCameraSize;

    public static float timeToZoom = 1f;

    public static bool zooming;
    public bool zoomedOnMe;

    private void Awake()
    {
        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        initialColor = mySpriteRenderer.color;
        mySpriteRenderer.color = new Color(darkenedColor, darkenedColor, darkenedColor, 1);
        foreach (GameObject target in objectsToDarkenWithMe)
        {
            var targetSprite = target.GetComponent<SpriteRenderer>();
            targetSprite.color = new Color(darkenedColor, darkenedColor, darkenedColor, 1);
        }
    }

    private void OnMouseEnter()
    {
        if (!zooming)
        {
            mySpriteRenderer.color = initialColor;
            foreach (GameObject target in objectsToDarkenWithMe)
            {
                var targetSprite = target.GetComponent<SpriteRenderer>();
                targetSprite.color = new Color(1, 1, 1, 1);
            }
        }


        StartCoroutine(ZoomInOnMe());
    }

    private void OnMouseExit()
    {
        if (!zooming)
        {
            mySpriteRenderer.color = new Color(darkenedColor, darkenedColor, darkenedColor, 1);
            foreach (GameObject target in objectsToDarkenWithMe)
            {
                var targetSprite = target.GetComponent<SpriteRenderer>();
                targetSprite.color = new Color(darkenedColor, darkenedColor, darkenedColor, 1);
            }
        }


        StartCoroutine(ZoomOutFromMe());
    }

    public IEnumerator ZoomInOnMe()
    {
        if (!zooming && !zoomedOnMe)
        {
            zooming = true;
            StartCoroutine(AnimationManager.SwoopAndZoomTheCameraAtSomething(Camera.main, new Vector3(zoomedX, zoomedY, -10), zoomedCameraSize, timeToZoom));
            yield return new WaitForSeconds(timeToZoom);
            zoomedOnMe = true;
            zooming = false;
            yield return null;
        }
    }

    public IEnumerator ZoomOutFromMe()
    {
        if (!zooming && zoomedOnMe)
        {
            zooming = true;
            StartCoroutine(AnimationManager.SwoopAndZoomTheCameraAtSomething(Camera.main, new Vector3(0, 0, -10), 5, timeToZoom));
            yield return new WaitForSeconds(timeToZoom);
            zoomedOnMe = false;
            zooming = false;
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
