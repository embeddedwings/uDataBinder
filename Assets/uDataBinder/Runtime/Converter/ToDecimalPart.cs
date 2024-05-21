using System;
using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class ToDecimalPart : DataConverter
    {
        [SerializeField] protected int _digits = 2;

        protected override Task<object> ConvertAsync(object v)
        {
            var zeros = new string('0', _digits);
            switch (v)
            {
                case int:
                case long:
                    return Task.FromResult(zeros as object);
                case float f:
                    return Task.FromResult(Math.Floor((f - (int)f) * Mathf.Pow(10, _digits)).ToString(zeros) as object);
                case double d:
                    return Task.FromResult(Math.Floor((d - (int)d) * Mathf.Pow(10, _digits)).ToString(zeros) as object);
            }

            var value = DataBinding.ConvertValue<float>(v);
            return Task.FromResult(Math.Floor((value - (int)value) * Mathf.Pow(10, _digits)).ToString(zeros) as object);
        }
    }
}