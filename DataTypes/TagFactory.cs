using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public static class TagFactory
    {
        public static IAuthority CreateTager(AuthoritySearchOptions options)
        {
            switch (options)
            {
                case AuthoritySearchOptions.SearchEngine:
                    return new SearchEngineTager();
                case AuthoritySearchOptions.HighAuthoritySite:
                    return new HighAuthorityTager();
                default:
                    return new NullTager();
            }
        }
    }
}
