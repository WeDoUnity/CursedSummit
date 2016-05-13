using UnityEngine;
using UnityEngine.UI;

namespace CursedSummit.UI
{
    /// <summary>
    /// A generic Progressbar helper class
    /// </summary>
    public class Progressbar : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private Text label; //Progressbar status label
        [SerializeField]
        private Image bar;  //Progressbar fill image
        #endregion

        #region Methods
        /// <summary>
        /// Sets the Progressbar label
        /// </summary>
        /// <param name="text">New label text</param>
        public void SetLabel(string text) => this.label.text = text;

        /// <summary>
        /// Sets the Progressbar fill
        /// </summary>
        /// <param name="progress">Current filled percentage (between 0 and 1)</param>
        public void SetProgress(float progress) => this.bar.fillAmount = Mathf.Clamp01(progress);
        #endregion
    }
}
