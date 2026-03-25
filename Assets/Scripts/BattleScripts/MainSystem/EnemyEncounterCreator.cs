using UnityEngine;
using System.Collections.Generic;


public static class EnemyEncounterGenerator
{
    public static List<EnemyBattleData> Create(EnemyEncounterTableData table)
    {
        int totalWeight = 0;

        // calc total weight
        foreach (var entry in table.encounters)
            totalWeight += entry.weight;

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        // find encounter based on roll
        foreach (var entry in table.encounters)
        {
            current += entry.weight;

            if (roll < current)
                return new List<EnemyBattleData>(entry.enemies);
        }

        return new List<EnemyBattleData>();
    }
}