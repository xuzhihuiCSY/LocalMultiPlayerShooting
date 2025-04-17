using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkBehaviour
{   
    private NetworkPlayerController playerCtrl;
    public int maxHealth = 5;
    private int currentHealth;
    public float chaseRange = 25f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float repathInterval = 0.5f;
    private bool remoteDiePlayed = false;

    private NavMeshAgent agent;
    private Transform target;
    private Animation anim;
    private float lastAttackTime;
    private float lastRepathTime;
    private bool isDead = false;

    // Use enum instead of string for syncing animation state
    private NetworkVariable<EnemyAnimState> netAnimState = new NetworkVariable<EnemyAnimState>(
        EnemyAnimState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Start()
    {   
        if (IsServer)
        {
            currentHealth = maxHealth;
        }
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animation>();

        agent.autoBraking = false;
        agent.stoppingDistance = attackRange - 0.3f;
    }

    void Update()
    {
        if (!IsServer)
        {
            HandleRemoteAnimation();
            return;
        }

        if (isDead) return;

        ServerLogic();
    }
    public void TakeDamage(int damage)
    {
        if (!IsServer || isDead) return;

        currentHealth -= damage;
        Debug.Log($"{name} took {damage} dmg. HP now {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        agent.ResetPath();
        PlayDieAnimation();
        // Despawn after 3 seconds
        Invoke(nameof(Despawn), 3f);
    }

    private void Despawn()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
    private void ServerLogic()
    {
        FindClosestPlayer();
        if (target == null) 
        {
            PlayIdleAnimation();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (Time.time - lastRepathTime > repathInterval)
        {
            if (distance > attackRange && agent.destination != target.position)
            {
                agent.SetDestination(target.position);
            }
            lastRepathTime = Time.time;
        }

        if (distance <= attackRange)
        {   
            var playerCtrl = target.GetComponent<NetworkPlayerController>();
            if (playerCtrl != null && playerCtrl.IsDead())
            {
                agent.ResetPath();
                PlayIdleAnimation();
                return;
            }
            agent.ResetPath();
            FaceTarget();
            PlayAttackAnimation();
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                // var playerCtrl = target.GetComponent<NetworkPlayerController>();
                if (playerCtrl != null)
                {
                    playerCtrl.TakeDamage(1);
                }
            }
        }
        else if (distance <= chaseRange)
        {
            PlayWalkAnimation();
        }
        else
        {
            agent.ResetPath();
            PlayIdleAnimation();
        }
    }

    private void FindClosestPlayer()
    {
        var players = FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        Transform closest = null;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        target = closest;
    }

    private void FaceTarget()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 5f);
        }
    }

    private void PlayIdleAnimation()
    {
        if (anim != null && !anim.IsPlaying("idle"))
        {
            anim.CrossFade("idle");
            netAnimState.Value = EnemyAnimState.Idle;
        }
    }

    private void PlayWalkAnimation()
    {
        if (anim != null && !anim.IsPlaying("walk"))
        {
            anim.CrossFade("walk");
            netAnimState.Value = EnemyAnimState.Walk;
        }
    }

    private void PlayAttackAnimation()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        // lastAttackTime = Time.time;
        if (anim == null) return;

        string[] attackAnims = new[] {
            "attack-kick-left",
            "attack-kick-right",
            "attack-melee-left",
            "attack-melee-right"
        };

        int random = Random.Range(0, attackAnims.Length);
        string chosen = attackAnims[random];
        anim.CrossFade(chosen);

        netAnimState.Value = (EnemyAnimState)random + 2; // match enum offset
    }

    private void PlayDieAnimation()
    {
        if (anim == null) return;
        // Make sure you have a "die" clip
        anim.CrossFade("die");
        netAnimState.Value = EnemyAnimState.Die;
    }

    private float remoteLastAttackTime;

    private void HandleRemoteAnimation()
    {
        if (anim == null) return;

        string clipName = netAnimState.Value switch
        {
            EnemyAnimState.Idle => "idle",
            EnemyAnimState.Walk => "walk",
            EnemyAnimState.AttackKickLeft => "attack-kick-left",
            EnemyAnimState.AttackKickRight => "attack-kick-right",
            EnemyAnimState.AttackMeleeLeft => "attack-melee-left",
            EnemyAnimState.AttackMeleeRight => "attack-melee-right",
            EnemyAnimState.Die => "die",
            _ => "idle"
        };

        if (netAnimState.Value == EnemyAnimState.Die)
        {
            if (remoteDiePlayed) return;
            remoteDiePlayed = true;
        }
        
        bool isAttackAnim = netAnimState.Value >= EnemyAnimState.AttackKickLeft;
        if (isAttackAnim)
        {
            // Enforce client cooldown to avoid attack spam visually
            if (Time.time - remoteLastAttackTime < attackCooldown) return;

            remoteLastAttackTime = Time.time;
        }

        if (!anim.IsPlaying(clipName))
        {
            anim.CrossFade(clipName);
        }
    }

}
