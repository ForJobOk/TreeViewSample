using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomTreeElement
{
    [SerializeField] int _id;
    [SerializeField] string _name;
    [SerializeField] int _depth;
    [SerializeField] LogCategory _category;
    [SerializeField] bool _isEnable;
    [SerializeField] bool _isEnablePreviousValue;
    [NonSerialized] CustomTreeElement _parent;
    [NonSerialized] List<CustomTreeElement> _children;

    public int Id
    {
        get => _id;
        set => _id = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public int Depth
    {
        get => _depth;
        set => _depth = value;
    }

    public LogCategory Category
    {
        get => _category;
        set => _category = value;
    }

    public bool IsEnable
    {
        get => _isEnable;
        set => _isEnable = value;
    }

    public bool IsEnablePreviousValue
    {
        get => _isEnablePreviousValue;
        set => _isEnablePreviousValue = value;
    }

    public CustomTreeElement Parent
    {
        get => _parent;
        set => _parent = value;
    }

    public List<CustomTreeElement> Children
    {
        get => _children;
        set => _children = value;
    }

    public bool HasChildren => !(_children is null) && _children.Count > 0;

    public CustomTreeElement(string name, int depth, int id, LogCategory category, bool isEnable)
    {
        _name = name;
        _id = id;
        _depth = depth;
        _category = category;
        _isEnable = isEnable;
    }
}