using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(BattlePartyDataManager))]
public class BattlePartyDataManagerEditor : Editor
{
    private BattlePartyDataManager manager;

    private void OnEnable()
    {
        manager = (BattlePartyDataManager)target;

        // make sure lists exist for editor sliders
        if (manager.initialHP == null) manager.initialHP = new List<int>();
        if (manager.initialJuice == null) manager.initialJuice = new List<int>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime HP & Juice Debug", EditorStyles.boldLabel);

        // ensure debug lists match party size
        while (manager.initialHP.Count < manager.partyMembers.Count) manager.initialHP.Add(0);
        while (manager.initialJuice.Count < manager.partyMembers.Count) manager.initialJuice.Add(0);

        for (int i = 0; i < manager.partyMembers.Count; i++)
        {
            var member = manager.partyMembers[i];
            if (member == null) continue;

            EditorGUILayout.BeginVertical("box");  // each character has a box w/ this!
            EditorGUILayout.LabelField(member.characterName.ToString(), EditorStyles.boldLabel);

            // HP slider
            manager.initialHP[i] = EditorGUILayout.IntSlider(
                "HP",
                manager.initialHP[i] > 0 ? manager.initialHP[i] : member.baseHP,  // default to base HP if no debug val set
                0,  // min is 0
                member.baseHP
            );

            // juice slider
            manager.initialJuice[i] = EditorGUILayout.IntSlider(
                "Juice",
                manager.initialJuice[i] > 0 ? manager.initialJuice[i] : member.baseJuice,
                0,  // min is 0
                member.baseJuice
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // update changes immediately
        if (GUI.changed)
        {
            manager.InitializeRuntimeValues();
            EditorUtility.SetDirty(manager);
        }
    }
}