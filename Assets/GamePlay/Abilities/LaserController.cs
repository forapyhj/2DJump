using UnityEngine;

public class LaserController : MonoBehaviour
{
	public LaserConfig config; // 引用 LaserConfig
	
    [Header("References")]
    public LineRenderer lineRenderer;
    public BoxCollider2D laserCollider;
    public Transform startPoint;
    public Transform endPoint;

    private bool isActive = false;

    void Start()
    {
        InitializeLaser();
		            
		if(config.isMoving) ActivateLaser(true);
		else StartCoroutine(LaserCycle());
    }
	private float _moveTimer;
	void Update()
	{
		if (config.isMoving)
		{
			// 让激光端点移动
			_moveTimer += Time.deltaTime;
			if(_moveTimer > config.maxMoveTime) 
			{
				if(config._moveDirection == Vector2.right)
				{
					config._moveDirection = Vector2.left;
				}
				else if(config._moveDirection == Vector2.left)
				{
					config._moveDirection = Vector2.right;
				}
				else if(config._moveDirection == Vector2.up)
				{
					config._moveDirection = Vector2.down;
				}
				else if(config._moveDirection == Vector2.down)
				{
					config._moveDirection = Vector2.up;
				}
				_moveTimer = 0f;
			}				
			//startPoint.Translate(config._moveDirection * config.moveSpeed * Time.deltaTime);
			//endPoint.Translate(config._moveDirection * config.moveSpeed * Time.deltaTime);
			transform.Translate(config._moveDirection * config.moveSpeed * Time.deltaTime);
			UpdateLaserPositions();
		}
	}
	void UpdateLaserPositions()
	{
		lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
	}
    void InitializeLaser()
    {
		// 设置 LineRenderer 参数
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = config.width;
        lineRenderer.endWidth = config.width;
        lineRenderer.material = config.laserMaterial;
        lineRenderer.startColor = config.laserColor;
        lineRenderer.endColor = config.laserColor;

		// 初始化碰撞体大小和位置
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        float length = Vector3.Distance(startPos, endPos);
        if(config._moveDirection.x == 0) laserCollider.size = new Vector2(length, config.width);
		else if(config._moveDirection.y ==0) laserCollider.size = new Vector2(config.width, length);
        laserCollider.transform.position = (startPos + endPos) / 2;
		//laserCollider.transform.right = (endPos - startPos).normalized;
    }

    System.Collections.IEnumerator LaserCycle()
    {
        while (true)
        {
            // 警告阶段（闪烁）
            yield return StartCoroutine(WarningEffect());

            // 发射激光
            ActivateLaser(true);
            yield return new WaitForSeconds(config.duration);

            // 关闭激光
            ActivateLaser(false);
            yield return new WaitForSeconds(config.interval);
        }
    }

    System.Collections.IEnumerator WarningEffect()
    {
        float warningTime = 1f;
        float blinkInterval = 0.2f;
        int blinkCount = Mathf.FloorToInt(warningTime / (blinkInterval * 2));

        for (int i = 0; i < blinkCount; i++)
        {
            lineRenderer.material.color = config.warningColor;
            yield return new WaitForSeconds(blinkInterval);
            lineRenderer.material.color = Color.clear;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void ActivateLaser(bool state)
    {
        isActive = state;
        lineRenderer.enabled = state;
        laserCollider.enabled = state;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(config.damage);
        }
    }
}