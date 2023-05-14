using System;
using System.Collections.Generic;
using System.Linq;

public class CustomTreeModel<T> where T : CustomTreeElement
{
    private IList<T> _data;
    private T _root;

    public T Root => _root;

    public event Action ModelChanged;

    public CustomTreeModel(IList<T> data)
    {
        SetData(data);
    }

    public void SetData(IList<T> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data", "Input data is null.");
        }

        _data = data;
        if (_data.Count > 0)
        {
            _root = CustomTreeElementUtility.ListToTree(data);
        }
    }

    private T Find(int id)
    {
        return _data.FirstOrDefault(element => element.Id == id);
    }

    public IList<int> GetAncestors(int id)
    {
        var parents = new List<int>();
        CustomTreeElement treeElement = Find(id);
        if (treeElement != null)
        {
            while (treeElement.Parent != null)
            {
                parents.Add(treeElement.Parent.Id);
                treeElement = treeElement.Parent;
            }
        }

        return parents;
    }

    public IList<int> GetDescendantsThatHaveChildren(int id)
    {
        var searchFromThis = Find(id);
        if (searchFromThis != null)
        {
            return GetParentsBelowStackBased(searchFromThis);
        }

        return new List<int>();
    }

    private IList<int> GetParentsBelowStackBased(CustomTreeElement searchFromThis)
    {
        var stack = new Stack<CustomTreeElement>();
        stack.Push(searchFromThis);

        var parentsBelow = new List<int>();
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.HasChildren)
            {
                parentsBelow.Add(current.Id);
                foreach (var T in current.Children)
                {
                    stack.Push(T);
                }
            }
        }

        return parentsBelow;
    }

    public void MoveElements(CustomTreeElement parentTreeElement, int insertionIndex, List<CustomTreeElement> elements)
    {
        if (insertionIndex < 0)
        {
            throw new ArgumentException("Invalid input: insertionIndex is -1");
        }

        if (parentTreeElement == null)
        {
            return;
        }

        if (insertionIndex > 0)
        {
            insertionIndex -= parentTreeElement.Children.GetRange(0, insertionIndex).Count(elements.Contains);
        }

        foreach (var draggedItem in elements)
        {
            draggedItem.Parent.Children.Remove(draggedItem);
            draggedItem.Parent = parentTreeElement;
        }

        if (parentTreeElement.Children == null)
        {
            parentTreeElement.Children = new List<CustomTreeElement>();
        }

        parentTreeElement.Children.InsertRange(insertionIndex, elements);

        CustomTreeElementUtility.UpdateDepthValues(Root);
        CustomTreeElementUtility.TreeToList(_root, _data);

        Changed();
    }

    private void Changed()
    {
        if (ModelChanged != null)
        {
            ModelChanged();
        }
    }
}