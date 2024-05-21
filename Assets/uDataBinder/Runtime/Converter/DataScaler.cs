using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Converter
{
    [DefaultExecutionOrder(200)]
    public class DataScaler : DataConverter
    {
        [SerializeField] protected float _scale = 2.0f;

        protected override Task<object> ConvertAsync(object v)
        {
            switch (v)
            {
                case int i:
                    return Task.FromResult(i * _scale as object);
                case long l:
                    return Task.FromResult(l * _scale as object);
                case float f:
                    return Task.FromResult(f * _scale as object);
                case double d:
                    return Task.FromResult(d * _scale as object);
                case Vector2 vec2:
                    return Task.FromResult(vec2 * _scale as object);
                case Vector3 vec3:
                    return Task.FromResult(vec3 * _scale as object);
                case Vector4 vec4:
                    return Task.FromResult(vec4 * _scale as object);
            }

            var value = DataBinding.ConvertValue<float>(v);
            return Task.FromResult(value * _scale as object);
        }
    }
}