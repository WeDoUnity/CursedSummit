using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CursedSummit.Loading
{
    /// <summary>
    /// Read-only wrapper class around a list of loaded objects and a
    /// path-><typeparamref name="T"/> dictionary. IEnumerable implementation around the 
    /// list for usage with LINQ extensions.
    /// </summary>
    /// <typeparam name="T">Type of object stored in this data structure</typeparam>
    public class LoaderList<T> : IEnumerable<T>
    {
        #region Fields
        protected readonly List<T> objects;               //All objects
        protected readonly Dictionary<string, T> files;   //Path -> object map
        #endregion

        #region Properties
        /// <summary>
        /// Amount of objects currently loaded
        /// </summary>
        public int Count => this.objects.Count;

        /// <summary>
        /// New array containing all the file paths of the loaded objects
        /// </summary>
        public string[] Paths => this.files.Keys.ToArray();
        #endregion

        #region Indexers
        /// <summary>
        /// Obtains the object at the given index in the loaded objects list
        /// </summary>
        /// <param name="i">Index of the object to get</param>
        /// <returns>Loaded object at this index</returns>
        public T this[int i] => this.objects[i];

        /// <summary>
        /// Obtains the object associated to this given path
        /// </summary>
        /// <param name="path">Object path</param>
        /// <returns>Object at this path</returns>
        public T this[string path] => this.files[path];
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes only the object list
        /// </summary>
        /// <param name="objects">Loaded objects</param>
        protected LoaderList(List<T> objects)
        {
            //Ensures we are not using extra memory space
            this.objects = new List<T>(objects);
        }

        /// <summary>
        /// Creates a new LoaderList with the given objects and files data structures
        /// </summary>
        /// <param name="objects">Loaded objects list</param>
        /// <param name="files">Path -> object dictionary</param>
        public LoaderList(List<T> objects, Dictionary<string, T> files) : this(objects)
        {
            //Ensures we are not using extra memory space
            this.files = new Dictionary<string, T>(files);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the loaded objects contain the passed reference
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns>If an equal loaded object exists</returns>
        public bool Contains(T item) => this.objects.Contains(item);

        /// <summary>
        /// Finds the first index of the given object within the loaded objects
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns>The index of the given loaded object, or -1 if none was found</returns>
        public int IndexOf(T item) => this.objects.IndexOf(item);

        /// <summary>
        /// Finds the index of the first matching object within the loaded objects
        /// </summary>
        /// <param name="match">Identification delegate</param>
        /// <returns>The index of the matched loaded object, or -1 if none was found</returns>
        public int FindIndex(Predicate<T> match) => this.objects.FindIndex(match);
        
        /// <summary>
        /// Determines if an object loaded at the given path exists within the loaded objects
        /// </summary>
        /// <param name="path">Path to look for</param>
        /// <returns>If an object at this given path has been loaded</returns>
        public bool ContainsPath(string path) => this.files.ContainsKey(path);
        
        /// <summary>
        /// Tries to find the object loaded at the given path, and stores it in the out parameter. Else, stores the default value.
        /// </summary>
        /// <param name="path">Path to look for</param>
        /// <param name="item">Out var where the result is stored</param>
        /// <returns>IF the object has been found</returns>
        public bool TryGetObject(string path, out T item) => this.files.TryGetValue(path, out item);

        /// <summary>
        /// Enumerator of all the loaded objects
        /// </summary>
        /// <returns>IEnumerator implementation of the internal list</returns>
        public IEnumerator<T> GetEnumerator() => this.objects.GetEnumerator();

        /// <summary>
        /// Non-generic enumerator implementation
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
