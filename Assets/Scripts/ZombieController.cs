using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class ZombieController : NetworkBehaviour
{
    [Header("Zombie Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int maxHealth = 100;
    private bool isAttacking;
    private NavMeshAgent agent;
    private Transform targetPlayer;
    private GameController gameController; 

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    private void Start()
    {
        if (!IsServer) return;

        agent = GetComponent<NavMeshAgent>();
        currentHealth.Value = maxHealth;
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
            agent.SetDestination(targetPlayer.position);
        }
    }

    private void AttackPlayer()
    {
        if (targetPlayer == null || isAttacking) return; // ป้องกันการโจมตีซ้ำ
        isAttacking = true; 
        animator.SetTrigger("Attack"); 

        PlayerAnimationController playerController = targetPlayer.GetComponent<PlayerAnimationController>();
        if (playerController != null && IsServer)
        {
            playerController.TakeDamage(10);
            Debug.Log("🧟 ซอมบี้โจมตีผู้เล่น!");
        }
        StartCoroutine(ResetAttack());
    }

        private IEnumerator ResetAttack()
    {   
        yield return new WaitForSeconds(1.5f); // รอ 1.5 วินาทีก่อนโจมตีใหม่
        isAttacking = false;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (currentHealth.Value <= 0) return;

        currentHealth.Value -= damage;
        Debug.Log($"Zombie ถูกโจมตี! เหลือ {currentHealth.Value} HP");

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        animator.SetTrigger("Die");
    }

    private void Die()
    {
        DieClientRpc();
        Debug.Log("Zombie ตายแล้ว!");

        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    // ✅ ฟังก์ชันให้ GameController ส่งข้อมูลไปยังซอมบี้
    public void SetGameController(GameController controller)
    {
        gameController = controller;
    }

    // ✅ ฟังก์ชันให้ GameController ส่งข้อมูลผู้เล่นให้ซอมบี้
    public void SetPlayer(Transform playerTransform)
    {
        targetPlayer = playerTransform;
    }
}
