using System.IO;
using UnityEngine;

namespace CursedSummit.Loading.Loaders
{
    /// <summary>
    /// A .png texture loader
    /// </summary>
    public class TextureLoader : Loader<Texture2D>, IMultipleExtensions
    {
        #region Constants
        string[] IMultipleExtensions.ExtraExtensions => ExtraExtensions;
        /// <summary>
        /// Extra file extensions to load
        /// </summary>
        private static readonly string[] ExtraExtensions = { ".jpg" };
        #endregion

        #region Properties
        /// <summary>
        /// Loader name
        /// </summary>
        protected override string Name => "TextureLoader";

        /// <summary>
        /// Loader file extension
        /// </summary>
        protected override string Extension => ".png";
        #endregion

        #region Overrides
        /// <summary>
        /// Loading callback
        /// </summary>
        /// <param name="file">File to load</param>
        /// <returns>The loaded texture, or null if it failed.</returns>
        public override Texture2D LoadObject(FileInfo file)
        {
            Texture2D texture = new Texture2D(1, 1);
            return texture.LoadImage(File.ReadAllBytes(file.FullName)) ? texture : null;
        }
        #endregion
    }
}
