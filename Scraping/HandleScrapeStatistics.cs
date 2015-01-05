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
    public sealed class HandleScrapeStatistics
    {
        private static HandleScrapeStatistics _instance = null;
        private static readonly object _locker = new object();
        private Window _currentWD;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }

        private TextBox _queueTB;

        public TextBox QueueTB
        {
            get { return _queueTB; }
            set { _queueTB = value; }
        }

        private TextBox _successTB;

        public TextBox SuccessTB
        {
            get { return _successTB; }
            set { _successTB = value; }
        }

        private TextBox _failedTB;

        public TextBox FailedTB
        {
            get { return _failedTB; }
            set { _failedTB = value; }
        }

        private TextBox _threadTB;

        public TextBox ThreadTB
        {
            get { return _threadTB; }
            set { _threadTB = value; }
        }

        private TextBox _savedTB;

        public TextBox SavedTB
        {
            get { return _savedTB; }
            set { _savedTB = value; }
        }

        HandleScrapeStatistics()
        {
            _currentWD = new Window();
        }

        private Action<Window, TextBox, int> _update;

        public static HandleScrapeStatistics Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleScrapeStatistics();
                    }
                }
                return _instance;
            }
        }

        public void SetupHandle(Window wd, Action<Window, TextBox, int> updateStatistics, TextBox queue, TextBox success, TextBox failed, TextBox thread, TextBox saved)
        {
            _currentWD = wd;
            _update = updateStatistics;
            _queueTB = queue;
            _successTB = success;
            _failedTB = failed;
            _threadTB = thread;
            _savedTB = saved;
        }

        public void UpdateTextBox(string name, int value)
        {
            switch (name)
            {
                case "tbQueue":
                    _update.Invoke(_currentWD, _queueTB, value);
                    break;
                case "tbSuccess":
                    _update.Invoke(_currentWD, _successTB, value);
                    break;
                case "tbFailed":
                    _update.Invoke(_currentWD, _failedTB, value);
                    break;
                case "tbThreads":
                    _update.Invoke(_currentWD, _threadTB, value);
                    break;
                case "tbSaved":
                    _update.Invoke(_currentWD, _savedTB, value);
                    break;
            };
        }
    }
}
