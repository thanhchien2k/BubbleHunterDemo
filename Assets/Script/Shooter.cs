using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Shooter : Singleton<Shooter>
{
    [SerializeField] private LineController lineController;
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Transform nextTransform;
    [SerializeField] private float _allowedShotAngle = 50f;
    [SerializeField] private float _shootingForce = 20f;

    private float _lookAngle;
    private Vector2 curHitPoint;
    private Bubble curBubble;
    private Bubble nextBubble;

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            lineController.gameObject.SetActive(true);
            lineController.UpdatePoint(0, shootTransform.position);
        }

        if (Input.GetMouseButton(0) && lineController != null)
        {
            var lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            this._lookAngle = lookAngle;

            if (Mathf.Abs(lookAngle - 90) > _allowedShotAngle)
            {
                lineController.gameObject.SetActive(false);
            }

            var hit = Physics2D.Raycast(shootTransform.position, lookDirection);

            if (hit.collider != null)
            {
                //if (curHitPoint == hit.point) return;

                curHitPoint = hit.point;
                lineController.UpdatePoint(1, hit.point);

                if (hit.collider.CompareTag("Wall"))
                {
                    Vector2 direct = Vector2.Reflect(lookDirection, hit.normal);
                    Vector2 pos = direct.normalized + hit.point;
                    hit = Physics2D.Raycast(pos, direct);

                    lineController.UpdatePoint(2, hit.point);
                    
                }
            }
        }



        if (Input.GetMouseButtonUp(0) && curBubble != null && lineController.isActiveAndEnabled)
        {
            lineController.gameObject.SetActive(false);

            if (GameManager.Instance.IsCanPlay)
            {
                GameManager.Instance.IsCanPlay = false;
                Shoot();
                curBubble = null;
            }

        }
    }

    private void Shoot()
    {
        Debug.Log("shoot");
        curBubble.ShootBubble(_shootingForce, _lookAngle);
        _lookAngle = 90f;
    }

    public void OnBubbleCollided()
    {
        if (GameManager.Instance.ShotsLeft == 0)
        {
            GameManager.Instance.OnEndGame();
        }

        //_ableToShoot = true;
        //_isSwaping = false;

        if (GameManager.Instance.ShotsLeft > 0 && GameManager.Instance.BubblesLeft > 0)
        {
            curBubble = nextBubble;
            curBubble.transform.SetParent(shootTransform);

            MoveToTarget(curBubble.transform, shootTransform.position, 0.2f, ()=>
            {
                if (GameManager.Instance.ShotsLeft > 1 && GameManager.Instance.BubblesLeft > 0)
                {
                    CheckCurrentBubbleColor();
                    SpawnBubble();
                    GameManager.Instance.IsCanPlay = true;
                }
            });

            nextBubble = null;

        }


    }

    public void SpawnBubble()
    {
        List<BubbleColor> colors = GameManager.Instance.colorsInScene;
        if(colors == null ) return;

        if(nextBubble == null)
        {
            nextBubble = CreateNewBubble(colors);
        }

        if(curBubble == null)
        {
            curBubble = nextBubble;
            curBubble.transform.SetParent(shootTransform);
            curBubble.transform.position = shootTransform.position;

            nextBubble = CreateNewBubble(colors);
        }

        nextBubble.transform.SetParent(nextTransform);
        nextBubble.transform.position = nextTransform.position;
    }

    public Bubble CreateNewBubble(List<BubbleColor> colors)
    {
        Bubble bubble = Instantiate(GameConfig.Bubble);
        bubble.SetupBubble(GetBubbleInfo(colors[Random.Range(0, colors.Count)], BubbleType.Normal));
        bubble.circleCollider.enabled = false;
        Debug.Log(bubble.color);
        return bubble;
    }

    public BubbleInfo GetBubbleInfo(BubbleColor color , BubbleType type)
    {
        return GameConfig.BubbleInfos.Find(item => item.Color == color && item.Type == type);
    }

    private void MoveToTarget(Transform transform, Vector3 target, float time, Action OnCompleted = null)
    {
        transform.DOMove(target, time).SetEase(Ease.InOutSine).OnComplete(()=> OnCompleted?.Invoke());
    }

    private void CheckCurrentBubbleColor()
    {
        List<BubbleColor> colors = GameManager.Instance.colorsInScene;

        if (!colors.Contains(curBubble.color))
        {
            Debug.Log("chedck");
            Destroy(curBubble);
            curBubble = CreateNewBubble(colors);
            curBubble.transform.position = shootTransform.position;
        }
    }
}