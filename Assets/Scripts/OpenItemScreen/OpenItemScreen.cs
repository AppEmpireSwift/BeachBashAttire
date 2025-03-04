using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WearsItems;

namespace OpenItemScreen
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
        
        private 
    }
}