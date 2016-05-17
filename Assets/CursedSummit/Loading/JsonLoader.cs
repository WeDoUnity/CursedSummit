using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CursedSummit.Loading
{
    public abstract class JsonLoader<T> : IJsonLoader<T>, IEnumerable<T>
    {
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

        private readonly List<T> objects = new List<T>();
        /// <summary>
        /// All objects loaded by this loader
        /// </summary>
        List<T> ILoader<T>.Objects => this.objects;

        /// <summary>
        /// File path => Loaded objects mapping dictionary
        /// </summary>
        public Dictionary<string, T[]> Files { get; } = new Dictionary<string, T[]>();

        /// <summary>
        /// Json extension (.json)
        /// </summary>
        public string Extension => ".json";
        #endregion

        #region Abstract properties
        /// <summary>
        /// Name of this loader
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Json specific extension of this loader
        /// </summary>
        public abstract string JsonExtension { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Tries to get loaded <typeparamref name="T"/>s at this file path
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="obj">Loaded objects are stored in this out parameter</param>
        /// <returns>If files had been found at this path</returns>
        public bool TryGetObjects(string path, out T[] obj) => this.Files.TryGetValue(path, out obj);

        /// <summary>
        /// File loading coroutine, loads all relevant files into memory
        /// </summary>
        /// <param name="files">Specific files to load</param>
        /// <returns>Enumerator returns loading instructions (continue/break out)</returns>
        IEnumerator<LoaderInstruction> ILoader.LoadAll(List<FileInfo> files)
        {
            if (this.Loaded) { yield break; }

            this.current = -1;
            foreach (FileInfo file in files)
            {
                string data;
                using (StreamReader r = file.OpenText())
                {
                    data = r.ReadToEnd();
                }
                yield return LoaderInstruction.CONTINUE;

                T[] array = JsonConvert.DeserializeObject<T[]>(data);
                this.Files.Add(Path.ChangeExtension(file.FullName, null), array);
                this.objects.AddRange(array);
                this.current++;
                yield return LoaderInstruction.CONTINUE;
            }
            this.Loaded = true;
        }

        /// <summary>
        /// Enumerator of all currently loaded files for this instance.
        /// WARNING: The files are not sorted and appear in loading order
        /// </summary>
        /// <returns>Lazy sequence of the loaded files</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.objects.GetEnumerator();

        /// <summary>
        /// Non-generic enumerator implementation
        /// </summary>
        /// <returns>Enumerator for this sequence</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.objects.GetEnumerator();
        #endregion
    }
}
