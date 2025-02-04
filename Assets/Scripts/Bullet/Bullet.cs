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
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))  // ถ้าชนกับซอมบี้
        {
            // เรียกฟังก์ชันลดเลือดของซอมบี้
            other.GetComponent<ZombieController>().TakeDamage(damage);  // ลดเลือดของซอมบี้
            Destroy(gameObject);  // ทำลายกระสุนหลังจากชน
        }
    }
}



