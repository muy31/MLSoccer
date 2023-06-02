using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopBall : MonoBehaviour
{
    private Collider me;
    private BallBehavior ballScript;
    public float throwInHeight;

    public bool cornerChecking;
    public bool cornersOnRedOut;

    public Vector3[] corners;
    public Vector3 centerKickIn;

    [SerializeField] ManagerPlayer manager;

    private void Start()
    {
        me = gameObject.GetComponent<Collider>();
        manager.resetAllPosOutOfBounds = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.tag == "Ball")
        {
            ballScript = other.gameObject.GetComponent<BallBehavior>();
            ballScript.lastValidPoint = other.transform.position - other.attachedRigidbody.velocity * Time.fixedDeltaTime;
            ballScript.lastValidPoint = new Vector3(ballScript.lastValidPoint.x, throwInHeight, ballScript.lastValidPoint.z);
            manager.resetAllPosOutOfBounds = false;

            if (cornerChecking)
            {
                if ((ballScript.lastPlayerCollided == "RED" && cornersOnRedOut) || (ballScript.lastPlayerCollided == "BLUE" && !cornersOnRedOut))
                {
                    ballScript.lastValidPoint = closestCorner(ballScript.lastValidPoint);
                }
                else
                {
                    ballScript.lastValidPoint = centerKickIn;
                    manager.resetAllPosOutOfBounds = true;
                    manager.teamToReset = ballScript.lastPlayerCollided == "RED";
                }
            }
        }
    }

    private Vector3 closestCorner(Vector3 location)
    {
        float bestDistance = Mathf.Infinity;
        Vector3 closest = corners[0];

        foreach (Vector3 corner in corners)
        {
            float distance = (corner - location).magnitude;
            if(distance < bestDistance)
            {
                bestDistance = distance;
                closest = corner;
            }
        }

        return closest;
    }
}
