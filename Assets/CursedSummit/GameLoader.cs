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
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);

            Debug.Log("Running TheGame version " + GameVersion.VersionString);
            this.loadingbar.SetLabel("Loading...");

            //Start loading sequence
            StartCoroutine(LoadComponents());
        }
        #endregion
    }
}
