using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static CursedSummit.Loading.LoaderInstruction;

namespace CursedSummit.Loading
{
    /// <summary>
    /// Generic file loading class. Finds and loads all files with the
    /// filename.ext file structure, where "ext" is the return value
    /// of the Extension property. All loaded files are then stored internally
    /// and are accessible through the LoaderList(T) property.
    /// </summary>
    /// <typeparam name="T">Type of object loaded from Json data by this loader</typeparam>
    public abstract class Loader<T> : ILoader<T>
    {
        #region Instance
        private static Loader<T> instance;
        /// <summary>
        /// Returns the current instance of the loader, or null if none yet exist
        /// </summary>
        public static Loader<T> Instance => instance ?? (instance = GameLoader.GetLoaderInstance<Loader<T>>());
        #endregion

        #region 
        int ILoader.Current => this.current;
        /// <summary>
        /// Current file loading index
        /// </summary>
        private int current;

        string ILoader.Status => this.status;
        /// <summary>
        /// Currentl loading bar status
        /// </summary>
        private string status;

        /// <summary>
        /// If this loader is done loading
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// Read-only collection of all the loaded objects
        /// </summary>
        public LoaderList<T> LoadedObjects { get; private set; }
        #endregion

        #region Abstract properties
        string ILoader.Name => this.Name;
        /// <summary>
        /// Name of this loader
        /// </summary>
        protected abstract string Name { get; }

        string ILoader.Extension => this.Extension;
        /// <summary>
        /// File extension of this loader
        /// </summary>
        protected abstract string Extension { get; }
        #endregion

        #region Methods
        /// <summary>
        /// File loading coroutine, loads all relevant files into memory
        /// </summary>
        /// <param name="files">Specific files to load</param>
        /// <returns>Enumerator returns loading instructions (continue/break out)</returns>
        IEnumerator<LoaderInstruction> ILoader.LoadAll(IList<FileInfo> files)
        {
            if (this.Loaded) { yield break; }

            List<T> objects = new List<T>();
            Dictionary<string, T> paths = new Dictionary<string, T>();
            this.current = -1;
            LoaderInstruction inst = CONTINUE;
            foreach (FileInfo file in files)
            {
                this.current++;
                this.status = $"[{this.Name}]: Loading {file.FullName}";
                T obj;
                try
                {
                    obj = LoadObject(file);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{this.Name}]: Encountered an exception loading file {file.FullName}.\n{e.GetType().Name}\n{e.StackTrace}");
                    obj = default(T);
                    inst = BREAK;
                }
                yield return inst;

                if (obj != null)
                {
                    paths.Add(file.GetLocalPath(), obj);
                    objects.Add(obj);
                }
                else { Debug.LogWarning($"[{this.Name}]: File {file.FullName} loaded a null object"); }
            }
            this.LoadedObjects = new LoaderList<T>(objects, paths);
            this.Loaded = true;
        }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Loads a specific file into memory
        /// </summary>
        /// <param name="file">File to load</param>
        /// <returns>The loaded object</returns>
        public abstract T LoadObject(FileInfo file);
        #endregion
    }
}
