using UnityEngine;

public class WaterAudioZone : MonoBehaviour
{
    public static bool IsPlayerInWater { get; private set; }

    [Header("Oyuncu ve Su Katmaný")]
    public Transform player;
    public LayerMask waterLayerMask;
    public float raycastHeight = 5f;
    public float maxRaycastDistance = 20f;
    public float submergeOffset = 0.3f;

    [Header("Ses Dosyalarý")]
    public AudioClip splashEnterClip;
    public AudioClip splashExitClip;
    public AudioClip swimLoopClip;
    [Range(0f, 1f)] public float splashVolume = 0.8f;
    [Range(0f, 1f)] public float swimLoopVolume = 0.6f;

    private AudioSource oneShotSource;
    private AudioSource loopSource;
    private bool wasInWater = false;

    private void Awake()
    {
        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = 0f;
        loopSource.volume = swimLoopVolume;
        loopSource.clip = swimLoopClip;
    }

    private void Update()
    {
        if (player == null) return;

        bool isInWater = CheckIsInWater();
        IsPlayerInWater = isInWater;

        if (isInWater && !wasInWater)
            EnterWater();
        else if (!isInWater && wasInWater)
            ExitWater();

        wasInWater = isInWater;
    }

    private bool CheckIsInWater()
    {
        Vector3 origin = player.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxRaycastDistance, waterLayerMask))
        {
            return player.position.y <= hit.point.y + submergeOffset;
        }

        return false;
    }

    private void EnterWater()
    {
        if (splashEnterClip != null)
            oneShotSource.PlayOneShot(splashEnterClip, splashVolume);

        if (swimLoopClip != null)
            loopSource.Play();
    }

    private void ExitWater()
    {
        if (splashExitClip != null)
            oneShotSource.PlayOneShot(splashExitClip, splashVolume * 0.7f);

        loopSource.Stop();
    }
}