using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class PortValueEncrypt
    {
        public string portSignal;
        public string portValue;

        public PortValueEncrypt(string signal, string value)
        {
            portSignal = signal;
            portValue = value;
        }
    }
}
