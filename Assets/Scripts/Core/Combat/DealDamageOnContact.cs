using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.attachedRigidbody == null) return;

        if (col.attachedRigidbody.TryGetComponent<NetworkObject>(out var networkObject))
        {
            if (ownerClientId == networkObject.OwnerClientId)
            {
                return; // Ignore collisions with the owner of the projectile
            }
        }

        if (col.attachedRigidbody.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(damage);
            Debug.Log($"Dealt {damage} damage to {col.name}");
        }
    }
}
