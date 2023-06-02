using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    public BallData vb;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == vb.gameObject)
        {
            if (vb.lastTeamTouched == "RED")
            {
                foreach(VBMovement player in vb.playerScripts)
                {
                    if(player.gameObject.tag == "Red")
                    {
                        player.AddReward(0.5f);
                    }
                }
            }

            if(vb.lastTeamTouched == "BLUE")
            {
                foreach (VBMovement player in vb.playerScripts)
                {
                    if (player.gameObject.tag == "Blue")
                    {
                        player.AddReward(0.5f);
                    }
                }
            }
        }
    }
}
