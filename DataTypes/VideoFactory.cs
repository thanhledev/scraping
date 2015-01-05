using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public static class VideoFactory
    {
        public static IVideo CreateVideoInserter(AuthoritySearchOptions option)
        {
            switch (option)
            {
                case AuthoritySearchOptions.HighAuthoritySite:
                    return new HighAuthorityVideoInserter();                    
                case AuthoritySearchOptions.SearchEngine:
                    return new SearchEngineVideoInserter();                    
                default:
                    return new NullVideoInserter();                    
            }
        }
    }
}
