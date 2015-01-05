using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Xml;
using DataTypes.Collections;
using DataTypes.Enums;

namespace Scraping
{
    public sealed class RecordHandler
    {
        #region variables

        private static RecordHandler _instance = null;
        private object _lockObject = null;
        private static readonly object _instancelocker = new object(); //using for instance
        private static readonly object _locker = new object();

        // record list
        public List<Record> _records = new List<Record>();
        private XmlDocument xmlDoc = new XmlDocument();

        #endregion

        #region Constructors

        public static RecordHandler Instance
        {
            get
            {
                lock (_instancelocker)
                {
                    if (_instance == null)
                    {
                        _instance = new RecordHandler();
                    }
                }
                return _instance;
            }
        }

        RecordHandler()
        {
            xmlDoc.Load("records.xml");
            //Load records            
            LoadRecords();
        }

        #endregion

        #region PrivateMethods

        private void LoadRecords()
        {
            using (XmlReader reader = XmlReader.Create("records.xml"))
            {
                _records = (from rcds in reader.GetRecords()
                            select rcds).ToList();
            }
        }

        private bool IsExisted(Record item)
        {
            return _records.Where(a => a.url == item.url).Any();
        }

        private XmlElement FromRecordToElement(Record item)
        {
            XmlElement newRecords = xmlDoc.CreateElement("record");
            
            newRecords.SetAttribute("url", item.url);
            newRecords.SetAttribute("hUrl", item.hUrl);  
            newRecords.SetAttribute("downloaded", item.downloaded.ToString());
            newRecords.SetAttribute("published", item.published.ToString());
            newRecords.SetAttribute("filePath", item.filePath);
            newRecords.SetAttribute("isPosted", item.isPosted.ToString());

            return newRecords;
        }

        #endregion

        #region UtilityMethods

        public bool CheckLock(object check)
        {
            return _lockObject.Equals(check);
        }

        public void ConsumeHandler(object lockObject, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject != null)
                {
                    if (!CheckLock(lockObject))
                        result = false;
                    else
                        result = true;
                }
                else
                {
                    _lockObject = lockObject;
                    result = true;
                }
            }
        }

        public void ReleaseHandler(object lockObject, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject == null)
                    result = false;
                else
                {
                    if (!CheckLock(lockObject))
                        result = false;
                    else
                    {
                        _lockObject = null;
                        result = true;
                    }
                }
            }
        }

        public void AddNewRecords(object lockObject, Record item)
        {
            if (CheckLock(lockObject))
            {
                if (!IsExisted(item))
                {
                    //add record to list
                    _records.Add(item);

                    //append to xml file
                    XmlElement newItem = FromRecordToElement(item);

                    xmlDoc.DocumentElement.AppendChild(newItem);
                    xmlDoc.Save("records.xml");
                }
            }
        }

        public List<Record> GetRecordsByDate(object lockObject, DateTime From, DateTime To)
        {
            if (CheckLock(lockObject))
            {
                return _records.Where(a => a.published >= From && a.published <= To).OrderBy(a=>a.published).ToList();
            }
            return null;
        }

        public void Initialize()
        {

        }

        #endregion
    }
}
