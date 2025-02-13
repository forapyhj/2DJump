using UnityEngine;
using System.Collections;
public class PlayerHealth: MonoBehaviour
{
    public float invincibleTime = 1f;
	public int maxHealth = 3;
	public PlayerController player;
    private float _currentHealth;
    private bool _isInvincible;

    private void Start()
    {
		player = GetComponent<PlayerController>();
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_isInvincible) return;

        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            StartCoroutine(Death());
        }
        else
        {
			player.PlayAnimator("Hurt");
            StartCoroutine(InvincibilityFrame());
        }
    }
    IEnumerator InvincibilityFrame()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        _isInvincible = false;
    }
	
	IEnumerator Death()
    {
        // 玩家死亡逻辑
        player.PlayAnimator("Dying");
        yield return new WaitForSeconds(1.5f);
        // 示例：重新加载场景
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // 禁用玩家对象
        gameObject.SetActive(false);
    }
}