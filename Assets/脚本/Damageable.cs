//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(Rigidbody2D))]
//public class Damageable : MonoBehaviour
//{
//    [Header("基础属性")]
//    [SerializeField] private int maxHealth = 3;
//    [SerializeField] private float knockbackForce = 5f;
//    [SerializeField] private float knockbackDuration = 0.2f;

//    [Header("事件回调")]
//    public System.Action OnTakeDamage; // 受击回调
//    public System.Action OnDeath;      // 死亡回调

//    private Rigidbody2D rb;
//    private int currentHealth;
//    private bool isKnockbacking; // 击退中标记
//    private EnemyAI enemyAI;     // 仅怪物使用

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        currentHealth = maxHealth;
//        enemyAI = GetComponent<EnemyAI>(); // 怪物AI引用
//    }

//    // 对外暴露的受击方法
//    public void TakeDamage(int damage, Vector2 knockbackDirection)
//    {
//        // 击退中/已死亡 不响应伤害
//        if (isKnockbacking || currentHealth <= 0) return;

//        currentHealth -= damage;
//        Debug.Log($"{gameObject.name} 受到{damage}伤害，剩余生命值：{currentHealth}");

//        // 触发受击回调
//        OnTakeDamage?.Invoke();

//        // 怪物受击时暂停AI
//        if (enemyAI != null)
//        {
//            enemyAI.PauseAI(true);
//        }

//        // 死亡判定
//        if (currentHealth <= 0)
//        {
//            Die();
//            return;
//        }

//        // 触发击退
//        StartCoroutine(KnockbackCoroutine(knockbackDirection));
//    }

//    // 击退逻辑
//    private IEnumerator KnockbackCoroutine(Vector2 direction)
//    {
//        isKnockbacking = true;
//        rb.velocity = Vector2.zero;
//        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

//        yield return new WaitForSeconds(knockbackDuration);

//        isKnockbacking = false;
//        rb.velocity = Vector2.zero;

//        // 恢复怪物AI
//        if (enemyAI != null)
//        {
//            enemyAI.PauseAI(false);
//        }
//    }

//    // 死亡逻辑
//    private void Die()
//    {
//        Debug.Log($"{gameObject.name} 死亡");
//        OnDeath?.Invoke();

//        // 玩家/怪物死亡处理（可扩展）
//        if (enemyAI != null)
//        {
//            enemyAI.enabled = false;
//        }
//        else
//        {
//            // 玩家死亡逻辑（示例：暂停游戏/返回菜单）
//            Time.timeScale = 0;
//        }

//        Destroy(gameObject, 0.5f); // 延迟销毁
//    }

//    // 外部获取生命值
//    public int GetCurrentHealth() => currentHealth;
//    public bool IsDead() => currentHealth <= 0;
//}