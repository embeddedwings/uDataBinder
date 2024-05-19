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

        public Transform[] True = new Transform[] { };
        public Transform[] False = new Transform[] { };

        public override void Initialize()
        {
            base.Initialize();

            SetActive(false);
        }

        public override void Release()
        {
            SetActive(false);

            base.Release();
        }

        public virtual void SetActive(bool active)
        {
            if (True.Length == 0 && False.Length == 0)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(active);
                }
            }
            else
            {
                foreach (var child in True)
                {
                    child.gameObject.SetActive(active);
                }
                foreach (var child in False)
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