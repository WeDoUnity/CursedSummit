using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

//TODO: Switch to local path
namespace CursedSummit.Loading
{
    /// <summary>
    /// Generic Json loading class. Finds and loads all files with the
    /// filename.jext.json file structure, where "jext" is the return value
    /// of the JsonExtension property. All loaded files are then stored internally
    /// and are accessible through the indexers, or IEnumerable implementation.
    /// </summary>
    /// <typeparam name="T">Type of object loaded from Json data by this loader</typeparam>
    public abstract class JsonLoader<T> : IJsonLoader<T>
    {
        #region Instance
        private static JsonLoader<T> instance;
        /// <summary>
        /// Returns the current instance of the loader, or null if none yet exist
        /// </summary>
        public static JsonLoader<T> Instance => instance ?? (instance = GameLoader.GetJsonLoaderInstance<JsonLoader<T>>());
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
        public JsonLoaderList<T> LoadedObjects { get; private set; }

        /// <summary>
        /// Json extension (.json)
        /// </summary>
        string ILoader.Extension => ".json";
        #endregion

        #region Abstract properties
        string ILoader.Name => this.Name;
        /// <summary>
        /// Name of this loader
        /// </summary>
        protected abstract string Name { get; }

        string IJsonLoader.JsonExtension => this.JsonExtension;
        /// <summary>
        /// Json specific secondary extension of this loader
        /// </summary>
        protected abstract string JsonExtension { get; }
        #endregion

        #region Methods
        /// <summary>
        /// File loading coroutine, loads all relevant files into memory
        /// </summary>
        /// <param name="files">Specific files to load</param>
        /// <returns>Enumerator returns loading instructions (continue/break out)</returns>
        IEnumerator<LoaderInstruction> ILoader.LoadAll(List<FileInfo> files)
        {
            if (this.Loaded) { yield break; }

            List<T> objects = new List<T>();
            Dictionary<string, T[]> paths = new Dictionary<string, T[]>();
            this.current = -1;
            foreach (FileInfo file in files)
            {
                string data = file.ReadFile();
                yield return LoaderInstruction.CONTINUE;

                T[] array = JsonConvert.DeserializeObject<T[]>(data);
                paths.Add(file.FullName, array);
                objects.AddRange(array);
                this.current++;
                yield return LoaderInstruction.CONTINUE;
            }
            this.LoadedObjects = new JsonLoaderList<T>(objects, paths);
            this.Loaded = true;
        }
        #endregion
    }
}
