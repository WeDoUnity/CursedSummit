using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CursedSummit.UI;
using CursedSummit.Utils;
using UnityEngine;
using FindFiles;
using Debug = UnityEngine.Debug;

namespace CursedSummit
{
    /// <summary>
    /// General Game loader (nothing really implemented, only a framework skeleton)
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

        #region Constants
        public const string folderName = "CSData";
        #endregion

        #region Fields
        [SerializeField]
        private Progressbar loadingbar;    //Loading bar     
        private List<ILoader> loaders = new List<ILoader>();
        private List<FileInfo> allFiles = new List<FileInfo>();
        private Dictionary<string, List<FileInfo>> extensions = new Dictionary<string, List<FileInfo>>();
        #endregion

        #region Methods
        /// <summary>
        /// Loading sequence
        /// </summary>
        /// <returns>Loading sequence Coroutine</returns>
        private IEnumerator<YieldInstruction> LoadComponents()
        {
            //Not implemented yet, just creating a 5 seconds "loading"
            for (int i = 0; i < 9; i++)
            {
                this.loadingbar.SetProgress(i / 9f);
                yield return new WaitForSeconds(0.5f);
            }

            this.loadingbar.SetProgress(1);
            this.loadingbar.SetLabel("Complete");
            yield return new WaitForSeconds(0.5f);

            Debug.Log("Loading finished, going to Menu");
            GameLogic.Instance.LoadScene(GameScenes.MENU);
        }
        #endregion

        #region Functions
        private IEnumerator Start()
        {
            Stopwatch watch = Stopwatch.StartNew();
            string localPath = Path.Combine(CSUtils.RootPath, folderName);
            if (!Directory.Exists(localPath))
            {
                Debug.LogWarning("[GameLoader]: CSData folder could not be located. Creating new one.");
                Directory.CreateDirectory(localPath);
            }

            using (FileSystemEnumerator e = new FileSystemEnumerator(localPath, "*", true))
            {
                foreach (FileInfo file in e.Matches())
                {
                    this.loadingbar.SetLabel("Locating file " + file.FullName);

                    this.allFiles.Add(file);
                    List<FileInfo> files;
                    if (!this.extensions.TryGetValue(file.Extension, out files))
                    {
                        files = new List<FileInfo>();
                        this.extensions.Add(file.Extension, files);
                    }

                    files.Add(file);
                    Debug.Log("[GameLoader]: Located " + file.FullName);
                    yield return null;
                }
            }

            watch.Stop();
            Debug.Log($"[GameLoader]: Found {this.allFiles.Count} files in {watch.Elapsed.TotalSeconds}s");

            watch.Reset();
            watch.Start();
            foreach (ILoader loader in this.loaders)
            {
                Debug.Log("[GameLoader]: Starting loader " + loader.Name);
                List<FileInfo> files;
                using (IEnumerator<LoaderInstruction> e = loader.LoadAll(new List<FileInfo>())) //TODO: put the right list here
                {
                    while (e.MoveNext())
                    {
                        if (e.Current == LoaderInstruction.BREAK) { break; }
                        yield return null;
                    }
                }
            }
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);

            //Get all Loaders to the list
        }
        #endregion
    }
}
