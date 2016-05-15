using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CursedSummit
{
    public abstract class JsonLoader<T> : ILoader
    {
        #region Fields
        private Dictionary<string, T[]> files;
        private List<T> objects;
        #endregion

        #region Properties
        public int Total { get; } = 1;   //Temp

        public int Current { get; } = 1; //Temp
        #endregion

        #region Abstract properties
        public abstract string Name { get; }

        public abstract string Extension { get; }
        #endregion

        #region Methods
        public bool TryGetObjects(string path, out T[] obj)
        {
            return this.files.TryGetValue(path, out obj);
        }

        public T FindObject(Predicate<T> filter)
        {
            return this.objects.Find(filter);
        }
        
        public IEnumerator LoadAll()
        {
            string[] ext = { this.Extension };
            yield break; //Temp to prevent errors

          /* All of the lower is pseudocode
           *foreach (File file in directory)
           *{
           *    if (file.extension == ".json" && file.name.TrimEnd(ext) != file.name)
           *    {
           *        T[] array = JsonConvert.DeserializeObject<T[]>(file.data));
           *        this.files.Add(file.path, array));
           *        for (int i = 0; i < array.Length; i++)
           *        {
           *            this.objects.Add(array[i]);
           *        }
           *    }
           *    yield return null;
           *}
           */
        }
        #endregion
    }

    //Exemple class
    public class JsonVersion : JsonLoader<Version>
    {
        #region Properties
        public override string Name => "Version";

        public override string Extension => ".version";
        #endregion
    }
}
