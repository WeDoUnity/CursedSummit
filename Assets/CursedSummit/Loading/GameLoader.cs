using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CursedSummit.Extensions;
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
        /// <summary>
        /// Loaded assembly informational wrapper
        /// </summary>
        public struct LoadedAssembly
        {
            #region Properties
            /// <summary>
            /// Code assembly
            /// </summary>
            public Assembly Assembly { get; }

            /// <summary>
            /// Assembly version
            /// </summary>
            public Version Version { get; }

            /// <summary>
            /// Assembly file
            /// </summary>
            public FileInfo File { get; }

            /// <summary>
            /// Assembly .dll name
            /// </summary>
            public string DllName { get; }

            /// <summary>
            /// Assembly local path, from the CSData folder
            /// </summary>
            public string Path { get; }
            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new loaded assembly info
            /// </summary>
            /// <param name="assembly">Assembly being loaded</param>
            /// <param name="file">Assembly file</param>
            /// <param name="path">Assembly path. Defaults to local path from the CSData folder</param>
            /// <param name="version">Assembly version. Defaults to AssemblyInformationalVersion, or AssemblyVersion</param>
            internal LoadedAssembly(Assembly assembly, FileInfo file, string path = null, Version version = null)
            {
                this.Assembly = assembly;
                this.File = file;
                this.DllName = file.Name;
                this.Path = path ?? file.GetLocalPath();
                this.Version = version ?? assembly.GetVersion();
            }
            #endregion
        }

        #region Static fields
        //Loader lists
        private static List<ILoader> loaders = new List<ILoader>();
        private static List<IJsonLoader> jsonLoaders = new List<IJsonLoader>();
        #endregion

        #region Static properties
        /// <summary>
        /// If an instance of GameLoader has been initialized
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// If the GameLoader is done loading
        /// </summary>
        public static bool Loaded { get; private set; }
        
        /// <summary>
        /// All currently loaded game/mods loaded assemblies
        /// </summary>
        public static List<LoadedAssembly> LoadedAssemblies { get; private set; }
        #endregion

        #region Fields
        [SerializeField]
        private Progressbar loadingbar;    //Loading bar

        //File lists
        private List<FileInfo> dlls = new List<FileInfo>();
        private List<FileInfo> allFiles = new List<FileInfo>();

        //Extension -> file list dictionaries
        private Dictionary<string, List<FileInfo>> filesByExt = new Dictionary<string, List<FileInfo>>();
        private Dictionary<string, Dictionary<string, List<FileInfo>>> jsonFilesByExt = new Dictionary<string, Dictionary<string, List<FileInfo>>>();
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
                foreach (FileInfo file in e.Matches().Where(f => f.Attributes == FileAttributes.Normal && !string.IsNullOrEmpty(f.Extension)))
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
            LoadedAssemblies = new List<LoadedAssembly>(this.dlls.Count + 1)
            {
                //Game assembly
                new LoadedAssembly(typeof(GameLoader).Assembly,
                                   new FileInfo(Path.Combine(Application.dataPath, "Managed/Assembly-CSharp.dll")),
                                   "../CursedSummit_Data/Managed/Assembly-CSharp.dll",
                                   GameVersion.Version)
            };
            yield return null;

            //Loop through all .dll files
            foreach (FileInfo dll in this.dlls)
            {
                Debug.Log("[GameLoader]: Loading " + dll.FullName);
                //Load to current AppDomain
                LoadedAssemblies.Add(new LoadedAssembly(Assembly.LoadFile(dll.FullName), dll));
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
            foreach (Type type in GetAllTypes().Where(t => t.IsAssignableFrom(loaderType) && t.IsClass && !t.IsAbstract && !t.IsValueType))
            {
                //If IJsonLoader
                if (type.IsAssignableFrom(jLoaderType))
                {
                    Debug.Log($"[GameLoader]: Initializing IJsonLoader {type.FullName} in {type.Assembly.FullName}");
                    //Create new instance
                    jsonLoaders.Add((IJsonLoader)Activator.CreateInstance(type));
                }
                //Else, assume ILoader
                else
                {
                    Debug.Log($"[GameLoader]: Initializing ILoader {type.FullName} in {type.Assembly.FullName}");
                    //Create new instance
                    loaders.Add((ILoader)Activator.CreateInstance(type));
                }
                yield return null;
            }
        }

        /// <summary>
        /// Runs the loading sequence of all created IJsonLoader implementations
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator RunAllJsonLoaders()
        {
            Debug.Log("[GameLoader]: Running all IJsonLoader implementations...");
            Stopwatch watch = new Stopwatch();
            //Loop through loaders
            foreach (IJsonLoader jLoader in jsonLoaders)
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
        private IEnumerator RunAllLoaders()
        {
            Debug.Log("[GameLoader]: Running all ILoader implementations...");
            Stopwatch watch = new Stopwatch();
            //Loop through loaders
            foreach (ILoader loader in loaders)
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
        #endregion

        #region Static methods
        /// <summary>
        /// Finds all the types in the current AppDomain
        /// </summary>
        /// <returns>Lazy enumeration of all the types in the current AppDomain</returns>
        private static IEnumerable<Type> GetAllTypes()
        {
            return LoadedAssemblies.Select(a => a.Assembly)
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

        /// <summary>
        /// Gets the first ILoader implementation of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Loader implementation type</typeparam>
        /// <returns>The active instance of the loader, or null if none was found</returns>
        public static T GetLoaderInstance<T>() where T : class, ILoader => (T)loaders.FirstOrDefault(l => l is T);

        /// <summary>
        /// Gets the first IJsonLoader implementation of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Loader implementation type</typeparam>
        /// <returns>The active instance of the loader, or null if none was found</returns>
        public static T GetJsonLoaderInstance<T>() where T : class, IJsonLoader => (T)jsonLoaders.FirstOrDefault(jl => jl is T);
        #endregion

        #region Functions
        private IEnumerator Start()
        {
            //TODO: Hook everything to the loading bar

            //Locate files in CSData folder
            Stopwatch loading = Stopwatch.StartNew(), watch = Stopwatch.StartNew();
            yield return FindAllFiles();
            watch.Stop();
            Debug.Log($"[GameLoader]: Found {this.allFiles.Count} files in {watch.Elapsed.TotalSeconds}s");

            //Set current working directory to CSData directory
            CSUtils.SetCurrentDirectory(CSUtils.CSDataPath);

            //Load all external assemblies
            watch.Restart();
            yield return LoadAllDlls();
            watch.Stop();
            Debug.Log($"[GameLoader]: Loaded {LoadedAssemblies.Count} external assemblies in {watch.Elapsed.TotalSeconds}s");

            //Find all loader implementations
            watch.Restart();
            yield return FetchAllLoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Located {loaders.Count} ILoader implementations and {jsonLoaders.Count} IJsonLoader implementations in {watch.Elapsed.TotalSeconds}s");

            //Run all Json loaders
            watch.Restart();
            yield return RunAllJsonLoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Ran {jsonLoaders.Count} IJsonLoaders in {watch.Elapsed.TotalSeconds}s");

            //Run all classic loaders
            watch.Restart();
            yield return RunAllLoaders();
            watch.Stop();
            Debug.Log($"[GameLoader]: Ran {loaders.Count} ILoaders in {watch.Elapsed.TotalSeconds}s");

            //Reset workind directory
            CSUtils.ResetCurrentDirectory();

            //Clear now unneeded cache
            this.dlls = null;
            this.allFiles = null;
            this.filesByExt = null;
            this.jsonFilesByExt = null;

            //Reset lists size
            loaders = new List<ILoader>(loaders);
            jsonLoaders = new List<IJsonLoader>(jsonLoaders);

            //Complete
            loading.Stop();
            Loaded = true;
            Debug.Log($"[GameLoader]: Completed loading sequence in {loading.Elapsed.TotalSeconds}s, going to main menu...");
            GameLogic.Instance.LoadScene(GameScenes.MENU);
        }

        private void Awake()
        {
            if (!Initialized) { Initialized = true; }
            else { Destroy(this); }
        }
        #endregion
    }
}