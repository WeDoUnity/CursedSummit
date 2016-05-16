using System.Collections.Generic;
using System.IO;

namespace CursedSummit
{
    /// <summary>
    /// ILoader coroutine instruction
    /// </summary>
    public enum LoaderInstruction
    {
        /// <summary>
        /// Continue loading execution
        /// </summary>
        CONTINUE = 0,
        /// <summary>
        /// Abort loading execution and break out to next loader
        /// </summary>
        BREAK = 1
    }

    /// <summary>
    /// Game Loader general interface
    /// </summary>
    public interface ILoader
    {
        #region Properties
        /// <summary>
        /// Currently loaded file index
        /// </summary>
        int Current { get; }

        /// <summary>
        /// Name of the loader
        /// </summary>
        string Name { get; }

        /// <summary>
        /// File extension of this loader type
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// If this loader is done loading
        /// </summary>
        bool Loaded { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Coroutine enumerator loading all the relevant files
        /// </summary>
        /// <param name="files">All files of this type to load</param>
        /// <returns>The loading coroutine</returns>
        IEnumerator<LoaderInstruction> LoadAll(List<FileInfo> files);
        #endregion
    }

    /// <summary>
    /// Generic typed game loader interface
    /// </summary>
    /// <typeparam name="T">Object type loaded by this</typeparam>
    public interface ILoader<T> : ILoader
    {
        #region Properties
        /// <summary>
        /// List of all objects loaded by this loader
        /// </summary>
        List<T> Objects { get; }
        #endregion
    }

    public interface IJsonLoader<T> : ILoader<T>
    {
        #region Properties
        /// <summary>
        /// Secondary Json file extension of this loader
        /// </summary>
        string JsonExtension { get; }

        /// <summary>
        /// Dictionary matching file path and all Json objects in this file
        /// </summary>
        Dictionary<string, T[]> Files { get; }
        #endregion
    }
}
