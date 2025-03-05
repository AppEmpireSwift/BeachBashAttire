using System;
using PhotoPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;
using DG.Tweening;

namespace Description
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class DescriptionScreen : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _materialsText;
        [SerializeField] private ImagePlacer _imagePlacer;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.3f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private CanvasGroup _canvasGroup;

        public event Action BackClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        public void Enable(WearData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _screenVisabilityHandler.EnableScreen();

            _canvasGroup
                .DOFade(1f, _animationDuration)
                .From(0f)
                .SetEase(_animationEase);

            _nameText.transform.localScale = Vector3.zero;
            _nameText.text = data.Name;
            _nameText.transform.DOScale(Vector3.one, _animationDuration)
                .SetEase(_animationEase)
                .SetDelay(_animationDuration * 0.2f);

            _materialsText.transform.localScale = Vector3.zero;
            _materialsText.text = data.Materials;
            _materialsText.transform.DOScale(Vector3.one, _animationDuration)
                .SetEase(_animationEase)
                .SetDelay(_animationDuration * 0.4f);

            if (data.Photo != null)
            {
                _imagePlacer.transform.localScale = Vector3.zero;
                _imagePlacer.SetImage(data.Photo);
                _imagePlacer.transform.DOScale(Vector3.one, _animationDuration)
                    .SetEase(_animationEase)
                    .SetDelay(_animationDuration * 0.6f);
            }
        }

        public void Disable()
        {
            _canvasGroup
                .DOFade(0f, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    _screenVisabilityHandler.DisableScreen();
                    ResetScreen();
                });
        }

        private void ResetScreen()
        {
            _nameText.text = string.Empty;
            _materialsText.text = string.Empty;
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
            Disable();
        }
    }
}