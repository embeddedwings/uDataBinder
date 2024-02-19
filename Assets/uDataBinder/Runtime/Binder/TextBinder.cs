using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace uDataBinder.Binder
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Text))]
    public class TextBinder : DataBinder
    {
        protected Text _text;
        protected Text Text => this == null || _text != null ? _text : _text = GetComponent<Text>();

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