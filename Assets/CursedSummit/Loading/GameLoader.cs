using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CursedSummit.Extensions;
using CursedSummit.UI;
using CursedSummit.Utils;
using FindFiles;
using Newtonsoft.Json;
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
            /// The Assembly's name
            /// </summary>
            public string Name { get; }

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
            internal LoadedAssembly(Assembly assembly, FileInfo file)
            {
                this.Assembly = assembly;
                this.Name = assembly.GetName().Name;
                this.File = file;
                this.DllName = file.Name;
                this.Path = file.GetLocalPath();
                this.Version = assembly.GetVersion();
            }
            #endregion
        }

        #region Constants
        private const string DebugPrefix = "[GameLoader]: ";
        private const float Step = 0.25f;
        #endregion

        #region Static fields
        //Loader lists
        private static List<ILoader> loaders;
        private static List<IJsonLoader> jsonLoaders;
        private static Dictionary<string, LoadedAssembly> assembliesByPath, assembliesByName;
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
        public static ReadOnlyCollection<LoadedAssembly> LoadedAssemblies { get; private set; }
        #endregion

        #region Fields
        //Loading bar
        [SerializeField]
        private Progressbar loadingbar;
        private int stage;
        private float curr;
        private Stopwatch watch;

        //File lists
        private List<Assembly> assemblies = new List<Assembly>();
        private readonly List<FileInfo> dlls = new List<FileInfo>();
        private readonly List<FileInfo> allFiles = new List<FileInfo>();

        //Extension -> file list dictionaries
        private readonly Dictionary<string, List<FileInfo>> filesByExt = new Dictionary<string, List<FileInfo>>();
        private readonly Dictionary<string, Dictionary<string, List<FileInfo>>> jsonFilesByExt = new Dictionary<string, Dictionary<string, List<FileInfo>>>();
        #endregion

        #region Properties
        private float Progress => (Step * this.stage) + (Step * this.curr);
        #endregion

        #region Methods
        /// <summary>
        /// Finds all the types in the currently used assemblies
        /// </summary>
        /// <returns>Lazy enumeration of all the types in the current ly needed assemblies</returns>
        private IEnumerable<Type> GetAllTypes()
        {
            return this.assemblies
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
        /// Finds all files within the CSData folder and loads them to memory
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator FindAllFiles()
        {
            string localPath = CSUtils.CSDataPath;
            Log($"Locating all files in CSData folder (@{localPath})");
            //Locate data folder
            if (!Directory.Exists(localPath))
            {
                Debug.LogWarning(DebugPrefix + "CSData folder could not be located. Creating new one.");
                Directory.CreateDirectory(localPath);
                yield break; //Created folder will be empty
            }

            //Enumerate through all files in all the folders starting at the root folder
            using (FileSystemEnumerator e = new FileSystemEnumerator(localPath, "*", true))
            {
                //Loop through all "normal" files
                foreach (FileInfo file in e.Matches().Where(f => f.Attributes == FileAttributes.Normal && !string.IsNullOrEmpty(f.Extension)))
                {
                    this.loadingbar.SetLabel("Locating file " + file.FullName);
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

                    Log("Located " + file.FullName);
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
            Log("Loading external assemblies...");
            this.assemblies = new List<Assembly>(this.dlls.Count + 1) { typeof(GameLoader).Assembly };
            List<LoadedAssembly> loadedAssemblies = new List<LoadedAssembly>(this.dlls.Count);
            yield return null;
            float f = 0;
            //Loop through all .dll files
            foreach (FileInfo dll in this.dlls)
            {
                string message = "Loading " + dll.FullName;
                this.curr = f++ / this.dlls.Count;
                SetLoading(message);
                
                Log(message);
                //Load to memory
                Assembly a = Assembly.LoadFile(dll.FullName);
                LoadedAssembly la = new LoadedAssembly(a, dll);
                this.assemblies.Add(a);
                loadedAssemblies.Add(la);
                assembliesByPath.Add(la.Path, la);
                assembliesByName.Add(la.Name, la);
                yield return null;
            }
            LoadedAssemblies = loadedAssemblies.AsReadOnly();
        }

        /// <summary>
        /// Finds all ILoader and IJsonLoader implementations in the current LoadedAssemblies and creates instances of them
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator FetchAllLoaders()
        {
            Log("Initializing all ILoader interface implementations...");
            //Loader types
            Type loaderType = typeof(ILoader), jLoaderType = typeof(IJsonLoader);

            //Finds all loader implementations within the loaded assemblies
            foreach (Type type in GetAllTypes().Where(t => t.IsAssignableFrom(loaderType) && t.IsClass && !t.IsAbstract && !t.IsValueType))
            {
                //If IJsonLoader
                if (type.IsAssignableFrom(jLoaderType))
                {
                    string message = $"Initializing IJsonLoader {type.FullName} in {type.Assembly.FullName}";
                    this.loadingbar.SetLabel(message);
                    Log(message);
                    //Create new instance
                    jsonLoaders.Add((IJsonLoader)Activator.CreateInstance(type));
                }
                //Else, assume ILoader
                else
                {
                    string message = $"Initializing ILoader {type.FullName} in {type.Assembly.FullName}";
                    this.loadingbar.SetLabel(message);
                    Log(message);
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
            Log("Running all IJsonLoader implementations...");
            Stopwatch watch = new Stopwatch();
            float ratio = 1f / jsonLoaders.Count;
            float f1 = -1;
            //Loop through loaders
            foreach (IJsonLoader jLoader in jsonLoaders)
            {
                f1++;
                float f2 = 0;
                Dictionary<string, List<FileInfo>> jsonExts;
                List<FileInfo> files;
                //Get list of files with the right extension and secondary extension
                if (this.jsonFilesByExt.TryGetValue(jLoader.Extension, out jsonExts) && jsonExts.TryGetValue(jLoader.JsonExtension, out files))
                {
                    Log("Starting IJsonLoader " + jLoader.Name);
                    watch.Restart();
                    using (IEnumerator<LoaderInstruction> e = jLoader.LoadAll(files))
                    {
                        //Run loading sequence
                        while (e.MoveNext())
                        {
                            this.curr = (f1 + (f2++ / files.Count)) * ratio;
                            SetLoading(jLoader.Status);
                            //If abort instruction is encountered
                            if (e.Current == LoaderInstruction.BREAK)
                            {
                                Log($"Encountered BREAK statement during {jLoader.Name} execution, aborting");
                                break;
                            }
                            yield return null;
                        }
                    }
                    watch.Stop();
                    Log($"Ran {jLoader.Name} in {watch.Elapsed.TotalSeconds}");
                }
                else { Log($"No files of {jLoader.JsonExtension} Json extension under file extension {jLoader.Extension}, skipping IJsonLoader {jLoader.Name}"); }
            }
        }

        /// <summary>
        /// Runs the loading sequence of all created Iloader implementations
        /// </summary>
        /// <returns>Lazy loading coroutine, loading on demand</returns>
        private IEnumerator RunAllLoaders()
        {
            Log("Running all ILoader implementations...");
            Stopwatch watch = new Stopwatch();
            float ratio = 1f / loaders.Count;
            float f1 = -1;
            //Loop through loaders
            foreach (ILoader loader in loaders)
            {
                f1++;
                float f2 = 0;
                List<FileInfo> files;
                //Get file list by file extension
                if (this.filesByExt.TryGetValue(loader.Extension, out files))
                {
                    Log("Starting ILoader " + loader.Name);
                    watch.Restart();
                    using (IEnumerator<LoaderInstruction> e = loader.LoadAll(files))
                    {
                        //Run loading sequence
                        while (e.MoveNext())
                        {
                            this.curr = (f1 + (f2++ / files.Count)) * ratio;
                            SetLoading(loader.Status);
                            //If abort instruction is encountered
                            if (e.Current == LoaderInstruction.BREAK)
                            {
                                Log($"Encountered BREAK statement during {loader.Name} execution, aborting");
                                break;
                            }
                            yield return null;
                        }
                    }
                    watch.Stop();
                    Log($"Ran {loader.Name} in {watch.Elapsed.TotalSeconds}");
                }
                else { Log($"No files of {loader.Extension} extension, skipping ILoader {loader.Name}"); }
            }
        }

        /// <summary>
        /// Resets Loadingbar related objects at the end of a loading coroutine
        /// </summary>
        private void EndLoader()
        {
            this.watch.Stop();
            this.curr = 0;
            this.stage++;
            this.loadingbar.SetProgress(this.Progress);
        }

        /// <summary>
        /// Sets the loadingbar's label and progress simultaneously
        /// </summary>
        /// <param name="message">Progressbar message</param>
        private void SetLoading(string message)
        {
            this.loadingbar.SetLabel(message);
            this.loadingbar.SetProgress(this.Progress);
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Logs a standard message with the correct prefix
        /// </summary>
        /// <param name="message">Message to log</param>
        private static void Log(string message) => Debug.Log(DebugPrefix + message);

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

        /// <summary>
        /// Tries to find a LoadedAssembly from the given .dll file name. This uses a dictionary, so runs in ammortized O(1) time.
        /// </summary>
        /// <param name="dllName">Dll name to find</param>
        /// <param name="assembly">Variable where the found LoadedAssembly is placed</param>
        /// <returns>True if a match was found, false otherwise</returns>
        public static bool TryGetAssemblyByName(string dllName, out LoadedAssembly assembly) => assembliesByName.TryGetValue(dllName, out assembly);

        /// <summary>
        /// Tries to find a LoadedAssembly at the given local path. This uses a dictionary, so runs in ammortized O(1) time.
        /// </summary>
        /// <param name="path">Path to find the .dll at</param>
        /// <param name="assembly">Variable where the found LoadedAssembly is placed</param>
        /// <returns>True if a match was found, false otherwise</returns>
        public static bool TryGetAssemblyByPath(string path, out LoadedAssembly assembly) => assembliesByPath.TryGetValue(path, out assembly);
        #endregion

        #region Functions
        private IEnumerator Start()
        {
            //Locate files in CSData folder
            Stopwatch loading = Stopwatch.StartNew();
            this.watch = Stopwatch.StartNew();
            yield return FindAllFiles();
            this.watch.Stop();
            Log($"Found {this.allFiles.Count} files in {this.watch.Elapsed.TotalSeconds}s");

            //Set current working directory to CSData directory
            CSUtils.SetCurrentDirectory(CSUtils.CSDataPath);

            //Load all external assemblies
            this.watch.Restart();
            yield return LoadAllDlls();
            EndLoader();
            Log($"Loaded {LoadedAssemblies.Count} external assemblies in {this.watch.Elapsed.TotalSeconds}s");

            //Find all loader implementations
            this.watch.Restart();
            yield return FetchAllLoaders();
            EndLoader();
            Log($"Located {loaders.Count} ILoader implementations and {jsonLoaders.Count} IJsonLoader implementations in {this.watch.Elapsed.TotalSeconds}s");

            //Run all Json loaders
            this.watch.Restart();
            yield return RunAllJsonLoaders();
            EndLoader();
            Log($"Ran {jsonLoaders.Count} IJsonLoaders in {this.watch.Elapsed.TotalSeconds}s");

            //Run all classic loaders
            this.watch.Restart();
            yield return RunAllLoaders();
            EndLoader();
            this.loadingbar.SetLabel("Complete");
            this.loadingbar.SetProgress(1);
            Log($"Ran {loaders.Count} ILoaders in {this.watch.Elapsed.TotalSeconds}s");

            //Reset workind directory
            CSUtils.ResetCurrentDirectory();

            //Reset lists size
            loaders = new List<ILoader>(loaders);
            jsonLoaders = new List<IJsonLoader>(jsonLoaders);

            //Complete
            loading.Stop();
            Loaded = true;
            Log($"Completed loading sequence in {loading.Elapsed.TotalSeconds}s, going to main menu...");
            GameLogic.Instance.LoadScene(GameScenes.MENU);
        }

        private void Awake()
        {
            if (Initialized) { Destroy(this); return; }

            loaders = new List<ILoader>();
            jsonLoaders = new List<IJsonLoader>();
            assembliesByPath = new Dictionary<string, LoadedAssembly>();
            assembliesByName = new Dictionary<string, LoadedAssembly>();
            Initialized = true;
        }
        #endregion
    }
}