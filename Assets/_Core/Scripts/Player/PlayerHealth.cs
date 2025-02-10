using UnityEngine;
using System.Collections;
public class PlayerHealth: MonoBehaviour
{
    public float invincibleTime = 1f;
	public int maxHealth = 3;
	public AdvancedPlayerController player;
    private float currentHealth;
    private bool isInvincible;

    private void Start()
    {
		player = GetComponent<AdvancedPlayerController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
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
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
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