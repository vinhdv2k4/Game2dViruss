using UnityEngine;

public class FallingSpikeTrapAdvanced : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float resetDelay = 3f;
    [SerializeField] private float fallDelay = 0.3f;


    [SerializeField] private Rigidbody2D spikeRigidbody;


    private Vector2 originalPosition;
    private bool isActive = true;

    private void Awake()
    {
        originalPosition = spikeRigidbody.transform.position;
        spikeRigidbody.gravityScale = 0;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            Invoke("ReleaseSpike", fallDelay);
        }
        Debug.Log("Detected");
    }

    private void ReleaseSpike()
    {
        isActive = false;
        spikeRigidbody.gravityScale = fallSpeed;
        Invoke("ResetSpike", resetDelay);
    }

    private void ResetSpike()
    {
        spikeRigidbody.gravityScale = 0;
        spikeRigidbody.velocity = Vector2.zero;
        spikeRigidbody.transform.position = originalPosition;
        isActive = true;
    }

}