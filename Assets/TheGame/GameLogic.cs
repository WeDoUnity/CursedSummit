using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGame
{
    /// <summary>
    /// Game scenes
    /// </summary>
    public enum GameScenes
    {
        LOADING = 0,
        MENU = 1
    }

    /// <summary>
    /// General Game utility logic. Access through instance member
    /// </summary>
    public class GameLogic : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Current "immortal" GameLogic instance
        /// </summary>
        public static GameLogic Instance { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads and launches the game into the given scene
        /// </summary>
        /// <param name="scene">Scene to load</param>
        internal void LoadScene(GameScenes scene) => SceneManager.LoadScene((int)scene);

        /// <summary>
        /// Loads and launches the given scene atop of the current loaded scenes
        /// </summary>
        /// <param name="scene">Scene to load</param>
        internal void LoadSceneLayered(GameScenes scene) => SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);

        /// <summary>
        /// Unloads the given scene
        /// </summary>
        /// <param name="scene">Scene to unload</param>
        internal void UnloadScene(GameScenes scene) => SceneManager.UnloadScene((int)scene);
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
