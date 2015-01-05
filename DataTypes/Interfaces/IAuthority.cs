using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using System.Net;
namespace DataTypes.Interfaces
{
    public interface IAuthority
    {
        void InsertTags(AuthorityKeywords tagType, AuthorityApperance apperance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents);
        void InsertTags(AuthorityKeywords tagType, AuthorityApperance apperance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, List<string> agents);
    }
}
