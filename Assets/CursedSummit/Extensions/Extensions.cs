using UnityEngine;

namespace CursedSummit.Extensions
{
    public static class UnityObjectExtensions
    {
        #region Methods
        /// <summary>
        /// Calls Object.Destroy on this Object
        /// </summary>
        /// <param name="obj">Object to destroy</param>
        public static void DestroyThis(this Object obj) => Object.Destroy(obj);
        #endregion
    }
}
