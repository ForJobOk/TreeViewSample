using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeViewSampleScriptableObject", menuName = "ScriptableObjects/TreeView Sample ScriptableObject")]
public class CustomTreeViewSettings : ScriptableObject
{
    [SerializeField, HideInInspector] private List<string> _logCategories = new List<string>();
    [SerializeField, HideInInspector] private List<CustomTreeElement> _treeElements = new List<CustomTreeElement>();

    private Dictionary<string, CustomTreeElement> _categoryAndElementDictionary =
        new Dictionary<string, CustomTreeElement>();

    private const string RootName = "Root";

    /// <summary>
    /// TreeViewの要素群
    /// </summary>
    public List<CustomTreeElement> TreeElements => _treeElements;

    private void OnEnable()
    {
        var currentLogCategories = GetLogCategoryList();
        var currentTreeElements = GenerateTree();

        //まだ要素群が生成されていない場合のTree生成処理
        if (_treeElements.Count == 0)
        {
            _logCategories = currentLogCategories;
            _treeElements = currentTreeElements;
        }

        //カテゴリの文字列と要素を紐づけた辞書を作成する。
        //増えた/減った要素を文字列をKeyとして取得することを目的とする。
        _categoryAndElementDictionary.Clear();
        for (int i = 1; i < _treeElements.Count; i++)
        {
            var category = _treeElements[i].Category.ToString();
            var element = _treeElements[i];
            if (_categoryAndElementDictionary.Keys.Contains(category)) continue;
            _categoryAndElementDictionary.Add(category, element);
        }

        //カテゴリに変更があった場合
        if (!_logCategories.SequenceEqual(currentLogCategories))
        {
            _logCategories = currentLogCategories;

            var sameElements = new List<CustomTreeElement>();
            var treeElementsOriginal = new List<CustomTreeElement>();
            treeElementsOriginal.AddRange(_treeElements);

            foreach (var element in currentTreeElements.Where(element => element.Name != RootName))
            {
                if (!_categoryAndElementDictionary.ContainsKey(element.Name))
                {
                    _treeElements.Add(element);
                }
                else
                {
                    sameElements.Add(_categoryAndElementDictionary[element.Name]);
                }
            }

            var diffList = treeElementsOriginal.Except(sameElements);
            foreach (var diff in diffList.Where(diff => diff.Name != RootName))
            {
                _treeElements.Remove(diff);
            }
        }
    }

    /// <summary>
    /// カテゴリを文字列のリストで取得する。
    /// カテゴリ変更を検知するために利用する。
    /// </summary>
    /// <returns>生成したカテゴリの文字列リスト</returns>
    private List<string> GetLogCategoryList()
    {
        var categoryList = new List<string>();
        var enumLength = Enum.GetValues(typeof(LogCategory)).Length;
        for (int i = 0; i < enumLength; i++)
        {
            var category = (LogCategory)Enum.ToObject(typeof(LogCategory), i);
            categoryList.Add(category.ToString());
        }

        return categoryList;
    }

    /// <summary>
    /// Tree生成処理
    /// </summary>
    /// <returns>生成したTreeの要素群</returns>
    private List<CustomTreeElement> GenerateTree()
    {
        var treeElement = new List<CustomTreeElement> { CreateRootElement() };

        //ログのカテゴリ分の要素を生成し、それぞれに初期値を割り当てる
        var enumLength = Enum.GetValues(typeof(LogCategory)).Length;
        for (int i = 0; i < enumLength; i++)
        {
            var category = (LogCategory)Enum.ToObject(typeof(LogCategory), i);
            var element = new CustomTreeElement(category.ToString(), 0, i, category, true);
            treeElement.Add(element);
        }

        return treeElement;
    }

    /// <summary>
    /// TreeViewの仕様上、ルートとなる要素を作る必要があるため作成する。
    /// なお、このルートはTreeViewとしてEditor上に表示されない。
    /// </summary>
    /// <returns>ルートとしてふるまう、Elementのインスタンス</returns>
    private CustomTreeElement CreateRootElement()
    {
        var rootCategory = (LogCategory)Enum.ToObject(typeof(LogCategory), 0);
        var root = new CustomTreeElement(RootName, -1, -1, rootCategory, true);
        return root;
    }
}