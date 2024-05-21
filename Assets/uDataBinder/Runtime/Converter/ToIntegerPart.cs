using System;
using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class ToIntegerPart : DataConverter
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
                    return Task.FromResult(((int)f).ToString(zeros) as object);
                case double d:
                    return Task.FromResult(((int)d).ToString(zeros) as object);
            }

            var value = DataBinding.ConvertValue<int>(v);
            return Task.FromResult(value.ToString(zeros) as object);
        }
    }
}