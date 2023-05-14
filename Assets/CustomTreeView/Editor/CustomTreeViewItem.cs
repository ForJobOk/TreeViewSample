using UnityEditor.IMGUI.Controls;

public class CustomTreeViewItem<T> : TreeViewItem where T : CustomTreeElement
{
    public T Data { get; }

    public CustomTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
    {
        Data = data;
    }
}