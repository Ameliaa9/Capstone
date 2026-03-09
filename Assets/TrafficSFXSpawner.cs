using System.Collections;
using UnityEngine;

public class TrafficSFXSpawner3D : MonoBehaviour
{
    [Header("Target (usually player or camera)")]
    public Transform listenerTarget;

    [Header("Clips")]
    public AudioClip[] carHonks;

    [Header("Timing (seconds)")]
    public float minInterval = 8f;
    public float maxInterval = 20f;

    [Header("Spawn ring around target (meters)")]
    public float minRadius = 15f;
    public float maxRadius = 45f;

    [Header("Volume")]
    public float minVolume = 0.10f;
    public float maxVolume = 0.22f;

    [Header("Pitch")]
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    [Header("Height offset")]
    public float yOffset = 0f;

    private AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();

        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;   
        src.dopplerLevel = 0f;   
    }

    void Start()
    {
        if (listenerTarget == null)
        {
            // fallback: main camera
            if (Camera.main != null) listenerTarget = Camera.main.transform;
        }

        if (listenerTarget != null && carHonks != null && carHonks.Length > 0)
            StartCoroutine(LoopPlay());
    }

    IEnumerator LoopPlay()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

           
            if (Random.value > 0.6f)
                continue;

            
            Vector2 dir2 = Random.insideUnitCircle.normalized;
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 pos = listenerTarget.position + new Vector3(dir2.x, 0f, dir2.y) * radius;
            pos.y += yOffset;

            transform.position = pos;

            var clip = carHonks[Random.Range(0, carHonks.Length)];
            src.volume = Random.Range(minVolume, maxVolume);
            src.pitch = Random.Range(minPitch, maxPitch);

            src.PlayOneShot(clip);
        }
    }
}
