using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class KingCrawlerBehaviour : IBattleEnemyBehaviour
{  
    public IEnumerator OnEndOfRound(BattleManager manager, BattleContext context, EnemyBattleActor king)
    {
        // check if one or less sprout moles exist
        int moleCount = context.enemies.FindAll(e =>
            e.isAlive &&
            e is EnemyBattleActor eb &&
            eb.enemyData.characterName == CharacterName.LostSproutMole).Count;
        bool moleExists = moleCount > 0;

        if (!moleExists)
        {
            // spawn mole
            EnemyBattleData moleData = king.enemyData.enemyToSpawn;
            EnemyBattleActor newMole = new EnemyBattleActor(moleData);
            context.enemies.Add(newMole);  // add to context so it can be targeted
            manager.RebuildTurnOrder();  // rebuild turn order to include new mole

            // update UIs to show new mole
            EnemySpawnPosition openPosition = manager.uiManager.IsPositionOccupied(EnemySpawnPosition.Left) ? EnemySpawnPosition.Right 
                : EnemySpawnPosition.Left;  // if left is occupied, go to right instead
            manager.uiManager.AddEnemyUI(newMole, openPosition);
            manager.uiManager.UpdateAll();

            BattleDialogManager.instance.Show("King Crawler calls to Lost Sprout Mole!");

            while (BattleDialogManager.instance.typing)
                yield return null;

            yield return new WaitForSeconds(manager.postDialogDelay);
        }
        else
        {
            // consume mole if exists
            EnemyBattleActor mole = (EnemyBattleActor)
                context.enemies.Find(e =>
                    e.isAlive &&
                    e is EnemyBattleActor eb &&
                    eb.enemyData.characterName == CharacterName.LostSproutMole);

            if (mole != null)
            {
                BattleActionData consume = king.enemyData.GetRandomHealAction();  // only one heal action available so random is fine
                BattleActionResult result = manager.UseAction(king, consume, new List<BattleActor> { king });  // target self with heal action

                // remove mole 
                mole.TakeDamage(mole.currentHP);
                manager.RemoveActorFromPlans(mole);  // if mole was planned to act later in the round, remove it from planned actions to avoid acting after death

                if (mole.ui != null)  // play toast animation
                {
                    mole.ui.SetToastAnimation();
                    mole.ui.SlideOffScreen();
                    yield return new WaitForSeconds(1f);  // wait for animation to finish before removing from UI
                    manager.uiManager.RemoveEnemyUI(mole);
                }

                context.enemies.Remove(mole);  // remove from context and immediately rebuild turn order
                manager.RebuildTurnOrder();

                BattleDialogManager.instance.Show(consume, result);  // show heal dialog

                while (BattleDialogManager.instance.typing)
                    yield return null;
                yield return new WaitForSeconds(5f);  
            }
        }

        yield return new WaitForSeconds(manager.postDialogDelay);
    }
}
