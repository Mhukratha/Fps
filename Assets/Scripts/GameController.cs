using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
    public GameObject zombiePrefab;
    public Transform spawnAreaCenter;
    public Vector3 spawnAreaSize;
    public Text timerText;
    public Text winText;
    public Transform player;
    public Text waveText;

    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float waveInterval = 5f;

    private int waveNumber = 1;
    private int zombiesPerWave;
    private bool gameEnded = false;

    public GameObject menuButton;
    public GameObject quitButton;
    public GameObject restartButton;

    private NetworkVariable<float> gameTime = new NetworkVariable<float>(300f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsServer)
        {
            gameTime.Value = 300f;
            StartCoroutine(SpawnZombies());
        }

        gameEnded = false;
        winText.gameObject.SetActive(false);
                waveText.gameObject.SetActive(true);
        zombiesPerWave = 5;

        menuButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameEnded) return;

        if (IsServer)
        {
            gameTime.Value -= Time.deltaTime;
        }

        int minutes = Mathf.FloorToInt(gameTime.Value / 60);
        int seconds = Mathf.FloorToInt(gameTime.Value % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        waveText.text = "Wave: " + waveNumber;

        if (gameTime.Value <= 0)
        {
            gameEnded = true;
            timerText.gameObject.SetActive(false);
            winText.gameObject.SetActive(true);
            Time.timeScale = 0;

            quitButton.gameObject.SetActive(true);
            menuButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
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

            waveText.text = "Wave: " + waveNumber;

            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void SpawnZombie()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2),
            spawnAreaCenter.position.y,
            Random.Range(spawnAreaCenter.position.z - spawnAreaSize.z / 2, spawnAreaCenter.position.z + spawnAreaSize.z / 2)
        );

        GameObject newZombie = Instantiate(zombiePrefab, spawnPosition, spawnAreaCenter.rotation);
        newZombie.GetComponent<NetworkObject>().Spawn();

        if (newZombie.TryGetComponent<ZombieController>(out ZombieController zombieController))
        {
            zombieController.SetGameController(this);
            zombieController.SetPlayer(player);
        }
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

