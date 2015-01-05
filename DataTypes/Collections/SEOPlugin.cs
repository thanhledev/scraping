using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;

namespace DataTypes.Collections
{
    public class SEOPlugin
    {
        private SEOPluginType? _type;

        public SEOPluginType? Type
        {
            get { return _type; }            
        }

        private string _name;

        public string Name
        {
            get { return _name; }            
        }

        public SEOPlugin(SEOPluginType? type, string name)
        {
            _type = type;
            _name = name;
        }
    }
}
