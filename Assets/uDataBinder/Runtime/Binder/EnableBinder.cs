using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(300)]
    public class EnableBinder : DataBinder
    {
        [SerializeField] protected string _condition;
        public string Condition
        {
            get { return _condition; }
            set
            {
                if (_condition == value)
                {
                    return;
                }

                DataBinding.Unregister(this);
                _condition = value;
                SetDirty();
            }
        }

        [SerializeField] protected MonoBehaviour[] _components = new MonoBehaviour[] { };

        public override void Initialize()
        {
            base.Initialize();

            SetEnable(false);
        }

        public virtual void SetEnable(bool enabled)
        {
            foreach (var component in _components)
            {
                component.enabled = enabled;
            }
        }

        protected override Task RebuildAsync()
        {
            var active = ConditionBinding.Parse(_condition, this);
            SetEnable(active);
            return Task.CompletedTask;
        }
    }
}