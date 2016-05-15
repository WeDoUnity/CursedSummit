using System.Collections;
using System.Collections.Generic;
using CursedSummit.UI;
using UnityEngine;

namespace CursedSummit
{
    /// <summary>
    /// General Game loader (nothing really implemented, only a framework skeleton)
    /// </summary>
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

            foreach (ILoader loader in this.loaders)
            {
                Debug.Log("Loading " + loader.Name);
                yield return StartCoroutine(loader.LoadAll());
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
