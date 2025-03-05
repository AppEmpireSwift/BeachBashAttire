using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WearsItems
{
    public class WearsPlane : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _selectionNameInput;
        [SerializeField] private TMP_InputField _materialsNameInput;
        [SerializeField] private Button _removeButton;
        [SerializeField] private PhotosController _photosController;

        public event Action InputedData;

        public bool IsActive { get; private set; }
        public WearData WearData { get; private set; }

        private void OnEnable()
        {
            _selectionNameInput.onValueChanged.AddListener((_ => InputedData?.Invoke()));
            _materialsNameInput.onValueChanged.AddListener((_ => InputedData?.Invoke()));
        }

        private void OnDisable()
        {
            _selectionNameInput.onValueChanged.RemoveAllListeners();
            _materialsNameInput.onValueChanged.RemoveAllListeners();
        }

        public void Enable()
        {
            IsActive = true;
            gameObject.SetActive(IsActive);
            ResetPlane();
        }

        public void Enable(WearData data)
        {
            WearData = data ?? throw new ArgumentNullException(nameof(data));

            ResetPlane();

            IsActive = true;
            gameObject.SetActive(IsActive);

            _selectionNameInput.text = data.Name;
            _materialsNameInput.text = data.Materials;

            if (data.Photo == null)
                return;

            _photosController.SetPhotos(data.Photo);
        }

        public WearData GetData()
        {
            if (string.IsNullOrEmpty(_selectionNameInput.text) && string.IsNullOrEmpty(_materialsNameInput.text) &&
                _photosController.GetPhoto() == null)
                return null;

            return new WearData(_selectionNameInput.text, _materialsNameInput.text,
                _photosController.GetPhoto());
        }

        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(IsActive);
            ResetPlane();
        }

        private void ResetPlane()
        {
            _selectionNameInput.text = string.Empty;
            _materialsNameInput.text = string.Empty;
            _photosController.ResetPhotos();
        }
    }
}