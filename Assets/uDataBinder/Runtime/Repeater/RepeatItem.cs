using uDataBinder.Binder;
using uDataBinder.Utils;
using UnityEngine;

namespace uDataBinder.Repeater
{
    [DefaultExecutionOrder(400)]
    public class RepeatItem : AliasGroup
    {
        protected RectTransform _rectTransform;
        public RectTransform RectTransform => this == null || _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeField] protected int _dataIndex = -1;
        public int DataIndex => _dataIndex;

        [SerializeField] protected int _index = -1;
        public int Index => _index;

        protected readonly Vector2 LeftTop = new(0, 1);

        public override string Alias
        {
            get => base.Alias + DataBinding.Delimiter + _dataIndex;
        }

        protected virtual void Awake()
        {
            RectTransform.anchorMin = LeftTop;
            RectTransform.anchorMax = LeftTop;
            RectTransform.pivot = LeftTop;
        }

        public virtual void InitializeItem(string alias, int dataIndex, int index)
        {
            _alias = alias;
            _dataIndex = dataIndex;
            _index = index;
            _value = null;
            SetDirty();
        }

        public virtual void ReleaseItem()
        {
            _alias = "";
            _dataIndex = -1;
            _index = -1;
            _value = null;
        }

        public Repeater GetParent()
        {
            return transform.GetComponentInParentIncludeInactive<Repeater>();
        }
    }
}