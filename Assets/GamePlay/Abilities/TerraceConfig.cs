using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewTerraceConfig", menuName = "Terrace Config", order = 52)]
public class TerraceConfig : ScriptableObject
{
    [Header("Terrace Settings")]
	public bool isCanDestroy; //销毁 
	public bool isCanDisappear; //消失
    public int touchCount = 1; //触碰多少次被销毁
	public float disappearTime = 1f; //消失时间
	
	public bool isCanMoving; //是否可以移动
	public float moveSpeed = 1f;
	public float movingTime = 3f;//移动时间
	public Vector2 moveDirection = Vector2.zero; //移动方向
	
}