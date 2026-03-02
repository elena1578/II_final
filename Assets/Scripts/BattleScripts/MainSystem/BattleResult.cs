using UnityEngine;
using System.Collections.Generic;

public class BattleResult
{
    // ref'd in BattleManager when battle ends
    public bool win;
    public int expEarned;
    public int clamsEarned;
    public List<BattleActor> defeatedEnemies;

    public BattleResult()
    {
        defeatedEnemies = new List<BattleActor>();
    }

    public BattleResult(bool win)
    {
        this.win = win;
        defeatedEnemies = new List<BattleActor>();
    }
}
