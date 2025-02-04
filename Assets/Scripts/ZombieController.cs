using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [Header("Zombie Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float speed = 2f;

    [Header("References")]
    [SerializeField] private Transform player;

    public AudioSource bite;  
    private GameController gameController; // เพิ่มตัวแปรอ้างอิงไปยัง GameController
    private int currentHealth;
    private bool isAttacking = false;
    private NavMeshAgent agent;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = animator ?? GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    private void Update()
    {
        if (currentHealth <= 0) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        FacePlayer();

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");
            
            if (bite != null)
        {
            bite.Play(); 
        }
        }
        else if (distanceToPlayer > attackRange)
        {
            isAttacking = false;
            animator.SetBool("Walking", true);
            MoveTowardsPlayer();
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            animator.SetTrigger("Die");
            Destroy(gameObject, 2f);
        }
    }

    public void AttackPlayer()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerAnimationController playerController = player.GetComponent<PlayerAnimationController>();
            if (playerController != null)
            {
                playerController.TakeDamage(10);
            }
             
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AttackPlayer();
        }
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(20);
            Destroy(other.gameObject);
        }
    }

    // ✅ เพิ่มฟังก์ชัน SetGameController()
    public void SetGameController(GameController controller)
    {
        gameController = controller;
    }

    // ✅ เพิ่มฟังก์ชัน SetPlayer() 
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}






