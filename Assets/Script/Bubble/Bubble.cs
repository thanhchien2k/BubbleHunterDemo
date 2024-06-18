
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] BubbleType type;
    [SerializeField] BubbleColor color;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleCollider2D circleCollider;

    public void ShootBubble(float shootingForce, float angles)
    {
        circleCollider.enabled = true;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angles - 90f);
        rb.AddForce(this.transform.up * shootingForce, ForceMode2D.Impulse);
    }

    public void EnabbleCircleCollider()
    {
        circleCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble"))
        {
            //if (collision.gameObject.GetComponent<Bubble>())
            //{
            //    HasCollided();
            //}
            HasCollided();
        }
        //else if (collision.gameObject.GetComponent<RoofTrigger>() != null)
        //{
        //    HasCollided();
        //}
    }

    public void HasCollided()
    {
        if (rb != null)
        {
            Destroy(rb);
        }
    }
}