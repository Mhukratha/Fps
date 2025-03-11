using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerAnimationController : NetworkBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 250f;

    [Header("Player Stats")]
    [SerializeField] private int health = 100;
    private bool isDead = false;

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

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
    private Camera playerCamera;
    private Image healthBar;
    private Text gameOverText; 

    private void Start()
    {
        if (!IsOwner) return; // ✅ ป้องกัน Client จากการแก้ไข NetworkVariable

        if (IsServer) // ✅ ให้เซิร์ฟเวอร์เป็นคนกำหนดค่า
        {
            currentHealth.Value = health; 
        }

        isDead = false;
        Debug.Log($"🎮 Player {NetworkObjectId} เริ่มเกม | HP: {currentHealth.Value}");
    }   

    public override void OnNetworkSpawn()
    {
            if (!IsOwner)
        {
            Camera playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ✅ ใช้ตัวแปร canvas เพียงครั้งเดียว
        GameObject canvas = GameObject.FindWithTag("GameCanvas");
        if (canvas != null)
        {
            Transform healthBarTransform = canvas.transform.Find("HealthBar");
            if (healthBarTransform != null)
            {
                healthBar = healthBarTransform.GetComponent<Image>();
            }

            Transform gameOverTransform = canvas.transform.Find("GameOverText");
            if (gameOverTransform != null)
            {
                gameOverText = gameOverTransform.GetComponent<Text>();
                gameOverText.gameObject.SetActive(false); // ✅ ปิด Game Over Text สำหรับทุกคน
            }
        }

            if (IsServer) // ✅ เรียก ClientRpc จาก Server เท่านั้น
        {
            DisableGameOverClientRpc();
        }
    }

    [ClientRpc]
    private void DisableGameOverClientRpc()
    {
         Debug.Log($"🛠️ [Client {NetworkManager.Singleton.LocalClientId}] ปิด GameOverText");

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
            Debug.Log($"✅ [Client {NetworkManager.Singleton.LocalClientId}] ปิด GameOverText สำเร็จ");
        }
        else
        {
            Debug.LogError($"❌ [Client {NetworkManager.Singleton.LocalClientId}] ไม่พบ gameOverText");
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

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, mouseX, 0);
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Fire();
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
        if (!IsOwner) return;
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (currentHealth.Value <= 0) return;

        currentHealth.Value -= damage;
        Debug.Log($"💥 Player {NetworkObjectId} ถูกโจมตี! HP เหลือ {currentHealth.Value}");

        UpdateHealthBarClientRpc(currentHealth.Value, NetworkObjectId);

        if (currentHealth.Value <= 0)
        {
            DieClientRpc(NetworkObjectId);
        }
    }

    [ClientRpc]
    private void UpdateHealthBarClientRpc(int health, ulong playerId)
    {
        if (NetworkObjectId != playerId) return;

        if (healthBar != null)
        {
            healthBar.fillAmount = (float)health / this.health;
        }
    }

    [ClientRpc]
    private void DieClientRpc(ulong playerId)
    {
        if (NetworkObjectId != playerId) return;

        isDead = true;
        animator.SetTrigger("Die");

        Debug.Log($"❌ Player {NetworkObjectId} ตายแล้ว!");

        if (IsOwner)
        {
            Time.timeScale = 0;
            if (GameController.Instance != null)
            {
                GameController.Instance.ShowGameOverClientRpc(); // ✅ ให้ GameController แสดง Game Over
            }
        }
    }
    [ClientRpc]
    public void ShowGameOverClientRpc()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true); // ✅ แสดงข้อความ Game Over
        }   
    }
}