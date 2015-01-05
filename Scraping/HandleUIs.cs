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
using DataTypes.Collections;
using DataTypes.Enums;
using System.Net;

namespace Scraping
{
    public class HandleUIs
    {        
        //public List<DockPanel> _enable;
        public List<DockPanel> _hidden;
        public List<DockPanel> _show;

        public Window _currentWD;

        //private Action<Window, List<DockPanel>, bool> _enableChanged;
        private Action<Window, List<DockPanel>, bool> _hiddenChanged;
        private Action<Window, List<DockPanel>, bool> _showChanged;

        //public bool _enableFlag;

        public HandleUIs()
        {
            _currentWD = new Window();
            //_enable = new List<DockPanel>();
            _hidden = new List<DockPanel>();
            _show = new List<DockPanel>();            
        }
        public void SetupHandle(Window wd, Action<Window, List<DockPanel>, bool> hiddenChanged, Action<Window, List<DockPanel>, bool> showChanged)
        {
            _currentWD = wd;            
            //_enableChanged = enableChanged;
            _hiddenChanged = hiddenChanged;
            _showChanged = showChanged;
        }

        //public void AddEnable(DockPanel ctr)
        //{
        //    _enable.Add(ctr);
        //}

        public void AddHidden(DockPanel ctr)
        {
            _hidden.Add(ctr);
        }

        public void AddShow(DockPanel ctr)
        {
            _show.Add(ctr);
        }

        public void RunHandle(bool enable)
        {
            //_enableFlag = enable;
            //if (_enable.Count > 0)
            //    _enableChanged.Invoke(_currentWD, _enable, _enableFlag);
            if(_hidden.Count > 0)
                _hiddenChanged.Invoke(_currentWD, _hidden, true);
            if(_show.Count > 0)
                _showChanged.Invoke(_currentWD, _show, true);
        }

        public void ReverseHandle()
        {
            //if (_enable.Count > 0)
            //    _enableChanged.Invoke(_currentWD, _enable, !_enableFlag);
            if (_hidden.Count > 0)
                _hiddenChanged.Invoke(_currentWD, _hidden, false);
            if (_show.Count > 0)
                _showChanged.Invoke(_currentWD, _show, false);
        }

        public void ResetHandle()
        {            
            //_enable.Clear();
            _hidden.Clear();
            _show.Clear();
        }        
    }
}
