using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Ghoul : MonoBehaviour
{
    //gemini stuff
    public event Action<Ghoul> OnEnemyDied;

    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [SerializeField] private Transform target;

        // How close the player can get before the enemy takes notice
    [SerializeField] float chaseRange = 5.0f;

    float distanceToTarget = Mathf.Infinity;

    [SerializeField] float turnSpeed = 5.0f;

    //UnityEngine.AI.
    NavMeshAgent nma;

    EnemyHealth health;

    private bool isProvoked = false;

    private void Awake()
        {
            currentHealth = maxHealth;
        }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Notify the platform (and anything else listening) that this enemy died
        OnEnemyDied?.Invoke(this);

        Destroy(gameObject);
    }
    

    // Start is called before the first frame update
    void Start()
    {

        nma = GetComponent<NavMeshAgent>();

        health = GetComponent<EnemyHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if(health.isDead)
        {
            enabled = false;

            nma.enabled = false;

            return;
        }

        // Measures the distance between the enemy and the player
        distanceToTarget = Vector3.Distance(target.position,transform.position);

        if(isProvoked)
        {
            EngageTarget(); // Not defined yet
        }
        
        else if(distanceToTarget <= chaseRange)
        {
            isProvoked = true;
            // nma.SetDestination(target.position);
        }


        

    }



    //void OnDrawGizmosSelected()
    //{
        // Display the chase radius when selected
       // Gizmos.color = Color.yellow; // Choose a color
        //Gizmos.DrawWireSphere(transform.position, chaseRange);
    //}


    private void EngageTarget()
    {
        FaceTarget();

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


    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    public void OnDamageTaken()
    {
        isProvoked = true;
    }

}
