using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.Net;
namespace DataTypes
{
    public class NullTager : IAuthority
    {
        public void InsertTags(AuthorityKeywords tagType, AuthorityApperance appearance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents)
        {
            //never be implemented
        }

        public void InsertTags(AuthorityKeywords tagType, AuthorityApperance appearance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, List<string> agents)
        {
            //never be implemented
        }
    }
}
