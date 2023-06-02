using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerBehavior : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    //Inputs to AI
    Vector3[] playersOnMyTeam;
    Vector3[] playersOnEnemyTeam;
    Vector3 ballPos;
    Vector3 goalPos;
    Vector3 enemyGoalPos;

    //Start Positions
    [SerializeField] Vector3 startPosNeutral;
    [SerializeField] Vector3 startPosAttacking;
    [SerializeField] Vector3 startPosDefending;

    //Constants
    public bool red;
    int playerType; //Simple, Attacker, mid, defender, goalkeeper  (0 - 5)


    //OUTPUT TO GAME |
    //               V

    //Kick parameters
    public int kickOption;
    //an integer from 0 to 4 (if 0, no kick just simple collision)
    //if 1 = hold
    //if 2 = A collision point boost
    //if 3 = kickforward
    //if 4 = kickforward addtorque
    public float upKick; //upwards modifier of kick
    public float kickForce; //strength of kick

    //Movement parameters
    public float forwardMoveForce; //positive or negative
    public float degRotation; //degrees turned per frame




    //Movement Constraints
    public float maxVelocity; //maximum speed attainable
    public float maxAcceleration; //max speed differential attainable in one frame
    public float maxRotation; //max degree rotation in one frame
    public float maxKickForce; //max forward kick force
    public float minKickForce; //min forward kick force (negative means backwards)
    public float maxUpForce; //max upwardsModifier/up kick force

    //Non-edits
    private Vector3 previousValidPosition;
    private Vector3 previousValidVelocity;
    private RaycastHit towardsGround;
    private int groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        previousValidPosition = startPosNeutral;
        previousValidVelocity = new Vector3();

        startPosNeutral = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        KickingConstrain();
        Movement();
        StayInBoundsRaycast();
        PositionUpdate();
    }

    private void Movement()
    {
        RandomOperation();

        //Rotation
        degRotation = Mathf.Clamp(degRotation, -maxRotation, maxRotation);
        rb.transform.Rotate(new Vector3(0f, degRotation, 0f));

        //Forward movement
        forwardMoveForce = Mathf.Clamp(forwardMoveForce, - maxAcceleration, maxAcceleration);
  
        if ((rb.velocity - transform.forward * forwardMoveForce).magnitude <= maxVelocity)
        {
            rb.AddForce(transform.forward * forwardMoveForce, ForceMode.VelocityChange);
        }
    }

    private void KickingConstrain()
    {
        kickForce = Mathf.Clamp(kickForce, minKickForce, maxKickForce);
        upKick = Mathf.Clamp(upKick, -maxUpForce, maxUpForce);
    }

    private void PositionUpdate()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out towardsGround, Mathf.Infinity, 1 << groundLayer))
        {
            previousValidPosition = transform.position;
            previousValidVelocity = rb.velocity;
        }
    }

    private void StayInBoundsRaycast()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out towardsGround, Mathf.Infinity, 1 << groundLayer))
        {
            transform.position = previousValidPosition - previousValidVelocity * Time.fixedDeltaTime;
        }
    }

    public void ResetToStart()
    {
        StartCoroutine(MoveToLocation(2f, startPosNeutral));
    }

    public void ResetAfterGoal(bool redScored)
    {

        if (redScored == red)
        {
            StartCoroutine(MoveToLocation(2f, startPosDefending));
        }
        else
        {
            StartCoroutine(MoveToLocation(2f, startPosAttacking));
        }   
    }

    public void ForceMoveTo(float seconds, Vector3 location)
    {
        StartCoroutine(MoveToLocation(seconds, location));
    }

    IEnumerator MoveToLocation(float seconds, Vector3 location)
    {
        rb.isKinematic = true;
        float timeSpent = 0;
        Vector3 initPos = transform.position;

        while (timeSpent < seconds)
        {
            timeSpent += Time.deltaTime;
            transform.position = (1 - timeSpent / seconds) * initPos + location * (timeSpent / seconds);
            yield return null;
        }
        transform.position = location;

        yield return new WaitForSeconds(seconds);

        rb.isKinematic = false;
        rb.velocity = new Vector3();
    }

    public void RandomOperation()
    {
        kickOption = Random.Range(0, 4);
        upKick = Random.Range(-maxUpForce, maxUpForce);
        kickForce = Random.Range(minKickForce, maxKickForce);
        forwardMoveForce = Random.Range(-maxAcceleration, maxAcceleration);
        degRotation = Random.Range(-maxRotation, maxRotation);
    }
}
