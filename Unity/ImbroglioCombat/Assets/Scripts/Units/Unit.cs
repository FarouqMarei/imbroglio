using UnityEngine;
using System.Collections;
using ImbroglioCombat.Core;

namespace ImbroglioCombat.Units
{
    public abstract class Unit : MonoBehaviour
    {
        [Header("Unit Properties")]
        public string unitName;
        public bool isPlayerUnit;
        public UnitStats stats;

        [Header("Visual")]
        public SpriteRenderer spriteRenderer;
        public HealthBar healthBar;
        public Color playerColor = Color.blue;
        public Color enemyColor = Color.red;

        [Header("Position")]
        public HexTile currentTile;

        [Header("Animation")]
        public float moveSpeed = 5f;
        public float attackAnimDuration = 0.3f;

        protected bool hasActedThisTurn = false;

        protected virtual void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (stats == null)
            {
                stats = new UnitStats();
            }
        }

        public virtual void Initialize(HexTile startTile, bool isPlayer)
        {
            isPlayerUnit = isPlayer;
            currentTile = startTile;
            transform.position = startTile.transform.position;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isPlayer ? playerColor : enemyColor;
            }

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(stats.maxHealth);
                healthBar.UpdateHealth(stats.currentHealth);
            }
        }

        public virtual void MoveTo(HexTile targetTile)
        {
            if (targetTile != null)
            {
                currentTile = targetTile;
                StartCoroutine(MoveAnimation(targetTile.transform.position));
            }
        }

        protected IEnumerator MoveAnimation(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0;
            float duration = Vector3.Distance(startPosition, targetPosition) / moveSpeed;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
        }

        public virtual void TakeDamage(int damage)
        {
            stats.currentHealth = Mathf.Max(0, stats.currentHealth - damage);
            
            if (healthBar != null)
            {
                healthBar.UpdateHealth(stats.currentHealth);
            }

            StartCoroutine(DamageFlash());

            if (stats.currentHealth <= 0)
            {
                OnDeath();
            }
        }

        protected IEnumerator DamageFlash()
        {
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
            }
        }

        public virtual void Heal(int amount)
        {
            stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + amount);
            
            if (healthBar != null)
            {
                healthBar.UpdateHealth(stats.currentHealth);
            }
        }

        public virtual void Attack(Unit target)
        {
            if (target != null)
            {
                StartCoroutine(AttackAnimation(target));
            }
        }

        protected virtual IEnumerator AttackAnimation(Unit target)
        {
            Vector3 originalPosition = transform.position;
            Vector3 targetPosition = target.transform.position;
            Vector3 midPoint = Vector3.Lerp(originalPosition, targetPosition, 0.5f);

            // Move towards target
            float elapsed = 0;
            while (elapsed < attackAnimDuration / 2)
            {
                transform.position = Vector3.Lerp(originalPosition, midPoint, elapsed / (attackAnimDuration / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Deal damage
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AttackUnit(this, target);
            }

            // Return to original position
            elapsed = 0;
            while (elapsed < attackAnimDuration / 2)
            {
                transform.position = Vector3.Lerp(midPoint, originalPosition, elapsed / (attackAnimDuration / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPosition;
        }

        protected virtual void OnDeath()
        {
            // Override in derived classes for specific death behavior
        }

        public virtual void ResetTurnState()
        {
            hasActedThisTurn = false;
        }

        public bool CanAct()
        {
            return !hasActedThisTurn && stats.currentHealth > 0;
        }

        public void SetActed()
        {
            hasActedThisTurn = true;
        }
    }

    [System.Serializable]
    public class UnitStats
    {
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int attack = 20;
        public int defense = 5;
        public int moveRange = 3;
        public int attackRange = 1;

        public UnitStats()
        {
            maxHealth = 100;
            currentHealth = 100;
            attack = 20;
            defense = 5;
            moveRange = 3;
            attackRange = 1;
        }

        public UnitStats(int health, int atk, int def, int moveRng, int atkRng)
        {
            maxHealth = health;
            currentHealth = health;
            attack = atk;
            defense = def;
            moveRange = moveRng;
            attackRange = atkRng;
        }

        public UnitStats Clone()
        {
            return new UnitStats(maxHealth, attack, defense, moveRange, attackRange)
            {
                currentHealth = this.currentHealth
            };
        }
    }
}

