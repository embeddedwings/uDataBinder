using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformBinder : DataBinder
    {
        protected RectTransform _rectTransform;
        protected RectTransform RectTransform => this == null || _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeField] protected string _x = "";
        [SerializeField] protected string _y = "";
        [SerializeField] protected string _width = "";
        [SerializeField] protected string _height = "";

        public override void Initialize()
        {
            base.Initialize();

            DataBinding.Register(_x, this, gameObject);
            DataBinding.Register(_y, this, gameObject);
            DataBinding.Register(_width, this, gameObject);
            DataBinding.Register(_height, this, gameObject);
        }

        public override void Release()
        {
            DataBinding.Unregister(this);

            base.Release();
        }

        protected virtual float GetFloat(string key, float defaultValue = 0)
        {
            return string.IsNullOrEmpty(key) ? defaultValue : DataBinding.GetValue<float>(key, gameObject);
        }

        protected override Task RebuildAsync()
        {
            if (_x != "" || _y != "")
            {
                var position = RectTransform.anchoredPosition;
                RectTransform.anchoredPosition = new Vector2(GetFloat(_x, position.x), GetFloat(_y, position.y));
            }

            if (_width != "" || _height != "")
            {
                var size = RectTransform.sizeDelta;
                RectTransform.sizeDelta = new Vector2(GetFloat(_width, size.x), GetFloat(_height, size.y));
            }

            return Task.CompletedTask;
        }
    }
}