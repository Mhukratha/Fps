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
        300f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            gameTime.Value = 300f;
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

    private void SpawnZombie()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2),
            spawnAreaCenter.position.y,
            Random.Range(spawnAreaCenter.position.z - spawnAreaSize.z / 2, spawnAreaCenter.position.z + spawnAreaSize.z / 2)
        );

        GameObject newZombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        newZombie.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    private void UpdateWaveClientRpc(int wave)
    {
        waveText.text = "Wave: " + wave;
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (!IsOwner) return;

        timerText.gameObject.SetActive(false);
        winText.gameObject.SetActive(true);
        Time.timeScale = 0;

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
