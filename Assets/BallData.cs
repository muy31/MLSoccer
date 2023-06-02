using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallData : MonoBehaviour
{

    [SerializeField] Rigidbody me;
    [SerializeField] Text score;
    public VBMovement[] playerScripts;

    public Vector3[] startPositions;


    public string lastTeamTouched;
    [SerializeField] int teamTouches;

    public bool redPoint;
    public bool bluePoint;
    public bool ended;

    int rS;
    int bS;

    public void Start()
    {
        rS = 0;
        bS = 0;
        ended = false;
        teamTouches = 0;
        lastTeamTouched = "";
        redPoint = false;
        bluePoint = false;
        if(score != null)
        {
            score.text = "Red Score: " + rS + "\nBlue Score: " + bS;
        }
            
    }

    // Start is called before the first frame update
    public void Reset()
    {
        transform.localPosition = startPositions[Random.Range(0, startPositions.Length)];
        me.velocity = new Vector3();
        ended = false;

        teamTouches = 0;
        lastTeamTouched = "";
        redPoint = false;
        bluePoint = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (score != null)
        {
            score.text = "Red Score: " + rS + "\nBlue Score: " + bS;
        }

        //Out of field
        if (transform.position.y <= 0f)
        {
            if (lastTeamTouched == "RED")
            {
                bluePoint = true;
            }
            else if (lastTeamTouched == "BLUE")
            {
                redPoint = true;
            }
            else
            {
                Reset();
            }
        }

        //Too many touches
        if(teamTouches > 3)
        {
            if (lastTeamTouched == "RED")
            {
                bluePoint = true;
            }
            else if (lastTeamTouched == "BLUE")
            {
                redPoint = true;
            }
        }

        
        
        foreach(VBMovement player in playerScripts)
        {
            if (redPoint && player.gameObject.tag == "Blue" || bluePoint && player.gameObject.tag == "Red")
            {
                player.AddReward(increment: -1f);
                player.myReward--;
                player.EndEpisode();
            }
            else if (redPoint && player.gameObject.tag == "Red" || bluePoint && player.gameObject.tag == "Blue")
            {
                player.AddReward(1f);
                player.myReward++;
                player.EndEpisode();
            }
        }

        if (redPoint)
        {
            rS++;
            Reset();
        }
        else if (bluePoint)
        {
            bS++;
            Reset();
        }


        //Debug.Log(Time.time + " | " +lastTeamTouched + " " + teamTouches + " " + bluePoint + " " + redPoint);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //In-game point
        if(collision.gameObject.tag == "Court")
        {
            if(transform.position.z > 0f)
            {
                bluePoint = true;
            }
            else if (transform.position.z < 0f)
            {
                redPoint = true;
            }
        }

        //Out of bounds point
        if (collision.gameObject.tag == "Out")
        {
            if (lastTeamTouched == "RED")
            {
                bluePoint = true;
            }
            else if (lastTeamTouched == "BLUE")
            {
                redPoint = true;
            }
        }

        //keeps track of team touches
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            VBMovement vbPlayer = collision.gameObject.GetComponent<VBMovement>();

            if(me.velocity.y < 0)
            {
                me.velocity = new Vector3(me.velocity.x, 0f, me.velocity.z);
            }
            me.AddForce(vbPlayer.ballHitForce, ForceMode.VelocityChange);
            Debug.Log(vbPlayer.ballHitForce.y);

            if(collision.gameObject.tag == "Blue" && (lastTeamTouched == "RED" || lastTeamTouched == ""))
            {
                teamTouches = 0;
                lastTeamTouched = "BLUE";
            }
            else if (collision.gameObject.tag == "Red" && (lastTeamTouched == "BLUE" || lastTeamTouched == ""))
            {
                teamTouches = 0;
                lastTeamTouched = "RED";
            }
            else
            {
                teamTouches++;
            }
        }
    }
}
