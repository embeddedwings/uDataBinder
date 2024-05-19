using System;
using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class DataAnimation : DataConverter
    {
        [SerializeField] protected float _duration = 1f;
        [SerializeField] protected AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected float _currentValue = 0;
        protected float _fromValue = -1;
        protected float _toValue = -1;
        protected float _startTime = 0f;

        protected float Animate(float from, float to, float time)
        {
            return from + (to - from) * time;
        }

        protected override Task<object> ConvertAsync(object v)
        {
            var value = DataBinding.ConvertValue<float>(v);

            if (_currentValue == value)
            {
                return Task.FromResult(value as object);
            }

            var now = Time.time;
            if (_toValue != value)
            {
                _fromValue = _currentValue;
                _toValue = value;
                _startTime = now;
            }

            var time = Math.Clamp((now - _startTime) / _duration, 0, 1);
            _currentValue = Animate(_fromValue, _toValue, _curve.Evaluate(time));

            if (time < 1)
            {
                RebuildNextFrame();
            }
            return Task.FromResult(_currentValue as object);
        }
    }
}
