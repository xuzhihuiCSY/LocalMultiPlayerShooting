using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float lifeTime = 3f;
    public float speed = 20f;
    private Vector3 direction;
    public int damage = 1;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroySelf), lifeTime);
        }
    }

    void Update()
    {
        if (!IsServer) return;

        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        Debug.Log("Bullet hit: " + collision.collider.name);
        DestroySelf();
    }

    private void DestroySelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}
