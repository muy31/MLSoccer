using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodedVBPlayer : MonoBehaviour
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
    [SerializeField] bool isOnGround;
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

    // Start is called before the first frame update
    void Start()
    {
        jumpForce = 0f;
        zMovement = 0f;
        xMovement = 0f;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        JumpCheck();
        //Decide where to move
        MoveTowardsBall();

        //Move
        MoveAccordance();

        StayInBoundsRaycast();
        PositionUpdate();
    }


    private void JumpCheck()
    {
        //Check if can jump
        if (Physics.CheckSphere(groundCheck.position, groundRadiusCheck, ~(1 << 8)))
        {
            isOnGround = true;
        }
        else
        {
            isOnGround = false;
        }
    }

    private void MoveAccordance()
    {
        Vector3 currentMotion = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 force = new Vector3(xMovement, 0f, zMovement);

        if ((currentMotion + force).magnitude <= maxSpeed)
        {
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        jumpForce = Mathf.Clamp(jumpForce, 0f, 1f);

        if (isOnGround)
        {
            rb.AddForce(Vector3.up * jumpForce * jumpForceMult, ForceMode.VelocityChange);
        }
    }

    void MoveTowardsBall()
    {
        Vector3 dir =  ballRB.position - transform.position;
        dir = new Vector3(dir.x, 0f, dir.z).normalized * acceleration;

        xMovement = dir.x;
        zMovement = dir.z;
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
        }
    }
}
