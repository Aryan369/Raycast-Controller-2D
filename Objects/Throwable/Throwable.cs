using UnityEngine;

public class Throwable : MonoBehaviour
{
    [HideInInspector] public ThrowableStates state = ThrowableStates.Idle;

    public LayerMask _collisionMask;
    
    //private float velocity = 2.5f; // Implement for different velocity for diffent throwables.

    private float collisionMaskRange;

    public bool canBePicked;
    
    private SpriteRenderer _sr;
    public TrailRenderer _tr;
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Player _player;
    
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _tr = GetComponentInChildren<TrailRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _player = Player.Instance;
        collisionMaskRange = transform.localScale.x * 0.5f;
    }
    
    void Update()
    {
        ThrownCollisionCheck();
        StateCheck();
    }

    #region Methods
    public void Throw(float _velocity)
    {
        state = ThrowableStates.Thrown;
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y + 0.25f);
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Instance.mousePosAction.ReadValue<Vector2>());
        Vector2 angle = (mousePos - transform.position).normalized;

        _rb.velocity = angle * _velocity;
    }

    public void SwitchPos()
    {
        _tr.Clear();
        state = ThrowableStates.TeleportPosSwitched;
        _rb.velocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 5f;
    }

    #endregion


    #region Collision Check

    private void ThrownCollisionCheck()
    {
        if (state == ThrowableStates.Thrown || state == ThrowableStates.TeleportPosSwitched)
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y);
            Collider2D thrownHit = Physics2D.OverlapCircle(origin, collisionMaskRange, _collisionMask);
            if (thrownHit)
            {
                if (thrownHit.CompareTag("Enemy"))
                {
                    thrownHit.GetComponent<EnemyAI>().KillEnemy();
                }
                state = ThrowableStates.Discarded;
            }
        }
    }
    
    #endregion


    #region State Check
    private void StateCheck()
    {
        if (state == ThrowableStates.Picked)
        {
            _tr.Clear();
            _tr.enabled = false;
            _sr.enabled = false;
            _collider.enabled = false;
        }
        else if(state != ThrowableStates.Discarded)
        {
            _sr.enabled = true;
            _collider.enabled = true;
        }

        if (state == ThrowableStates.Discarded)
        {
            Destroy(gameObject);
        }
        
        if (state == ThrowableStates.Thrown)
        {
            _tr.enabled = true;
            Destroy(gameObject, 10f);
        }
    }
    
    #endregion
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, collisionMaskRange);
    }
}
