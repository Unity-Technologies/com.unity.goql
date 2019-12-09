using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.GoQL
{
    public class GoQLExecutor
    {
        Stack<object> stack;
        List<GameObject> selection;

        public string Error
        {
            get;
            private set;
        } = string.Empty;

        static Dictionary<string, Type> typeCache;

        static Type FindType(string name)
        {
            if (typeCache == null)
            {
                typeCache = new Dictionary<string, Type>();
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var i in assemblies)
                {
                    foreach (var j in i.GetTypes())
                    {
                        if (j.IsSubclassOf(typeof(Component)))
                        {
                            if (typeCache.ContainsKey(j.Name))
                            {
                                typeCache.Add($"{j.Namespace}.{j.Name}", j);
                                var existingType = typeCache[j.Name];
                                typeCache.Add($"{existingType.Namespace}.{existingType.Name}", j);
                            }
                            else
                            {
                                typeCache.Add(j.Name, j);
                            }
                        }
                    }
                }
            }
            if (typeCache.TryGetValue(name, out Type type))
                return type;
            return null;
        }

        public GameObject[] Execute(List<object> instructions)
        {
            stack = new Stack<object>();
            Error = string.Empty;
            if (selection != null) selection.Clear();
            foreach (var i in instructions)
            {
                if (i is GoQLCode)
                {
                    ExecuteCode((GoQLCode)i);
                }
                else
                {
                    stack.Push(i);
                }
            }

            var results = selection == null ? new GameObject[0] : selection.ToArray();
            selection = null;
            return results;
        }

        void ExecuteCode(GoQLCode i)
        {
            switch (i)
            {
                case GoQLCode.EnterChildren:
                    EnterChildren();
                    break;
                case GoQLCode.FilterComponent:
                    FilterComponent();
                    break;
                case GoQLCode.FilterIndex:
                    FilterIndex();
                    break;
                case GoQLCode.FilterName:
                    FilterName();
                    break;
            }
        }

        void CheckSelection()
        {
            if (selection == null)
            {
                selection = new List<GameObject>();
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    foreach (var j in scene.GetRootGameObjects())
                    {
                        AddToSelection(j, selection);
                    }
                }
            }
        }

        void AddToSelection(GameObject gameObject, List<GameObject> gameObjects)
        {
            gameObjects.Add(gameObject);
            foreach (Transform transform in gameObject.transform)
            {
                AddToSelection(transform.gameObject, gameObjects);
            }
        }

        void FilterName()
        {
            CheckSelection();
            var q = stack.Pop().ToString();
            var results = DiscriminateByName(q, selection).ToArray();
            selection.Clear();
            selection.AddRange(results);
        }

        void FilterIndex()
        {
            CheckSelection();
            if (selection.Count > 0)
            {
                var argCount = (int)stack.Pop();
                var candidates = new List<GameObject>();
                var indices = new int[selection.Count];
                var lengths = new int[selection.Count];
                for (var i = 0; i < selection.Count; i++)
                {
                    indices[i] = selection[i].transform.GetSiblingIndex();
                    lengths[i] = selection[i].transform.parent.childCount;
                }
                for (var i = 0; i < argCount; i++)
                {
                    var arg = stack.Pop();
                    if (arg is int)
                    {
                        for (var j = 0; j < selection.Count; j++)
                        {
                            var index = mod(((int)arg), lengths[i]);
                            if (index == indices[j])
                            {
                                candidates.Add(selection[j]);
                            }
                        }
                    }
                    else if (arg is Range)
                    {
                        var range = (Range)arg;
                        for (var index = range.start; index < range.end; index++)
                        {
                            for (var j = 0; j < selection.Count; j++)
                            {
                                if (mod(index, lengths[j]) == indices[j])
                                {
                                    candidates.Add(selection[j]);
                                }
                            }
                        }
                    }
                }
                selection.Clear();
                candidates.Reverse();
                selection.AddRange(candidates);
            }
        }

        void FilterComponent()
        {
            CheckSelection();
            var argCount = (int)stack.Pop();
            var candidates = selection.ToList();
            for (var i = 0; i < argCount; i++)
            {
                var arg = stack.Pop();
                if (arg is string)
                {
                    var type = FindType((string)arg);
                    if (type != null)
                    {
                        candidates.RemoveAll(g => !g.TryGetComponent(type, out Component component));
                    }
                    else
                    {
                        Error = $"Cannot load type {arg}";
                    }
                }
            }
            selection.Clear();
            selection.AddRange(candidates);
        }

        void EnterChildren()
        {
            if (selection == null)
            {
                selection = new List<GameObject>();
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    selection.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
                }
            }
            else
            {
                var children = new List<GameObject>();
                foreach (var i in selection)
                {
                    for (var j = 0; j < i.transform.childCount; j++)
                        children.Add(i.transform.GetChild(j).gameObject);
                }
                selection.Clear();
                selection.AddRange(children);
            }
        }

        IEnumerable<GameObject> DiscriminateByName(string q, List<GameObject> candidates)
        {
            if (q.StartsWith("*") && q.EndsWith("*"))
            {
                q = q.Substring(1, q.Length - 2);
                return (from i in candidates where i.name.Contains(q) select i);
            }
            else if (q.StartsWith("*"))
            {
                q = q.Substring(1);
                return (from i in candidates where i.name.EndsWith(q) select i);
            }
            else if (q.EndsWith("*"))
            {
                q = q.Substring(0, q.Length - 1);
                return (from i in candidates where i.name.StartsWith(q) select i);
            }
            else
            {
                return (from i in candidates where i.name == q select i);
            }
        }

        int mod(int a, int b) => a - b * Mathf.FloorToInt(1f * a / b);

    }

}