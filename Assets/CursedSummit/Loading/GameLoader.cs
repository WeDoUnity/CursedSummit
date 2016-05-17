using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CursedSummit.UI;
using CursedSummit.Utils;
using FindFiles;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CursedSummit.Loading
{
    /// <summary>
    /// General Game loader
    /// </summary>
    [DisallowMultipleComponent]
    public class GameLoader : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// General instance of the GameLoader
        /// </summary>
        public static GameLoader Instance { get; private set; }
        #endregion

        #region Fields
        [SerializeField]
        private Progressbar loadingbar;    //Loading bar

        //Data structures   
        private readonly List<FileInfo> dlls = new List<FileInfo>();
        private readonly List<FileInfo> allFiles = new List<FileInfo>();
        private readonly List<ILoader> loaders = new List<ILoader>();
        private readonly List<IJsonLoader> jsonLoaders = new List<IJsonLoader>();
        private readonly Dictionary<string, List<FileInfo>> filesByExt = new Dictionary<string, List<FileInfo>>();
        private readonly Dictionary<string, Dictionary<string, List<FileInfo>>> jsonFilesByExt = new Dictionary<string, Dictionary<string, List<FileInfo>>>();
        #endregion

        #region Methods
        /// <summary>
        /// Finds all files within the CSData folder and loads them to memory
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator FindAllFiles()
        {
            string localPath = CSUtils.CSDataPath;
            Debug.Log($"[GameLoader] Locating all files in CSData folder (@{localPath})");
            //Locate data folder
            if (!Directory.Exists(localPath))
            {
                Debug.LogWarning("[GameLoader]: CSData folder could not be located. Creating new one.");
                Directory.CreateDirectory(localPath);
                yield break; //Created folder will be empty
            }

            //Enumerate through all files in all the folders starting at the root folder
            using (FileSystemEnumerator e = new FileSystemEnumerator(localPath, "*", true))
            {
                //Loop through all "normal" files
                foreach (FileInfo file in e.Matches().Where(f => f.Attributes == FileAttributes.Normal))
                {
                    List<FileInfo> files;
                    string jsonExt = Path.GetExtension(file.Name);

                    //If there is a secondary extension, assume it's a potential Json file
                    if (!string.IsNullOrEmpty(jsonExt))
                    {
                        Dictionary<string, List<FileInfo>> jsonFiles;
                        if (!this.jsonFilesByExt.TryGetValue(file.Extension, out jsonFiles))
                        {
                            jsonFiles = new Dictionary<string, List<FileInfo>>();
                            this.jsonFilesByExt.Add(file.Extension, jsonFiles);
                        }
                        if (!jsonFiles.TryGetValue(jsonExt, out files))
                        {
                            files = new List<FileInfo>();
                            jsonFiles.Add(jsonExt, files);
                        }
                        files.Add(file);
                    }

                    //If .dll file
                    if (file.Extension == ".dll") { this.dlls.Add(file); }

                    //Add to normal extension dict
                    if (!this.filesByExt.TryGetValue(file.Extension, out files))
                    {
                        files = new List<FileInfo>();
                        this.filesByExt.Add(file.Extension, files);
                    }
                    files.Add(file);
                    this.allFiles.Add(file);

                    Debug.Log("[GameLoader]: Located " + file.FullName);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Loads all external assemblies into the current AppDomain
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator LoadAllDlls()
        {
            Debug.Log("[GameLoader]: Loading external assemblies...");
            //Loop through all .dll files
            foreach (FileInfo dll in this.dlls)
            {
                Debug.Log("[GameLoader]: Loading " + dll.FullName);
                //Load to current AppDomain
                Assembly.LoadFile(dll.FullName);
                yield return null;
            }
        }

        /// <summary>
        /// Finds all ILoader and IJsonLoader implementations in the current AppDomain and creates instances of them
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator FetchAllLoaders()
        {
            Debug.Log("[GameLoader]: Initializing all ILoader interface implementations...");
            //Loader types
            Type loaderType = typeof(ILoader), jLoaderType = typeof(IJsonLoader);

            //Finds all loader implementations within the loaded assemblies
            foreach (Type type in GetAllTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsValueType && t.IsAssignableFrom(loaderType)))
            {
                //If IJsonLoader
                if (type.IsAssignableFrom(jLoaderType))
                {
                    Debug.Log($"[GameLoader]: Initializing IJsonLoader {type.FullName} in {type.Assembly.FullName}");
                    //Create new instance
                    IJsonLoader jLoader = (IJsonLoader)Activator.CreateInstance(type);
                    this.jsonLoaders.Add(jLoader);
                }
                //Else, assume ILoader
                else
                {
                    Debug.Log($"[GameLoader]: Initializing ILoader {type.FullName} in {type.Assembly.FullName}");
                    //Create new instance
                    ILoader loader = (ILoader)Activator.CreateInstance(type);
                    this.loaders.Add(loader);
                }
                yield return null;
            }
        }

        /// <summary>
        /// Runs the loading sequence of all created IJsonLoader implementations
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator RunAllIJsonLoaders()
        {
            Debug.Log("[GameLoader]: Running all IJsonLoader implementations...");
            Stopwatch watch = new Stopwatch();
            //Loop through loaders
            foreach (IJsonLoader jLoader in this.jsonLoaders)
            {
                Dictionary<string, List<FileInfo>> jsonExts;
                List<FileInfo> files;
                //Get list of files with the right extension and secondary extension
                if (this.jsonFilesByExt.TryGetValue(jLoader.Extension, out jsonExts) && jsonExts.TryGetValue(jLoader.JsonExtension, out files))
                {
                    Debug.Log("[GameLoader]: Starting IJsonLoader " + jLoader.Name);
                    watch.Restart();
                    using (IEnumerator<LoaderInstruction> e = jLoader.LoadAll(files))
                    {
                        //Run loading sequence
                        while (e.MoveNext())
                        {
                            //If abort instruction is encountered
                            if (e.Current == LoaderInstruction.BREAK)
                            {
                                Debug.Log($"[GameLoader]: Encountered BREAK statement during {jLoader.Name} execution, aborting");
                                break;
                            }
                            yield return null;
                        }
                    }
                    watch.Stop();
                    Debug.Log($"[GameLoader]: Ran {jLoader.Name} in {watch.Elapsed.TotalSeconds}");
                }
                else { Debug.Log($"[GameLoader]: No files of {jLoader.JsonExtension} Json extension under file extension {jLoader.Extension}, skipping IJsonLoader {jLoader.Name}"); }
            }
        }

        /// <summary>
        /// Runs the loading sequence of all created Iloader implementations
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator RunAllILoaders()
        {
            Debug.Log("[GameLoader]: Running all ILoader implementations...");
            Stopwatch watch = new Stopwatch();
            //Loop through loaders
            foreach (ILoader loader in this.loaders)
            {
                List<FileInfo> files;
                //Get file list by file extension
                if (this.filesByExt.TryGetValue(loader.Extension, out files))
                {
                    Debug.Log("[GameLoader]: Starting ILoader " + loader.Name);
                    watch.Restart();
                    using (IEnumerator<LoaderInstruction> e = loader.LoadAll(files))
                    {
                        //Run loading sequence
                        while (e.MoveNext())
                        {
                            //If abort instruction is encountered
                            if (e.Current == LoaderInstruction.BREAK)
                            {
                                Debug.Log($"[GameLoader]: Encountered BREAK statement during {loader.Name} execution, aborting");
                                break;
                            }
                            yield return null;
                        }
                    }
                    watch.Stop();
                    Debug.Log($"[GameLoader]: Ran {loader.Name} in {watch.Elapsed.TotalSeconds}");
                }
                else { Debug.Log($"[GameLoader]: No files of {loader.Extension} extension, skipping ILoader {loader.Name}"); }
            }
        }

        /// <summary>
        /// Gets the first ILoader implementation of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Loader implementation type</typeparam>
        /// <returns>The active instance of the loader, or null if none was found</returns>
        public T GetLoaderInstance<T>() where T : class, ILoader
        {
            return (T)this.loaders.FirstOrDefault(l => l is T);
        }

        /// <summary>
        /// Gets the first IJsonLoader implementation of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Loader implementation type</typeparam>
        /// <returns>The active instance of the loader, or null if none was found</returns>
        public T GetJsonLoaderInstance<T>() where T : class, IJsonLoader
        {
            return (T)this.jsonLoaders.FirstOrDefault(jl => jl is T);
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Finds all the types in the current AppDomain
        /// </summary>
        /// <returns>Lazy enumeration of all the types in the current AppDomain</returns>
        private static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (Exception)
                    {
                        return Type.EmptyTypes;
                    }
                });

        }
        #endregion

        #region Functions
        private IEnumerator Start()
        {
            //TODO: Hook everything to the loading bar

            //Locate files in CSData folder
            Stopwatch general = Stopwatch.StartNew(), watch = Stopwatch.StartNew();
            yield return FindAllFiles();
            watch.Stop();
            Debug.Log($"[GameLoader]: Found {this.allFiles.Count} files in {watch.Elapsed.TotalSeconds}s");

            //Load all external assemblies
            if (this.dlls.Count > 0)
            {
                watch.Restart();
                yield return LoadAllDlls();
                watch.Stop();
                Debug.Log($"[GameLoader]: Loaded {this.dlls.Count} external assemblies in {watch.Elapsed.TotalSeconds}s");
            }
            else { Debug.Log("[GameLoader]: No external assmblies to load, skipping");}

            //Find all loader implementations
            watch.Restart();
            yield return FetchAllLoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Located {this.loaders.Count} ILoader implementations and {this.jsonLoaders.Count} IJsonLoader implementations in {watch.Elapsed.TotalSeconds}s");

            //Run all Json loaders
            watch.Restart();
            yield return RunAllIJsonLoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Ran {this.jsonLoaders.Count} IJsonLoaders in {watch.Elapsed.TotalSeconds}s");

            //Run all classic loaders
            watch.Restart();
            yield return RunAllILoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Ran {this.loaders.Count} ILoaders in {watch.Elapsed.TotalSeconds}s");

            //Complete
            general.Stop();
            Debug.Log($"[GameLoader]: Completed loading sequence in {general.Elapsed.TotalSeconds}s, going to main menu...");
            GameLogic.Instance.LoadScene(GameScenes.MENU);
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);
        }
        #endregion
    }
}