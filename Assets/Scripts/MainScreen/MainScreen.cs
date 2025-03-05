using System;
using System.Collections.Generic;
using System.Linq;
using AddItem;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;
using DG.Tweening;
using OpenItem;
using SaveSystem;

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
        [SerializeField] private OpenItemScreen _openItemScreen;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.3f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private CanvasGroup _canvasGroup;
        private ScreenVisabilityHandler _screenVisabilityHandler;
        private ItemDataSaver _itemDataSaver;

        public event Action<ItemPlane> PlaneClicked;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            SetupButtonAnimations();
            _itemDataSaver = new ItemDataSaver();
        }

        private void OnEnable()
        {
            _addItemScreen.ItemDataCreated += EnableItemPlane;
            _addItemScreen.BackButtonClicked += EnableScreen;
            _addItemScreen.DataEdited += SaveItemPlanes;
            _createButton.onClick.AddListener(AddItemClicked);
            _addButton.onClick.AddListener(AddItemClicked);

            _openItemScreen.BackClicked += EnableScreen;
            _openItemScreen.DeleteClicked += DeletePlane;

            foreach (ItemPlane itemPlane in _itemPlanes)
            {
                itemPlane.PlaneOpened += OnPlaneClicked;
            }
        }

        private void OnDisable()
        {
            _addItemScreen.ItemDataCreated -= EnableItemPlane;
            _addItemScreen.BackButtonClicked -= EnableScreen;
            _addItemScreen.DataEdited -= SaveItemPlanes;
            _createButton.onClick.RemoveListener(AddItemClicked);
            _addButton.onClick.RemoveListener(AddItemClicked);

            _openItemScreen.BackClicked -= EnableScreen;
            _openItemScreen.DeleteClicked -= DeletePlane;

            foreach (ItemPlane itemPlane in _itemPlanes)
            {
                itemPlane.PlaneOpened -= OnPlaneClicked;
            }
        }

        private void Start()
        {
            DisableAllPlanes();
            LoadItemPlanes();
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
            SaveItemPlanes();
        }

        private void DeletePlane(ItemPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            EnableScreen();

            plane.gameObject.transform
                .DOScale(Vector3.zero, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    plane.Disable();
                    ToggleObjects();
                    SaveItemPlanes();
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

        private void OnPlaneClicked(ItemPlane plane)
        {
            plane.gameObject.transform
                .DOPunchScale(Vector3.one * 0.1f, _animationDuration, 2, 0.5f)
                .SetEase(_animationEase);

            PlaneClicked?.Invoke(plane);
            _openItemScreen.EnableScreen(plane);
            DisableScreen();
        }

        private void AddItemClicked()
        {
            _addItemScreen.Enable();
            DisableScreen();
        }

        private void SaveItemPlanes()
        {
            try
            {
                List<ItemData> activeDatas = _itemPlanes
                    .Where(plane => plane.IsActive)
                    .Select(plane => plane.ItemData)
                    .ToList();

                _itemDataSaver.SaveData(activeDatas);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void LoadItemPlanes()
        {
            try
            {
                List<ItemData> loadedDatas = _itemDataSaver.LoadData();

                if (loadedDatas == null || loadedDatas.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < loadedDatas.Count; i++)
                {
                    if (i >= _itemPlanes.Count)
                    {
                        break;
                    }

                    _itemPlanes[i].Enable(loadedDatas[i]);
                    _itemPlanes[i].gameObject.transform
                        .DOScale(Vector3.one, _animationDuration)
                        .From(Vector3.zero)
                        .SetEase(_animationEase);
                }

                ToggleObjects();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}