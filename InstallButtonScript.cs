using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstallButtonScript : MonoBehaviour
{
    private Button thisButton;
    private GameObject focusedPart;
    private DesignsManagerScript designsManagerScript;
    public GameObject partToInstall;
    

    // Start is called before the first frame update
    void Start()
    {
        thisButton = GetComponent<Button>();
        designsManagerScript = GameObject.Find("DesignsManager").GetComponent<DesignsManagerScript>();
        thisButton.onClick.AddListener(BeginInstallingPart);
    }

    public void BeginInstallingPart()
    {
        focusedPart = designsManagerScript.focusedPart;
        if (focusedPart != null)
        {
            partToInstall = Instantiate(focusedPart, new Vector3(7f, 1.5f, 0), focusedPart.transform.rotation);
            if (designsManagerScript.levelOfZoom == 2)
            {
                partToInstall.transform.localScale *= 1.5f;
            }
            else if (designsManagerScript.levelOfZoom == 0)
            {
                partToInstall.transform.localScale /= 1.5f;
            }
            IndividualPartScript individualPartScript = partToInstall.GetComponent<IndividualPartScript>();
            individualPartScript.readyToInstall = true;
            designsManagerScript.installingPart = true;
            designsManagerScript.focusedPart = partToInstall;
        }
    }

        

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
