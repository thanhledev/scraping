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
using DataTypes.Collections;
using DataTypes.Enums;

namespace Scraping
{
    public sealed class HandleMessageBox
    {
        #region variables

        private static HandleMessageBox _instance = null;
        private static readonly object _locker = new object();
        private static readonly object _actionLocker = new object();
        private ConfirmMessageBox _confirmMessageBox;

        private Window _owner;        
        public Window Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public bool Answer { get; set; }

        #endregion

        #region Constructors

        public static HandleMessageBox Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleMessageBox();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region UtilityMethods

        public void SetupHandle(Window owner)
        {
            _owner = owner;
        }

        public void GetConfirmation(SystemMessage message)
        {
            lock (_actionLocker)
            {
                if (!SysSettings.Instance.IsStopNotify)
                {
                    _confirmMessageBox = new ConfirmMessageBox(_owner, message);
                    _confirmMessageBox.ShowDialog();
                    Answer = _confirmMessageBox.Answer;
                }
                else
                    Answer = true;
            }
        }

        #endregion
    }
}
