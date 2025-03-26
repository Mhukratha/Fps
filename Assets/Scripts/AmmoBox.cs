using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public int ammoAmount = 12;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAnimationController player = other.GetComponent<PlayerAnimationController>();
            if (player != null)
            {
                player.AddAmmo(ammoAmount);
                Destroy(gameObject); // ทำลายกล่องกระสุนหลังเก็บ
            }
        }
    }
}
