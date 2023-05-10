using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentlyEditedMechContainer : MonoBehaviour
{
    // Start is called before the first frame update

    //Core mech parameters
    public string mechName = "Mech Name";
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
    public float accuracy = 1;

    //Active abilities
    public static bool attackPlasmaBallLauncher = false;

    void Start()
    {
    
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

}
