using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Shooter : Singleton<Shooter>
{
    [SerializeField] private LineController lineController;
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Transform nextTransform;
    [SerializeField] private Transform mesh;
    [SerializeField] private TextMeshPro shotsLeftText;
    [SerializeField] private float allowedShotAngle = 50f;

    private Bubble curBubble;
    private Bubble nextBubble;
    private bool ableShoot = true; 
    private List<Vector3> shootPos;
    private Vector3 shooterLocalScale;
    private Vector3 nextLocalScale;
    private int ReflectCount;
    private LevelInfo levelInfo;
    int LayerMark;
    public bool IsChange { get; set; } = false;

    private int shotsLeft;
    public int ShotsLeft
    {
        get { return shotsLeft; }

        set
        {
            shotsLeft = value;
            UpdateShotsLeftText();
        }
    }

    private void Start()
    {
        levelInfo = GameManager.Instance.levelInfo;
        ShotsLeft = levelInfo.shootCount;
        shooterLocalScale = shootTransform.localPosition;
        nextLocalScale = nextTransform.localPosition;  
        shootPos = new List<Vector3>();
        mesh.gameObject.SetActive(false);
        LayerMark = LayerMask.GetMask("Mesh");

        LayerMark = ~LayerMark;
    }
    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if(CameraController.Instance.CameraIsMoving || GameManager.Instance.IsEndGame || IsChange) return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && ableShoot)
        {
            lineController.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0) && lineController.gameObject.activeSelf)
        {
            Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(lookAngle - 90) > allowedShotAngle)
            {
                lineController.gameObject.SetActive(false);
            }



            shootPos.Clear();
            ReflectCount = 0;
            bool LoockActive = true;
            var curHitPoint = shootTransform.position;
            lineController.UpdatePoint(0, curHitPoint);
            shootPos.Add(curHitPoint);
            Vector2 fixPoint = Vector2.zero;

            while (LoockActive)
            {
                var hit = Physics2D.Raycast(curHitPoint, lookDirection, Mathf.Infinity, LayerMark);

                if (hit && hit.collider.CompareTag("Wall"))
                {
                    //if (!GameManager.Instance.IsableReflect(hit.point))
                    //{
                    //    ReflectCount++;
                    //    lineController.UpdatePoint(ReflectCount, hit.point);
                    //    shootPos.Add(hit.point);
                    //    break;
                    //}

                    lookDirection = Vector2.Reflect(lookDirection, hit.normal);
                    curHitPoint = lookDirection.normalized / 10 + hit.point;

                    if(ReflectCount < 3)
                    {
                        ReflectCount++;
                        lineController.UpdatePoint(ReflectCount, hit.point);
                    }
                    shootPos.Add(hit.point);
                }
                else if(hit && (hit.collider.CompareTag("Bubble") || hit.collider.CompareTag("Roof")))
                {
                    if(ReflectCount < 3)
                    {
                        ReflectCount++;
                        lineController.UpdatePoint(ReflectCount, hit.point);
                    }
                    shootPos.Add(hit.point);
                    LoockActive = false;
                }
                else if (!hit)
                {
                    shootPos.Clear();
                    LoockActive = false;
                }

            }
            Debug.DrawLine(shootPos[0], shootPos[1]);
        }

        if (Input.GetMouseButtonUp(0) && curBubble != null && lineController.gameObject.activeSelf)
        {
            lineController.gameObject.SetActive(false);

            if (GameManager.Instance.IsCanPlay && shootPos.Count > 1)
            {
                GameManager.Instance.IsCanPlay = false;
                Shoot();
                curBubble = null;
            }
        }
    }

    private void Shoot()
    {
        ableShoot = false;
        curBubble.StratMoveToTarget(shootPos);
    }

    public void OnBubbleCollided()
    {
        if (!mesh.gameObject.activeSelf)
        {
            mesh.gameObject.SetActive(true);
        }

        if (nextBubble == null)
        {
            SpawnBubble();
            GameManager.Instance.IsCanPlay = true;
            return;
        }


        if (ShotsLeft == 0)
        {
            GameManager.Instance.OnEndGame();
        }

        
        if (ShotsLeft > 0 && GameManager.Instance.BubblesLeft > 0)
        {
            curBubble = nextBubble;
            curBubble.transform.SetParent(mesh);
            MoveToTarget(curBubble.transform, shootTransform.position, 0.2f, ()=>
            {
                if (ShotsLeft > 1 && GameManager.Instance.BubblesLeft > 0)
                {
                    CheckCurrentBubbleColor();
                    SpawnBubble();
                    ShotsLeft--;
                    ableShoot = true;
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
            if (!colors.Contains(nextBubble.color) && colors.Count > 0)
            {
                Destroy(nextBubble);
                SpawnBubble();
            }
        }

        if(curBubble == null)
        {
            curBubble = nextBubble;
            curBubble.transform.SetParent(mesh);
            curBubble.transform.position = shootTransform.position;

            nextBubble = CreateNewBubble(colors);
        }

        nextBubble.transform.SetParent(mesh);
        nextBubble.transform.position = nextTransform.position;
    }

    public Bubble CreateNewBubble(List<BubbleColor> colors)
    {
        Bubble bubble = Instantiate(GameConfig.Bubble);
        bubble.SetupBubble(GetBubbleInfo(colors[Random.Range(0, colors.Count)], BubbleType.Normal));
        bubble.circleCollider.enabled = false;
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
            Destroy(curBubble.gameObject);
            curBubble = CreateNewBubble(colors);
            curBubble.transform.position = shootTransform.position;
            curBubble.transform.SetParent(mesh);
        }
    }

    public void ChangeBubble(float time)
    {
        Bubble temp = nextBubble;
        nextBubble = curBubble;
        curBubble = temp;
        curBubble.transform.DOLocalMove(shooterLocalScale , time).SetEase(Ease.Linear);
        nextBubble.transform.DOLocalMove(nextLocalScale , time).SetEase(Ease.Linear).OnComplete(()=> IsChange = false);
    }

    private void UpdateShotsLeftText()
    {
        shotsLeftText.text = shotsLeft.ToString();
    }
}