using UnityEngine;
using ImbroglioCombat.Core;

namespace ImbroglioCombat.Units
{
    public class PlayerUnit : Unit
    {
        [Header("Player Specific")]
        public int experience = 0;
        public int level = 1;

        protected override void Awake()
        {
            base.Awake();
            
            // Set player-specific stats
            stats = new UnitStats(120, 25, 8, 4, 1);
            unitName = "Player Unit";
        }

        public override void Initialize(HexTile startTile, bool isPlayer)
        {
            base.Initialize(startTile, true);
        }

        public void GainExperience(int amount)
        {
            experience += amount;
            
            // Simple level up system
            int expForNextLevel = level * 100;
            if (experience >= expForNextLevel)
            {
                LevelUp();
            }
        }

        void LevelUp()
        {
            level++;
            experience = 0;

            // Increase stats
            stats.maxHealth += 20;
            stats.currentHealth = stats.maxHealth;
            stats.attack += 5;
            stats.defense += 2;

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(stats.maxHealth);
                healthBar.UpdateHealth(stats.currentHealth);
            }

            Debug.Log($"{unitName} leveled up to {level}!");
        }

        public void UseSpecialAbility()
        {
            // Example special ability: Heal self
            Heal(20);
            SetActed();
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Debug.Log($"{unitName} has fallen!");
        }

        // Player can be selected by clicking
        void OnMouseDown()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.PlayerTurn)
            {
                GameManager.Instance.SelectUnit(this);
            }
        }
    }
}

