using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Interfaces
{
    public interface ISEOPlugin
    {
        void SetupSEOFactors(string title, string desc, string metaKeywords, string addition, List<string> chosenKeywords);
        CustomField[] createSEOFactors();
        CustomField[] remakeSEOFactors(CustomField[] inputFactors, string keywordSignature, List<string> chosenKeywords, string projName);
    }
}
