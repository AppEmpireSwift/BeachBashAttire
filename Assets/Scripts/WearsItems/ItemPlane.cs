using System;
using System.Collections.Generic;
using System.Linq;
using PhotoPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WearsItems
{
    public class ItemPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _sectionName;
        [SerializeField] private TMP_Text _categoryName;
        [SerializeField] private Button _openButton;
        [SerializeField] private List<ImagePlacer> _images;

        public ItemData ItemData { get; private set; }
        public bool IsActive { get; private set; }

        public event Action<ItemPlane> PlaneOpened;

        private void OnEnable()
        {
            _openButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _openButton.onClick.RemoveListener(OnButtonClicked);
        }

        public void Enable(ItemData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            DisableAllImages();
            ItemData = data;
            IsActive = true;
            gameObject.SetActive(IsActive);

            Debug.Log(data.WearDatas.Count());

            _sectionName.text = ItemData.SectionName;
            _categoryName.text = ItemData.CategoryName;

            if (ItemData.WearDatas.Count <= 0)
                return;

            foreach (var itemDataWearData in ItemData.WearDatas)
            {
                var availableImage = _images.FirstOrDefault(image => !image.gameObject.activeSelf);

                if (availableImage != null)
                {
                    availableImage.gameObject.SetActive(true);
                    availableImage.SetImage(itemDataWearData.Photo);
                }
            }
        }

        public void Disable()
        {
            ResetPlane();

            IsActive = false;
            gameObject.SetActive(IsActive);
        }

        private void DisableAllImages()
        {
            foreach (var imagePlacer in _images)
            {
                imagePlacer.gameObject.SetActive(false);
            }
        }

        private void ResetPlane()
        {
            if (ItemData == null)
                return;

            _categoryName.text = string.Empty;
            _sectionName.text = string.Empty;
            DisableAllImages();

            ItemData = null;
        }

        private void OnButtonClicked()
        {
            PlaneOpened?.Invoke(this);
        }
    }
}