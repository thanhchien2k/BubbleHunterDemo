
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Bubble : MonoBehaviour
{
    //
    public BubbleColor color;
    public BubbleType type;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float timeMovePerDistance = 0.05f;

    Rigidbody2D rb;

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

    //public void ShootBubble(float shootingForce, float angles)
    //{
    //    IsFixed = false;
    //    circleCollider.enabled = true;
    //    rb = gameObject.AddComponent<Rigidbody2D>();
    //    rb.gravityScale = 0f;
    //    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    //    this.transform.rotation = Quaternion.Euler(0f, 0f, angles - 90f);
    //    rb.AddForce(this.transform.up * shootingForce, ForceMode2D.Impulse);
    //}


    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Bubble") && rb != null )
    //    {

    //        if(collision.transform.GetComponent<Bubble>().IsFixed && !IsFixed)
    //        {
    //            ConnectToMap();
    //        }
    //    }
    //    else if (collision.gameObject.CompareTag("Roof"))
    //    {
    //        ConnectToMap();
    //    }
    //}

    //public Vector3 GetTargetPos(Bubble bubble, Bubble move)
    //{
    //    rb.velocity = Vector2.zero;
    //    Destroy(rb);
    //    return GameManager.Instance.GetTargetPos(bubble, move);
    //}

    public void ConnectToMap()
    {
        IsFixed = true;
        circleCollider.enabled = true;
        GameManager.Instance.SnapToNearestGripPosition(this);
        GameManager.Instance.ProcessTurn(this);
    }

    public List<Bubble> GetNeighbors()
    {
        List<Bubble> neighbors = new List<Bubble>();
        var hits = Physics2D.OverlapCircleAll(transform.position, GameManager.Instance.NeighborDetectionRange);

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
        if (type == BubbleType.Gem)
        {
            GamePlayCanvasControl.Instance.GemCount++;
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
        RandomForce();
    }

    public void RandomForce()
    {
        float randomX = Random.Range(-2f, 2f);
        float randomY = Random.Range(-3f, -1f);

        Vector2 force = new Vector2(randomX, randomY);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void CheckFallingBubble()
    {
        circleCollider.enabled = false;

        if (type == BubbleType.Spark)
        {
            return;
        }
        else if (type == BubbleType.Gem)
        {
            GamePlayCanvasControl.Instance.GemCount++;
        }

        SetUpRigibody();
        StartCoroutine(DelayDestroy());
    }

    public void StratMoveToTarget(List<Vector3> targets)
    {
        int count = targets.Count;
        Vector3 endPoint;
        endPoint = FindPointOnLine(targets[count - 1], targets[count - 2]);
        targets[count - 1] = endPoint;

        for(int i = 1; i< targets.Count - 1; i++)
        {

            Vector3 fixTarget = targets[i];
            if (fixTarget.x > 0)
            {
                fixTarget.x -= GameManager.Instance.CellSize.x / 2;
            }
            else
            {
                fixTarget.x += GameManager.Instance.CellSize.x / 2;
            }

            targets[i] = fixTarget;
        }

        Vector3[] fixtargets = targets.ToArray();
        MoveToTarget(fixtargets);
    }

    Vector3 FindPointOnLine(Vector3 a, Vector3 b)
    {
        float distance = GameManager.Instance.CellSize.x/2;
        Vector3 AB = b - a;

        float AB_length = AB.magnitude;

        Vector3 AB_unit = AB / AB_length;

        Vector3 AP = distance * AB_unit;

        Vector3 P = a + AP;
        return P;
    }

    public void MoveToTarget(Vector3[] targets)
    {
        float time = 0;

        for (int i = 1;i< targets.Length;i++)
        {
          time += Vector3.Distance(targets[i], targets[i-1]) * timeMovePerDistance;
        }

        transform.DOPath(targets, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            ConnectToMap();
        });
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