using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class SEOSentence
    {
        public string _originalSentence;
        public string _seoSentence;

        public SEOSentence()
            : this(null, null)
        {
        }

        public SEOSentence(string original, string seo)
        {
            _originalSentence = original;
            _seoSentence = seo;
        }
    }
}
