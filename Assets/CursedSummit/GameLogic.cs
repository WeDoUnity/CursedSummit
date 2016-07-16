using CursedSummit.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CursedSummit
{
    /// <summary>
    /// Game scenes
    /// </summary>
    public enum GameScenes
    {
        LOADING = 0,
        MAIN_MENU = 1
    }

    /// <summary>
    /// General Game utility logic. Access through instance member
    /// </summary>
    [DisallowMultipleComponent]
    public class GameLogic : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Current "immortal" GameLogic instance
        /// </summary>
        public static GameLogic Instance { get; private set; }
        #endregion

        #region Properties
        public GameScenes CurrentScene { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads and launches the game into the given scene
        /// </summary>
        /// <param name="scene">Scene to load</param>
        public void LoadScene(GameScenes scene)
        {
            Debug.Log("Loading scene: " + EnumUtils.GetNameTitleCase(scene));
            this.CurrentScene = scene;
            SceneManager.LoadScene((int)scene);
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Closes the instance of the game
        /// </summary>
        internal static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else //UNITY_EDITOR
            Application.Quit();
#endif
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion
    }
}
