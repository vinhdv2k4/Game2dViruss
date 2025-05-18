using UnityEngine;

public class MovingTrap : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    private Transform currentTarget;
    private float waitTimer;
    private bool isWaiting;

    private void Start()
    {
        currentTarget = pointA;
        transform.position = pointA.position;
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentTarget = currentTarget == pointA ? pointB : pointA;
            }
            return;
        }
        transform.position = Vector2.MoveTowards(
            transform.position,
            currentTarget.position,
            speed * Time.deltaTime
        );
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.01f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    public void SetPoints(Transform a, Transform b)
    {
        pointA = a;
        pointB = b;
    }
}