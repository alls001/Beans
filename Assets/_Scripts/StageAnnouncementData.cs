using UnityEngine;

[CreateAssetMenu(fileName = "StageAnnouncementData", menuName = "Game/Stage Announcement Data")]
public class StageAnnouncementData : ScriptableObject
{
    [Header("Level Intro")]
    public string levelMessage = "Defeat all enemies to keep the beanstalk growing.";

    [Header("Wave Intro")]
    public string waveMessage = "Get ready for the next wave of monsters.";

    [Header("Final Objective")]
    public string finalMessage = "Climb the beanstalk.";
}