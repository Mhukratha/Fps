using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode; // ✅ เพิ่ม Netcode

public class PlayerAnimationController : NetworkBehaviour // ✅ เปลี่ยนเป็น NetworkBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float rotationSpeed = 250f;

    [Header("Player Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private Image healthBar;

    [Header("Shooting")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject flashMuzzle;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;

    [Header("UI Elements")]
    [SerializeField] private Text gameOverText;

    public AudioSource gunshot;  
    private Animator animator;
    private Rigidbody rb;
    private int currentHealth;
    private float nextFire = 0.0f;
    private bool isDead = false;

    public GameObject restartButton;
    public GameObject menuButton;
    public GameObject quitButton;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        currentHealth = health;
        if (flashMuzzle != null) flashMuzzle.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);

        restartButton.gameObject.SetActive(false); 
        menuButton.gameObject.SetActive(false); 
        quitButton.gameObject.SetActive(false);

        // ✅ ตรวจสอบว่าเป็นเจ้าของ (Client ที่ควบคุมตัวละครนี้)
        if (!IsOwner) 
        {
            enabled = false; // ❌ ปิดการควบคุมถ้าไม่ใช่เจ้าของ
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (!IsOwner) return; // ✅ ตรวจสอบว่าผู้เล่นนี้เป็นเจ้าของหรือไม่

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        transform.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);

        if (horizontal != 0 || vertical != 0)
        {
            animator.SetBool("IsWalking", true);
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
            animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
            transform.Translate(new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > nextFire)
        {
            animator.SetBool("IsShooting", true);
            nextFire = Time.time + fireRate;
            Fire();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool("IsShooting", false);
        }
    }

    private void Fire()
    {
        if (bullet != null && firePoint != null)
        {
            Instantiate(bullet, firePoint.position, firePoint.rotation);
        }

        if (flashMuzzle != null)
        {
            flashMuzzle.SetActive(true);
            StartCoroutine(HideMuzzle(0.12f));
        }
        if (gunshot != null)
        {
            gunshot.Play(); 
        }
    }

    private IEnumerator HideMuzzle(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (flashMuzzle != null) flashMuzzle.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthBar();
        if (currentHealth <= 0) Die();
        else animator.SetTrigger("Hit");
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER";

            restartButton.gameObject.SetActive(true); 
            menuButton.gameObject.SetActive(true); 
            quitButton.gameObject.SetActive(true);
        }
        Time.timeScale = 0;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / health;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            TakeDamage(10);
        }
    }
}
