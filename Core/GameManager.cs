using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState _gameState = GameState.Play;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                if (_gameState != GameState.Paused)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_gameState != GameState.Paused)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
        InputManager.Instance.playerInput.SwitchCurrentActionMap("UI");
        _gameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync("PauseMenu");
        InputManager.Instance.playerInput.SwitchCurrentActionMap("Gameplay");
        _gameState = GameState.Play;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public enum GameState
{
    Play,
    Paused,
    BulletTime,
    Rinnegan
}