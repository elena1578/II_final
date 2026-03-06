using UnityEngine;
using System.Collections;


public interface IBattleEnemyBehaviour
{
    // can also add start of round events here 
    IEnumerator OnEndOfRound(BattleManager manager, BattleContext context, EnemyBattleActor self);
}
