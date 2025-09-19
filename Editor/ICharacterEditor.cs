using UnityEngine;
using UnityEditor;
using System.Linq;
using HSM;

// ICharacter 인터페이스를 구현하는 모든 MonoBehaviour에 이 에디터를 적용합니다.
// (예: PlayerCharacter, EnemyCharacter 등)
[CustomEditor(typeof(MonoBehaviour), true)]
public class CharacterStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 현재 선택된 오브젝트가 ICharacter 인터페이스를 구현하는지 확인합니다.
        ICharacter character = target as ICharacter;
        if (character == null)
        {
            base.OnInspectorGUI();
            return;
        }

        // 상태 정보가 비어있지 않은지 확인합니다.
        if (character.states != null && character.states.Length > 0)
        {
            // 상태 경로를 문자열로 조합합니다.
            string statePath = string.Join(" -> ", character.states.Where(s => s != null).Select(s => s.Name));

            // 인스펙터에 라벨을 추가합니다.
            // EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current State Path", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(statePath, EditorStyles.wordWrappedLabel);

            // 현재 상태가 변경되면 인스펙터를 업데이트하도록 설정합니다.
            EditorUtility.SetDirty(target);
        }
        base.OnInspectorGUI();
    }
}