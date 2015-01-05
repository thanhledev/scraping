using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataTypes.Collections;
using DataTypes.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Scraping
{
    public sealed class HandleSystemStatistics
    {
        #region variables

        private static readonly object _locker = new object();
        private static HandleSystemStatistics _instance = null;
        
        private Thread _cpuWatcher;
        private Thread _ramWatcher;
        private static Process p = Process.GetCurrentProcess();
        private static PerformanceCounter _cpuCounter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);
        private static PerformanceCounter _ramCounter = new PerformanceCounter("Process", "Working Set - Private", p.ProcessName);

        private Window _currentWD;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }

        private TextBox _cpuTextBox;

        public TextBox CpuTextBox
        {
            get { return _cpuTextBox; }
            set { _cpuTextBox = value; }
        }

        private TextBox _ramTextBox;

        public TextBox RamTextBox
        {
            get { return _ramTextBox; }
            set { _ramTextBox = value; }
        }

        public Action<Window, TextBox, string> updateCPUStatistic;
        public Action<Window, TextBox, string> updateRamStatistic;

        #endregion

        #region Constructors

        HandleSystemStatistics()
        {
            _cpuTextBox = new TextBox();
            _ramTextBox = new TextBox();
        }

        public static HandleSystemStatistics Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleSystemStatistics();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region UtilityMethods

        public void SetupHandle(Window wd, TextBox cpu, TextBox ram, Action<Window, TextBox, string> updateTextbox)
        {
            _currentWD = wd;
            _cpuTextBox = cpu;
            _ramTextBox = ram;
            updateCPUStatistic = updateTextbox;
            updateRamStatistic = updateTextbox;

            _cpuWatcher = new Thread(MonitoringCPUUsage);
            _ramWatcher = new Thread(MonitoringRAMUsage);
        }

        public void RunHandle()
        {
            _cpuWatcher.Start();
            _ramWatcher.Start();
        }

        public void StopHandle()
        {
            _cpuWatcher.Abort();
            _ramWatcher.Abort();
        }

        private void MonitoringCPUUsage()
        {
            while (true)
            {
                dynamic firstValue = _cpuCounter.NextValue();
                Thread.Sleep(1000);
                dynamic secondValue = _cpuCounter.NextValue();

                updateCPUStatistic.Invoke(_currentWD, _cpuTextBox, string.Format("{0:N1} %", secondValue));
            }
        }

        private void MonitoringRAMUsage()
        {
            while (true)
            {
                double ram = _ramCounter.NextValue();
                updateRamStatistic.Invoke(_currentWD, _ramTextBox, string.Format("{0:N2} MB", ram / 1024 / 1024));
                Thread.Sleep(1000);
            }
        }
        
        #endregion
    }
}
