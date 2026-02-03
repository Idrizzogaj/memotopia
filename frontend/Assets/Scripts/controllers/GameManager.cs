using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public static List<string> completedAchievements { get; set; } = new List<string>();

    void Awake()
    {
        if (gameManager != null)
            GameObject.Destroy(gameManager);
        else
            gameManager = this;

        DontDestroyOnLoad(this);
    }
}