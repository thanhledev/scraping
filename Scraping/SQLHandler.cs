using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Scraping
{
    public sealed class SQLHandler
    {
        #region variables

        private static SQLHandler _instance = null;
        private object _lockObject = null;
        private static readonly object _instancelocker = new object(); //using for instance
        private static readonly object _locker = new object();
        private readonly SqlConnection conn = new SqlConnection(@"Data Source=MPV17035;Initial Catalog=MockData;Integrated Security=True");
        //private readonly SqlConnection conn = new SqlConnection(@"Data Source=KUPO-PC\\SQLEXPRESS;Initial Catalog=MockData;Integrated Security=True");
        #endregion

        #region Constructors

        SQLHandler()
        {

        }

        public static SQLHandler Instance
        {
            get
            {
                lock (_instancelocker)
                {
                    if (_instance == null)
                    {
                        _instance = new SQLHandler();
                    }
                }
                return _instance;
            }
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
                    conn.Open();
                    _lockObject = lockObject;
                    result = true;
                }
            }
        }

        public Tuple<bool, SqlConnection> GetDBConnection(object check)
        {
            if (!CheckLock(check))
                return new Tuple<bool, SqlConnection>(false, null);
            else
            {
                return new Tuple<bool, SqlConnection>(true, conn);
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
                        conn.Close();
                        _lockObject = null;
                        result = true;
                    }
                }
            }
        }

        #endregion
    }
}
