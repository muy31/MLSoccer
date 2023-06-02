using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerPlayer : MonoBehaviour
{
    [SerializeField] Text scoreDisplay;
    [SerializeField] BallBehavior ballScript;
    [SerializeField] private int redScore;
    [SerializeField] private int blueScore;

    public float matchTime;
    [SerializeField] private float timeLeft;

    [SerializeField] int numPlayersTeam;
    int numRedPlayers;
    int numBluePlayers;

    [SerializeField] Transform[] redTeamPlayers;
    [SerializeField] Transform[] blueTeamPlayers;
    [SerializeField] Transform ball;
    [SerializeField] Transform redGoal;
    [SerializeField] Transform blueGoal;

    float normDeltaTime;

    public LearnerScript[] playerScripts;

    int playerLayer;
    public bool resetAllPosOutOfBounds;
    public bool teamToReset;

    // Start is called before the first frame update
    void Start()
    {

        redScore = 0;
        blueScore = 0;
        scoreDisplay.text = "Red Score: " + redScore + "\n" + "Blue Score: " + blueScore;
        timeLeft = matchTime;
        normDeltaTime = Time.fixedDeltaTime;
        playerLayer = LayerMask.NameToLayer("Player");

        /*
        redTeamPlayers = new Transform[numPlayersTeam];
        blueTeamPlayers = new Transform[numPlayersTeam];
        playerScripts = new SimplePlayerBehavior[transform.childCount];

        numRedPlayers = 0;
        numBluePlayers = 0;

        for(int i = 0; i < transform.childCount; i++)
        {
            Transform player = transform.GetChild(i);
            if (player.gameObject.layer == playerLayer)
            {
                playerScripts[i] = player.GetComponent<SimplePlayerBehavior>();
                if (playerScripts[i].red)
                {
                    redTeamPlayers[numRedPlayers] = player;
                    numRedPlayers++;
                }
                else
                {
                    blueTeamPlayers[numBluePlayers] = player;
                    numBluePlayers++;
                }
            }

        }
        */

        playerScripts = new LearnerScript[redTeamPlayers.GetLength(0) + blueTeamPlayers.GetLength(0)];

        int g = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform player = transform.GetChild(i);
            if (player.gameObject.layer == playerLayer)
            {
                playerScripts[g] = player.GetComponent<LearnerScript>();
                g++;
            }
        }

    }

    void Update()
    {
        Slow();

        
    }

    void Slow()
    {
        if (Input.GetButton("Jump"))
        {
            Time.timeScale = 0.2f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeLeft -= Time.fixedDeltaTime;
        scoreDisplay.text = "Red Score: " + redScore + "\n" + "Blue Score: " + blueScore;

        float averageRedRewards = 0f;
        float averageBlueRewards = 0f;

        int nRed = 0;
        int nBlue = 0;

        foreach (LearnerScript pS in playerScripts)
        {
            if (pS.red)
            {
                averageRedRewards += pS.GetCumulativeReward();
                nRed++;
            }
            else
            {
                averageBlueRewards += pS.GetCumulativeReward();
                nBlue++;
            }

        }

        averageRedRewards /= nRed;
        averageBlueRewards /= nBlue;

        Debug.Log("Red average rewards: " + averageRedRewards + " " + "Blue Average Rewards: " + averageBlueRewards);


        if (timeLeft <= 0)
        {
        
        }


        if (ballScript.outOfBounds)
        {
            if (resetAllPosOutOfBounds)
            {
                ResetTeamBack(teamToReset);
            }

            Transform playerToMove = null;

            //Brings nearest player in for throw-in/kick-in
            if (ballScript.lastPlayerCollided == "RED")
            {
                //Rewards
                foreach (LearnerScript pS in playerScripts)
                {
                    if (pS.red)
                    {
                        pS.AddReward(increment: -1f);
                        Debug.Log(message: pS.GetCumulativeReward());
                    }
                }

                playerToMove = ClosestPlayer(blueTeamPlayers, ballScript.lastValidPoint);
            }
            else if (ballScript.lastPlayerCollided == "BLUE")
            {
                //Rewards
                foreach (LearnerScript pS in playerScripts)
                {
                    if (!pS.red)
                    {
                        pS.AddReward(increment: -1f);
                        Debug.Log(message: pS.GetCumulativeReward());
                    }
                }

                playerToMove = ClosestPlayer(redTeamPlayers, ballScript.lastValidPoint);
            }

            if(playerToMove != null)
            {
                Debug.Log("LAstHIT " + ballScript.lastPlayerCollided + " " + Time.time + " " + playerToMove);
                LearnerScript bh = playerToMove.gameObject.GetComponent<LearnerScript>();

                //Puts player behind ball towards center of field
                Vector3 moveLocation = new Vector3(ballScript.lastValidPoint.x, 1f, ballScript.lastValidPoint.z) + new Vector3(ballScript.lastValidPoint.x, 0f, ballScript.lastValidPoint.z).normalized;
                moveLocation = new Vector3(moveLocation.x, 1f, moveLocation.z);

                //Moves Player
                bh.ForceMoveTo(2f, moveLocation);
            }

            ballScript.ResetToBounds(); //Must come after forced player movement
        }


    }

    public Transform ClosestPlayer(Transform[] players, Vector3 point)
    {
        float leastDistance = Mathf.Infinity;
        Transform best = null;

        foreach(Transform tr in players)
        {
            if(tr != null)
            {
                float dist = (tr.position - point).magnitude;
                if (dist < leastDistance)
                {
                    leastDistance = dist;
                    best = tr;
                }
            } 
        }

        return best;
    }

    public int getScore(bool red)
    {
        if (red)
        {
            return redScore;
        }
        return blueScore;
    }

    public void AddGoal(bool red)
    {
        if (red)
        {
            foreach(LearnerScript pS in playerScripts)
            {
                if (pS.red)
                {
                    pS.AddReward(increment: 100f);
                    Debug.Log(message: pS.GetCumulativeReward());
                }
                else
                {
                    pS.AddReward(increment: -100f);
                    Debug.Log(message: pS.GetCumulativeReward());
                }
                
            }
            redScore++;
            Debug.Log("Red Goal!");
        }
        else
        {
            foreach (LearnerScript pS in playerScripts)
            {
                if (!pS.red)
                {
                    pS.AddReward(increment: 100f);
                    Debug.Log(message: pS.GetCumulativeReward());
                }
                else
                {
                    pS.AddReward(increment: -100f);
                    Debug.Log(pS.GetCumulativeReward());
                }
            }
            blueScore++;
            Debug.Log("Blue Goal!");
        }
        SmallReset(red);
    }

    public Transform[] getMyTeam(bool red)
    {
        if (red)
        {
            return redTeamPlayers;
        }
        else
        {
            return blueTeamPlayers;
        }
    }

    public Transform getMyGoal(bool red)
    {
        if (red)
        {
            return redGoal;
        }
        else
        {
            return blueGoal;
        }
    }

    public Transform getEnemyGoal(bool red)
    {
        if (!red)
        {
            return redGoal;
        }
        else
        {
            return blueGoal;
        }
    }

    public Transform[] getEnemyTeam(bool red)
    {
        if (!red)
        {
            return redTeamPlayers;
        }
        else
        {
            return blueTeamPlayers;
        }
    }

    public void Reset()
    {

    }

    public void SmallReset(bool scorer)
    {
        ballScript.ResetToCenter();
        foreach(LearnerScript bS in playerScripts)
        {
            bS.ResetAfterGoal(scorer);
        }
    }

    public void ResetTeamBack(bool red)
    {
        foreach (LearnerScript bS in playerScripts)
        {
            if(bS.red == red)
            {
                bS.ResetAfterGoal(red);
            }
        }
    }

    public Transform getBall()
    {
        return ball;
    }

}
