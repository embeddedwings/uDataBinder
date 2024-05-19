using System.Threading.Tasks;
using uDataBinder.Binder;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class DataConverter : DataBinder
    {
        [SerializeField] protected string _target;
        [SerializeField] protected string _key = "$";

        protected virtual object Value => DataBinding.GetValue(_target, gameObject);

        protected virtual string ConvertedKey => _key;

        public override void Initialize()
        {
            base.Initialize();

            DataBinding.Register(_target, this, gameObject);
        }

        public override void Release()
        {
            DataBinding.Unset(ConvertedKey, gameObject);
            DataBinding.Unregister(this);

            base.Release();
        }

        protected virtual Task<object> ConvertAsync(object value)
        {
            return Task.FromResult(value);
        }

        protected virtual void RebuildNextFrame()
        {
            DataBinding.RebuildNextFrame(this);
        }

        protected override async Task RebuildAsync()
        {
            var convertedValue = await ConvertAsync(Value);
            DataBinding.Set(ConvertedKey, convertedValue, gameObject);
        }
    }
}
