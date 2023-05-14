using System;
using System.Collections.Generic;

public static class CustomTreeElementUtility
{
    public static void TreeToList<T>(T root, IList<T> result) where T : CustomTreeElement
    {
        if (result == null)
        {
            throw new NullReferenceException("The input 'IList<T> result' list is null");
        }

        result.Clear();

        var stack = new Stack<T>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            result.Add(current);

            if (current.Children != null && current.Children.Count > 0)
            {
                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    stack.Push((T)current.Children[i]);
                }
            }
        }
    }

    public static T ListToTree<T>(IList<T> list) where T : CustomTreeElement
    {
        ValidateDepthValues(list);

        foreach (var element in list)
        {
            element.Parent = null;
            element.Children = null;
        }

        for (int parentIndex = 0; parentIndex < list.Count; parentIndex++)
        {
            var parent = list[parentIndex];
            var alreadyHasValidChildren = parent.Children != null;
            if (alreadyHasValidChildren) continue;

            var parentDepth = parent.Depth;
            var childCount = 0;

            for (int i = parentIndex + 1; i < list.Count; i++)
            {
                if (list[i].Depth == parentDepth + 1)
                {
                    childCount++;
                }

                if (list[i].Depth <= parentDepth) break;
            }

            List<CustomTreeElement> childList = null;
            if (childCount != 0)
            {
                childList = new List<CustomTreeElement>(childCount);
                childCount = 0;
                for (int i = parentIndex + 1; i < list.Count; i++)
                {
                    if (list[i].Depth == parentDepth + 1)
                    {
                        list[i].Parent = parent;
                        childList.Add(list[i]);
                        childCount++;
                    }

                    if (list[i].Depth <= parentDepth) break;
                }
            }

            parent.Children = childList;
        }

        return list[0];
    }

    public static void UpdateDepthValues<T>(T root) where T : CustomTreeElement
    {
        if (root == null)
        {
            throw new ArgumentNullException("root", "The root is null");
        }

        if (!root.HasChildren) return;

        var stack = new Stack<CustomTreeElement>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.Children != null)
            {
                foreach (var child in current.Children)
                {
                    child.Depth = current.Depth + 1;
                    stack.Push(child);
                }
            }
        }
    }

    private static void ValidateDepthValues<T>(IList<T> list) where T : CustomTreeElement
    {
        if (list.Count == 0)
        {
            throw new ArgumentException(
                "list should have items, count is 0, check before calling ValidateDepthValues",
                "list");
        }

        if (list[0].Depth != -1)
        {
            throw new ArgumentException(
                "list item at index 0 should have a depth of -1 (since this should be the hidden root of the tree). Depth is: " +
                list[0].Depth, "list");
        }

        for (int i = 0; i < list.Count - 1; i++)
        {
            var depth = list[i].Depth;
            var nextDepth = list[i + 1].Depth;
            if (nextDepth > depth && nextDepth - depth > 1)
            {
                throw new ArgumentException(string.Format(
                    "Invalid depth info in input list. Depth cannot increase more than 1 per row. Index {0} has depth {1} while index {2} has depth {3}",
                    i, depth, i + 1, nextDepth));
            }
        }

        for (int i = 1; i < list.Count; ++i)
        {
            if (list[i].Depth < 0)
            {
                throw new ArgumentException("Invalid depth value for item at index " + i +
                                            ". Only the first item (the root) should have depth below 0.");
            }
        }

        if (list.Count > 1 && list[1].Depth != 0)
        {
            throw new ArgumentException("Input list item at index 1 is assumed to have a depth of 0", "list");
        }
    }
}