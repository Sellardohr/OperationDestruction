using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraScript : MonoBehaviour
{
    //Variables that find needed game objects
    public Camera mainCamera;
    public EdgeCollider2D screenEdgeCollider;

    //Variables that help with edge panning
    public int edgeBuffer = 50;
    public int scrollSpeed = 5;

    private void Awake()
    {
        mainCamera = this.gameObject.GetComponent<Camera>();
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
