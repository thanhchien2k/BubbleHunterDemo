
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bubble : MonoBehaviour
{
    [SerializeField] BubbleType type;
    public BubbleColor color;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rb;
    public CircleCollider2D circleCollider;

    public bool IsConnected { get; set; }
    public bool IsFixed { get; set; }
    private bool animStarted;

    private void Awake()
    {
        IsConnected = true;
        IsFixed = true;
        animStarted = false;

    }

    public void ShootBubble(float shootingForce, float angles)
    {
        IsFixed = false;
        circleCollider.enabled = true;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angles - 90f);
        rb.AddForce(this.transform.up * shootingForce, ForceMode2D.Impulse);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble") && rb != null )
        {

            if(collision.transform.GetComponent<Bubble>().IsFixed && !IsFixed)
            {
               //transform.position = GetTargetPos(collision.transform.GetComponent<Bubble>(), this);
                ConnectToMap(this);
            }
        }
        else if (collision.gameObject.CompareTag("Roof"))
        {
            ConnectToMap(this);
        }
    }

    //public Vector3 GetTargetPos(Bubble bubble, Bubble move)
    //{
    //    rb.velocity = Vector2.zero;
    //    Destroy(rb);
    //    return GameManager.Instance.GetTargetPos(bubble, move);
    //}

    public void ConnectToMap(Bubble bubble)
    {
        rb.velocity = Vector2.zero;
        Destroy(rb);
        rb = null;
        IsFixed = true;
        GameManager.Instance.SnapToNearestGripPosition(bubble);
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

    public void OnRecycle() 
    {
        if (rb != null) Debug.Log("rb != null");
        circleCollider.enabled = false;

        if (this.type == BubbleType.Gem)
        {

        }

        Destroy(gameObject);
    }

    public void SetupBubble(BubbleInfo info)
    {
        spriteRenderer.sprite = info.Sprite;
        type = info.Type;
        color = info.Color;
    }

    public void SetUpRigibody()
    {
        if(rb != null) return;
        rb = gameObject.AddComponent<Rigidbody2D>();

    }

    public void DestroyFallingBubble()
    {
        if(this.type == BubbleType.Gem)
        {

        }

        StartCoroutine(DelayDestroy());
    }

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(GameConfig.BubbleLifeTime);
        Destroy(gameObject);
    }

    public void PlayHitAnim(Vector3 pos)
    {
        if(rb != null || animStarted) return;
        StartCoroutine(PlayHitAnimCor(pos, 0.05f));
    }
    private IEnumerator PlayHitAnimCor(Vector3 newBallPos, float force)
    {
        animStarted = true;
        //if (tag == "chicken") yield break;
        yield return new WaitForFixedUpdate();
        if (rb != null) yield break;
        float dist = Vector3.Distance(transform.position, newBallPos);
        force = 1 / dist + force;
        newBallPos = transform.position - newBallPos;
        if (transform.parent == null)
        {
            animStarted = false;
            yield break;
        }
        newBallPos = Quaternion.AngleAxis(transform.parent.parent.rotation.eulerAngles.z, Vector3.back) * newBallPos;
        newBallPos = newBallPos.normalized;
        newBallPos = transform.localPosition + (newBallPos * force / 10);

        float startTime = Time.time;
        Vector3 startPos = transform.localPosition;
        float speed = force * 5;
        float distCovered = 0;
        while (distCovered < 1 && !float.IsNaN(newBallPos.x))
        {
            distCovered = (Time.time - startTime) * speed;
            if (this == null) yield break;
            //   if( destroyed ) yield break;
            //if (falling)
            //{
            //    //           transform.localPosition = startPos;
            //    yield break;
            //}
            transform.localPosition = Vector3.Lerp(startPos, newBallPos, distCovered);
            yield return new WaitForEndOfFrame();
        }
        Vector3 lastPos = transform.localPosition;
        startTime = Time.time;
        distCovered = 0;
        while (distCovered < 1 && !float.IsNaN(newBallPos.x))
        {
            distCovered = (Time.time - startTime) * speed;
            if (this == null) yield break;
            //if (falling)
            //{
            //    //      transform.localPosition = startPos;
            //    yield break;
            //}
            transform.localPosition = Vector3.Lerp(lastPos, startPos, distCovered);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = startPos;
        animStarted = false;
    }


}