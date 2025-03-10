using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 0.5f;
    public float secondDestroy = 1.2f;
    private float startTime;
    public int damage = 10;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        // เคลื่อนที่กระสุนไปข้างหน้า
        transform.position -= speed * Time.deltaTime * transform.forward;
        
        // ทำลายกระสุนเมื่อถึงเวลา
        if (Time.time - startTime >= secondDestroy)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))  // ถ้าชนกับซอมบี้
        {
            ZombieController zombie = other.GetComponent<ZombieController>();
            if (zombie != null && zombie.IsServer)  // ✅ ต้องเช็คว่าเป็น Server
            {
                zombie.TakeDamageServerRpc(damage);  // ✅ ใช้ ServerRpc เพื่อให้ Sync กับทุกเครื่อง
            }

            Destroy(gameObject);  // ทำลายกระสุนหลังจากชน
        }
    }
}
