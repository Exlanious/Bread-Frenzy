using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public int xpAmount = 1;

    [Header("Idle Bob")]
    public float bobAmplitude = 0.2f;
    public float bobSpeed = 3f;

    [Header("Attraction")]
    public float attractRadius = 6f;
    public float moveSpeed = 10f;
    public float verticalOffset = 0.5f; 

    [Header("Drop To Ground")]
    public float hoverHeight = 0.3f;      
    public float dropSpeed = 8f;         
    public float attractDelay = 0.3f;    
    public LayerMask groundMask;          

    Transform player;

    Vector3 startPos;      
    Vector3 groundPos;     
    bool hasLanded = false;
    bool isAttracted = false;
    float landedTime = 0f;

    void Start()
    {
        var playerXP = FindObjectOfType<PlayerXP>();
        if (playerXP != null)
            player = playerXP.transform;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f, groundMask))
        {
            groundPos = hit.point + Vector3.up * hoverHeight;
        }
        else
        {
            groundPos = transform.position;
            hasLanded = true;
            startPos = groundPos;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (!hasLanded)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                groundPos,
                dropSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, groundPos) < 0.01f)
            {
                hasLanded = true;
                startPos = groundPos;
                transform.position = groundPos;
                landedTime = 0f;
            }

            return; 
        }

        if (!isAttracted)
        {
            landedTime += Time.deltaTime;

            Vector3 pos = startPos;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = pos;

            if (landedTime >= attractDelay)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attractRadius)
                {
                    isAttracted = true;
                }
            }
        }

        if (isAttracted)
        {
            Vector3 target = player.position + Vector3.up * verticalOffset;
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
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
