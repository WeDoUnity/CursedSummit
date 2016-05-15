using System.Collections;

namespace CursedSummit
{
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
        /// Name of the loader
        /// </summary>
        string Name { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Coroutine enumerator loading all the relevant files
        /// </summary>
        /// <returns>The loading coroutine</returns>
        IEnumerator LoadAll();
        #endregion
    }
}
