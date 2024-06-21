
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] BubbleType type;
    public BubbleColor color;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rb;
    public CircleCollider2D circleCollider;
    public bool IsConnected = true;
    public void ShootBubble(float shootingForce, float angles)
    {
        IsConnected = false;
        circleCollider.enabled = true;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angles - 90f);
        rb.AddForce(this.transform.up * shootingForce, ForceMode2D.Impulse);
    }

    public void EnabbleCircleCollider()
    {
        circleCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble") && rb != null && !IsConnected)
        {
            IsConnected = true;
            ConnectToMap(this);
            rb.velocity = Vector2.zero;
        }
    }

    public void ConnectToMap(Bubble bubble)
    {
        Destroy(rb);
        GameManager.Instance.ProcessTurn(bubble);
    }

    public List<Bubble> GetNeighbors()
    {
        List<Bubble> neighbors = new List<Bubble>();
        var hits = Physics2D.OverlapCircleAll(transform.position, GameManager.Instance._neighborDetectionRange);

        foreach (var hit in hits)
        {
            if (hit != null)
            {
                Bubble bubble = hit.gameObject.GetComponent<Bubble>();
                if (bubble != null && bubble != this) neighbors.Add(bubble);
            }

        }

        return neighbors;
    }

    public void SetupBubble(BubbleInfo info, bool isSpam = false)
    {
        spriteRenderer.sprite = info.Sprite;
        type = info.Type;
        color = info.Color;

        if(isSpam) return;

        IsConnected = true;
        EnabbleCircleCollider();

    }
}