using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections;
using Utilities;

namespace Scraping
{
    public sealed class HandleAutoPostMessages
    {
        #region variables

        //for singleton
        private static HandleAutoPostMessages _instance = null;
        private static readonly object _locker = new object();
        private static readonly object _locker1 = new object();
        //for threading
        private BackgroundWorker _worker = new BackgroundWorker();
        public ManualResetEvent _signal = new ManualResetEvent(false);

        //for window & controls collaboration
        private Window _currentWD;
        private TextBox _messageTB;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }
        
        public TextBox MessageTB
        {
            get { return _messageTB; }
            set { _messageTB = value; }
        }

        //for using variables
        private Queue<string> _messages = new Queue<string>();
        public string Format { get; set; }
        private int _delay = 20;

        #endregion

        #region constructors

        HandleAutoPostMessages()
        {
            _messageTB = new TextBox();
            _delay = 20;
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            _worker.WorkerSupportsCancellation = true;
            _signal.Set();
        }

        public static HandleAutoPostMessages Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleAutoPostMessages();
                    }
                }
                return _instance;
            }
        }

        #endregion        

        #region UtilityMethods

        /// <summary>
        /// Handle main operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Action<Window, TextBox, string> action = e.Result as Action<Window, TextBox, string>;

            while (true)
            {
                _signal.WaitOne();
                if (_worker.CancellationPending)
                    break;
                string message = string.Empty;
                lock (_locker1)
                {
                    if (_messages.Count > 0)
                        message = _messages.Dequeue();
                }
                if (message != string.Empty)
                {
                    int a = 0;
                    action.Invoke(_currentWD, _messageTB, message);
                }
                Thread.Sleep(_delay);
            }
            e.Result = action;
        }

        /// <summary>
        /// After finish handle main operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Error!");
            }
            Action<Window, TextBox, string> action = e.Result as Action<Window, TextBox, string>;

            if (e.Cancelled)
                action.Invoke(_currentWD, _messageTB, Utilities.Helper.CreateMessage(Format, "Canceled!"));
            else
                action.Invoke(_currentWD, _messageTB, Utilities.Helper.CreateMessage(Format, "Done!"));
        }

        /// <summary>
        /// Setup handle
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="tb"></param>
        /// <param name="delayInterval"></param>
        public void SetupHandle(Window wd, TextBox tb, int delayInterval)
        {
            _currentWD = wd;
            _messageTB = tb;
            _delay = delayInterval;
        }

        /// <summary>
        /// Reset handle
        /// </summary>
        public void ResetHandle()
        {
            CurrentWD = null;
            MessageTB = null;
            _delay = 10;
        }

        /// <summary>
        /// Start handle operation
        /// </summary>
        /// <param name="action"></param>
        public void RunHandle(Action<Window, TextBox, string> action)
        {
            _worker.RunWorkerAsync(action);
        }

        /// <summary>
        /// Cancel handle operation
        /// </summary>
        public void CancelHandle()
        {
            _worker.CancelAsync();
        }

        /// <summary>
        /// Add new message to Handle
        /// </summary>
        /// <param name="message"></param>
        public void AddMessages(string message)
        {
            lock (_locker1)
                _messages.Enqueue(message);
        }

        /// <summary>
        /// Pause handle operation
        /// </summary>
        public void PauseHandle()
        {
            _signal.Reset();
        }

        /// <summary>
        /// Resume handle operation
        /// </summary>
        public void ResumeHandle()
        {
            _signal.Set();
        }

        #endregion
    }
}
