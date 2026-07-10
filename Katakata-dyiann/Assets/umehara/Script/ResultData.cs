using UnityEngine;

public static class ResultData
{
    public static bool isClear = false;
    public static float clearTime = 0f;
    public static int score = 0;
    public static int defeatedEnemyCount = 0;

    public static void Reset()
    {
        isClear = false;
        clearTime = 0f;
        score = 0;
        defeatedEnemyCount = 0;
    }
}