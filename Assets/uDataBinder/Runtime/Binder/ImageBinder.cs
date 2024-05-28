using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
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

            if (location.StartsWith("http"))
            {
                using var request = UnityWebRequestTexture.GetTexture(location);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    SetSprite(sprite);
                }
                return;
            }

#if UNITY_ADDRESSABLES
            var handle = Addressables.LoadAssetAsync<Sprite>(location);
            handle.Completed += (data) => {
                Sprite sprite = null;
                if (data.Status == AsyncOperationStatus.Succeeded) {
                    sprite = data.Result;
                }

                SetSprite(sprite);
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

                SetSprite(sprite);
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