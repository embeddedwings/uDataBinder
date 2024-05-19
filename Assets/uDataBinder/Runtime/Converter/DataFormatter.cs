using System;
using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class DataFormatter : DataConverter
    {
        [SerializeField] protected string _format = "0.00";

        protected override Task<object> ConvertAsync(object v)
        {
            switch (v)
            {
                case int i:
                    return Task.FromResult(i.ToString(_format) as object);
                case float f:
                    return Task.FromResult(f.ToString(_format) as object);
                case double d:
                    return Task.FromResult(d.ToString(_format) as object);
                case DateTime dt:
                    return Task.FromResult(dt.ToString(_format) as object);
            }

            var value = DataBinding.ConvertValue<float>(v);
            return Task.FromResult(value.ToString(_format) as object);
        }
    }
}
