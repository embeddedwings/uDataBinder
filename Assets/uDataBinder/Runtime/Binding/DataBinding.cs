using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uDataBinder.Binder;
using uDataBinder.Utils;
using UnityEngine;

namespace uDataBinder
{
    public static class DataBinding
    {
        private static readonly Dictionary<Utils.Nullable<GameObject>, DataBindingReference> _references = new();
        private static SortedDictionary<int, HashSet<DataBinder>> _rebuildList = new();
        private static readonly HashSet<DataBinder> _rebuildCompleted = new();

        private static readonly Dictionary<string, List<(string, DataBinder, GameObject)>> _lazyRegister = new();

        public const char LeftDelimiter = '{';
        public const char RightDelimiter = '}';
        public const char Delimiter = '.';
        public const char AliasSymbol = '*';

        public static void Set(string key, object value, GameObject baseObject = null)
        {
            if (!_references.ContainsKey(baseObject))
            {
                _references[baseObject] = new DataBindingReference();
            }

            _references[baseObject].Set(key, value);

            var removeKeys = new List<string>();
            foreach (var lazyRegister in _lazyRegister)
            {
                if (lazyRegister.Key.StartsWith(key))
                {
                    foreach (var v in lazyRegister.Value)
                    {
                        Register(v.Item1, v.Item2, v.Item3);
                    }
                    removeKeys.Add(lazyRegister.Key);
                }
            }

            foreach (var removeKey in removeKeys)
            {
                _lazyRegister.Remove(removeKey);
            }

            Notify(key, baseObject);
        }

        public static DataBindingReference Find(string keys, GameObject baseObject = null)
        {
            var keyArray = keys.Split(Delimiter).ToList();
            var key = keyArray.FirstOrDefault();

            var gameObject = baseObject;
            while (gameObject != null)
            {
                if (_references.ContainsKey(gameObject))
                {
                    var reference = _references[gameObject];
                    if (reference.Exist(key))
                    {
                        return reference;
                    }
                }
                gameObject = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
            }

            if (_references.ContainsKey(null) && _references[null].Exist(key))
            {
                var reference = _references[null];
                if (reference.Exist(key))
                {
                    return reference;
                }
            }

            return null;
        }

        public static bool Exist(string key, GameObject baseObject = null)
        {
            return Find(key, baseObject) != null;
        }

        public static object Get(string key, GameObject baseObject = null)
        {
            var reference = Find(key, baseObject);
            if (reference == null)
            {
                return null;
            }
            return reference.Get(key);
        }

        public static void Unset(string key, GameObject baseObject = null)
        {
            var reference = Find(key, baseObject);
            if (reference == null)
            {
                return;
            }
            reference.Remove(key);
        }

        public static void Register(string keys, DataBinder dataBinder, GameObject baseObject)
        {
            if (dataBinder == null || string.IsNullOrEmpty(keys))
            {
                return;
            }

            keys = ResolveKey(keys, baseObject);
            var reference = Find(keys, baseObject);
            if (reference == null)
            {
                if (!_lazyRegister.ContainsKey(keys))
                {
                    _lazyRegister[keys] = new List<(string, DataBinder, GameObject)>();
                }
                _lazyRegister[keys].Add((keys, dataBinder, baseObject));
                return;
            }

            reference.Register(keys, dataBinder);
        }

        public static void Unregister(DataBinder dataBinder)
        {
            foreach (var reference in _references)
            {
                reference.Value.Unregister(dataBinder);
            }
        }

        public static void LateRebuild(DataBinder dataBinder, bool force)
        {
            var hierarchyLevel = dataBinder.HierarchyLevel;
            if (!_rebuildList.ContainsKey(hierarchyLevel))
            {
                _rebuildList.Add(hierarchyLevel, new HashSet<DataBinder>());
            }
            _rebuildList[hierarchyLevel].Add(dataBinder);

            if (force)
            {
                if (_rebuildCompleted.Contains(dataBinder))
                {
                    _rebuildCompleted.Remove(dataBinder);
                }
            }
        }

        public static async Task Execute(bool force = false)
        {
            var tasks = new List<Task>();

            while (_rebuildList.Count > 0)
            {
                var currentRebuildList = _rebuildList;

                _rebuildList = new SortedDictionary<int, HashSet<DataBinder>>();

                foreach (var target in currentRebuildList)
                {
                    tasks.Clear();
                    foreach (var dataBinder in target.Value)
                    {
                        if (dataBinder != null)
                        {
                            if (_rebuildCompleted.Contains(dataBinder))
                            {
                                continue;
                            }

                            if (!force || (force && target.Key != 0))
                            {
                                if (!dataBinder.gameObject.activeSelf || !dataBinder.enabled)
                                {
                                    continue;
                                }
                            }

                            _rebuildCompleted.Add(dataBinder);

                            tasks.Add(dataBinder.Rebuild());
                        }
                    }

                    await Task.WhenAll(tasks);
                }
            }

            if (_rebuildCompleted.Count() > 0)
            {
#if UNITY_EDITOR
                Debug.Log($"Rebuild: {_rebuildCompleted.Count()}\n {string.Join(", ", _rebuildCompleted.Select(v => v.name))}");
#endif
                _rebuildCompleted.Clear();
            }
        }

        public static void Notify(string key, GameObject baseObject = null, bool strict = false)
        {
            // Find reference
            var reference = Find(key, baseObject);
            if (reference == null)
            {
                return;
            }

            reference.Notify(key, baseObject, strict);
        }

        public static void Release()
        {
            foreach (var reference in _references)
            {
                if (reference.Key == null)
                {
                    continue;
                }
                reference.Value.Release();
            }

            _rebuildCompleted.Clear();
        }

        public static string ResolveKey(string keys, GameObject baseObject = null)
        {
            if (baseObject == null || !keys.Contains(AliasSymbol))
            {
                return keys;
            }

            var keyArray = keys.Split(Delimiter).ToList();
            for (var i = 0; i < keyArray.Count; ++i)
            {
                var key = keyArray[i];
                if (key == AliasSymbol.ToString())
                {
                    var aliasGroup = baseObject.transform.GetComponentInParentIncludeInactive<AliasGroup>();
                    if (aliasGroup == null || aliasGroup.transform.parent == null)
                    {
                        continue;
                    }

                    keyArray.RemoveRange(0, i + 1);
                    var newKeys = aliasGroup.Alias.Split(Delimiter).Concat(keyArray);
                    return ResolveKey(string.Join(Delimiter.ToString(), newKeys), aliasGroup.transform.parent.gameObject);
                }
            }
            return string.Join(Delimiter.ToString(), keyArray);
        }

        public static object GetValue(string keys, GameObject baseObject = null)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return null;
            }

            // Resolve key
            var keyList = ResolveKey(keys, baseObject).Split(Delimiter).Where(v => !string.IsNullOrEmpty(v)).ToList();
            if (keyList.Count == 0)
            {
                return null;
            }

            // Find reference
            var fullkey = string.Join(Delimiter.ToString(), keyList);
            var reference = Find(fullkey, baseObject);
            if (reference == null)
            {
                return null;
            }

            return reference.GetValue(keyList);
        }

        public class RequireStruct<T> where T : struct { }
        public class RequireClass<T> where T : class { }

        public static T GetValue<T>(string keys, GameObject baseObject = null, RequireStruct<T> missing = null) where T : struct
        {
            try
            {
                var value = GetValue(keys, baseObject);
                if (value == null)
                {
                    return (T)Convert.ChangeType(keys, typeof(T));
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (FormatException)
            {
                return default;
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        public static T GetValue<T>(string keys, GameObject baseObject = null, RequireClass<T> missing = null)
        where T : class
        {
            return GetValue(keys, baseObject) as T;
        }

        public static bool SetValue(string keys, object value, GameObject baseObject = null)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return false;
            }

            // Resolve key
            var keyList = new Queue<string>(ResolveKey(keys, baseObject).Split(Delimiter).Where(v => !string.IsNullOrEmpty(v)));
            if (keyList.Count == 0)
            {
                return false;
            }

            // Find reference
            var fullkey = string.Join(Delimiter.ToString(), keyList);
            var reference = Find(fullkey, baseObject);
            if (reference == null)
            {
                return false;
            }

            // Set value
            if (reference.SetValue(keyList, value) == null)
            {
                return false;
            }

            Notify(keys, baseObject);
            return true;
        }
    }
}