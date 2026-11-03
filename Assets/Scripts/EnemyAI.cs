using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // How close the player can get before the enemy takes notice
    [SerializeField] float chaseRange = 5.0f;

    float distanceToTarget = Mathf.Infinity;

    [SerializeField] Transform target;

    UnityEngine.AI.NavMeshAgent nma;

    private bool isProvoked = false;
    

    // Start is called before the first frame update
    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Measures the distance between the enemy and the player
        distanceToTarget = Vector3.Distance(target.position,transform.position);

        if(isProvoked)
        {
            EngageTarget(); // Not defined yet
        }
        
        else if(distanceToTarget <= chaseRange)
        {
            isProvoked = true;
            // Comment out, but keep to use later
            // nma.SetDestination(target.position);
        }


        

    }



    void OnDrawGizmosSelected()
    {
        // Display the chase radius when selected
        Gizmos.color = Color.yellow; // Choose a color
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }


    private void EngageTarget()
    {
        if(distanceToTarget >= nma.stoppingDistance)
        {
            ChaseTarget();
        }

        if(distanceToTarget <= nma.stoppingDistance)
        {
            AttackTarget();
        }
    }


    private void ChaseTarget()
    {
        GetComponent<Animator>().SetBool("attack", false);
        GetComponent<Animator>().SetTrigger("run");
        nma.SetDestination(target.position);
    }

    private void AttackTarget()
    {
        GetComponent<Animator>().SetBool("attack", true);
        // Temp for now, to show that the enemy is attacking
        //print(name + " is attacking " + target.name);
    }


}
