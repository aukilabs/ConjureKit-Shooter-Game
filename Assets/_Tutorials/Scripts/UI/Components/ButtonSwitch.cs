using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ConjureKitShooter.UI.Components
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class ButtonSwitch : MonoBehaviour
    {
        [SerializeField] private Sprite toggleImage;

        private Sprite _baseImage;
        private Image _image;
        private Button _button;

        private bool _toggled;

        // Start is called before the first frame update
        void Awake()
        {
            _image = GetComponent<Image>();
            _baseImage = _image.sprite;
            _button = GetComponent<Button>();
        
            _button.onClick.AddListener(ToggleSprite);
        }

        private void ToggleSprite()
        {
            _toggled = !_toggled;
            _image.sprite = _toggled ? toggleImage : _baseImage;
        }

        public void AddListener(UnityAction call)
        {
            _button.onClick.AddListener(call);
        }

        public void RemoveListener(UnityAction call)
        {
            _button.onClick.RemoveListener(call);
        }

        public void RemoveAllListeners()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(ToggleSprite);
        }
    }
}