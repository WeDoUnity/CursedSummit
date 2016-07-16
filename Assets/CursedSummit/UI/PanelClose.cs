using UnityEngine;
using UnityEngine.UI;

namespace CursedSummit.UI
{
    /// <summary>
    /// Closes the given panel on button click
    /// </summary>
    [RequireComponent(typeof(Button)), AddComponentMenu("UI/Panel Close"), DisallowMultipleComponent]
    public class PanelClose : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private GameObject panel;   //Panel to close
        #endregion

        #region Methods
        /// <summary>
        /// Event to fire on button click
        /// </summary>
        private void OnClick() => this.panel.SetActive(false);
        #endregion

        #region Functions
        private void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);
        #endregion
    }
}
