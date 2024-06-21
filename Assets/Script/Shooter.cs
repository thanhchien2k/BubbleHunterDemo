using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Shooter : Singleton<Shooter>
{
    [SerializeField] private LineController lineController;
    [SerializeField] private Transform currentTransform;
    [SerializeField] private Transform nextTransform;
    [SerializeField] private float _allowedShotAngle = 50f;
    private float _lookAngle;
    private Vector2 curHitPoint;
    [SerializeField] private float _shootingForce = 20f;
    [SerializeField] private Bubble curBubble;

    private void Start()
    {
        curHitPoint = currentTransform.position;
        SpawnBubble();
    }
    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && curBubble != null)
        {
            lineController.gameObject.SetActive(true);
            lineController.SetUpLine(currentTransform);
        }

        if (Input.GetMouseButton(0) && lineController != null && lineController.isActiveAndEnabled)
        {
            var lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            this._lookAngle = lookAngle;

            if (Mathf.Abs(lookAngle - 90) > _allowedShotAngle)
            {
                lineController.gameObject.SetActive(false);
            }

            var hit = Physics2D.Raycast(currentTransform.position, lookDirection);

            if (hit.collider != null)
            {
                //if (curHitPoint == hit.point) return;

                curHitPoint = hit.point;
                lineController.UpdatePoint(hit.point);

                //if (hit.collider.CompareTag("Wall"))
                //{
                //    Debug.Log("wall");
                //    Vector2 direct = curHitPoint - (Vector2)currentTransform.position;
                //    var hit2 = Physics2D.Raycast(curHitPoint, 2 * Mathf.Cos(90 - lookAngle) * direct );

                //    if (hit.collider != null)
                //    {
                //        lineController.UpdateNewPoint(hit2.point);
                //    }
                //}
            }
        }

        if (Input.GetMouseButtonUp(0) && curBubble != null && lineController.isActiveAndEnabled)
        {
            Shoot();
            lineController.gameObject.SetActive(false);
        }
    }

    private void Shoot()
    {
        curBubble.ShootBubble(_shootingForce, _lookAngle);
        _lookAngle = 90f;
    }

    public void SpawnBubble()
    {
        curBubble = Instantiate(GameConfig.Bubble, currentTransform);
        int maxIndex = GameConfig.BubbleInfos.Count;

        curBubble.SetupBubble(GameConfig.BubbleInfos[Random.Range(0, maxIndex)], true);
    }
}