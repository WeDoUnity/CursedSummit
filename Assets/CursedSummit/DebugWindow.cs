using System.Collections.Generic;
using CursedSummit.Extensions;
using CursedSummit.UI;
using CursedSummit.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CursedSummit
{
    [RequireComponent(typeof(Canvas))]
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

        #region Instance
        public static DebugWindow Instance { get; private set; }
        #endregion

        #region Static fields
        private const int maxLogs = 500;
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
        private Text version;
        [SerializeField]
        private GameObject window, logPrefab;
        [SerializeField]
        private VerticalLayoutGroup layout;
        private readonly LogQueue<GameObject> queue = new LogQueue<GameObject>(maxLogs);
        #endregion

        #region Properties
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

        private int index;
        private int Index
        {
            get
            {
                if (this.index >= maxLogs) { this.index = 0; }
                return this.index++;
            }
        }
        #endregion

        #region Methods
        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (stackTrace.Contains("Wwise")) { return; } //Okay so logging Wwise exception makes it go nuts... nice.

            GameObject element = Instantiate(this.logPrefab);
            element.name += this.Index;
            element.transform.parent = this.layout.transform;
            Text label = element.GetComponent<Text>();
            label.text = message + (type == LogType.Exception ? stackTrace : string.Empty);
            label.color = colours[type];
            this.queue.Log(element)?.DestroyThis();
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);

            this.version.text += GameVersion.VersionString;
            Application.logMessageReceived += OnLog;
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKey(KeyCode.F11)) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F11)))
            {
                this.Visible = !this.Visible;
            }
        }
        #endregion
    }
}
