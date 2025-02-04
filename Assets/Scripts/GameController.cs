using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject zombiePrefab;  // ซอมบี้ที่ใช้ spawn
    public Transform spawnAreaCenter;  // จุดกลางของเขต spawn
    public Vector3 spawnAreaSize;    // ขนาดของเขต spawn (กว้าง, ยาว, สูง)
    public Text timerText;           // UI Text สำหรับแสดงเวลาที่เหลือ
    public Text winText;             // UI Text สำหรับแสดงข้อความเมื่อชนะ
    public Transform player;         // ผู้เล่น
    public Text waveText;  // UI Text สำหรับแสดงหมายเลขเวฟ


    [SerializeField] private float gameTime = 300f;  // เวลาที่สามารถปรับใน Inspector (เริ่มต้น 5 นาที)
    [SerializeField] private float spawnInterval = 1f; // เวลาระหว่างการ spawn ซอมบี้แต่ละตัว
    [SerializeField] private float waveInterval = 5f; // เวลาพักระหว่างเวฟ

    private int waveNumber = 1;      // เริ่มเวฟที่ 1
    private int zombiesPerWave;     // จำนวนซอมบี้ที่ spawn ในแต่ละเวฟ
    private bool gameEnded = false;

    private void Start()
    {
        winText.gameObject.SetActive(false); // ซ่อนข้อความชนะตอนเริ่มเกม
        waveText.gameObject.SetActive(true);  // แสดง waveText ตอนเริ่มเกม
        zombiesPerWave = 5;  // จำนวนซอมบี้ในเวฟแรก
        StartCoroutine(SpawnZombies()); // เริ่มการ spawn ซอมบี้
    }

    private void Update()
    {
        if (gameEnded) return;

        // คำนวณเวลาที่เหลือ
        gameTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // แสดงหมายเลขเวฟ
        waveText.text = "Wave: " + waveNumber;

        // เมื่อหมดเวลา
        if (gameTime <= 0)
        {
            gameEnded = true;
            timerText.gameObject.SetActive(false); // ซ่อนตัวจับเวลา
            winText.gameObject.SetActive(true);    // แสดงข้อความชนะ
            Time.timeScale = 0; // หยุดเกม
        }
    }


    // Coroutine สำหรับการ spawn ซอมบี้
    private IEnumerator SpawnZombies()
    {
        while (!gameEnded)
        {
            for (int i = 0; i < zombiesPerWave; i++)
            {
                SpawnZombie();
                yield return new WaitForSeconds(spawnInterval);  // รอเวลาก่อน spawn ตัวต่อไป
            }

            waveNumber++; // เพิ่มจำนวนเวฟ
            zombiesPerWave += 2;  // เพิ่มจำนวนซอมบี้ในแต่ละเวฟ

            // แสดงหมายเลขเวฟ
            waveText.text = "Wave: " + waveNumber;

            yield return new WaitForSeconds(waveInterval);  // รอเวลาก่อนเริ่มเวฟใหม่
        }
    }


    // ฟังก์ชันสำหรับการ spawn ซอมบี้ภายในเขตที่กำหนด
    private void SpawnZombie()
    {
        // คำนวณตำแหน่งสุ่มภายในเขต spawn
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2),
            spawnAreaCenter.position.y,  
            Random.Range(spawnAreaCenter.position.z - spawnAreaSize.z / 2, spawnAreaCenter.position.z + spawnAreaSize.z / 2)
        );

        GameObject newZombie = Instantiate(zombiePrefab, spawnPosition, spawnAreaCenter.rotation);
        
        // เชื่อมโยง ZombieController กับ GameController
        ZombieController zombieController = newZombie.GetComponent<ZombieController>();
        if (zombieController != null)
        {
            zombieController.SetGameController(this);
            zombieController.SetPlayer(player);
        }
    }

    // ฟังก์ชันเพื่อวาดขอบเขต spawn ใน Unity Editor
    private void OnDrawGizmos()
    {
        if (spawnAreaCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter.position, spawnAreaSize); 
        }
    }
}





