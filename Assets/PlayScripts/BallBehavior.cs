using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    [SerializeField] int playerLayer; 
    [SerializeField] Rigidbody ballBody;
    [SerializeField] ManagerPlayer manager;

    //Holding
    public GameObject heldBy = null;
    public bool isHeld = false;
    public float holdDistance;
    public float holdHeight;
    public float maxHoldAngle;
    public float maxHoldTime;

    //Bounding and Resetting
    public bool outOfBounds;
    public float startHeight;
    public Vector3 lastValidPoint;

    //Current Holding Coroutine
    private IEnumerator coroutine = null;

    //
    public string lastPlayerCollided;

    // Start is called before the first frame update
    void Start()
    {
        lastPlayerCollided = null;
        playerLayer = LayerMask.NameToLayer("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(transform.position.y <= -1)
        {
            outOfBounds = true;
        }
        else
        {
            outOfBounds = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        DeHold();

        if (collision.gameObject.layer == playerLayer)
        {
            LearnerScript playerScript = collision.gameObject.GetComponent<LearnerScript>();

            if (playerScript.red)
            {
                lastPlayerCollided = "RED";
                //Reward for hitting ball
                foreach(LearnerScript players in manager.playerScripts)
                {
                    if (players.red)
                    {
                        players.AddReward(increment: 0.5f);
                    }
                }
            }
            else
            {
                lastPlayerCollided = "BLUE";
                //Reward for hitting ball
                foreach (LearnerScript players in manager.playerScripts)
                {
                    if (!players.red)
                    {
                        players.AddReward(increment: 0.5f);
                    }
                }
            }

            Vector3 dir = (transform.position - collision.transform.position).normalized;
            //Debug.Log(transform.forward);
            dir = new Vector3(dir.x, 0, dir.z);

            //Debug.Log("Collision!" + Time.time + " " + Vector3.Angle(dir, collision.transform.forward));

            if (playerScript.kickOption == 2)
            {
                KickAtPoint(collision.GetContact(0).point, playerScript.kickForce, playerScript.upKick);
            }
            else if(playerScript.kickOption == 3)
            {
                KickDirection(collision.transform.forward, playerScript.kickForce, playerScript.upKick);
            }
            else if(playerScript.kickOption == 4)
            {
                KickTorque(collision.GetContact(0).point, collision.transform.forward, playerScript.kickOption, playerScript.upKick);
            }
            else if(playerScript.kickOption == 1 && !isHeld && (Vector3.Angle(dir, collision.transform.forward) < maxHoldAngle))
            {
                coroutine = BeingHeld(collision.gameObject, maxHoldTime);
                StartCoroutine(coroutine);
            }  
        }
    }

    public void ResetToCenter()
    {
        StartCoroutine(MoveBallToCenter(2f));
        lastPlayerCollided = null;
        ballBody.velocity = new Vector3();
    }

    IEnumerator MoveBallToCenter(float seconds)
    {
        yield return new WaitForSeconds(seconds/2);
        ballBody.detectCollisions = false;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        DeHold();

        ballBody.isKinematic = true;
        float timeSpent = 0;
        Vector3 initPos = transform.position;

        while (timeSpent < seconds)
        {

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            timeSpent += Time.deltaTime;
            transform.position = (1 - timeSpent/seconds)*transform.position + new Vector3(0f, startHeight, 0f)*(timeSpent/seconds);
            yield return null;
        }
        transform.position = new Vector3(0f, startHeight, 0f);
        ballBody.isKinematic = false;
        ballBody.velocity = new Vector3();
        ballBody.detectCollisions = true;
    }

    IEnumerator MoveBallToLocation(float seconds, Vector3 location)
    {
        ballBody.detectCollisions = false;
        yield return new WaitForSeconds(seconds / 2);

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        DeHold();

        ballBody.isKinematic = true;
        float timeSpent = 0;
        Vector3 initPos = transform.position;

        while (timeSpent < seconds)
        {

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            timeSpent += Time.deltaTime;
            transform.position = (1 - timeSpent / seconds) * transform.position + location * (timeSpent / seconds);
            yield return null;
        }
        transform.position = location;
        ballBody.isKinematic = false;
        ballBody.velocity = new Vector3();
        ballBody.detectCollisions = true;
    }

    public void KickAtPoint(Vector3 point, float force, float up)
    {
        ballBody.AddExplosionForce(force, point, 1f, up, ForceMode.Impulse);
    }

    public void KickDirection(Vector3 ford, float force, float up)
    {
        ballBody.AddForce(ford.normalized * force , ForceMode.Impulse);
        ballBody.AddForce(Vector3.up * up , ForceMode.Impulse);
    }

    public void KickTorque(Vector3 point, Vector3 direction, float force, float up)
    {
        ballBody.AddForceAtPosition(direction.normalized * force , point, ForceMode.Impulse);
        ballBody.AddForce(Vector3.up * up, ForceMode.Impulse);
    }

    void GetHeld(GameObject holder)
    {
        isHeld = true;
        heldBy = holder;
        ballBody.isKinematic = true;

        Vector3 dir = (transform.position - holder.transform.position).normalized;
        Vector3 holdPos = dir * 0.5f + holder.transform.forward * 0.5f;

        transform.position = holder.transform.position + holdPos * holdDistance;
        transform.position = new Vector3(transform.position.x, holdHeight, transform.position.z);
    }

    void DeHold()
    {
        heldBy = null;
        isHeld = false;
        ballBody.isKinematic = false;
    }

    IEnumerator BeingHeld(GameObject holder, float seconds)
    {
        float timeSpent = 0;
        while(timeSpent < seconds)
        {
            timeSpent += Time.deltaTime;
            GetHeld(holder);
            yield return null;
        }
        DeHold();
    }

    public void ResetToBounds()
    {
        ballBody.velocity = new Vector3();
        StartCoroutine(MoveBallToLocation(2f, lastValidPoint));
        lastPlayerCollided = null;
        ballBody.velocity = new Vector3();
    }
}
