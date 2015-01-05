using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class Language
    {
        //unique name of the source
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //Choosen or not
        private bool _choosen;

        public bool Choosen
        {
            get { return _choosen; }
            set { _choosen = value; }
        }

        public Language(string name, bool choosen)
        {
            Name = name;
            Choosen = choosen;
        }
    }
}
