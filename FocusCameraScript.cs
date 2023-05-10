using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCameraScript : MonoBehaviour
{
    public Vector2 focusBottom;
    public Vector2 focusTop;
    public float speed = 0.66f;
    public float timeDelay = 1;

    public void SwitchFocus()
    {
        StopAllCoroutines();
        StartCoroutine(PanUpAndDown(focusBottom, focusTop, speed, timeDelay));
    }
    public IEnumerator PanUpAndDown(Vector2 bottom, Vector2 top, float speed, float timeDelay)
    {
        
        while (true)
        {
            transform.position = new Vector3(bottom.x, bottom.y, transform.position.z);
            yield return new WaitForSeconds(timeDelay);
            while (transform.position.y < top.y)
            {
                var vectorToTravel = top - bottom;
                var vector3ToTravel = new Vector3(vectorToTravel.x, vectorToTravel.y, 0);
                transform.Translate(vector3ToTravel.normalized * speed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(timeDelay);
            while (transform.position.y > bottom.y)
            {
                var vectorToTravel = bottom - top;
                var vector3ToTravel = new Vector3(vectorToTravel.x, vectorToTravel.y, 0);
                transform.Translate(vector3ToTravel.normalized * speed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(timeDelay);
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
