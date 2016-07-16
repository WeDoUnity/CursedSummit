using UnityEngine;
using UnityEngine.UI;

namespace CursedSummit.UI
{
    /// <summary>
    /// Allows switching between two label texts and to bold style on toggle events
    /// </summary>
    [RequireComponent(typeof(Text), typeof(Toggle)), DisallowMultipleComponent]
    public class ExpandableText : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private string normal, toggled; //Normal/toggled text
        private Text label;             //Text component
        #endregion

        #region Methods
        /// <summary>
        /// Toggle event listener
        /// </summary>
        /// <param name="value">State of the toggle</param>
        public void OnToggle(bool value)
        {
            this.label.text = value ? this.toggled : this.normal;
            this.label.fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
        }

        /// <summary>
        /// Sets the label texts
        /// </summary>
        /// <param name="normal">Normal text</param>
        /// <param name="toggled">Toggled text</param>
        /// <param name="colour">Colour of the text</param>
        public void SetText(string normal, string toggled, Color colour)
        {
            this.label.text = this.label.text == this.normal ? normal : toggled;
            this.normal = normal;
            this.toggled = toggled;
            this.label.color = colour;

        }
        #endregion

        #region Functions
        private void Awake()
        {
            this.label = GetComponent<Text>();
            this.label.text = this.normal;
            GetComponent<Toggle>().onValueChanged.AddListener(OnToggle);
        }
        #endregion
    }
}
