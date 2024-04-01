using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace uDataBinder.Repeater
{
    [DefaultExecutionOrder(300)]
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRepeater : Repeater
    {
        protected ScrollRect _scrollRect;
        protected ScrollRect ScrollRect => this == null || _scrollRect != null ? _scrollRect : _scrollRect = GetComponent<ScrollRect>();

        [SerializeField]
        protected Vector2 _spacing = new(4, 4);

        protected override Transform Content => ScrollRect.content;

        protected override List<int> Indexes
        {
            get
            {
                var result = base.Indexes;
                return result;
            }
        }

        protected override void InitializeItem(RepeatItem repeatItem, int dataIndex, int index)
        {
            base.InitializeItem(repeatItem, dataIndex, index);

            var itemSize = repeatItem.RectTransform.sizeDelta;
            if (ScrollRect.vertical)
            {
                var rows = Rows;
                var x = Mathf.CeilToInt(index % rows);
                var y = Mathf.CeilToInt(index / rows);
                repeatItem.RectTransform.localPosition = new Vector3(x * (itemSize.x + _spacing.x), -y * (itemSize.y + _spacing.y));
            }
            else
            {
                var cols = Cols;
                var x = Mathf.CeilToInt(index / cols);
                var y = Mathf.CeilToInt(index % cols);
                repeatItem.RectTransform.localPosition = new Vector3(x * (itemSize.x + _spacing.x), -y * (itemSize.y + _spacing.y));
            }
        }

        protected Vector2? _itemSize;
        protected Vector2 ItemSize
        {
            get
            {
                if (_itemSize == null)
                {
                    _itemSize = _prefab.GetComponent<RectTransform>().sizeDelta;
                }
                return _itemSize.Value;
            }
        }

        protected float _width = 0;
        protected float Width
        {
            get
            {
                if (_width == 0)
                {
                    var rectTransform = ScrollRect.GetComponent<RectTransform>();
                    _width = rectTransform.rect.width;
                    if (ScrollRect.vertical && ScrollRect.verticalScrollbar != null)
                    {
                        var rect = ScrollRect.verticalScrollbar.GetComponent<RectTransform>().rect;
                        _width -= rect.width + ScrollRect.verticalScrollbarSpacing;
                    }
                }
                return _width;
            }
        }

        protected float _height = 0;
        protected float Height
        {
            get
            {
                if (_height == 0)
                {
                    var rectTransform = ScrollRect.GetComponent<RectTransform>();
                    _height = rectTransform.rect.height;
                    if (ScrollRect.horizontal && ScrollRect.horizontalScrollbar != null)
                    {
                        var rect = ScrollRect.horizontalScrollbar.GetComponent<RectTransform>().rect;
                        _height -= rect.height + ScrollRect.horizontalScrollbarSpacing;
                    }
                }
                return _height;
            }
        }

        protected int Rows
        {
            get
            {
                var rows = Mathf.FloorToInt((Width + _spacing.x) / (ItemSize.x + _spacing.x));
                if (rows <= 0)
                {
                    rows = 1;
                }
                return rows;
            }
        }

        protected int Cols
        {
            get
            {
                var cols = Mathf.FloorToInt((Height + _spacing.y) / (ItemSize.y + _spacing.y));
                if (cols <= 0)
                {
                    cols = 1;
                }
                return cols;
            }
        }

        protected override Task RebuildAsync()
        {
            var task = base.RebuildAsync();

            var scrollRectTransform = ScrollRect.GetComponent<RectTransform>();
            var rectTransform = Content.GetComponent<RectTransform>();
            var itemSize = _prefab.GetComponent<RectTransform>().sizeDelta;

            int cols;
            int rows;
            if (ScrollRect.vertical)
            {
                rows = Rows;
                cols = Mathf.CeilToInt((float)Count / rows);
                if (cols <= 0)
                {
                    cols = 1;
                }
            }
            else
            {
                cols = Cols;
                rows = Mathf.CeilToInt((float)Count / cols);
                if (rows <= 0)
                {
                    rows = 1;
                }
            }

            rectTransform.sizeDelta = new Vector2(
                (rows <= 1) ? rectTransform.sizeDelta.x : rows * (itemSize.x + _spacing.x) - _spacing.x - scrollRectTransform.rect.width,
                (cols <= 1) ? rectTransform.sizeDelta.y : cols * (itemSize.y + _spacing.y) - _spacing.y
            );

            return task;
        }
    }
}