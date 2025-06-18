using System;
using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(210)]
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
                    var fStr = f.ToString($"F{_digits}");
                    return Task.FromResult(fStr.Substring(fStr.IndexOf('.') + 1, _digits) as object);
                case double d:
                    var dStr = d.ToString($"F{_digits}");
                    return Task.FromResult(dStr.Substring(dStr.IndexOf('.') + 1, _digits) as object);
            }

            var value = DataBinding.ConvertValue<float>(v);
            return Task.FromResult(Math.Floor((value - (int)value) * Mathf.Pow(10, _digits)).ToString(zeros) as object);
        }
    }
}