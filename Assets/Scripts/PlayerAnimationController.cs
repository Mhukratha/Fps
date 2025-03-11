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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Camera playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false); // ❌ ปิดกล้องของ Player ที่ไม่ใช่เจ้าของ
            }
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.transform.root != transform) 
            {
                cam.gameObject.SetActive(false);
            }
        }

        Camera myCamera = GetComponentInChildren<Camera>();
        if (myCamera != null)
        {
            myCamera.gameObject.SetActive(true);
        }

        GameObject canvas = GameObject.FindWithTag("GameCanvas");
        if (canvas != null)
        {
            Transform healthBarTransform = canvas.transform.Find("HealthBar");
            if (healthBarTransform != null)
            {
                healthBar = healthBarTransform.GetComponent<Image>();
            }
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
        TakeDamageServerRpc(damage, NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage, ulong playerId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId)) return;

        if (NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.TryGetComponent(out PlayerAnimationController player))
        {
            if (player.currentHealth.Value <= 0) return;

            player.currentHealth.Value -= damage;
            player.UpdateHealthBarClientRpc(player.currentHealth.Value, playerId);

            if (player.currentHealth.Value <= 0)
            {
                player.DieClientRpc(playerId);
            }
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

        Debug.Log("🔴 [PlayerAnimationController] Player ตาย -> Game Over!");


        if (IsOwner)
        {
            Time.timeScale = 0;
        }
    }
}