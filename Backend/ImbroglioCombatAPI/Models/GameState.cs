using System.ComponentModel.DataAnnotations;

namespace ImbroglioCombatAPI.Models
{
    public class GameState
    {
        [Key]
        public int Id { get; set; }
        
        public string PlayerId { get; set; } = "Player1";
        
        public int CurrentTurn { get; set; }
        
        public int Score { get; set; }
        
        public string State { get; set; } = "PlayerTurn"; // PlayerTurn, EnemyTurn, GameOver, Victory
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public string? SerializedGameData { get; set; } // JSON data for full game state
        
        public List<UnitState> Units { get; set; } = new List<UnitState>();
    }

    public class UnitState
    {
        [Key]
        public int Id { get; set; }
        
        public int GameStateId { get; set; }
        
        public string UnitName { get; set; } = string.Empty;
        
        public bool IsPlayerUnit { get; set; }
        
        public int CurrentHealth { get; set; }
        
        public int MaxHealth { get; set; }
        
        public int Attack { get; set; }
        
        public int Defense { get; set; }
        
        public int MoveRange { get; set; }
        
        public int AttackRange { get; set; }
        
        public int PositionQ { get; set; }
        
        public int PositionR { get; set; }
        
        public int PositionS { get; set; }
        
        public bool HasActed { get; set; }
    }

    public class LeaderboardEntry
    {
        [Key]
        public int Id { get; set; }
        
        public string PlayerName { get; set; } = "Anonymous";
        
        public int Score { get; set; }
        
        public int TurnsCompleted { get; set; }
        
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        public TimeSpan CompletionTime { get; set; }
    }
}

