using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private Sprite cue0;
    [SerializeField] private Sprite cue1;
    private SpriteRenderer _sr;

    public bool _inRange { get; private set; }
    private bool _inRangeOld;

    private Controller _controller;
    
    private void Start()
    {
        _sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _sr.sprite = cue0;
        _controller = Player.Instance.GetComponent<Controller>();
    }

    private void Update()
    {
        CollisionCheck();
        CheckInRange();
        CheckInteraction();
    }

    private void CollisionCheck()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Collider2D hit = Physics2D.OverlapCircle(origin, range, collisionMask);
        
        if (!hit)
        {
            _inRange = false;
            return;
        }
        if (hit)
        {
            _inRange = true;
        }
    }

    void CheckInRange()
    {
        if (_inRange != _inRangeOld)
        {
            _controller.collisionData.inRange = _inRange;
            
            if (_inRange)
            {
                _sr.sprite = cue1;
            }
            else
            {
                _sr.sprite = cue0;
            }

            _controller.collisionData.canInteract = _inRange;
        }

        _inRangeOld = _inRange;
    }

    void CheckInteraction()
    {
        if (_inRange && !_controller.collisionData.isInteracting && Player.Instance._interactInp)
        {
            print("Do Something");
            Player.Instance._interactInp = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
