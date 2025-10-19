using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ImbroglioCombat.Core;

namespace ImbroglioCombat.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainUIPanel;
        public GameObject gameOverPanel;
        public GameObject victoryPanel;

        [Header("Text Elements")]
        public TextMeshProUGUI turnText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI gameStateText;
        public TextMeshProUGUI gameOverText;

        [Header("Buttons")]
        public Button endTurnButton;
        public Button undoButton;
        public Button restartButton;
        public Button menuButton;

        void Start()
        {
            SetupButtons();
            HideGameOverScreens();
        }

        void SetupButtons()
        {
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            if (undoButton != null)
            {
                undoButton.onClick.AddListener(OnUndoClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        public void UpdateTurnText(int turn, GameState state)
        {
            if (turnText != null)
            {
                turnText.text = $"Turn: {turn}";
            }

            if (gameStateText != null)
            {
                switch (state)
                {
                    case GameState.PlayerTurn:
                        gameStateText.text = "Your Turn";
                        gameStateText.color = Color.green;
                        break;
                    case GameState.EnemyTurn:
                        gameStateText.text = "Enemy Turn";
                        gameStateText.color = Color.red;
                        break;
                    case GameState.Processing:
                        gameStateText.text = "Processing...";
                        gameStateText.color = Color.yellow;
                        break;
                    case GameState.GameOver:
                        gameStateText.text = "Game Over";
                        gameStateText.color = Color.red;
                        break;
                    case GameState.Victory:
                        gameStateText.text = "Victory!";
                        gameStateText.color = Color.gold;
                        break;
                }
            }

            // Enable/disable buttons based on state
            if (endTurnButton != null)
            {
                endTurnButton.interactable = (state == GameState.PlayerTurn);
            }

            if (undoButton != null)
            {
                undoButton.interactable = (state == GameState.PlayerTurn);
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameOver(bool isVictory)
        {
            HideGameOverScreens();

            if (isVictory && victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                if (gameOverText != null)
                {
                    gameOverText.text = $"Victory!\nFinal Score: {GameManager.Instance?.score ?? 0}";
                }
            }
            else if (!isVictory && gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverText != null)
                {
                    gameOverText.text = $"Defeat!\nFinal Score: {GameManager.Instance?.score ?? 0}";
                }
            }
        }

        void HideGameOverScreens()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }

        void OnEndTurnClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPlayerTurn();
            }
        }

        void OnUndoClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UndoLastMove();
            }
        }

        void OnRestartClicked()
        {
            HideGameOverScreens();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        void OnMenuClicked()
        {
            // Load main menu scene or show menu panel
            Debug.Log("Menu clicked - implement scene loading");
        }

        public void ShowNotification(string message, float duration = 2f)
        {
            // You can implement a notification system here
            Debug.Log($"Notification: {message}");
        }
    }
}

