using Unity.Netcode;
using UnityEngine;

public class GunShoot : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireCooldown = 2f;
    private NetworkPlayerController playerCtrl;

    private float nextFireTime = 0f;
    void Start()
    {
        playerCtrl = GetComponentInParent<NetworkPlayerController>();
    }
    void Update()
    {
        if (!IsOwner) return;
        if (playerCtrl != null && playerCtrl.netIsDead.Value)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            ShootServerRpc(firePoint.position, firePoint.forward);
        }
    }

    [ServerRpc]
    void ShootServerRpc(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.SetDirection(direction);
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }

        NetworkObject netObj = bullet.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
}
