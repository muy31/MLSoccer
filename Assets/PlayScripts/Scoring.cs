using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    [SerializeField] bool redGoal;
    [SerializeField] ManagerPlayer manager;
    [SerializeField] GameObject marker;
    private float goalTimer;
    public bool debug;

    // Start is called before the first frame update
    void Start()
    {
        goalTimer = 1f;
    }

    void Update()
    {
        goalTimer -= Time.deltaTime;
        goalTimer = Mathf.Clamp(goalTimer, -1f, 4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball" && goalTimer <= 0)
        {
            manager.AddGoal(redGoal);
            if (debug)
            {
                Instantiate(marker, other.transform.position, other.transform.rotation);
            }
            
            goalTimer = 2f;
        }
    }
}
