using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProBinder : DataBinder
    {
        protected TextMeshProUGUI _text;
        protected TextMeshProUGUI Text => this == null || _text != null ? _text : _text = GetComponent<TextMeshProUGUI>();

        [SerializeField][TextArea(3, 10)] protected string _templateText;
        public string TemplateText
        {
            get { return _templateText; }
            set
            {
                if (_templateText == value)
                {
                    return;
                }

                DataBinding.Unregister(this);
                _templateText = value;
                SetDirty();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Text.text = "";
        }

        public override void Release()
        {
            _text = null;

            base.Release();
        }

        protected override Task RebuildAsync()
        {
            Text.text = TemplateBinding.Parse(TemplateText, this);
            return Task.CompletedTask;
        }
    }
}
