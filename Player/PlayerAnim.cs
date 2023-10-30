using System.Collections;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [Header("JUMP ANIM")]
    public float JumpAnim_XFactor = .6f; //.5f
    public float JumpAnim_YFactor = 1.8f; //2f
    private Vector3 ReverseAnim_Scale; //For Reverse Anim (Jump Anim)

    Player player;

    public static PlayerAnim Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        player = Player.Instance;
        ReverseAnim_Scale = transform.localScale;
    }

    //Anim
    public IEnumerator SpawnAnimation(float time)
    {
        player.canMove = false;
        Vector3 tempScale = transform.localScale;
        transform.localScale = Vector3.zero;

        float i = 0;
        float rate = 1 / time;

        Vector3 fromScale = Vector3.zero;
        Vector3 toScale = tempScale;
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }

        player.canMove = true;
    }

    public IEnumerator ScaleDownAnim(float time)
    {
        player.canMove = false;

        float i = 0;
        float rate = 1 / time;

        Vector3 fromScale = transform.localScale;
        Vector3 toScale = Vector3.zero;
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }

        player.canMove = true;
    }

    public IEnumerator JumpAnim(float time)
    {
        float i = 0;
        float rate = 1 / time;

        Vector3 fromScale = transform.localScale;
        Vector3 toScale = new Vector3(transform.localScale.x * JumpAnim_XFactor, transform.localScale.y * JumpAnim_YFactor, transform.localScale.z);
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }

        StartCoroutine(ReverseAnim(time));

        IEnumerator ReverseAnim(float time)
        {
            time /= 2;
            float i = 0;
            float rate = 1 / time;

            Vector3 ReverseAnim_tempScale;

            ReverseAnim_tempScale = new Vector3(ReverseAnim_Scale.x * player.controller.collisionData.faceDir, ReverseAnim_Scale.y);

            Vector3 fromScale = transform.localScale;
            Vector3 toScale = new Vector3(ReverseAnim_tempScale.x, ReverseAnim_tempScale.y);
            while (i < 1)
            {
                i += Time.deltaTime * rate;
                transform.localScale = Vector3.Lerp(fromScale, toScale, i);
                yield return 0;
            }
        }
    }

    public IEnumerator RollAnim(float time)
    {
        float i = 0;
        float rate = 1 / time;

        Vector3 fromScale = transform.localScale;
        Vector3 toScale = new Vector3(transform.localScale.x, transform.localScale.y * .5f, transform.localScale.z);
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }

        StartCoroutine(ReverseAnim(time));

        IEnumerator ReverseAnim(float time)
        {
            time /= 2;
            float i = 0;
            float rate = 1 / time;

            Vector3 ReverseAnim_tempScale;

            ReverseAnim_tempScale = new Vector3(ReverseAnim_Scale.x * player.controller.collisionData.faceDir, ReverseAnim_Scale.y);

            Vector3 fromScale = transform.localScale;
            Vector3 toScale = new Vector3(ReverseAnim_tempScale.x, ReverseAnim_tempScale.y);
            while (i < 1)
            {
                i += Time.deltaTime * rate;
                transform.localScale = Vector3.Lerp(fromScale, toScale, i);
                yield return 0;
            }
        }
    }

    public void DeathAnim(float waitTime)
    {

        IEnumerator DestroyTimer(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            Instantiate(player.deathParticle, transform.position, Quaternion.identity);
            StartCoroutine(DeathScaleDownAnim(.25f));
            
            GameManager.Instance.RestartScene();
            Destroy(gameObject);

            IEnumerator DeathScaleDownAnim(float time)
            {
                player.canMove = false;

                float i = 0;
                float rate = 1 / time;

                Vector3 fromScale = transform.localScale;
                Vector3 toScale = Vector3.zero;
                while (i < 1)
                {
                    i += Time.deltaTime * rate;
                    transform.localScale = Vector3.Lerp(fromScale, toScale, i);
                    yield return 0;
                }

                GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        StartCoroutine(DestroyTimer(waitTime));
    }


    //Tilt
    public void Tilt()
    {
        if (player.canMove)
        {
            if (player.controller.collisionData.below)
            {
                if (InputManager.Instance.moveAction.ReadValue<Vector2>().x > 0f)
                {
                    //transform.eulerAngles = new Vector3(0f, 0f, -10f * player.controller.collisionData.faceDir);

                    if (player.controller.collisionData.faceDir == 1)
                    {
                        transform.eulerAngles = new Vector3(0f, 0f, -10f);
                    }
                    else
                    {
                        transform.eulerAngles = new Vector3(0f, 0f, 10f);
                    }
                }
                else
                {
                    transform.eulerAngles = Vector3.zero;
                }
            }
            else
            {
                transform.eulerAngles = Vector3.zero;
            }
        }
    }
}
