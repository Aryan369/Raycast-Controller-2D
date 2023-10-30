using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [HideInInspector] public Transform firePoint;
    public GameObject bulletPrefab;

    public int bulletPoolSize = 5; // Number of bullets to initially create in the pool
    private Queue<GameObject> bulletPool; // Queue to store bullets
    private List<GameObject> activeBullets; // List to store currently active bullets

    private void Start()
    {
        firePoint = transform.GetChild(0).transform;
        InitializeBulletPool();
    }

    public void SetFirePointRot(float angle)
    {
        firePoint.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public void Shoot()
    {
        // Check if there are bullets available in the pool
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.SetActive(true);
            activeBullets.Add(bullet);

            bullet.GetComponent<Bullet>().FireBullet();
            bullet.GetComponent<Bullet>().SetGun(this);
        }
    }

    private void InitializeBulletPool()
    {
        bulletPool = new Queue<GameObject>();
        activeBullets = new List<GameObject>();

        // Instantiate and deactivate bullets, then add them to the pool
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    // Called by bullets when they are deactivated
    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        activeBullets.Remove(bullet);
        bulletPool.Enqueue(bullet);
    }
}
