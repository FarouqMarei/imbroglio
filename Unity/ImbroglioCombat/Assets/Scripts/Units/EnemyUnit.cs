using UnityEngine;
using ImbroglioCombat.Core;

namespace ImbroglioCombat.Units
{
    public class EnemyUnit : Unit
    {
        [Header("Enemy Specific")]
        public EnemyAIType aiType = EnemyAIType.Aggressive;

        protected override void Awake()
        {
            base.Awake();
            
            // Set enemy-specific stats
            stats = new UnitStats(80, 18, 5, 3, 1);
            unitName = "Enemy Unit";
        }

        public override void Initialize(HexTile startTile, bool isPlayer)
        {
            base.Initialize(startTile, false);
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Debug.Log($"{unitName} has been defeated!");
        }

        // Enemy can be clicked to attack if player unit is selected and in range
        void OnMouseDown()
        {
            if (GameManager.Instance != null && 
                GameManager.Instance.currentState == GameState.PlayerTurn &&
                GameManager.Instance.selectedUnit != null)
            {
                Unit selectedUnit = GameManager.Instance.selectedUnit;
                if (selectedUnit.isPlayerUnit)
                {
                    GameManager.Instance.TryAttackUnit(selectedUnit, this);
                }
            }
        }
    }

    public enum EnemyAIType
    {
        Aggressive,  // Always moves towards and attacks player
        Defensive,   // Stays near starting position
        Balanced     // Mix of both
    }
}

