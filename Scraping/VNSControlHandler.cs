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
using System.Net;
using System.IO;
using Utilities;
using HtmlAgilityPack;
using DataTypes;
using DataTypes.Interfaces;
using DataTypes.Collections;
using DataTypes.Enums;

namespace Scraping
{
    public sealed class VNSHarvestControlHandler
    {
        #region variables

        private static VNSHarvestControlHandler _instance = null;
        private static readonly object _instanceLocker = new object();

        private object _lockObject = null;
        private string _lockObjectName = "";
        private static readonly object _locker = new object();

        #endregion

        #region constructors

        VNSHarvestControlHandler()
        {

        }

        public static VNSHarvestControlHandler Instance
        {
            get
            {
                lock (_instanceLocker)
                {
                    if (_instance == null)
                    {
                        _instance = new VNSHarvestControlHandler();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region public functions

        public bool CheckLock(object check, string name)
        {
            return _lockObject.Equals(check) && _lockObjectName == name;
        }

        public void ConsumeHandler(object lockObject, string name, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject != null)
                {
                    if (!CheckLock(lockObject, name))
                        result = false;
                    else
                        result = true;
                }
                else
                {
                    _lockObject = lockObject;
                    _lockObjectName = name;
                    result = true;
                }
            }
        }

        public void ReleaseHandler(object lockObject, string name, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject == null)
                    result = false;
                else
                {
                    if (!CheckLock(lockObject, name))
                        result = false;
                    else
                    {
                        _lockObject = null;
                        _lockObjectName = "";
                        result = true;
                    }
                }
            }
        }

        public void Initialize()
        {

        }

        #endregion

        #region private functions



        #endregion
    }
}
