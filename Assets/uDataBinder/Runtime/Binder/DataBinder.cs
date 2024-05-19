using System.Threading.Tasks;
using UnityEngine;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(500)]
    public class DataBinder : MonoBehaviour
    {
        protected int _hierarchyLevel;
        public int HierarchyLevel
        {
            get
            {
                if (_hierarchyLevel == 0)
                {
                    var parent = gameObject.transform.parent;
                    for (var i = 0; ; ++i)
                    {
                        if (parent == null)
                        {
                            _hierarchyLevel = i;
                            break;
                        }
                        parent = parent.parent;
                    }
                }
                return _hierarchyLevel;
            }
        }

        protected bool _isInitialized;

        protected Task _rebuildTask = Task.CompletedTask;

        protected void OnEnable()
        {
            Initialize();
        }

        protected void OnDisable()
        {
            Release();
        }

        protected void OnValidate()
        {
            if (_isInitialized)
            {
                Release();
                Initialize();
            }
        }

        public virtual void Initialize()
        {
            SetDirty();
            _isInitialized = true;
        }

        public virtual void Release()
        {
            _isInitialized = false;
            DataBinding.Unregister(this);

            if (!_rebuildTask.IsCompleted)
            {
                _rebuildTask.Dispose();
                _rebuildTask = Task.CompletedTask;
            }
        }

        protected virtual Task RebuildAsync()
        {
            return Task.CompletedTask;
        }

        public Task Rebuild()
        {
            if (!_rebuildTask.IsCompleted)
            {
                _rebuildTask.Dispose();
                _rebuildTask = Task.CompletedTask;
            }

            return _rebuildTask = RebuildAsync();
        }

        public void SetDirty(bool force = false)
        {
            DataBinding.LateRebuild(this, force);
        }
    }
}