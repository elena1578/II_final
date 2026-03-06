using UnityEngine;


public static class EnemyBehaviourFactory
{
    public static IBattleEnemyBehaviour Create(CharacterName characterName)
    {
        switch (characterName)
        {
            case CharacterName.KingCrawler:
                return new KingCrawlerBehaviour();

            // future enemies go here
            // case CharacterName.Sweetheart
            //      return new SweetheartBehaviour();

            default:
                return null;
        }
    }
}
