using System.Collections.Generic;
using CursedSummit.Extensions;
using CursedSummit.UI;
using CursedSummit.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CursedSummit
{
    [RequireComponent(typeof(Canvas)), DisallowMultipleComponent]
    public class DebugWindow : MonoBehaviour
    {
        /// <summary>
        /// A Queue implementation, forcing a fixed capacity, and dequeuing the latest member when it reaches capacity
        /// </summary>
        /// <typeparam name="T">Type of object stored in the Queue</typeparam>
        internal class LogQueue<T> : Queue<T>
        {
            #region Constants
            /// <summary>
            /// Default capacity of the queue
            /// </summary>
            private const int defaultCapacity = 10;
            #endregion

            #region Properties
            /// <summary>
            /// Capacity of the queue (read-only)
            /// </summary>
            public int Capacity { get; }

            /// <summary>
            /// If there is room for more members in the queue
            /// </summary>
            public bool HasRoom => this.Count < this.Capacity;
            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new LogQueue with the default capacity
            /// </summary>
            public LogQueue() : this(defaultCapacity) { }

            /// <summary>
            /// Creates a new LogQueue of the given capacity
            /// </summary>
            /// <param name="capacity">Capacity of the queue</param>
            public LogQueue(int capacity) : base(capacity)
            {
                this.Capacity = capacity;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Logs an item to the queue (and automatically dequeues if the queue is full)
            /// </summary>
            /// <param name="item">Item to log</param>
            /// <returns>The automatically Dequeued item, if any, else the default value of <typeparamref name="T"/></returns>
            public T Log(T item)
            {
                T removed = default(T);
                if (!this.HasRoom) { removed = Dequeue(); }
                base.Enqueue(item);
                return removed;
            }

            /// <summary>
            /// Logs an item to the queue only if there is enough capacity for it
            /// </summary>
            /// <param name="item">Item to log</param>
            public new void Enqueue(T item)
            {
                if (this.HasRoom) { base.Enqueue(item); }
            }
            #endregion
        }

        /// <summary>
        /// Temporary log data structure
        /// </summary>
        private struct TempLog
        {
            #region Fields
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;
            #endregion

            #region Constructors
            /// <summary>
            /// Sets a new TempLog structure
            /// </summary>
            /// <param name="message">Log message</param>
            /// <param name="stackTrace">Log stack trace</param>
            /// <param name="type">Log type</param>
            public TempLog(string message, string stackTrace, LogType type)
            {
                this.message = message;
                this.stackTrace = stackTrace;
                this.type = type;
            }
            #endregion
        }

        #region Instance
        /// <summary>
        /// Current DebugWindow instance
        /// </summary>
        public static DebugWindow Instance { get; private set; }
        #endregion

        #region Constants
        // Max amount of saved log messages
        private const int maxLogs = 500;
        //LogType -> Color conversion dictionary
        private static readonly Dictionary<LogType, Color> colours = new Dictionary<LogType, Color>(5)
        {
            { LogType.Log,       XKCDColours.White  },
            { LogType.Warning,   XKCDColours.Yellow },
            { LogType.Assert,    XKCDColours.Green  },
            { LogType.Error,     XKCDColours.Orange },
            { LogType.Exception, XKCDColours.Red    }
        };
        #endregion

        #region Fields
        [SerializeField]
        private Text version;                   //Version text label
        [SerializeField]
        private GameObject window, logPrefab;   //Debug window, log member prefab
        [SerializeField]
        private VerticalLayoutGroup layout;     //Log layout group
        private List<TempLog> preLogs = new List<TempLog>();                                //Logs posted before the window was correctly initiated
        private readonly LogQueue<GameObject> queue = new LogQueue<GameObject>(maxLogs);    //Queue of saved logs
        #endregion

        #region Properties
        /// <summary>
        /// Debug window visibility
        /// </summary>
        public bool Visible
        {
            get { return this.window.activeSelf; }
            set
            {
                if (value != this.window.activeSelf)
                {
                    this.window.SetActive(value);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prints logged messages to the debug window
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="stackTrace">Log stack trace</param>
        /// <param name="type">Log type</param>
        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(message)) { return; }

            GameObject element = Instantiate(this.logPrefab);
            element.transform.SetParent(this.layout.transform);
            element.GetComponent<ExpandableText>().SetText(message, message + (type == LogType.Exception ? stackTrace : string.Empty), colours[type]);
            this.queue.Log(element)?.DestroyThis();
        }

        /// <summary>
        /// Saves log messages to a list for future display
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="stackTrace">Log stack trace</param>
        /// <param name="type">Log type</param>
        private void OnLogDelayed(string message, string stackTrace, LogType type)
        {
            this.preLogs.Add(new TempLog(message, stackTrace, type));
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);

            Application.logMessageReceived += OnLogDelayed;
            Debug.Log("Running The Cursed Summit version " + GameVersion.VersionString);
            this.version.text += GameVersion.VersionString;
        }

        private void Start()
        {
            foreach (TempLog temp in this.preLogs)
            {
                OnLog(temp.message, temp.stackTrace, temp.type);
            }
            this.preLogs = null;

            Application.logMessageReceived -= OnLogDelayed;
            Application.logMessageReceived += OnLog;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F11))
            {
                this.Visible = !this.Visible;
            }
        }
        #endregion
    }
}
