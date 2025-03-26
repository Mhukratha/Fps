using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
    public static GameController Instance; // ✅ สร้าง Instance เพื่อให้เรียกใช้ได้ง่าย
    public Text gameOverText;
    public GameObject zombiePrefab;
    public Transform spawnAreaCenter;
    public Vector3 spawnAreaSize;
    public Text timerText;
    public Text winText;
    public Text waveText;

    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float waveInterval = 5f;

    private int waveNumber = 1;
    private int zombiesPerWave;
    private bool gameEnded = false;

    public GameObject menuButton;
    public GameObject quitButton;
    public GameObject restartButton;



    private NetworkVariable<float> gameTime = new NetworkVariable<float>(
        30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

      private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false); // ✅ ปิด Game Over Text ตอนเริ่มเกม
        }

        if (IsServer)
        {
            gameTime.Value = 30f;
            gameEnded = false;
            StartCoroutine(SpawnZombies());
        }

        winText.gameObject.SetActive(false);
        waveText.gameObject.SetActive(true);
        zombiesPerWave = 5;

        menuButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsServer || gameEnded) return;

        gameTime.Value -= Time.deltaTime;
        UpdateTimerClientRpc(gameTime.Value);

        if (gameTime.Value <= 0)
        {
            gameEnded = true;
            EndGameClientRpc();
        }
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private IEnumerator SpawnZombies()
    {
        while (!gameEnded)
        {
            for (int i = 0; i < zombiesPerWave; i++)
            {
                SpawnZombie();
                yield return new WaitForSeconds(spawnInterval);
            }

            waveNumber++;
            zombiesPerWave += 2;

            UpdateWaveClientRpc(waveNumber);
            yield return new WaitForSeconds(waveInterval);
        }
    }

    [ClientRpc]
    public void ShowGameOverClientRpc()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true); // ✅ แสดง Game Over Text ทุก Client
        }
    }

    private void SpawnZombie()
    {
        if (!IsServer) return; // ✅ ป้องกัน Client เรียก Spawn

        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2),
            spawnAreaCenter.position.y,
            Random.Range(spawnAreaCenter.position.z - spawnAreaSize.z / 2, spawnAreaCenter.position.z + spawnAreaSize.z / 2)
        );

        GameObject newZombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = newZombie.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn(true); // ✅ Spawn อย่างถูกต้องโดย Server เท่านั้น
        }
        else
        {
            Debug.LogError("❌ NetworkObject ไม่ถูกต้องใน ZombiePrefab!");
            return;
        }

        // ✅ รอ 0.1 วิ ให้แน่ใจว่า Spawn สำเร็จ ก่อน Sync ไปยัง Client
        StartCoroutine(SyncZombieAfterSpawn(newZombie, spawnPosition));
    }

    private IEnumerator SyncZombieAfterSpawn(GameObject zombie, Vector3 position)
    {
        yield return new WaitForSeconds(0.1f); // ✅ ให้แน่ใจว่า Spawn เสร็จแล้ว

        ZombieController zombieController = zombie.GetComponent<ZombieController>();
        if (zombieController != null)
        {
            zombieController.SyncZombieClientRpc(position, Quaternion.identity);
            Debug.Log("✅ Zombie ถูก Sync ไปยัง Client แล้ว!");
        }
        else
        {
            Debug.LogError("❌ ZombieController ไม่พบใน Prefab!");
        }
    }


    [ClientRpc]
    private void UpdateWaveClientRpc(int wave)
    {
        waveText.text = "Wave: " + wave;
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        timerText.gameObject.SetActive(false);
        winText.gameObject.SetActive(true);
        Time.timeScale = 0;

        Debug.Log("🔴 [GameController] เวลาในเกมหมด -> Game Over!");


        quitButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        if (spawnAreaCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter.position, spawnAreaSize);
        }
    }

}