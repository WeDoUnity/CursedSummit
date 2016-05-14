using System.Collections.Generic;
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
            public void Log(T item)
            {
                if (!this.HasRoom) { Dequeue(); }
                base.Enqueue(item);
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

        #region Fields
        [SerializeField]
        private Text title;
        [SerializeField]
        private GameObject window;
        private readonly LogQueue<string> log = new LogQueue<string>(500);
        #endregion

        #region Properties
        private bool visible;
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (value != this.visible)
                {
                    this.window.SetActive(value);
                    this.visible = value;
                }
            }
        }
        #endregion

        #region Methods
        private void OnLog(string condition, string stackTrace, LogType type)
        {
            this.log.Log(condition);
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this);

            this.title.text += GameVersion.VersionString;
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
