using UnityEngine;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(100)]
    public class AliasGroup : DataBinder
    {
        [SerializeField] protected string _alias;
        public virtual string Alias
        {
            get => _alias;
            set
            {
                if (_alias != value)
                {
                    _alias = value;
                    _value = null;
                    SetDirty();
                }
            }
        }

        protected object _value;
        public virtual object Value => _value ?? DataBinding.GetValue(Alias, gameObject);
    }
}