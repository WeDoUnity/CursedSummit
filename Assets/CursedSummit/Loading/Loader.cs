using System.Collections.Generic;
using System.IO;

namespace CursedSummit.Loading
{
    public abstract class Loader<T> : ILoader<T>
    {
        #region Instance
        private static Loader<T> instance;
        /// <summary>
        /// Returns the current instance of the loader, or null if none yet exist
        /// </summary>
        public static Loader<T> Instance => instance ?? (instance = GameLoader.GetLoaderInstance<Loader<T>>());
        #endregion

        #region Properties
        private int current;
        /// <summary>
        /// Current file loading index
        /// </summary>
        int ILoader.Current => this.current;

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
        IEnumerator<LoaderInstruction> ILoader.LoadAll(List<FileInfo> files)
        {
            if (this.Loaded) { yield break; }

            List<T> objects = new List<T>();
            Dictionary<string, T> paths = new Dictionary<string, T>();
            this.current = -1;
            foreach (FileInfo file in files)
            {
                T obj = LoadObject(file);
                paths.Add(file.FullName, obj);
                objects.Add(obj);
                this.current++;
                yield return LoaderInstruction.CONTINUE;
            }
            this.LoadedObjects = new LoaderList<T>(objects, paths);
            this.Loaded = true;
        }
        #endregion

        #region Abstract methods
        public abstract T LoadObject(FileInfo file);
        #endregion
    }
}
