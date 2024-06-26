using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(300)]
    public class ActiveBinder : DataBinder
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

        public Transform[] _true = new Transform[] { };
        public Transform[] _false = new Transform[] { };

        public override void Initialize()
        {
            base.Initialize();

            SetActive(false);
        }

        public virtual void SetActive(bool active)
        {
            if (_true.Length == 0 && _false.Length == 0)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(active);
                }
            }
            else
            {
                foreach (var child in _true)
                {
                    child.gameObject.SetActive(active);
                }
                foreach (var child in _false)
                {
                    child.gameObject.SetActive(!active);
                }
            }
        }

        protected override Task RebuildAsync()
        {
            var active = ConditionBinding.Parse(_condition, this);
            SetActive(active);
            return Task.CompletedTask;
        }
    }
}