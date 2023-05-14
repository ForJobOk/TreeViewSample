using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class CustomTreeViewWithTreeModel<T> : TreeView where T : CustomTreeElement
{
    private CustomTreeModel<T> _treeModel;
    private readonly List<TreeViewItem> _rows = new List<TreeViewItem>(100);

    private const float ToggleWidth = 18f;
    private const float EnumPopupWidth = 100f;
    private const float SellHeight = 25f;

    public CustomTreeModel<T> TreeModel => _treeModel;

    public event Action<IList<TreeViewItem>> BeforeDroppingDraggedItems;

    protected CustomTreeViewWithTreeModel(TreeViewState state, CustomTreeModel<T> model) : base(state)
    {
        Init(model);
    }

    private void Init(CustomTreeModel<T> model)
    {
        _treeModel = model;
        _treeModel.ModelChanged += ModelChanged;
        useScrollView = false;
        rowHeight = SellHeight;
    }

    private void ModelChanged()
    {
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        var depthForHiddenRoot = -1;
        return new CustomTreeViewItem<T>
        (
            _treeModel.Root.Id,
            depthForHiddenRoot,
            _treeModel.Root.Name,
            _treeModel.Root
        );
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        if (_treeModel.Root == null)
        {
            Debug.LogError("tree model root is null. did you call SetData()?");
        }

        _rows.Clear();

        if (_treeModel.Root.HasChildren)
        {
            AddChildrenRecursive(_treeModel.Root, 0, _rows);
        }

        SetupParentsAndChildrenFromDepths(root, _rows);

        return _rows;
    }

    private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> newRows)
    {
        foreach (var loggerTreeElement in parent.Children)
        {
            var child = (T)loggerTreeElement;
            var item = new CustomTreeViewItem<T>(child.Id, depth, child.Name, child);
            newRows.Add(item);

            if (child.HasChildren)
            {
                if (IsExpanded(child.Id))
                {
                    AddChildrenRecursive(child, depth + 1, newRows);
                }
                else
                {
                    item.children = CreateChildListForCollapsedParent();
                }
            }
        }
    }

    protected override IList<int> GetAncestors(int id)
    {
        return _treeModel.GetAncestors(id);
    }

    protected override IList<int> GetDescendantsThatHaveChildren(int id)
    {
        return _treeModel.GetDescendantsThatHaveChildren(id);
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (CustomTreeViewItem<T>)args.item;

        //ToggleのLayout調整
        var toggleRect = args.rowRect;
        toggleRect.x += GetContentIndent(item);
        toggleRect.width = ToggleWidth;

        item.Data.IsEnable = EditorGUI.Toggle(toggleRect, item.Data.IsEnable);
        var isChangedEnable = item.Data.IsEnablePreviousValue != item.Data.IsEnable;

        //子階層に要素が存在し、要素のBooleanへの変更があった場合のみ処理する
        if (item.Data.HasChildren && isChangedEnable)
        {
            ActivateChildren(item.Data.Children, item.Data.IsEnable);

            //子、孫、曾孫、、、と最下層まで再帰で処理する
            void ActivateChildren(List<CustomTreeElement> children, bool enable)
            {
                if (children is null)
                {
                    return;
                }

                foreach (var child in children)
                {
                    child.IsEnable = EditorGUI.Toggle(toggleRect, enable);
                    ActivateChildren(child.Children, enable);
                }
            }
        }

        //子を持たない最下層の要素の処理
        if (!item.Data.HasChildren)
        {
            //親のBooleanがFalseなら自身もFalse、親がTrueなら自身の状態に従う
            item.Data.IsEnable = EditorGUI.Toggle(toggleRect, item.Data.IsEnable);
        }

        item.Data.IsEnablePreviousValue = item.Data.IsEnable;

        //カテゴリ用EnumのLayout調整
        var categoryRect = args.rowRect;
        categoryRect.x += ToggleWidth;
        categoryRect.x += GetContentIndent(item);
        categoryRect.y += 2.5f;
        categoryRect.width = EnumPopupWidth;
        item.Data.Category = (LogCategory)EditorGUI.EnumPopup(categoryRect, item.Data.Category);
    }

    //---------------------
    // 以下Dragに関する処理
    //---------------------

    private const string DragID = "GenericDragColumnDragging";

    protected override bool CanStartDrag(CanStartDragArgs args)
    {
        return true;
    }

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        if (hasSearch)
        {
            return;
        }

        DragAndDrop.PrepareStartDrag();
        var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
        DragAndDrop.SetGenericData(DragID, draggedRows);
        DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
        var title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
        DragAndDrop.StartDrag(title);
    }

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        var draggedRows = DragAndDrop.GetGenericData(DragID) as List<TreeViewItem>;
        if (draggedRows == null)
        {
            return DragAndDropVisualMode.None;
        }

        switch (args.dragAndDropPosition)
        {
            case DragAndDropPosition.UponItem:
            case DragAndDropPosition.BetweenItems:
            {
                var validDrag = ValidDrag(args.parentItem, draggedRows);
                if (args.performDrop && validDrag)
                {
                    var parentData = ((CustomTreeViewItem<T>)args.parentItem).Data;
                    OnDropDraggedElementsAtIndex(draggedRows, parentData,
                        args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                }

                return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
            }

            case DragAndDropPosition.OutsideItems:
            {
                if (args.performDrop)
                {
                    OnDropDraggedElementsAtIndex(draggedRows, _treeModel.Root, _treeModel.Root.Children.Count);
                }

                return DragAndDropVisualMode.Move;
            }
            default:
                Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                return DragAndDropVisualMode.None;
        }
    }

    private void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, T parent, int insertIndex)
    {
        if (BeforeDroppingDraggedItems != null)
        {
            BeforeDroppingDraggedItems(draggedRows);
        }

        var draggedElements = new List<CustomTreeElement>();
        foreach (var x in draggedRows)
        {
            draggedElements.Add(((CustomTreeViewItem<T>)x).Data);
        }

        var selectedIDs = draggedElements.Select(x => x.Id).ToArray();
        _treeModel.MoveElements(parent, insertIndex, draggedElements);
        SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
    }

    private bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
    {
        var currentParent = parent;
        while (currentParent != null)
        {
            if (draggedItems.Contains(currentParent))
            {
                return false;
            }

            currentParent = currentParent.parent;
        }

        return true;
    }
}