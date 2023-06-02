using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class VBMovement : Agent
{
    //References
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform groundCheck;
    [SerializeField] Rigidbody ballRB;

    public TeamHolder team;
    public TeamHolder enemyTeam;
    Vector3 startPostion;

    //Restrictions
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float jumpForceMult;
    [SerializeField] bool isOnground;
    [SerializeField] float groundRadiusCheck;
    public float maxHitStrength;
    public float maxVertStrength;

    //Actions
    [SerializeField] float zMovement;
    [SerializeField] float xMovement;
    [SerializeField] float jumpForce;
    public Vector3 ballHitForce;

    //non-edits
    private Vector3 previousValidPosition;
    private Vector3 previousValidVelocity;
    public float myReward;

    //Init
    public override void Initialize()
    { 
        previousValidVelocity = new Vector3();
        startPostion = transform.position;
        ballHitForce = new Vector3();
        previousValidPosition = startPostion;
        myReward = 0;
    }

    //Init
    public override void OnEpisodeBegin()
    {
        transform.position = startPostion;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Relative Ball
        sensor.AddObservation(transform.position - ballRB.transform.position);

        //Relative Teammates
        foreach(Transform player in team.team)
        {
            if(player != transform)
            {
                sensor.AddObservation(transform.position - player.position);
            }
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        DecodeActionVector(vectorAction);
    }

    void DecodeActionVector(float[] action)
    {
        if (action[0] <= 0)
        {
            jumpForce = 0f;
        }
        else
        {
            jumpForce = action[0];
        }

        xMovement = acceleration * action[1];
        zMovement = acceleration * action[2];

        if(action[4] < 0)
        {
            action[4] *= 4;
        }
        else
        {
            action[4] *= maxVertStrength;
        }

        ballHitForce = new Vector3(action[3] * maxHitStrength, action[4], action[5] * maxHitStrength);
    }

    private void MoveAccordance()
    {
        Vector3 currentMotion = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 force = new Vector3(xMovement, 0f, zMovement);

        if((currentMotion + force).magnitude <= maxSpeed)
        {
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        jumpForce = Mathf.Clamp(jumpForce, 0f, 1f);

        if (isOnground)
        {
            rb.AddForce(Vector3.up * jumpForce * jumpForceMult, ForceMode.VelocityChange);
        }
    }

    private void JumpCheck()
    {
        //Check if can jump
        if (Physics.CheckSphere(groundCheck.position, groundRadiusCheck, ~(1 << 8)))
        {
            isOnground = true;
        }
        else
        {
            isOnground = false;
        }
    }

    void FixedUpdate()
    {
        JumpCheck();
        MoveAccordance();
        StayInBoundsRaycast();
        PositionUpdate();

        //Debug.Log(Time.time + " " +GetCumulativeReward());

        //Time-based reward
        AddReward(0.05f * Time.fixedDeltaTime);

        //Ball-height-based reward
        AddReward(0.01f * ballRB.position.y);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ball")
        {
            //ballRB.AddForce(ballHitForce * maxHitStrength, ForceMode.VelocityChange);
            AddReward(increment: 0.5f);
            Debug.Log("Reward Added " + GetCumulativeReward() +" " + Time.time);
        }
    }

    RaycastHit towardsGround;

    private void PositionUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out towardsGround, Mathf.Infinity, 1 << 9))
        {
            previousValidPosition = transform.position;
            previousValidVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private void StayInBoundsRaycast()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out towardsGround, Mathf.Infinity, 1 << 9))
        {
            transform.position = previousValidPosition - previousValidVelocity * Time.fixedDeltaTime;
            AddReward(increment: -0.5f);
            Debug.Log("Reward Taken " + GetCumulativeReward() + " " + Time.time);
        }
    }

    void RandomAction()
    {
        float[] actions = new float[6];
        for(int i = 0; i < 6; i++)
        {
            actions[i] = Random.Range(-1f, 1f);
        }

        DecodeActionVector(actions);
    }
}
