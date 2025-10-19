namespace ImbroglioCombatAPI.Models.DTOs
{
    public class GameStateDTO
    {
        public int Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public int CurrentTurn { get; set; }
        public int Score { get; set; }
        public string State { get; set; } = string.Empty;
        public List<UnitStateDTO> Units { get; set; } = new List<UnitStateDTO>();
    }

    public class UnitStateDTO
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public bool IsPlayerUnit { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MoveRange { get; set; }
        public int AttackRange { get; set; }
        public HexPosition Position { get; set; } = new HexPosition();
        public bool HasActed { get; set; }
    }

    public class HexPosition
    {
        public int Q { get; set; }
        public int R { get; set; }
        public int S { get; set; }
    }

    public class MoveRequest
    {
        public int GameStateId { get; set; }
        public int UnitId { get; set; }
        public HexPosition TargetPosition { get; set; } = new HexPosition();
    }

    public class AttackRequest
    {
        public int GameStateId { get; set; }
        public int AttackerUnitId { get; set; }
        public int TargetUnitId { get; set; }
    }

    public class CombatResult
    {
        public bool Success { get; set; }
        public int DamageDealt { get; set; }
        public bool TargetDestroyed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AITurnResult
    {
        public bool Success { get; set; }
        public List<AIAction> Actions { get; set; } = new List<AIAction>();
        public string Message { get; set; } = string.Empty;
    }

    public class AIAction
    {
        public string ActionType { get; set; } = string.Empty; // "Move", "Attack"
        public int UnitId { get; set; }
        public HexPosition? TargetPosition { get; set; }
        public int? TargetUnitId { get; set; }
    }

    public class LeaderboardEntryDTO
    {
        public int Id { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TurnsCompleted { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan CompletionTime { get; set; }
    }

    public class SaveGameRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public GameStateDTO GameState { get; set; } = new GameStateDTO();
    }

    public class LoadGameRequest
    {
        public string PlayerId { get; set; } = string.Empty;
    }
}

