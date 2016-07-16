using System;
using System.Collections.Generic;
using System.Linq;

namespace CursedSummit.Loading
{
    /// <summary>
    /// Read-only wrapper class around a list of loaded objects and a
    /// path-><typeparamref name="T"/>[] dictionary. IEnumerable implementation around the 
    /// list for usage with LINQ extensions.
    /// </summary>
    /// <typeparam name="T">Type of object stored in this data strucure</typeparam>
    public class JsonLoaderList<T> : LoaderList<T>
    {
        #region Fields
        //Path -> objects[] map
        protected new readonly Dictionary<string, T[]> files;
        #endregion

        #region Properties
        /// <summary>
        /// New array containing all the file paths of the loaded objects
        /// </summary>
        public new string[] Paths => this.files.Keys.ToArray();
        #endregion

        #region Indexers
        /// <summary>
        /// Obtains the objects associated to this given path
        /// </summary>
        /// <param name="path">Object path</param>
        /// <returns>Objects at this path</returns>
        public new T[] this[string path] => this.files[path];
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new JsonLoaderList with the given objects and files data structures
        /// </summary>
        /// <param name="objects">Loaded objects list</param>
        /// <param name="files">Path -> objects[] dictionary</param>
        public JsonLoaderList(List<T> objects, Dictionary<string, T[]> files) : base(objects)
        {
            //Ensures we are not using extra memory space
            this.files = new Dictionary<string, T[]>(files);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if an object loaded at the given path exists within the loaded objects
        /// </summary>
        /// <param name="path">Path to look for</param>
        /// <returns>If an object at this given path has been loaded</returns>
        public new bool ContainsPath(string path) => this.files.ContainsKey(path);

        /// <summary>
        /// Unused overriden method to hide base class equivalent
        /// </summary>
        [Obsolete("Unused, use JsonLoaderList<T>.TryGetObjects(string, out T[]) instead.", true)]
        private new bool TryGetObject(string path, out T item)
        {
            item = default(T);
            return false;
        }

        /// <summary>
        /// Tries to find the objects loaded at the given path, and stores it in the out parameter. Else, stores the default value.
        /// </summary>
        /// <param name="path">Path to look for</param>
        /// <param name="items">Out var where the results are stored</param>
        /// <returns>If the objects have been found</returns>
        public bool TryGetObjects(string path, out T[] items) => this.files.TryGetValue(path, out items);
        #endregion
    }
}
