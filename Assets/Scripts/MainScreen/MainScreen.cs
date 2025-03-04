using System;
using System.Collections.Generic;
using System.Linq;
using AddItem;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;
using DG.Tweening;

namespace MainScreen
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _emptyPlane;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private List<ItemPlane> _itemPlanes;
        [SerializeField] private AddItemScreen _addItemScreen;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.3f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private CanvasGroup _canvasGroup;
        private ScreenVisabilityHandler _screenVisabilityHandler;

        public event Action<ItemPlane> PlaneClicked;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            SetupButtonAnimations();
        }

        private void OnEnable()
        {
            _addItemScreen.ItemDataCreated += EnableItemPlane;
            _addItemScreen.BackButtonClicked += EnableScreen;
            _createButton.onClick.AddListener(AddItemClicked);
            _addButton.onClick.AddListener(AddItemClicked);

            foreach (ItemPlane itemPlane in _itemPlanes)
            {
                itemPlane.PlaneOpened += OnPlaneClicked;
            }
        }

        private void OnDisable()
        {
            _addItemScreen.ItemDataCreated -= EnableItemPlane;
            _addItemScreen.BackButtonClicked -= EnableScreen;
            _createButton.onClick.RemoveListener(AddItemClicked);
            _addButton.onClick.RemoveListener(AddItemClicked);

            foreach (ItemPlane itemPlane in _itemPlanes)
            {
                itemPlane.PlaneOpened -= OnPlaneClicked;
            }
        }

        private void Start()
        {
            DisableAllPlanes();
        }

        private void SetupButtonAnimations()
        {
            SetButtonInteractionAnimation(_createButton);
            SetButtonInteractionAnimation(_addButton);
            SetButtonInteractionAnimation(_settingsButton);
        }

        private void SetButtonInteractionAnimation(Button button)
        {
            if (button == null) return;

            button.onClick.AddListener(() =>
            {
                button.transform
                    .DOPunchScale(Vector3.one * 0.2f, _animationDuration, 2, 0.5f)
                    .SetEase(_animationEase);
            });
        }

        private void EnableItemPlane(ItemData data)
        {
            EnableScreen();

            var availablePlane = _itemPlanes.FirstOrDefault(plane => !plane.IsActive);

            if (availablePlane == null)
                throw new NullReferenceException(nameof(availablePlane));

            availablePlane.Enable(data);
            availablePlane.gameObject.transform
                .DOScale(Vector3.one, _animationDuration)
                .From(Vector3.zero)
                .SetEase(_animationEase);

            ToggleObjects();
        }

        private void DeletePlane(ItemPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            plane.gameObject.transform
                .DOScale(Vector3.zero, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    plane.Disable();
                    ToggleObjects();
                });
        }

        private void DisableAllPlanes()
        {
            foreach (var itemPlane in _itemPlanes)
            {
                itemPlane.gameObject.transform
                    .DOScale(Vector3.zero, _animationDuration)
                    .SetEase(_animationEase);

                itemPlane.Disable();
            }

            ToggleObjects();
        }

        private void ToggleObjects()
        {
            bool status = _itemPlanes.Any(plane => plane.IsActive);

            _emptyPlane.transform
                .DOScale(status ? Vector3.zero : Vector3.one, _animationDuration)
                .SetEase(_animationEase);

            _addButton.gameObject.transform
                .DOScale(status ? Vector3.one : Vector3.zero, _animationDuration)
                .SetEase(_animationEase);
        }

        public void EnableScreen()
        {
            _screenVisabilityHandler.EnableScreen();
            _canvasGroup
                .DOFade(1f, _animationDuration)
                .From(0f)
                .SetEase(_animationEase);
        }

        public void DisableScreen()
        {
            _canvasGroup
                .DOFade(0f, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() => { _screenVisabilityHandler.DisableScreen(); });
        }

        private void OnPlaneClicked(ItemPlane plane)
        {
            plane.gameObject.transform
                .DOPunchScale(Vector3.one * 0.1f, _animationDuration, 2, 0.5f)
                .SetEase(_animationEase);

            PlaneClicked?.Invoke(plane);
        }

        private void AddItemClicked()
        {
            _addItemScreen.Enable();
            DisableScreen();
        }
    }
}