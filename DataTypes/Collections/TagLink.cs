using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class TagLink
    {
        public string Tag { get; set; }
        public string AuthorityLink { get; set; }

        public TagLink()
            : this("", "")
        {

        }

        public TagLink(string tag, string link)
        {
            Tag = tag;
            AuthorityLink = link;
        }
    }
}
