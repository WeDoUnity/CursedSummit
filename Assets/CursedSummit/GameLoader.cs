using System.Collections;
using System.Collections.Generic;
using System.IO;
using CursedSummit.UI;
using CursedSummit.Utils;
using UnityEngine;

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
            Debug.Log("Running The Cursed Summit version " + GameVersion.VersionString);
            this.loadingbar.SetLabel("Loading...");

            string localPath = Path.Combine(CSUtils.RootPath, folderName);

            foreach (ILoader loader in this.loaders)
            {
                Debug.Log("Loading " + loader.Name);
                using (IEnumerator<LoaderInstruction> e = loader.LoadAll())
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
