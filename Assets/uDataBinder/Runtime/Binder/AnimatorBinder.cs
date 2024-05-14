using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Binder
{
    public enum AnimatorBinderType
    {
        Bool,
        Float,
        Int,
    }

    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Animator))]
    public class AnimatorBinder : DataBinder
    {
        protected Animator _animator;
        protected Animator Animator => this == null || _animator != null ? _animator : _animator = GetComponent<Animator>();

        [SerializeField] protected AnimatorBinderType _type = AnimatorBinderType.Int;
        [SerializeField] protected string _name;
        [SerializeField] protected string _value;

        public override void Initialize()
        {
            base.Initialize();

            DataBinding.Register(_value, this);
        }

        public override void Release()
        {
            DataBinding.Unregister(this);
            _animator = null;

            base.Release();
        }

        protected override Task RebuildAsync()
        {
            Debug.Log("RebuildAsync");
            switch (_type)
            {
                case AnimatorBinderType.Bool:
                    Animator.SetBool(_name, ConditionBinding.Parse(_value, this));
                    break;
                case AnimatorBinderType.Float:
                    Animator.SetFloat(_name, DataBinding.GetValue<float>(_value));
                    break;
                case AnimatorBinderType.Int:
                    Animator.SetInteger(_name, DataBinding.GetValue<int>(_value));
                    break;
            }
            return Task.CompletedTask;
        }
    }
}