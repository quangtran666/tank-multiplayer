using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        Destroy(gameObject);
    }
}
