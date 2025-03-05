using System;
using System.Collections.Generic;
using System.Linq;
using AddItem;
using Description;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;
using DG.Tweening;

namespace OpenItem
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class OpenItemScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _selectionName;
        [SerializeField] private TMP_Text _categoryName;
        [SerializeField] private TMP_Text _timeOfDayInput;
        [SerializeField] private List<WearsPlane> _wearsPlanes;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private DescriptionScreen _descriptionScreen;
        [SerializeField] private AddItemScreen _editItemScreen;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.3f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private ItemPlane _itemPlane;
        private CanvasGroup _canvasGroup;

        public event Action BackClicked;
        public event Action<ItemPlane> DeleteClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);
            _descriptionScreen.BackClicked += _screenVisabilityHandler.EnableScreen;
            _editItemScreen.BackFromEditButtonClicked += _screenVisabilityHandler.EnableScreen;
            _editItemScreen.ItemDataUpdated += EnableScreen;
            _editButton.onClick.AddListener(OnEditClicked);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _deleteButton.onClick.RemoveListener(OnDeleteClicked);
            _descriptionScreen.BackClicked -= _screenVisabilityHandler.EnableScreen;
            _editItemScreen.BackFromEditButtonClicked -= _screenVisabilityHandler.EnableScreen;
            _editItemScreen.ItemDataUpdated -= EnableScreen;
            _editButton.onClick.RemoveListener(OnEditClicked);
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        public void EnableScreen(ItemPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            _itemPlane = plane;
            _screenVisabilityHandler.EnableScreen();

            _canvasGroup
                .DOFade(1f, _animationDuration)
                .From(0f)
                .SetEase(_animationEase);

            AnimateTextEntry(_selectionName, _itemPlane.ItemData.SectionName);
            AnimateTextEntry(_categoryName, _itemPlane.ItemData.CategoryName);
            AnimateTextEntry(_timeOfDayInput, _itemPlane.ItemData.TimeOfDay);

            AnimateWearPlanes();
        }

        public void OpenDescription(WearsPlane plane)
        {
            _descriptionScreen.Enable(plane.WearData);

            transform.DOScale(Vector3.zero, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() => { _screenVisabilityHandler.DisableScreen(); });
        }

        private void AnimateTextEntry(TMP_Text textComponent, string newText)
        {
            textComponent.transform.localScale = Vector3.zero;
            textComponent.text = newText;
            textComponent.transform.DOScale(Vector3.one, _animationDuration)
                .SetEase(_animationEase);
        }

        private void AnimateWearPlanes()
        {
            foreach (var wearsPlane in _wearsPlanes)
            {
                wearsPlane.gameObject.SetActive(false);
            }

            for (int i = 0; i < _itemPlane.ItemData.WearDatas.Count; i++)
            {
                var itemDataWearData = _itemPlane.ItemData.WearDatas[i];
                var availablePlane = _wearsPlanes.FirstOrDefault(plane => !plane.gameObject.activeSelf);

                if (availablePlane != null)
                {
                    availablePlane.Enable();
                    availablePlane.transform.localScale = Vector3.zero;

                    availablePlane.transform.DOScale(Vector3.one, _animationDuration)
                        .SetEase(_animationEase)
                        .SetDelay(i * 0.1f);

                    availablePlane.Enable(itemDataWearData);
                }
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
            _selectionName.text = string.Empty;
            _categoryName.text = string.Empty;
            _timeOfDayInput.text = string.Empty;

            foreach (var wearsPlane in _wearsPlanes)
            {
                wearsPlane.Disable();
            }
        }

        private void OnDeleteClicked()
        {
            DeleteClicked?.Invoke(_itemPlane);
            Disable();
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
            Disable();
        }

        private void OnEditClicked()
        {
            _editItemScreen.EnableEditMode(_itemPlane);
            _screenVisabilityHandler.DisableScreen();
        }
    }
}