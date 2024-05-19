using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uDataBinder.Binder;
using UnityEngine;

namespace uDataBinder.Repeater
{
    [DefaultExecutionOrder(300)]
    public class Repeater : DataBinder
    {
        [SerializeField] protected RepeatItem _prefab = null;
        [SerializeField] protected string _target = "";
        [SerializeField] protected string _filter = "";
        [SerializeField] protected string _sort = "";
        [SerializeField] protected int _skip = 0;
        [SerializeField] protected int _limit = -1;

        public string Target
        {
            set
            {
                _target = value;
                SetDirty();
            }
        }

        public string Filter
        {
            set
            {
                _filter = value;
                SetDirty();
            }
        }

        public string Sort
        {
            set
            {
                _sort = value;
                SetDirty();
            }
        }

        public int Skip
        {
            set
            {
                _skip = value;
                SetDirty();
            }
        }

        public int Limit
        {
            set
            {
                _limit = value;
                SetDirty();
            }
        }

        protected virtual Transform Content => transform;
        protected virtual List<int> Indexes
        {
            get
            {
                var list = new List<int>();

                var value = DataBinding.GetValue<IEnumerable>(_target, gameObject);
                if (!string.IsNullOrEmpty(_filter))
                {
                    var i = 0;
                    foreach (var v in value)
                    {
                        var filter = _filter.Replace("*", _target + DataBinding.Delimiter + i++);
                        if (!ConditionBinding.Parse(filter))
                        {
                            continue;
                        }
                        list.Add(i - 1);
                    }
                }
                else
                {
                    var i = 0;
                    foreach (var v in value)
                    {
                        list.Add(i++);
                    }
                }

                IEnumerable<int> temp = list;
                if (!string.IsNullOrEmpty(_sort))
                {
                    var sort = _sort.TrimStart('-');
                    var reverse = _sort[0] == '-';

                    temp = list.OrderBy(index =>
                    {
                        var sortKey = sort.Replace("*", _target + DataBinding.Delimiter + index);
                        return DataBinding.GetValue(sortKey, gameObject);
                    });

                    if (reverse)
                    {
                        temp = temp.Reverse();
                    }
                }

                if (_skip > 0)
                {
                    temp = temp.Skip(_skip);
                }
                if (_limit >= 0)
                {
                    temp = temp.Take(_limit);
                }
                list = temp.ToList();

                _count = list.Count;
                return list;
            }
        }

        protected int? _count;
        protected virtual int Count
        {
            get
            {
                if (_count == null)
                {
                    _count = 0;
                    var value = DataBinding.GetValue<IEnumerable>(_target, gameObject);
                    if (value != null)
                    {
                        if (!string.IsNullOrEmpty(_filter))
                        {
                            var i = 0;
                            foreach (var v in value)
                            {
                                var filter = _filter.Replace("*", _target + DataBinding.Delimiter + i++);
                                if (!ConditionBinding.Parse(filter))
                                {
                                    continue;
                                }
                                ++_count;
                            }
                        }
                        else
                        {
                            foreach (var v in value)
                            {
                                ++_count;
                            }
                        }
                    }
                }

                return _count.Value;
            }
        }

        protected List<RepeatItem> _children;
        protected void Awake()
        {
            _children = new List<RepeatItem>();
            foreach (Transform child in Content)
            {
                var repeatItem = child.GetComponent<RepeatItem>();
                if (repeatItem != null)
                {
                    _children.Add(repeatItem);
                }
            }
        }

        public override void Release()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (var repeatItem in _children)
            {
                ReleaseItem(repeatItem);
            }
        }

        protected virtual void InitializeItem(RepeatItem repeatItem, int dataIndex, int index)
        {
            repeatItem.gameObject.SetActive(true);
            repeatItem.InitializeItem(_target, dataIndex, index);
        }

        protected virtual void ReleaseItem(RepeatItem repeatItem)
        {
            repeatItem.ReleaseItem();
            repeatItem.gameObject.SetActive(false);
        }

        protected override Task RebuildAsync()
        {
            DataBinding.Register(_target, this, gameObject);

            var indexes = Indexes;
            if (indexes.Count > 0)
            {
                var index = 0;
                foreach (var repeatItem in _children)
                {
                    if (index < indexes.Count)
                    {
                        ReleaseItem(repeatItem);
                        InitializeItem(repeatItem, indexes[index], index);
                    }
                    else
                    {
                        ReleaseItem(repeatItem);
                    }
                    ++index;
                }

                if (_prefab != null)
                {
                    for (; index < indexes.Count; ++index)
                    {
                        var repeatItem = Instantiate(_prefab, Content, false);
                        InitializeItem(repeatItem, indexes[index], index);
                        _children.Add(repeatItem);
                    }
                }
            }
            else
            {
                Clear();
            }

            return Task.CompletedTask;
        }
    }
}