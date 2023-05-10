using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationManager
{
    //This function holds various IEnumerators that control on-screen behavior -- flashing things on and off, growing and shrinking things, moving things around in various ways, etc. These are generalized functions that are called on a GameObject or a Transform.

    public static bool running;

    public static bool abortPulseTheObjectSize = false;
    public static Vector3 initialScaleForPulsing;

    public static bool abortFloatUpAndDown = false;
    public static Vector3 initialPositionForFloation;

    public static bool abortCameraSwoop = false;

    //Functions dealing with time

    public static IEnumerator SlowDownTimeForXSeconds(float timeSlowPercentage, float secondsToSlow)
    {
        yield return null;
    }


    //Functions that control object size and other intensive parameters
    public static IEnumerator PulseTheObjectSize(GameObject objectToPulse, float percentageSizeIncrease, float durationInSeconds, int numberOfTimes)
    {
        Transform objectTransform = objectToPulse.transform;
        var initialScale = objectTransform.localScale;
        initialScaleForPulsing = initialScale;
        int counter = 0;
        bool localAbort = false;
        if (abortPulseTheObjectSize)
        {
            yield return new WaitForSeconds(0.1f);
            abortPulseTheObjectSize = false;
        }
        while (counter < numberOfTimes && !localAbort)
        {
            while (objectTransform.localScale.sqrMagnitude <= (initialScale * (1 + (.01f * percentageSizeIncrease))).sqrMagnitude)
            {
                if (abortPulseTheObjectSize)
                {
                    localAbort = true;
                    break;
                }
                objectTransform.localScale += initialScale * ((.01f * percentageSizeIncrease) * Time.deltaTime / durationInSeconds);
                yield return null;
            }
            while (objectTransform.localScale.sqrMagnitude > initialScale.sqrMagnitude)
            {
                if (abortPulseTheObjectSize)
                {
                    localAbort = true;
                    break;
                }
                objectTransform.localScale -= initialScale * ((.01f * percentageSizeIncrease) * Time.deltaTime / durationInSeconds);
                yield return null;
            }
            counter++;
        }
        objectTransform.localScale = initialScale;
        abortPulseTheObjectSize = false;

        yield return null;
    }

    public static IEnumerator GrowAnObjectLinearly(GameObject objectToGrow, float scaleFactorToAchieve, float duration)
    {
        //
        //This function causes an object to grow linearly over a set interval to a set factor of its initial size.
        //It relies on the object being equal in scale in X and Y, since it just looks at the X scale to determine the delta between start and finish.
        //

        Vector3 initialScale = objectToGrow.transform.localScale;
        float timePassed = 0;

        float scaleFactorDelta = (initialScale.x * scaleFactorToAchieve) - initialScale.x;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float scaleFactorToApply = (timePassed / duration) * scaleFactorDelta;
            objectToGrow.transform.localScale = initialScale + new Vector3(scaleFactorToApply, scaleFactorToApply, scaleFactorToApply);
            yield return null;
        }

        objectToGrow.transform.localScale = initialScale * scaleFactorToAchieve;

        yield return null;
    }

    public static IEnumerator DarkenOrLightenASprite(GameObject objectToTreat, float targetR, float targetG, float targetB, float targetA, float timeToReachTarget)
    {
        var objectSprite = objectToTreat.GetComponent<SpriteRenderer>();
        var startColor = objectSprite.color;

        var targetColor = new Color(targetR, targetG, targetB, targetA);

        float timePassed = 0;

        while (timePassed < timeToReachTarget)
        {
            float interpParameter = timePassed / timeToReachTarget;
            timePassed += Time.deltaTime;
            var newColor = Color.Lerp(startColor, targetColor, interpParameter);
            objectSprite.color = newColor;
            yield return null;
        }

        objectSprite.color = targetColor;

        yield return null;
    }

    //Functions that control object position and other extensive parameters
    public static IEnumerator FloatUpAndDown(GameObject objectToFloat, float distance, float timeForCycleInSeconds)
    {
        //This function puts the object through a sinusoidal float up/down cycle with a period of timeForCycle.
        initialPositionForFloation = objectToFloat.transform.position;
        float frequency = Mathf.PI * 2 / timeForCycleInSeconds;
        float timePassed = 0;
        while (!abortFloatUpAndDown)
        {
            timePassed += Time.deltaTime;
            float yToAdd = distance * Mathf.Sin(frequency * timePassed);
            objectToFloat.transform.position = new Vector3(initialPositionForFloation.x, initialPositionForFloation.y + yToAdd, initialPositionForFloation.z);
            if (timePassed > timeForCycleInSeconds)
            {
                timePassed -= timeForCycleInSeconds;
            }
            yield return null;
        }
        objectToFloat.transform.position = initialPositionForFloation;
        yield return null;
    }

    public static IEnumerator TranslateLinearlyOverTime(GameObject objectToTranslate, Vector2 vectorToTranslateBy, float timeToTranslateInSeconds)
    {
        float timePassed = 0;
        Vector2 originalPosition = objectToTranslate.transform.position;
        while (timePassed < timeToTranslateInSeconds)
        {
            timePassed += Time.deltaTime;
            objectToTranslate.transform.Translate(vectorToTranslateBy * Time.deltaTime / timeToTranslateInSeconds);
            yield return null;
        }
        objectToTranslate.transform.position = originalPosition + vectorToTranslateBy;
        yield return null;
    }

    public static IEnumerator SmoothlyTranslateIn(GameObject objectToTranslate, Vector3 targetPosition, float totalTimeToTranslate, float accelerationIntervalPercentage, float decelerationIntervalPercentage)
    {
        //
        //This function takes the total time to translate and divides it into three parts.
        //Over the first interval it linearly accelerates, over the middle holds steady, and over the final linearly decelerates.
        //The total time of travel is passed as a parameter.
        //

        Vector3 startPos = objectToTranslate.transform.position;
        Vector3 vectorToTranslateAlong = targetPosition - objectToTranslate.transform.position;
        float distanceToTravel = Vector3.Magnitude(vectorToTranslateAlong);
        //Debug.Log("It seems we need to travel " + distanceToTravel);
        Vector3 unitVectorToTranslateAlong = Vector3.Normalize(vectorToTranslateAlong);
        //Debug.Log("We will travel along " + unitVectorToTranslateAlong);

        float timeSpentAccelerating = totalTimeToTranslate * accelerationIntervalPercentage;
        float timeSpentDecelerating = totalTimeToTranslate * decelerationIntervalPercentage;
        float timeSpentAtConstantSpeed = totalTimeToTranslate - (timeSpentAccelerating + timeSpentDecelerating);


        float averageVelocityOverall = distanceToTravel / totalTimeToTranslate;
        //Debug.Log("The average velocity overall would be " + averageVelocityOverall);
        float averageVelocityWhileConstant = distanceToTravel / (0.5f * timeSpentAccelerating + timeSpentAtConstantSpeed + 0.5f * timeSpentDecelerating);
        //Debug.Log("We will travel over the constant interval at " + averageVelocityWhileConstant);

        float momentaryVelocity = 0;
        float timePassed = 0;

        while (timePassed < timeSpentAccelerating)
        {
            timePassed += Time.deltaTime;
            momentaryVelocity = (timePassed / timeSpentAccelerating) * averageVelocityWhileConstant;
            objectToTranslate.transform.Translate(unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        timePassed = timeSpentAccelerating;
        momentaryVelocity = averageVelocityWhileConstant;

        while (timePassed < timeSpentAccelerating + timeSpentAtConstantSpeed)
        {
            timePassed += Time.deltaTime;
            objectToTranslate.transform.Translate(unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        timePassed = 0;
        momentaryVelocity = averageVelocityWhileConstant;

        while (timePassed < timeSpentDecelerating)
        {
            timePassed += Time.deltaTime;
            momentaryVelocity = averageVelocityWhileConstant - (averageVelocityWhileConstant * timePassed / timeSpentDecelerating);
            objectToTranslate.transform.Translate(unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        objectToTranslate.transform.position = targetPosition;



        Debug.Log("The smooth translate function is complete");
        yield return null;
    }

    public static IEnumerator SmoothlyTranslateInBackwards(GameObject objectToTranslate, Vector3 targetPosition, float totalTimeToTranslate, float accelerationIntervalPercentage, float decelerationIntervalPercentage)
    {
        //
        //This function takes the total time to translate and divides it into three parts.
        //Over the first interval it linearly accelerates, over the middle holds steady, and over the final linearly decelerates.
        //The total time of travel is passed as a parameter.
        //
        //This version of the function actually translates the object backwards, to be used in cases like the Combat Animation Overlay where there is a negative scale value.
        //

        Vector3 startPos = objectToTranslate.transform.position;
        Vector3 vectorToTranslateAlong = targetPosition - objectToTranslate.transform.position;
        float distanceToTravel = Vector3.Magnitude(vectorToTranslateAlong);
        //Debug.Log("It seems we need to travel " + distanceToTravel);
        Vector3 unitVectorToTranslateAlong = Vector3.Normalize(vectorToTranslateAlong);
        //Debug.Log("We will travel along " + unitVectorToTranslateAlong);

        float timeSpentAccelerating = totalTimeToTranslate * accelerationIntervalPercentage;
        float timeSpentDecelerating = totalTimeToTranslate * decelerationIntervalPercentage;
        float timeSpentAtConstantSpeed = totalTimeToTranslate - (timeSpentAccelerating + timeSpentDecelerating);


        float averageVelocityOverall = distanceToTravel / totalTimeToTranslate;
        //Debug.Log("The average velocity overall would be " + averageVelocityOverall);
        float averageVelocityWhileConstant = distanceToTravel / (0.5f * timeSpentAccelerating + timeSpentAtConstantSpeed + 0.5f * timeSpentDecelerating);
        //Debug.Log("We will travel over the constant interval at " + averageVelocityWhileConstant);

        float momentaryVelocity = 0;
        float timePassed = 0;

        while (timePassed < timeSpentAccelerating)
        {
            timePassed += Time.deltaTime;
            momentaryVelocity = (timePassed / timeSpentAccelerating) * averageVelocityWhileConstant;
            objectToTranslate.transform.Translate(-unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        timePassed = timeSpentAccelerating;
        momentaryVelocity = averageVelocityWhileConstant;

        while (timePassed < timeSpentAccelerating + timeSpentAtConstantSpeed)
        {
            timePassed += Time.deltaTime;
            objectToTranslate.transform.Translate(-unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        timePassed = 0;
        momentaryVelocity = averageVelocityWhileConstant;

        while (timePassed < timeSpentDecelerating)
        {
            timePassed += Time.deltaTime;
            momentaryVelocity = averageVelocityWhileConstant - (averageVelocityWhileConstant * timePassed / timeSpentDecelerating);
            objectToTranslate.transform.Translate(-unitVectorToTranslateAlong * momentaryVelocity * Time.deltaTime);
            yield return null;
        }

        objectToTranslate.transform.position = targetPosition;



        Debug.Log("The smoothly translate in backwards function is finished");
        yield return null;
    }

    public static IEnumerator ScrollLinearlyAndRepeat(GameObject objectToScroll, Vector2 directionOfScroll, float speed, float distanceBeforeRepeat)
    {
        Vector2 originalPosition = objectToScroll.transform.position;
        var positionToReach = originalPosition + (Vector2.ClampMagnitude(directionOfScroll, 1) * distanceBeforeRepeat);

        while (true)
        {
            if (Vector2.SqrMagnitude(new Vector2(objectToScroll.transform.position.x, objectToScroll.transform.position.y) - positionToReach) < 0.01f)
            {
                objectToScroll.transform.position = originalPosition;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                objectToScroll.transform.Translate(directionOfScroll * Time.deltaTime * speed);
                yield return null;
            }
        }
    }





    //Functions for dealing with Strings

    //Functions for controlling the main camera

    public static IEnumerator SwoopAndZoomTheCameraAtSomething(Camera cameraToSwoop, Vector3 targetLocation, float targetCameraSize, float secondsToSwoop)
    {
        //
        //This function has a zoom-in type effect for an orthographic camera.
        //It pans and zooms simultaneously, but most of the zooming is done up-front to give a swooping effect.
        //

        if (abortCameraSwoop)
        {
            abortCameraSwoop = false;
        }
        Transform cameraTransform = cameraToSwoop.transform;
        Vector3 initialPosition = cameraTransform.position;
        Vector3 vectorToTravel = targetLocation - cameraTransform.position;
        float distanceToTravel = Vector3.Magnitude(targetLocation - cameraTransform.position);
        float averageTransformVelocity = distanceToTravel / secondsToSwoop;

        float cameraSizeDelta = targetCameraSize - cameraToSwoop.orthographicSize;
        float averageSizeVelocity = cameraSizeDelta / secondsToSwoop;

        float timePassed = 0;
        float cameraSize = cameraToSwoop.orthographicSize;
        Vector3 velocity = Vector3.zero;

        while (timePassed < secondsToSwoop)
        {
            if (abortCameraSwoop)
            {
                break;
            }
            timePassed += Time.deltaTime;
            //cameraTransform.position = Vector3.SmoothDamp(initialPosition, targetLocation, ref velocity, secondsToSwoop);
            cameraTransform.Translate(Vector3.Normalize(vectorToTravel) * averageTransformVelocity * Time.deltaTime);

            //Debug.Log("This iteration moving the camera by " + Vector3.Normalize(vectorToTravel) * averageTransformVelocity * Time.deltaTime);

            float zoomVelocityScaleFactor = 2f - 2 * timePassed / secondsToSwoop;

            cameraSize += averageSizeVelocity * Time.deltaTime * zoomVelocityScaleFactor;
            cameraToSwoop.orthographicSize = cameraSize;
            //Debug.Log("This iteration growing the camera by " + averageSizeVelocity * Time.deltaTime);
            //Debug.Log("Time since last frame is " + Time.deltaTime);
            yield return null;
        }
        abortCameraSwoop = false;
        yield return null;
    }

    public static IEnumerator ZoomAndSwoopTheCameraAtSomething(Camera cameraToSwoop, Vector3 targetLocation, float targetCameraSize, float secondsToSwoop)
    {
        //
        //This function has a zoom-in type effect for an orthographic camera.
        //It pans and zooms simultaneously, but most of the panning is done up-front to give a swooping effect.
        //

        if (abortCameraSwoop)
        {
            abortCameraSwoop = false;
        }
        Transform cameraTransform = cameraToSwoop.transform;
        Vector3 initialPosition = cameraTransform.position;
        Vector3 vectorToTravel = targetLocation - cameraTransform.position;
        float distanceToTravel = Vector3.Magnitude(targetLocation - cameraTransform.position);
        float averageTransformVelocity = distanceToTravel / secondsToSwoop;

        float cameraSizeDelta = targetCameraSize - cameraToSwoop.orthographicSize;
        float averageSizeVelocity = cameraSizeDelta / secondsToSwoop;

        float timePassed = 0;
        float cameraSize = cameraToSwoop.orthographicSize;
        Vector3 velocity = Vector3.zero;

        while (timePassed < secondsToSwoop)
        {
            if (abortCameraSwoop)
            {
                break;
            }
            timePassed += Time.deltaTime;
            float panVelocityScaleFactor = 2f - 2 * timePassed / secondsToSwoop;
            //cameraTransform.position = Vector3.SmoothDamp(initialPosition, targetLocation, ref velocity, secondsToSwoop);
            cameraTransform.Translate(Vector3.Normalize(vectorToTravel) * averageTransformVelocity * panVelocityScaleFactor * Time.deltaTime);

            //Debug.Log("This iteration moving the camera by " + Vector3.Normalize(vectorToTravel) * averageTransformVelocity * Time.deltaTime);

            float zoomVelocityScaleFactor = 2f - 2 * timePassed / secondsToSwoop;

            cameraSize += averageSizeVelocity * Time.deltaTime;
            cameraToSwoop.orthographicSize = cameraSize;
            //Debug.Log("This iteration growing the camera by " + averageSizeVelocity * Time.deltaTime);
            //Debug.Log("Time since last frame is " + Time.deltaTime);
            yield return null;
        }
        abortCameraSwoop = false;
        yield return null;
    }



}
