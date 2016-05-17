using System.Collections.Generic;
using System.IO;

namespace CursedSummit.Loading
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
    /// Game loader general interface.
    /// NOTE: it is not reccomended to implement this interface directly. Instead,
    /// please implement ILoader(T), or even better, Loader(T). ILoader itself is used
    /// internally for generic type handling.
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
    /// Generic typed general game loader interface.
    /// NOTE: You might want to implement Loader(T) directly to have some
    /// basic functionality in place.
    /// </summary>
    /// <typeparam name="T">Object type loaded by this</typeparam>
    public interface ILoader<T> : ILoader
    {
        #region Properties
        /// <summary>
        /// List of all objects loaded by this loader
        /// </summary>
        LoaderList<T> LoadedObjects { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads one given file
        /// </summary>
        /// <param name="file">File to load</param>
        /// <returns>The loaded object</returns>
        T LoadObject(FileInfo file);
        #endregion
    }

    /// <summary>
    /// Json game loader interface.
    /// NOTE: it is not reccomended to implement this interface directly. Instead,
    /// please implement IJsonLoader(T), or even better, JsonLoader(T). IJsonLoader itself is
    /// used internally for generic type handling.
    /// </summary>
    public interface IJsonLoader : ILoader
    {
        #region Properties
        /// <summary>
        /// Json secondary file extension (/file.jsonext.ext)
        /// </summary>
        string JsonExtension { get; }
        #endregion
    }

    /// <summary>
    /// Generic typed Json loader interface.
    /// NOTE: You might want to implement JsonLoader(T) directly to have some
    /// basic functionality in place.
    /// </summary>
    /// <typeparam name="T">Json object typed loaded by this</typeparam>
    public interface IJsonLoader<T> : IJsonLoader
    {
        #region Properties
        /// <summary>
        /// Dictionary mapping file path => loaded Json object
        /// </summary>
        JsonLoaderList<T> LoadedObjects { get; }
        #endregion
    }
}
