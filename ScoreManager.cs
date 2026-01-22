using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    private int lastMilestone = 0;

    void Awake() { instance = this; }

    public void AddPoints(int amount)
    {
        score += amount;
        if (scoreText != null) scoreText.text = "Score: " + score;

        if (score < 180 && (score / 30 > lastMilestone))
        {
            lastMilestone = score / 30;
            FindObjectOfType<Player>()?.UnlockNextEnemy();
        }

        if (score >= 180)
        {
            FindObjectOfType<Player>()?.StartBossBattle();
        }
    }
}