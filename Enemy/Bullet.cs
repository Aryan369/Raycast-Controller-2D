using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 70f;
    private Rigidbody2D rb;
    private TrailRenderer tr;

    private float lifetime = 7f;
    private float lifetimeCounter;

    private enum States
    {
        fired,
        deflected
    }

    private States state;

    private Gun gun; // Reference to the gun that fired the bullet

    void Awake()
    {
        lifetimeCounter = lifetime;
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponentInChildren<TrailRenderer>();
    }

    private void Update()
    {
        if (state == States.fired)
        {
            lifetimeCounter -= Time.deltaTime;

            if (lifetimeCounter <= 0f)
            {
                gun.ReturnBulletToPool(gameObject); // Return bullet to the pool through the gun
            }
        }
    }

    // Set the gun that fired the bullet
    public void SetGun(Gun gun)
    {
        this.gun = gun;
    }

    public void FireBullet()
    {
        tr.Clear();
        state = States.fired;
        //TODO fire bullet from left
        rb.velocity = transform.right * speed;
    }

    public void DeflectBullet()
    {
        state = States.deflected;
        rb.velocity *= -1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(state == States.fired)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == 6)
            {
                Player.Instance.KillPlayer();
                gun.ReturnBulletToPool(gameObject);
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.layer == 6)
            {
                collision.GetComponent<EnemyAI>().KillEnemy();
                gun.ReturnBulletToPool(gameObject);
            }
        }
    }
}
