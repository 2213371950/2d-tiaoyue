//using UnityEngine;

//public class PlayerAttack : MonoBehaviour
//{
//    [SerializeField] private int attackDamage = 1;
//    [SerializeField] private LayerMask enemyLayer;

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        // 仅检测敌人层级
//        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

//        // 获取怪物的伤害组件
//        Damageable damageable = other.GetComponent<Damageable>();
//        if (damageable != null)
//        {
//            // 计算击退方向（从玩家指向怪物）
//            Vector2 knockbackDir = (other.transform.position - transform.parent.position).normalized;
//            damageable.TakeDamage(attackDamage, knockbackDir);
//            Debug.Log($"玩家击中{other.name}，造成{attackDamage}点伤害");
//        }
//    }
//}