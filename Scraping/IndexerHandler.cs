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
    public sealed class IndexerHandler
    {
        #region Variables

        //own variables
        private static IndexerHandler _instance = null;
        private static readonly object _instancelocker = new object(); //using for instance
        private static readonly object _locker = new object();
        private static readonly object _stopLocker = new object(); //using for worker 

        private int _indexInterval = 120;
        private static string _defaultValue = "???";

        private Thread _indexer;
        private Thread _indexerManager;

        private List<string> needIndexed_Urls = new List<string>();
        Queue<DateTime> _wakeUpPoint = new Queue<DateTime>();

        private bool _stopIndexer = false;

        //shared variables
        private List<IndexerService> _indexerServices = new List<IndexerService>();

        public List<IndexerService> IndexerServices
        {
            get { return _indexerServices; }            
        }

        public bool _autoindex;

        #endregion

        #region Constructors

        public static IndexerHandler Instance
        {
            get
            {
                lock (_instancelocker)
                {
                    if (_instance == null)
                    {
                        _instance = new IndexerHandler();
                    }
                }
                return _instance;
            }
        }

        IndexerHandler()
        {            
            if (System.IO.File.Exists(SysSettings._systemIniFile))
            {
                LoadIndexerSettings();
            }

            _indexerManager = new Thread(indexerManager_DoWork);
            _indexerManager.IsBackground = true;

            if (_autoindex)
            {
                CreateIndexSchedule();
                _indexerManager.Start();
            }
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Load Index settings
        /// </summary>
        private void LoadIndexerSettings()
        {
            _autoindex = Convert.ToBoolean(IniHelper.GetIniFileString(SysSettings._systemIniFile, "indexer", "autoindex", _defaultValue));
            List<string> services = IniHelper.GetKeys(SysSettings._systemIniFile, "indexerservices");

            foreach (var service in services)
            {
                string[] serviceInfos = IniHelper.GetIniFileString(SysSettings._systemIniFile, "indexerservices", service, _defaultValue).Split('|');
                IndexerService iService = new IndexerService(serviceInfos[0], serviceInfos[1], serviceInfos[2], Convert.ToBoolean(serviceInfos[3]));
                iService.DecryptPassword = StringCipher.Decrypt(serviceInfos[2], SysSettings.Instance.EncryptKey);

                _indexerServices.Add(iService);
            }
        }

        /// <summary>
        /// Save index settings
        /// </summary>
        private void SaveIndexerSettings()
        {
            IniHelper.WriteKey(SysSettings._systemIniFile, "indexer", "autoindex", _autoindex.ToString());
            int count = 1;
            foreach (var service in _indexerServices)
            {
                IniHelper.WriteKey(SysSettings._systemIniFile, "indexerservices", "service" + count, service.ServiceName + "|" + service.ServiceAPI + "|" + service.EncryptPassword + "|" + service.Chosen.ToString());
                count++;
            }
        }

        /// <summary>
        /// Create WakeUp Schedule for comeback interval
        /// </summary>
        private void CreateIndexSchedule()
        {
            _wakeUpPoint.Clear();

            DateTime holdPoint = DateTime.Now;
            DateTime temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, holdPoint.Hour, holdPoint.Minute, holdPoint.Second);

            _wakeUpPoint.Enqueue(holdPoint);
            do
            {
                temp = temp.AddMinutes(_indexInterval);
                if (temp < holdPoint)
                    continue;
                else
                    _wakeUpPoint.Enqueue(temp);
            } while (temp < holdPoint.AddDays(30));
        }

        /// <summary>
        /// Harvester worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        private void indexer_DoWork()
        {
            if (_indexerServices.Any(a => a.Chosen == true))
            {
                List<string> urls = new List<string>();
                lock (_locker)
                {
                    foreach (var i in needIndexed_Urls)
                    {
                        urls.Add(i);
                    }
                    needIndexed_Urls = new List<string>();
                }
                _indexerServices.Where(a => a.Chosen == true).First().IndexUrls(urls);
            }
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Insert need indexed url to system
        /// </summary>
        /// <param name="link"></param>
        public void InsertIndexUrls(string link)
        {
            lock (_locker)
            {
                needIndexed_Urls.Add(link);
            }
        }

        /// <summary>
        /// Try to stop index systems
        /// </summary>
        public void Stop()
        {
            lock (_stopLocker)
                _stopIndexer = true;            
        }

        public void RunHandle()
        {
            _indexerManager.Start();
        }        

        public void Initialized()
        {

        }

        public void SaveSystem()
        {
            SaveIndexerSettings();
        }

        public void UpdateIndexerService(IndexerService update)
        {
            if (!_indexerServices.Any(a => a.Chosen == true))
            {
                foreach (var service in _indexerServices)
                {
                    if (service.ServiceName == update.ServiceName)
                    {
                        service.UpdateService(update.ServiceAPI, StringCipher.Encrypt(update.DecryptPassword, SysSettings.Instance.EncryptKey), update.DecryptPassword, update.Chosen);
                    }
                }
            }
            else
            {
                if (update.Chosen)
                {
                    foreach (var service in _indexerServices)
                    {
                        if (service.ServiceName == update.ServiceName)
                        {
                            service.UpdateService(update.ServiceAPI, StringCipher.Encrypt(update.DecryptPassword, SysSettings.Instance.EncryptKey), update.DecryptPassword, update.Chosen);
                        }
                        else
                        {
                            service.Chosen = false;
                        }
                    }
                }
                else
                {
                    foreach (var service in _indexerServices)
                    {
                        if (service.ServiceName == update.ServiceName)
                        {
                            service.UpdateService(update.ServiceAPI, StringCipher.Encrypt(update.DecryptPassword, SysSettings.Instance.EncryptKey), update.DecryptPassword, update.Chosen);
                        }
                    }
                }
            }
        }

        public void UpdateAutoIndex(bool value)
        {
            if (_autoindex && value)
            {

            }
            else if (_autoindex && !value)
            {
                _autoindex = value;
                Stop();
            }
            else if (!_autoindex && value)
            {
                _autoindex = value;
                if(_stopIndexer)
                    _stopIndexer = false;

                _indexerManager = new Thread(indexerManager_DoWork);
                _indexerManager.IsBackground = true;
                _indexerManager.Start();
            }
            else
            {

            }
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Indexer doWork function
        /// </summary>
        private void indexerManager_DoWork()
        {
            do
            {
                DateTime wakeUpPoint = _wakeUpPoint.Dequeue();
                bool flag = false;

                while (!flag)
                {
                    lock (_stopLocker)
                    {
                        if (_stopIndexer)
                            goto StopThread;
                    }

                    if (DateTime.Now > wakeUpPoint)
                    {
                        flag = true;

                        _indexer = new Thread(indexer_DoWork);
                        _indexer.IsBackground = true;
                        _indexer.Start();
                    }
                    else
                        Thread.Sleep(60000);
                }

            } while (_wakeUpPoint.Count > 0);
            StopThread: if (_stopIndexer) StopIndexed();
        }

        /// <summary>
        /// Stop index function
        /// </summary>
        private void StopIndexed()
        {

        }

        #endregion
    }
}
