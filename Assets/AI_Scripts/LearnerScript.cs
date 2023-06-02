using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class LearnerScript : Agent
{
    [SerializeField] Rigidbody rb;

    //Inputs to AI
    Transform[] playersOnMyTeam;
    Transform[] playersOnEnemyTeam;
    [SerializeField] Transform ballPos;

    Vector3 goalPos;
    Vector3 enemyGoalPos;

    //Start Positions
    [SerializeField] Vector3 startPosNeutral;
    [SerializeField] Vector3 startPosAttacking;
    [SerializeField] Vector3 startPosDefending;

    [SerializeField] bool humanInput;

    //
    public ManagerPlayer manager;

    //Constants
    public bool red;
    public int playerType; //Simple, Attacker, mid, defender, goalkeeper  (0 - 4)
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

    public override void OnEpisodeBegin()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == gameObject.layer)
        {
            if(collision.gameObject.tag == gameObject.tag)
            {
                AddReward(increment: -0.3f);
            }
            else
            {
                AddReward(increment: -0.1f);
            }
            
        }else if(collision.gameObject.tag == "Ball")
        {
            AddReward(increment: 5f);
        }
    }

    public override void Initialize()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        previousValidPosition = startPosNeutral;
        previousValidVelocity = new Vector3();
        startPosNeutral = transform.position;
        if (playerType == 0 || playerType == 2 || playerType == 3 || playerType == 4)
        {
            startPosAttacking = transform.position;
            startPosDefending = transform.position;
        }
        else if (playerType == 1)
        {
            if (red)
            {
                startPosAttacking = transform.position - new Vector3(0.5f, 0f, 0f);
                startPosDefending = transform.position + new Vector3(0.5f, 0f, 0f);
            }
            else
            {
                startPosAttacking = transform.position + new Vector3(0.5f, 0f, 0f);
                startPosDefending = transform.position - new Vector3(0.5f, 0f, 0f);
            }

        }

        playersOnMyTeam = manager.getMyTeam(red);
        playersOnEnemyTeam = manager.getEnemyTeam(red);
        ballPos = manager.getBall();
        goalPos = manager.getMyGoal(red).position;
        enemyGoalPos = manager.getEnemyGoal(red).position;

        base.Initialize();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!humanInput)
        {
            //me
            sensor.AddObservation(transform.position.x);
            sensor.AddObservation(transform.position.z);
            sensor.AddObservation(transform.rotation.y);

            //ball
            sensor.AddObservation(ballPos.position.x);
            sensor.AddObservation(ballPos.position.z);
            sensor.AddObservation(ballPos.position.y);

            //goal
            sensor.AddObservation(goalPos.x);
            sensor.AddObservation(goalPos.z);
            sensor.AddObservation(goalPos.y);

            sensor.AddObservation(enemyGoalPos.x);
            sensor.AddObservation(enemyGoalPos.z);
            sensor.AddObservation(enemyGoalPos.y);

            sensor.AddObservation(rb.isKinematic);

            //team
            foreach (Transform vec in playersOnMyTeam)
            {
                if (vec != transform)
                {
                    sensor.AddObservation(vec.position.x);
                    sensor.AddObservation(vec.position.z);
                }
            }

            foreach (Transform vec in playersOnEnemyTeam)
            {
                sensor.AddObservation(vec.position.x);
                sensor.AddObservation(vec.position.z);
            }

            

        }
        else
        {
            //me
            sensor.AddObservation(transform.position.x);
            sensor.AddObservation(transform.position.z);
            sensor.AddObservation(transform.rotation.y);

            //ball
            sensor.AddObservation(ballPos.position.x);
            sensor.AddObservation(ballPos.position.z);
            sensor.AddObservation(ballPos.position.y);


        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        string guys = "Actions";

        //Making normalized actions between 0 and 1 instead of -1 and 1
        for(int i = 0; i < vectorAction.GetLength(0); i++)
        {
            vectorAction[i] += 1;
            vectorAction[i] /= 2;
            guys += " " + vectorAction[i];
        }

        //Debug.Log(guys);

        kickOption =  (int) Mathf.Floor(Mathf.Abs(vectorAction[0] * 5));
        kickForce = vectorAction[1] * (maxKickForce - minKickForce) + minKickForce;
        upKick = vectorAction[2] * (2 * maxUpForce) - maxUpForce;
        forwardMoveForce = vectorAction[3] * (2 * maxAcceleration) - maxAcceleration;
        degRotation = vectorAction[4] * (2 * maxRotation) - maxRotation;
    }

    public void RandomOperation()
    {
        kickOption = Random.Range(0, 4);
        upKick = Random.Range(-maxUpForce, maxUpForce);
        kickForce = Random.Range(minKickForce, maxKickForce);
        forwardMoveForce = Random.Range(-maxAcceleration, maxAcceleration);
        degRotation = Random.Range(-maxRotation, maxRotation);
    }

    float distanceToMyGoal;
    float distanceToEnemyGoal;

    int scoreUpdateFrequency = 50;
    int flag = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        KickingConstrain();
        Movement();
        StayInBoundsRaycast();
        PositionUpdate();

        distanceToMyGoal = Mathf.Abs((ballPos.position - goalPos).magnitude);
        distanceToEnemyGoal = Mathf.Abs((ballPos.position - enemyGoalPos).magnitude);

        /*
        if(flag <= 0)
        {
            AddReward(increment: -distanceToMyGoal * 0.01f * Time.fixedDeltaTime);
            flag = scoreUpdateFrequency;
        }
        flag--;
        */
        

    }

    private void Movement()
    {
        //Rotation
        degRotation = Mathf.Clamp(degRotation, -maxRotation, maxRotation);
        rb.transform.Rotate(new Vector3(0f, degRotation, 0f));

        //Forward movement
        forwardMoveForce = Mathf.Clamp(forwardMoveForce, -maxAcceleration, maxAcceleration);

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
        if (Physics.Raycast(transform.position, Vector3.down, out towardsGround, Mathf.Infinity, 1 << groundLayer))
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
            AddReward(increment: -0.5f);
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

        Debug.Log(gameObject + " Being moved!");

        yield return new WaitForSeconds(seconds);

        rb.isKinematic = false;
        rb.velocity = new Vector3();
    }

    
}
