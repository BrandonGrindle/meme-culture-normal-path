using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public int health = 3;
    public int Damage = 20;
    public int defense = 5;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask WhatIsGround, WhatIsPlayer;

    [SerializeField] private Vector3 walkPoint;
    bool walkpointSet;
    [SerializeField] private float walkrange;
    [SerializeField] private float speed;

    [SerializeField] private float SightRange, AttackRange;
    [SerializeField] private bool InAttackRange, InSightRange;
    bool alreadyAttacked;

    [SerializeField] private float WaitTimeMin, WaitTimeMax;

    [SerializeField] private Items ItemDrop;

    private Animator animator;
    private int _animIDAttack;
    private int _animIDDamaged;
    private int _animIDDeath;
    private int _animIDrun;

    public AudioSource source;
    public AudioClip Attacking;
    public AudioClip hurt;
    public AudioClip Death;
    IEnumerator DelayedDestruction(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }

    IEnumerator WaitAtPoint()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(WaitTimeMin,WaitTimeMax));
        walkpointSet = false;
        agent.isStopped = false;

    }
    private void Awake()
    {
        player = GameObject.Find("PlayerArmature").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        _animIDAttack = Animator.StringToHash("Attack");
        _animIDDamaged = Animator.StringToHash("Damaged");
        _animIDDeath = Animator.StringToHash("Dead");
        _animIDrun = Animator.StringToHash("run");
    }

    private void Update()
    {
        agent.isStopped = false;

        InSightRange = Physics.CheckSphere(transform.position, SightRange, WhatIsPlayer);
        InAttackRange = Physics.CheckSphere(transform.position, AttackRange, WhatIsPlayer);

        if (!InSightRange && !InAttackRange) { patrol(); }
        if (InSightRange && !InAttackRange) { chase(); }
        if (InSightRange && InAttackRange) { Attack(); }
    }

    public void DamageTaken(int damage)
    {
        health -= (damage / defense);
        animator.SetBool(_animIDDamaged, true);
        source.clip = hurt;
        source.volume = 1.0f;
        source.Play();
        if (health <= 0)
        {
            animator.SetBool(_animIDDeath, true);
            EventManager.Instance.cstmevents.SkeletonKilled();
            InventoryManager.Instance.AddItem(ItemDrop);
            source.clip = Death;
            source.volume = 1.0f;
            source.Play();
            StartCoroutine(DelayedDestruction(6));
        }
        
    }

    private void patrol()
    {
        if (!walkpointSet) { SearchWalkPoint(); }

        if (walkpointSet)
        {
            agent.speed = 2;
            agent.SetDestination(walkPoint);

            Vector3 disttoWP = transform.position - walkPoint;
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                walkpointSet = false;
            }
            else if (disttoWP.magnitude < 1f)
            {
                agent.isStopped = true;
                StartCoroutine(WaitAtPoint());
            }
            animator.SetFloat(_animIDrun, agent.velocity.magnitude);
        }
        else
        {
            animator.SetFloat(_animIDrun, 0f);
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = UnityEngine.Random.Range(-walkrange, walkrange);
        float randomX = UnityEngine.Random.Range(-walkrange, walkrange);
        NavMeshHit hit;

        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        int GraveyardMask = 1 << NavMesh.GetAreaFromName("GraveYard");
        if (NavMesh.SamplePosition(randomPoint, out hit, walkrange, GraveyardMask))
        {
            walkPoint = hit.position;
            walkpointSet = true;
            return;
        }
    }



    private void chase()
    {
        agent.speed = 4;
        agent.SetDestination(player.position);
        animator.SetFloat(_animIDrun, agent.velocity.magnitude);
        transform.LookAt(player);
    }

    private void Attack()
    {
        agent.SetDestination(transform.position);
        animator.SetFloat(_animIDrun, agent.velocity.magnitude);
        transform.LookAt(player);
        alreadyAttacked = animator.GetBool(_animIDAttack);
        if (!alreadyAttacked)
        {
            ThirdPersonController.instance.PlayerDamaged(Damage);
            animator.SetBool(_animIDAttack, true);
            source.clip = Attacking;
            source.volume = 1.0f;
            source.Play();
        }
    }
}
