
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace uDataBinder.Binder
{
    public enum LoadType
    {
        None,
        Resources,
        Web,
        Addressables,
    }

    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Image))]
    public class ImageBinder : DataBinder
    {
        public enum ImageSize
        {
            Default,
            Native,
            Contain,
            Cover,
        }

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
        [SerializeField] protected ImageSize _size = ImageSize.Default;
        [SerializeField] protected bool _customPivot = false;

        private LoadType currentLoadType = LoadType.None;
        private Sprite currentSprite;


        public override void Initialize()
        {
            base.Initialize();

            Image.color = Color.clear;
        }

        protected void UnloadCurrentAsset()
        {
            if (currentSprite != null)
            {
                if (currentLoadType == LoadType.Resources)
                {
                    Resources.UnloadAsset(currentSprite);
                }
                else if (currentLoadType == LoadType.Web)
                {
                    WebLoader.UnloadAsset(currentSprite);
                }
#if UNITY_ADDRESSABLES
                else if (loadType == LoadType.Addressables)
                {
                    Addressables.Release(sprite);
                }
#endif
                currentSprite = null;
                currentLoadType = LoadType.None;
            }
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
            UnloadCurrentAsset();
            base.Release();
        }

        protected void SetSprite(Sprite sprite, LoadType loadType)
        {
            UnloadCurrentAsset();

            if (sprite != null)
            {
                Image.sprite = sprite;
                Image.color = _color;

                if (_size == ImageSize.Native)
                {
                    SetNativeSize(sprite);
                }
                else if (_size == ImageSize.Contain)
                {
                    SetContainSize(sprite);
                }
                else if (_size == ImageSize.Cover)
                {
                    SetCoverSize(sprite);
                }

                if (_customPivot)
                {
                    SetCustomPivot(sprite);
                }

                currentSprite = sprite;
                currentLoadType = loadType;
            }
            else
            {
                Image.sprite = null;
                Image.color = Color.clear;
            }
        }

        protected override async Task RebuildAsync()
        {
            var location = TemplateBinding.Parse(Location, this);
            
            if(string.IsNullOrEmpty(location))
            {
                SetSprite(null, LoadType.None);
                return;
            }

            if (location.StartsWith("http"))
            {
                var sprite = await WebLoader.LoadAsset<Sprite>(location);

                SetSprite(sprite, LoadType.Web);
                return;
            }

#if UNITY_ADDRESSABLES
            var handle = Addressables.LoadAssetAsync<Sprite>(location);
            handle.Completed += (data) => {
                Sprite sprite = null;
                if (data.Status == AsyncOperationStatus.Succeeded) {
                    sprite = data.Result;
                }

                SetSprite(sprite, LoadType.Addressables);
            };
            await handle.Task;
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

                SetSprite(sprite, LoadType.Resources);
                task.SetResult(sprite);
            };
            await task.Task;
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

        public virtual void SetContainSize(Sprite sprite)
        {
            if (RectTransform == null)
            {
                return;
            }

            var size = RectTransform.sizeDelta;
            var ratio = sprite.rect.width / sprite.rect.height;
            if (size.x / ratio > size.y)
            {
                RectTransform.sizeDelta = new Vector2(size.y * ratio, size.y);
            }
            else
            {
                RectTransform.sizeDelta = new Vector2(size.x, size.x / ratio);
            }
        }

        public virtual void SetCoverSize(Sprite sprite)
        {
            if (RectTransform == null)
            {
                return;
            }

            var size = RectTransform.sizeDelta;
            var ratio = sprite.rect.width / sprite.rect.height;
            if (size.x / ratio < size.y)
            {
                RectTransform.sizeDelta = new Vector2(size.y * ratio, size.y);
            }
            else
            {
                RectTransform.sizeDelta = new Vector2(size.x, size.x / ratio);
            }
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