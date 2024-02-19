using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using uDataBinder.Binder;
using UnityEngine;

namespace uDataBinder
{
    public class DataBindingReference
    {
        private readonly Dictionary<string, object> _data = new();
        private readonly Dictionary<string, object> _cache = new();

        private readonly DataReference _reference = new();

        private static readonly Type[] _intType = { typeof(int) };
        private static readonly Type[] _longType = { typeof(long) };
        private static readonly Type[] _stringType = { typeof(string) };
        private static readonly object[] _tempObject = { 0 };

        public void Set(string key, object value)
        {
            _data[key] = value;
        }

        public object Get(string key)
        {
            object ret = null;
            if (_data.ContainsKey(key))
            {
                ret = _data[key];
            }
            return ret;
        }

        public void Remove(string key)
        {
            _data.Remove(key);
        }

        public bool Exist(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Release()
        {
            List<string> removeList = new List<string>();
            foreach (var data in _data)
            {
                removeList.Add(data.Key);
            }

            foreach (var key in removeList)
            {
                Remove(key);
            }

            _cache.Clear();
        }

        public void ClearCache(string keys = null, GameObject baseObject = null, bool strict = false)
        {
            if (string.IsNullOrEmpty(keys))
            {
                _cache.Clear();
            }
            else
            {
                var removeKeys = new List<string>();
                if (strict)
                {
                    foreach (var cache in _cache)
                    {
                        if (cache.Key == keys || cache.Key.StartsWith(keys + DataBinding.Delimiter))
                        {
                            removeKeys.Add(cache.Key);
                        }
                    }
                }
                else
                {
                    foreach (var cache in _cache)
                    {
                        if (cache.Key.StartsWith(keys))
                        {
                            removeKeys.Add(cache.Key);
                        }
                    }
                }
                foreach (var key in removeKeys)
                {
                    _cache.Remove(key);
                }
            }
        }

        private object GetValue(string preKey, IEnumerable<string> keyArray, object data)
        {
            // Find data of key
            var fullKey = preKey;
            foreach (var key in keyArray)
            {
                if (data == null)
                {
                    break;
                }
                fullKey += DataBinding.Delimiter + key;

                if (long.TryParse(key, out long index))
                {
                    var list = data as IList;
                    if (list != null)
                    {
                        if (0 <= index && index < list.Count)
                        {
                            data = list[(int)index];
                            _cache[fullKey] = data;
                        }
                        else
                        {
                            data = null;
                        }
                    }
                    else
                    {
                        var type = data.GetType();
                        var defaultMemberAttribute = type.GetCustomAttribute<DefaultMemberAttribute>();
                        if (defaultMemberAttribute == null)
                        {
                            data = null;
                        }
                        else
                        {
                            var memberName = defaultMemberAttribute.MemberName;
                            var indexerProperty = type.GetProperty(memberName, _longType);
                            if (indexerProperty != null)
                            {
                                _tempObject[0] = index;
                                data = indexerProperty.GetValue(data, _tempObject);
                                _cache[fullKey] = data;
                            }
                            else
                            {
                                indexerProperty = type.GetProperty(memberName, _intType);
                                if (indexerProperty != null)
                                {
                                    _tempObject[0] = (int)index;
                                    data = indexerProperty.GetValue(data, _tempObject);
                                    _cache[fullKey] = data;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var field = data.GetType().GetField(key);
                    if (field != null)
                    {
                        data = field.GetValue(data);
                        _cache[fullKey] = data;
                    }
                    else
                    {
                        var property = data.GetType().GetProperty(key);
                        if (property != null)
                        {
                            if (data.ToString() != "null")
                            {
                                data = property.GetValue(data);
                                _cache[fullKey] = data;
                            }
                            else
                            {
                                data = null;
                            }
                        }
                        else
                        {
                            var type = data.GetType();
                            var defaultMemberAttribute = type.GetCustomAttribute<DefaultMemberAttribute>();
                            if (defaultMemberAttribute != null)
                            {
                                var memberName = defaultMemberAttribute.MemberName;
                                var indexerProperty = type.GetProperty(memberName, _stringType);
                                if (indexerProperty != null)
                                {
                                    _tempObject[0] = key;
                                    data = indexerProperty.GetValue(data, _tempObject);
                                    _cache[fullKey] = data;
                                }
                                else
                                {
                                    data = null;
                                }
                            }
                            else
                            {
                                data = null;
                            }
                        }
                    }
                }
            }

            return data;
        }

        private object SetValue(Queue<string> keyArray, object data, object value)
        {
            if (keyArray.Count == 0)
            {
                return value;
            }

            var key = keyArray.Peek();

            if (long.TryParse(key, out long index))
            {
                if (data is IList list)
                {
                    if (0 <= index && index < list.Count)
                    {
                        keyArray.Dequeue();
                        list[(int)index] = SetValue(keyArray, list[(int)index], value);
                    }
                }
                else
                {
                    var type = data.GetType();
                    var defaultMemberAttribute = type.GetCustomAttribute<DefaultMemberAttribute>();
                    if (defaultMemberAttribute != null)
                    {
                        var memberName = defaultMemberAttribute.MemberName;
                        var indexerProperty = type.GetProperty(memberName, _longType);
                        if (indexerProperty != null)
                        {
                            keyArray.Dequeue();
                            _tempObject[0] = index;
                            var v = SetValue(keyArray, indexerProperty.GetValue(data, _tempObject), value);
                            indexerProperty.SetValue(data, Convert.ChangeType(v, indexerProperty.PropertyType), _tempObject);
                        }
                        else
                        {
                            indexerProperty = type.GetProperty(memberName, _intType);
                            if (indexerProperty != null)
                            {
                                keyArray.Dequeue();
                                _tempObject[0] = (int)index;
                                var v = SetValue(keyArray, indexerProperty.GetValue(data, _tempObject), value);
                                indexerProperty.SetValue(data, Convert.ChangeType(v, indexerProperty.PropertyType), _tempObject);
                            }
                        }
                    }
                }
            }
            else
            {
                var field = data.GetType().GetField(key);
                if (field != null)
                {
                    keyArray.Dequeue();
                    var fieldValue = field.GetValue(data);
                    var srcValue = SetValue(keyArray, fieldValue, value);

                    if (srcValue is Array srcArray)
                    {
                        var arrayElementType = fieldValue.GetType().GetElementType();
                        if (arrayElementType != null)
                        {
                            var dst = Array.CreateInstance(arrayElementType, srcArray.Length);
                            Array.Copy(srcArray, dst, srcArray.Length);
                            field.SetValue(data, Convert.ChangeType(dst, field.FieldType));
                        }
                    }
                    else
                    {
                        field.SetValue(data, Convert.ChangeType(srcValue, field.FieldType));
                    }
                }
                else
                {
                    var property = data.GetType().GetProperty(key);
                    if (property != null)
                    {
                        if (data.ToString() != "null")
                        {
                            keyArray.Dequeue();
                            var v = SetValue(keyArray, property.GetValue(data), value);
                            property.SetValue(data, Convert.ChangeType(v, property.PropertyType));
                        }
                    }
                    else
                    {
                        var type = data.GetType();

                        var defaultMemberAttribute = type.GetCustomAttribute<DefaultMemberAttribute>();
                        if (defaultMemberAttribute != null)
                        {
                            keyArray.Dequeue();
                            var memberName = defaultMemberAttribute.MemberName;
                            var indexerProperty = type.GetProperty(memberName, _stringType);
                            if (indexerProperty != null)
                            {
                                _tempObject[0] = key;
                                var v = SetValue(keyArray, indexerProperty.GetValue(data, _tempObject), value);
                                indexerProperty.SetValue(data, Convert.ChangeType(v, indexerProperty.PropertyType), _tempObject);
                            }
                        }
                    }
                }
            }

            return data;
        }

        public object GetValue(List<string> keyList)
        {
            // Get value (direct cache)
            var fullkey = string.Join(DataBinding.Delimiter.ToString(), keyList);
            if (_cache.ContainsKey(fullkey))
            {
                return _cache[fullkey];
            }

            // Get value (via cache)
            var keyStack = new List<string>(keyList);
            var remainKey = keyStack.Last();
            keyStack.RemoveAt(keyStack.Count - 1);
            while (keyStack.Count > 0)
            {
                var key = string.Join(DataBinding.Delimiter.ToString(), keyStack);

                if (_cache.ContainsKey(key))
                {
                    return GetValue(key, new List<string>(remainKey.Split(DataBinding.Delimiter)), _cache[key]);
                }

                remainKey = keyStack.Last() + DataBinding.Delimiter + remainKey;
                keyStack.RemoveAt(keyStack.Count - 1);
            }

            // Get root name
            var dataName = keyList.First();
            keyList.RemoveAt(0);

            // Get value
            return GetValue(dataName, keyList, Get(dataName));
        }

        public object SetValue(Queue<string> keyList, object value)
        {
            // Get root name
            if (!(keyList?.Count > 0))
            {
                return null;
            }
            var dataName = keyList.Dequeue();

            // Get data
            var data = Get(dataName);
            if (data == null)
            {
                return null;
            }

            // Set value
            return SetValue(keyList, data, value);
        }

        public void Register(string key, DataBinder dataBinder)
        {
            _reference.Register(key, dataBinder);
        }

        public void Unregister(DataBinder dataBinder)
        {
            _reference.Unregister(dataBinder);
        }

        public void Notify(string key, GameObject baseObject = null, bool strict = false)
        {
            ClearCache(key, baseObject, strict);
            _reference.SetDirty(key, strict);
        }
    }
}