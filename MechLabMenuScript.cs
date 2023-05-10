using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechLabMenuScript : MonoBehaviour
{
    private GameObject navButtons;
    private GameObject menuBG;
    private GameObject maximizeButton;

    // Start is called before the first frame update
    void Start()
    {
        navButtons = GameObject.Find("Nav Buttons");
        menuBG = GameObject.Find("Background");
        maximizeButton = GameObject.Find("Maximize Menu Button");
        maximizeButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    //void Update()
    //{
    //   
    //}

    //The following two methods are used with the "Explore" feature.
    public void MinimizeMenu()
    {
        navButtons.gameObject.SetActive(false);
        menuBG.gameObject.SetActive(false);
        maximizeButton.gameObject.SetActive(true);
    }

    public void MaximizeMenu()
    {
        maximizeButton.gameObject.SetActive(false);
        navButtons.gameObject.SetActive(true);
        menuBG.gameObject.SetActive(true);
    }
}
