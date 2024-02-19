using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Image))]
    public class ImageBinder : DataBinder
    {
        protected Image _image;
        protected Image Image => this == null || _image != null ? _image : _image = GetComponent<Image>();

        protected RectTransform _rectTransform;
        protected RectTransform RectTransform => this == null || _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeField] protected string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                if (_location == value)
                {
                    return;
                }

                DataBinding.Unregister(this);
                _location = value;
                SetDirty();
            }
        }

        [SerializeField] protected Color _color = Color.white;

        [SerializeField] protected bool _nativeSize = false;
        [SerializeField] protected bool _customPivot = false;

        public override void Initialize()
        {
            base.Initialize();

            Image.color = Color.clear;
        }

        public override void Release()
        {
            if (_image != null)
            {
                _image.color = Color.clear;
                _image.sprite = null;
            }
            _image = null;
            _rectTransform = null;

            base.Release();
        }

        protected void SetSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                Image.sprite = sprite;
                Image.color = _color;

                if (_nativeSize)
                {
                    SetNativeSize(sprite);
                }
                if (_customPivot)
                {
                    SetCustomPivot(sprite);
                }
            }
            else
            {
                Image.sprite = null;
                Image.color = Color.clear;
            }
        }

        protected override Task RebuildAsync()
        {
            var location = TemplateBinding.Parse(Location, this);
#if UNITY_ADDRESSABLES
            var handle = Addressables.LoadAssetAsync<Sprite>(location);
            handle.Completed += (data) => {
                Sprite sprite = null;
                if (data.Status == AsyncOperationStatus.Succeeded) {
                    sprite = data.Result;
                }

                SetSprite(sprite);
            };
            return handle.Task;
#else
            var handle = Resources.LoadAsync<Sprite>(location);
            var task = new TaskCompletionSource<Sprite>();
            handle.completed += (op) =>
            {
                Sprite sprite = null;
                if (op.isDone)
                {
                    sprite = handle.asset as Sprite;
                }

                SetSprite(sprite);
                task.SetResult(sprite);
            };
            return task.Task;
#endif
        }

        public virtual void SetCustomPivot(Sprite sprite)
        {
            if (RectTransform == null)
            {
                return;
            }
            RectTransform.pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
        }

        public virtual void SetNativeSize(Sprite sprite)
        {
            if (RectTransform == null)
            {
                return;
            }
            RectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
        }
    }
}