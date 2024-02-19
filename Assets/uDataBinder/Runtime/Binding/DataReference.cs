using System;
using System.Collections.Generic;
using uDataBinder.Binder;

namespace uDataBinder
{
    public class DataReference
    {
        private readonly HashSet<DataBinder> _dataBinders = new();
        private readonly Dictionary<string, DataReference> _children = new();

        public DataReference Find(string keys)
        {
            var dataReference = this;
            foreach (var key in keys.Split(DataBinding.Delimiter))
            {
                if (!dataReference._children.ContainsKey(key))
                {
                    dataReference._children[key] = new DataReference();
                }

                dataReference = dataReference._children[key];
            }

            return dataReference;
        }

        public void CleanAll()
        {
            _dataBinders.RemoveWhere(v => v == null);

            foreach (var item in _children)
            {
                item.Value.CleanAll();
            }
        }

        public void Register(string key, DataBinder dataBinder)
        {
            var item = Find(key);
            item.CleanAll();
            item._dataBinders.Add(dataBinder);
        }

        public void Unregister(DataBinder dataBinder)
        {
            _dataBinders.Remove(dataBinder);
            foreach (var item in _children)
            {
                item.Value.Unregister(dataBinder);
            }
        }

        public void Apply(Queue<string> keys, Action<DataReference> callback, bool strict)
        {
            if (keys == null || keys.Count == 0)
            {
                callback.Invoke(this);
                foreach (var item in _children)
                {
                    item.Value.Apply(null, callback, strict);
                }
            }
            else
            {
                var key = keys.Dequeue();

                if (strict)
                {
                    if (_children.ContainsKey(key))
                    {
                        _children[key].Apply(keys, callback, true);
                    }
                }
                else
                {
                    foreach (var child in _children)
                    {
                        if (child.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                        {
                            child.Value.Apply(keys, callback, false);
                        }
                    }
                }
            }
        }

        public void SetDirty(string key, bool strict)
        {
            var keys = new Queue<string>(key.Split(DataBinding.Delimiter));
            Apply(keys, reference =>
            {
                var dataBinders = new HashSet<DataBinder>(reference._dataBinders);
                foreach (var v in dataBinders)
                {
                    if (v != null)
                    {
                        v.SetDirty();
                    }
                }
            }, strict);
        }
    }
}