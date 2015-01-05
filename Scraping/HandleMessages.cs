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
    public sealed class HandleMessages
    {
        private static HandleMessages _instance = null;
        private static readonly object _locker = new object();
        private Queue<string> _messages = new Queue<string>();
        private BackgroundWorker _worker = new BackgroundWorker();
        public ManualResetEvent _signal = new ManualResetEvent(false);
        public string Format { get; set; }
        private int _delay = 0;

        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        private Window _currentWD;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }

        private TextBox _messageTB;

        public TextBox MessageTB
        {
            get { return _messageTB; }
            set { _messageTB = value; }
        }

        HandleMessages()
        {
            _delay = 20;            
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            _worker.WorkerSupportsCancellation = true;
            _signal.Set();
        }

        public static HandleMessages Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleMessages();
                    }
                }
                return _instance;
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Action<string, Window, TextBox> action = e.Argument as Action<string, Window, TextBox>;
            while (true)
            {
                _signal.WaitOne();                
                if (_worker.CancellationPending)
                    break;
                string message = string.Empty;
                lock (_locker)
                {
                    if (_messages.Count > 0)
                        message = _messages.Dequeue();
                }
                if (message != string.Empty)
                {
                    //MainWindow._main.UpdateMessagesBox(message);
                    action.Invoke(message, _currentWD, _messageTB);
                }
                Thread.Sleep(_delay);
            }
            e.Result = action;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Error!");
            }
            Action<string, Window, TextBox> action = e.Result as Action<string, Window, TextBox>;

            if (e.Cancelled)
                action.Invoke(Utilities.Helper.CreateMessage(Format, "Canceled!"), _currentWD, _messageTB);
            else
                action.Invoke(Utilities.Helper.CreateMessage(Format, "Done!"), _currentWD, _messageTB);
        }

        public void SetupHandle(Window wd, TextBox tb,int delayInterval)
        {
            CurrentWD = wd;
            MessageTB = tb;
            Delay = delayInterval;
        }

        public void ResetHandle()
        {
            CurrentWD = null;
            MessageTB = null;
            _delay = 10;
        }

        public void RunHandle(Action<string, Window, TextBox> action)
        {
            _worker.RunWorkerAsync(action);
        }

        public void CancelHandle()
        {
            _worker.CancelAsync();
        }

        public void AddMessages(string message)
        {
            lock (_locker)
            {
                _messages.Enqueue(message);
            }
        }

        public void PauseHandle()
        {
            _signal.Reset();
        }

        public void ResumeHandle()
        {
            _signal.Set();
        }
    }
}
