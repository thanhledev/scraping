using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class SystemProxy
    {
        private string _ip;

        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private string _port;

        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private bool _isPrivate;

        public bool IsPrivate
        {
            get { return _isPrivate; }
            set { _isPrivate = value; }
        }
        
        public SystemProxy()
        {
            _username = "";
            _password = "";
            _isPrivate = false;
        }

        public SystemProxy(string IP, string PORT, string USER, string PASS)
        {
            _ip = IP;
            _port = PORT;
            _username = USER;
            _password = PASS;
            _isPrivate = false;
        }

        public override string ToString()
        {
            if(IsPrivate)
                return String.Format("{0}:{1}:{2}:{3}", _ip, _port, _username, _password);
            else
                return String.Format("{0}:{1}", _ip, _port);
        }        
    }
}
