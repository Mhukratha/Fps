using UnityEngine;
using Unity.Netcode;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage = 10;
    private Vector3 moveDirection; // ✅ เพิ่มตัวแปร moveDirection

    public void SetDirection(Vector3 direction) // ✅ ฟังก์ชันกำหนดทิศทาง
    {
        moveDirection = direction.normalized;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                other.GetComponent<ZombieController>().TakeDamageServerRpc(damage);
            }
            else
            {
                SendDamageToServer(other.GetComponent<NetworkObject>().NetworkObjectId, damage);
            }

            Destroy(gameObject);
        }
    }

    [ServerRpc]
    private void SendDamageToServer(ulong zombieId, int damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(zombieId, out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent<ZombieController>(out ZombieController zombie))
            {
                zombie.TakeDamageServerRpc(damage);
            }
        }
    }
}
