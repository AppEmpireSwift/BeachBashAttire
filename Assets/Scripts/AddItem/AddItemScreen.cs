using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;
using DG.Tweening;

namespace AddItem
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class AddItemScreen : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_InputField _sectionNameInput;
        [SerializeField] private TMP_InputField _categoryInput;
        [SerializeField] private TMP_InputField _timeOfDayInput;
        [SerializeField] private List<WearsPlane> _wearsPlanes;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _addWearButton;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.5f;

        [SerializeField] private float _delayBetweenElements = 0.1f;
        [SerializeField] private Ease _animationEase = Ease.OutBack;
        [SerializeField] private Vector2 _startPosition = new Vector2(0, 100);

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private List<RectTransform> _uiElements = new List<RectTransform>();
        private Vector3[] _originalPositions;
        private Sequence _showSequence;
        private Sequence _hideSequence;

        public event Action<ItemData> ItemDataCreated;
        public event Action BackButtonClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _rectTransform = GetComponent<RectTransform>();

            CollectUIElements();
            StoreOriginalPositions();
        }

        private void Start()
        {
            Disable();
        }

        private void OnEnable()
        {
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _addWearButton.onClick.AddListener(OnAddWearClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);

            _sectionNameInput.onValueChanged.AddListener(_ => UpdateSaveButtonState());
            _categoryInput.onValueChanged.AddListener(_ => UpdateSaveButtonState());
            _timeOfDayInput.onValueChanged.AddListener(_ => UpdateSaveButtonState());

            foreach (var wearsPlane in _wearsPlanes)
            {
                wearsPlane.InputedData += UpdateSaveButtonState;
            }
        }

        private void OnDisable()
        {
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            _addWearButton.onClick.RemoveListener(OnAddWearClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            _sectionNameInput.onValueChanged.RemoveAllListeners();
            _categoryInput.onValueChanged.RemoveAllListeners();
            _timeOfDayInput.onValueChanged.RemoveAllListeners();

            foreach (var wearsPlane in _wearsPlanes)
            {
                wearsPlane.InputedData -= UpdateSaveButtonState;
            }

            KillAllTweens();
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
            AnimateScreenIn();
            UpdateSaveButtonState();
        }

        public void Disable()
        {
            AnimateScreenOut(() =>
            {
                _screenVisabilityHandler.DisableScreen();
                ResetScreen();
            });
        }

        private void CollectUIElements()
        {
            if (_backButton != null) _uiElements.Add(_backButton.GetComponent<RectTransform>());
            if (_sectionNameInput != null) _uiElements.Add(_sectionNameInput.GetComponent<RectTransform>());
            if (_categoryInput != null) _uiElements.Add(_categoryInput.GetComponent<RectTransform>());
            if (_timeOfDayInput != null) _uiElements.Add(_timeOfDayInput.GetComponent<RectTransform>());
            if (_saveButton != null) _uiElements.Add(_saveButton.GetComponent<RectTransform>());

            foreach (var plane in _wearsPlanes)
            {
                if (plane != null) _uiElements.Add(plane.GetComponent<RectTransform>());
            }
        }

        private void StoreOriginalPositions()
        {
            _originalPositions = new Vector3[_uiElements.Count];
            for (int i = 0; i < _uiElements.Count; i++)
            {
                _originalPositions[i] = _uiElements[i].anchoredPosition3D;
            }
        }

        private void KillAllTweens()
        {
            if (_showSequence != null && _showSequence.IsActive())
            {
                _showSequence.Kill();
            }

            if (_hideSequence != null && _hideSequence.IsActive())
            {
                _hideSequence.Kill();
            }

            foreach (var element in _uiElements)
            {
                DOTween.Kill(element);
            }

            DOTween.Kill(_canvasGroup);
            DOTween.Kill(_rectTransform);
        }

        private void AnimateScreenIn()
        {
            KillAllTweens();

            _canvasGroup.alpha = 0;
            for (int i = 0; i < _uiElements.Count; i++)
            {
                if (_uiElements[i] != null)
                {
                    _uiElements[i].anchoredPosition = _originalPositions[i] + (Vector3)_startPosition;
                    _uiElements[i].localScale = Vector3.one * 0.8f;
                }
            }

            _showSequence = DOTween.Sequence();

            _showSequence.Append(_canvasGroup.DOFade(1, _animationDuration * 0.5f));

            for (int i = 0; i < _uiElements.Count; i++)
            {
                int index = i;
                if (_uiElements[index] != null)
                {
                    _showSequence.Insert(_delayBetweenElements * index,
                        _uiElements[index].DOAnchorPos(_originalPositions[index], _animationDuration)
                            .SetEase(_animationEase));
                    _showSequence.Insert(_delayBetweenElements * index,
                        _uiElements[index].DOScale(1, _animationDuration).SetEase(_animationEase));
                }
            }

            _showSequence.Play();
        }

        private void AnimateScreenOut(Action onComplete = null)
        {
            KillAllTweens();

            _hideSequence = DOTween.Sequence();

            for (int i = _uiElements.Count - 1; i >= 0; i--)
            {
                int index = i;
                if (_uiElements[index] != null)
                {
                    Vector2 targetPos = _originalPositions[index] - (Vector3)_startPosition;
                    _hideSequence.Insert(0,
                        _uiElements[index].DOAnchorPos(targetPos, _animationDuration * 0.7f).SetEase(Ease.InBack));
                    _hideSequence.Insert(0,
                        _uiElements[index].DOScale(0.8f, _animationDuration * 0.7f).SetEase(Ease.InBack));
                }
            }

            _hideSequence.Append(_canvasGroup.DOFade(0, _animationDuration * 0.3f));

            if (onComplete != null)
            {
                _hideSequence.OnComplete(() => onComplete.Invoke());
            }

            _hideSequence.Play();
        }

        private void ResetScreen()
        {
            _sectionNameInput.text = string.Empty;
            _categoryInput.text = string.Empty;
            _timeOfDayInput.text = string.Empty;
            DisableAllPlanes();
        }

        private bool GetSaveButtonStatus()
        {
            return !string.IsNullOrEmpty(_sectionNameInput.text) && !string.IsNullOrEmpty(_categoryInput.text) &&
                   !string.IsNullOrEmpty(_timeOfDayInput.text) && _wearsPlanes.Any(item => item.IsActive);
        }

        private void UpdateSaveButtonState()
        {
            var isValid = GetSaveButtonStatus();

            if (_saveButton.interactable != isValid)
            {
                _saveButton.interactable = isValid;

                if (isValid)
                {
                    _saveButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
                }
            }
        }

        private void OnSaveButtonClicked()
        {
            _saveButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 3, 0.5f).OnComplete(() =>
            {
                List<WearData> wearDatas = new List<WearData>();
                foreach (var wearsPlane in _wearsPlanes.Where(plane => plane.IsActive))
                {
                    var wearData = wearsPlane.GetData();
                    if (wearData != null)
                    {
                        wearDatas.Add(wearData);
                    }
                }

                var data = new ItemData(_sectionNameInput.text, _categoryInput.text, _timeOfDayInput.text, wearDatas);
                ItemDataCreated?.Invoke(data);
                Disable();
            });
        }

        private void OnAddWearClicked()
        {
            _addWearButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 3, 0.5f).OnComplete(() =>
            {
                var availablePlane = _wearsPlanes.FirstOrDefault(plane => !plane.IsActive);

                if (availablePlane != null)
                {
                    RectTransform planeRect = availablePlane.GetComponent<RectTransform>();
                    Vector3 originalPos = planeRect.anchoredPosition3D;
                    Vector3 originalScale = planeRect.localScale;

                    planeRect.anchoredPosition3D = originalPos + new Vector3(0, 50, 0);
                    planeRect.localScale = Vector3.one * 0.8f;

                    availablePlane.Enable();

                    DOTween.Sequence()
                        .Append(planeRect.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutBack))
                        .Join(planeRect.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack));
                }
            });
        }

        private void DisableAllPlanes()
        {
            foreach (var wearsPlane in _wearsPlanes)
            {
                wearsPlane.Disable();
            }
        }

        private void OnBackButtonClicked()
        {
            BackButtonClicked?.Invoke();
            Disable();
        }
    }
}