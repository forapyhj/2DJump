using UnityEngine;

public class TerraceController : MonoBehaviour
{
	public TerraceConfig config;
	public BoxCollider2D top;
	public BoxCollider2D ground;
	private int _touchCount;
	private float _movingTime;
	void Update()
	{ 
		if(config.isCanMoving) 
		{
			ActivateTerrace(true);	
			_movingTime += Time.deltaTime;
			if(_movingTime >= config.movingTime)
			{
				if(config.moveDirection == Vector2.right)
				{
					config.moveDirection = Vector2.left;
				}
				else if(config.moveDirection == Vector2.left)
				{
					config.moveDirection = Vector2.right;
				}
				else if(config.moveDirection == Vector2.up)
				{
					config.moveDirection = Vector2.down;
				}
				else if(config.moveDirection == Vector2.down)
				{
					config.moveDirection = Vector2.up;
				}
				_movingTime = 0f;
			}
			transform.Translate(config.moveDirection * (config.moveSpeed * Time.deltaTime));
		}
	}
	System.Collections.IEnumerator DisappearTerrace()
    {
		ActivateTerrace(false);
		yield return new WaitForSeconds(config.disappearTime);
		ActivateTerrace(true);
    }
	void ActivateTerrace(bool state)
	{
		top.enabled = state;
		ground.enabled = state;
	}
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if(coll.gameObject.CompareTag("Player"))
		{
			if(_touchCount >= config.touchCount)
			{
				if(config.isCanDestroy) Destroy(gameObject);
				else if(config.isCanDisappear) StartCoroutine(DisappearTerrace());
			}
			else
			{
				PlayerController player = coll.gameObject.GetComponent<PlayerController>();
				if(player.IsGrounded)
				{
					_touchCount++;
				}
			}	
		}
	}
}
