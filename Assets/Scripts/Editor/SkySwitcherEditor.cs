using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkySwitcher))]//拡張するクラスを指定
public class SkySwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        SkySwitcher skySwitcher = target as SkySwitcher;

        //PrivateMethodを実行する用のボタン
        if (GUILayout.Button("Apply to scene view"))
        {
            skySwitcher.Apply(skySwitcher.current);
        }
    }
}