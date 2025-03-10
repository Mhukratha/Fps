using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerAnimationController : NetworkBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float rotationSpeed = 250f;

    [Header("Player Stats")]
    [SerializeField] private int health = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("Shooting")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject flashMuzzle;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;

    [Header("Audio")]
    public AudioSource gunshot;  

    private Animator animator;
    private Rigidbody rb;
    private float nextFire = 0.0f;

    private Image healthBar;
    private Text gameOverText;
    private GameObject restartButton;
    private GameObject menuButton;
    private GameObject quitButton;

    private Camera playerCamera;
    private float mouseX;
    private float mouseSensitivity = 2.0f;

    private void Start()
    {   
        if (!IsOwner) 
        {
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = health;
        isDead = false;

        if (flashMuzzle != null) flashMuzzle.SetActive(false);

        GameObject canvas = GameObject.FindWithTag("GameCanvas");
        if (canvas != null)
        {
            healthBar = canvas.transform.Find("HealthBar").GetComponent<Image>();
            gameOverText = canvas.transform.Find("GameOverText").GetComponent<Text>();
            restartButton = canvas.transform.Find("RestartButton").gameObject;
            menuButton = canvas.transform.Find("MenuButton").gameObject;
            quitButton = canvas.transform.Find("QuitButton").gameObject;
        }

        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false);
        if (quitButton != null) quitButton.SetActive(false);

        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!IsOwner || isDead) return;

        HandleMovement();
        HandleCamera();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime);

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

    private void HandleCamera()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0, mouseX, 0);
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
            GameObject newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
            newBullet.GetComponent<Bullet>().SetDirection(firePoint.forward);
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

        if (gameOverText != null) gameOverText.gameObject.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
        if (menuButton != null) menuButton.SetActive(true);
        if (quitButton != null) quitButton.SetActive(true);
        
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
