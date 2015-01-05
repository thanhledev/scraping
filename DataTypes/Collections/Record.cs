using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class Record
    {
        public string url { get; set; }
        public string hUrl { get; set; }
        public DateTime downloaded { get; set; }
        public DateTime published { get; set; }
        public string filePath { get; set; }
        public bool isPosted { get; set; }
    }
}
