using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class HarvestLink
    {
        public string articleUrl { get; set; }
        public DateTime posted { get; set; }
        public string harvestUrl { get; set; }

        public HarvestLink(string url, string hUrl)
            : this(url, DateTime.Now, hUrl)
        {
        }

        public HarvestLink(string url, DateTime value, string hUrl)
        {
            articleUrl = url;
            posted = value;
            harvestUrl = hUrl;
        }
    }
}
