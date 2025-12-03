using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public int xpAmount = 1;

    public float bobAmplitude = 0.2f;
    public float bobSpeed = 3f;

    public float attractRadius = 6f;
    public float moveSpeed = 10f;
    public float verticalOffset = 1f;

    Transform player;
    Vector3 startPos;
    bool isAttracted;

    void Start()
    {
        var playerXP = FindObjectOfType<PlayerXP>();
        if (playerXP != null)
            player = playerXP.transform;

        startPos = transform.position;
    }

    void Update()
    {
        if (player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (!isAttracted && dist <= attractRadius)
            isAttracted = true;

        if (isAttracted)
        {
            Vector3 target = player.position + Vector3.up * verticalOffset;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 pos = startPos;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = pos;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var xp = other.GetComponent<PlayerXP>();
        if (xp != null)
        {
            xp.GainXP(xpAmount);
            Destroy(gameObject);
        }
    }
}
