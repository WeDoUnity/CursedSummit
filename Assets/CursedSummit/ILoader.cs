using System.Collections.Generic;

namespace CursedSummit
{
    /// <summary>
    /// ILoader coroutine instruction
    /// </summary>
    public enum LoaderInstruction
    {
        CONTINUE = 0,
        BREAK = 1
    }

    public interface ILoader
    {
        #region Properties
        /// <summary>
        /// Total amount of files to load
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Currently loaded file index
        /// </summary>
        int Current { get; }

        /// <summary>
        /// Current loader status string
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Name of the loader
        /// </summary>
        string Name { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Coroutine enumerator loading all the relevant files
        /// </summary>
        /// <returns>The loading coroutine</returns>
        IEnumerator<LoaderInstruction> LoadAll();
        #endregion
    }
}
