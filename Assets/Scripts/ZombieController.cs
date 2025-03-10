using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ZombieController : NetworkBehaviour
{
    [Header("Zombie Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int maxHealth = 100;

    private bool isAttacking;
    private Transform targetPlayer;
    private GameController gameController;

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        if (!IsServer) return;

        currentHealth.Value = maxHealth;
        FindClosestPlayer();
        StartCoroutine(UpdateTarget());
    }

    private IEnumerator UpdateTarget()
    {
        while (true)
        {
            FindClosestPlayer();
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void Update()
    {
        if (!IsServer || currentHealth.Value <= 0 || targetPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z));
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player.transform;
            }
        }

        if (closest != null)
        {
            targetPlayer = closest;
        }
    }

    private void AttackPlayer()
    {
        if (targetPlayer == null || isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");

        PlayerAnimationController playerController = targetPlayer.GetComponent<PlayerAnimationController>();
        if (playerController != null && IsServer)
        {
            playerController.TakeDamage(10);
        }

        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {   
        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (currentHealth.Value <= 0) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DieClientRpc();

        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        animator.SetTrigger("Die");
    }

    public void SetGameController(GameController controller)
    {
        gameController = controller;
    }

    public void SetPlayer(Transform playerTransform)
    {
        targetPlayer = playerTransform;
    }
}
