using UnityEngine;

[CreateAssetMenu(fileName = "NewLaserConfig", menuName = "Laser Config", order = 51)]
public class LaserConfig : ScriptableObject
{
    [Header("Laser Settings")]
    public Color laserColor = Color.red; // 激光颜色
	public Color warningColor = Color.yellow; //警示颜色
    public float damage = 1f;           // 激光伤害
    public float duration = 2f;         // 激光持续时间
    public float interval = 1f;         // 激光间隔时间
    public float width = 0.1f;          // 激光宽度
    public Material laserMaterial;      // 激光材质
	public bool isMoving = false;		// 是否可移动
	public float moveSpeed = 2f;		// 移动速度
	public Vector2 _moveDirection = Vector2.right; //移动方向
	public float maxMoveTime;			// 移动时间
}