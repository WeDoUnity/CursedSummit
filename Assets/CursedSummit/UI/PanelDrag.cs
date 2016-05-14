using CursedSummit.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CursedSummit.UI
{
    [RequireComponent(typeof(RectTransform))]
     public class PanelDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        #region Fields
        private Vector2 originalMousePos, originalPanelPos;
        private RectTransform panelTransform, parentTransform;
        #endregion

        #region Methods
        /// <summary>
        /// Fires on mouse down over this transform
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnPointerDown(PointerEventData data)
        {
            this.originalPanelPos = this.panelTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.parentTransform, data.position, data.pressEventCamera, out this.originalMousePos);
        }

        /// <summary>
        /// Fires on mouse drag
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnDrag(PointerEventData data)
        {
            //Move window
            Vector2 mousePos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.parentTransform, data.position, data.pressEventCamera, out mousePos))
            {
                Vector2 offset = mousePos - this.originalMousePos;
                this.panelTransform.localPosition = this.originalPanelPos + offset;
            }

            //Clamp window 
            Vector2 pos = this.panelTransform.localPosition;
            Vector2 minPos = this.parentTransform.rect.min - this.panelTransform.rect.min;
            Vector2 maxPos = this.parentTransform.rect.max - this.panelTransform.rect.max;
            this.panelTransform.localPosition = CSUtils.ClampVector2(pos, minPos, maxPos);
        }
        #endregion

        #region Functions
        private void Awake()
        {
            this.panelTransform = (RectTransform)this.transform.parent;
            this.parentTransform = (RectTransform)this.panelTransform.parent;
        }
        #endregion
    }
}
