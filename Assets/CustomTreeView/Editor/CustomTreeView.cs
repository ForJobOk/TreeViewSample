using UnityEditor.IMGUI.Controls;

public class CustomTreeView : CustomTreeViewWithTreeModel<CustomTreeElement>
{
    public CustomTreeView(
        TreeViewState state,
        CustomTreeModel<CustomTreeElement> model) : base(state, model)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }
}