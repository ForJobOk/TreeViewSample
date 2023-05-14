using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// ScriptableObjectのEditor拡張
/// </summary>
[CustomEditor(typeof(CustomTreeViewSettings))]
public class CustomTreeViewEditor : Editor
{
    private CustomTreeView _treeView;
    private TreeViewState _treeViewState;
    private CustomTreeViewSettings CustomSettingsInstance => (CustomTreeViewSettings)target;
    private const string SessionStateKeyPrefix = "SampleTreeViewAsset";

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
        _treeViewState ??= new TreeViewState();

        //Treeの開閉状態を読み込む。
        //この処理をスキップすると、ScriptableObjectからフォーカスが外れるたびにTreeViewが閉じた状態となるのでその回避策。
        var jsonState = SessionState.GetString(SessionStateKeyPrefix + CustomSettingsInstance.GetInstanceID(), "");
        if (!string.IsNullOrEmpty(jsonState))
        {
            JsonUtility.FromJsonOverwrite(jsonState, _treeViewState);
        }

        var treeModel = new CustomTreeModel<CustomTreeElement>(CustomSettingsInstance.TreeElements);
        _treeView = new CustomTreeView(_treeViewState, treeModel);
        _treeView.BeforeDroppingDraggedItems += OnBeforeDroppingDraggedItems;
        _treeView.Reload();
    }

    /// <summary>
    /// カテゴリーを変更した際に、まれに表示が更新されないので明示的に更新処理を行う
    /// </summary>
    [InitializeOnLoadMethod]
    private static void RefreshAsset()
    {
        //すべてのインスペクターが更新された後に1度だけ呼び出される
        EditorApplication.delayCall += AssetDatabase.Refresh;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;

        //Treeの開閉状態を保存
        SessionState.SetString
        (
            SessionStateKeyPrefix + CustomSettingsInstance.GetInstanceID(),
            JsonUtility.ToJson(_treeView.state)
        );

        //設定保存処理
        EditorUtility.SetDirty(CustomSettingsInstance);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LayoutButton();

        var totalHeight = _treeView.totalHeight;
        var rect = GUILayoutUtility.GetRect(0, 10000, 0, totalHeight);
        var multiColumnTreeViewRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        _treeView.OnGUI(multiColumnTreeViewRect);
    }

    /// <summary>
    /// UndoRedo処理
    /// </summary>
    private void OnUndoRedoPerformed()
    {
        if (_treeView is null)
        {
            return;
        }

        _treeView.TreeModel.SetData(CustomSettingsInstance.TreeElements);
        _treeView.Reload();
    }

    /// <summary>
    /// 要素に対して操作する直前に行う処理
    /// </summary>
    private void OnBeforeDroppingDraggedItems(IList<TreeViewItem> draggedRows)
    {
        Undo.RecordObject
        (
            CustomSettingsInstance,
            $"Moving {draggedRows.Count} Item{(draggedRows.Count > 1 ? "s" : "")}"
        );
    }

    /// <summary>
    /// ボタンのLayout処理
    /// </summary>
    private void LayoutButton()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            var style = "miniButton";
            if (GUILayout.Button("Expand All", style))
            {
                _treeView.ExpandAll();
            }

            if (GUILayout.Button("Collapse All", style))
            {
                _treeView.CollapseAll();
            }

            GUILayout.FlexibleSpace();
        }
    }
}