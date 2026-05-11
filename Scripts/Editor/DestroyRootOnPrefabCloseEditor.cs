#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class DestroyRootOnPrefabCloseEditor
{
    static DestroyRootOnPrefabCloseEditor()
    {
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;
    }

    private static void OnPrefabStageClosing(PrefabStage stage)
    {
        if (stage == null || stage.prefabContentsRoot == null)
            return;

        DestroyRootOnPrefabClose marker =
            stage.prefabContentsRoot.GetComponent<DestroyRootOnPrefabClose>();

        if (marker == null)
            return;

        Transform root = stage.prefabContentsRoot.transform.Find(marker.rootName);

        if (root == null)
            return;

        Object.DestroyImmediate(root.gameObject);

        PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath);
        AssetDatabase.SaveAssets();
    }
}
#endif