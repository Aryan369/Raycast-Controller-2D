using UnityEngine;
using Cinemachine;

/*Allows the camera to shake when the player punches, gets hurt, etc. Put any other custom camera effects in this script!*/

public class CameraEffects : MonoBehaviour
{
    public Vector3 cameraWorldSize;
    public CinemachineFramingTransposer cinemachineFramingTransposer;
    [SerializeField] private CinemachineBasicMultiChannelPerlin multiChannelPerlin;
    public float screenYDefault;
    public float screenYTalking;
    [Range(0, 10)]
    [System.NonSerialized] public float shakeLength = 10;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        //Ensures we can shake the camera using Cinemachine. Don't really worry too much about this weird stuff. It's just Cinemachine's variables.
        cinemachineFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        screenYDefault = cinemachineFramingTransposer.m_ScreenX;

        //Inform the player what CameraEffect it should be controlling, no matter what scene we are on.
        player.GetComponent<Player>().cameraEffects = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        multiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //Tells the virtualCamera what to follow
        virtualCamera.Follow = player.transform;
    }

    void Update()
    {
        // player.GetComponent<Player>().cameraEffects = this;
        multiChannelPerlin.m_FrequencyGain += (0 - multiChannelPerlin.m_FrequencyGain) * Time.deltaTime * (10 - shakeLength);
    }

    public void Shake(float shake, float length)
    {
        shakeLength = length;
        multiChannelPerlin.m_FrequencyGain = shake;
    }
}
