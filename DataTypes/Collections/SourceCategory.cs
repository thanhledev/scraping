using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class SourceCategory
    {
        private string _sourceHost;

        public string SourceHost
        {
            get { return _sourceHost; }
            set { _sourceHost = value; }
        }

        private string _slug;

        public string Slug
        {
            get { return _slug; }
            set { _slug = value; }
        }

        private SourceCategory _parentCategory;

        public SourceCategory ParentCategory
        {
            get { return _parentCategory; }
            set { _parentCategory = value; }
        }

        public SourceCategory(string sourceHost, string slug)
            : this(sourceHost, slug, null)
        {

        }

        public SourceCategory(string sourceHost, string slug, SourceCategory parent)
        {
            _sourceHost = sourceHost;
            _slug = slug;
            if (parent != null)
                _parentCategory = parent;
        }

        public string GetParentCategorySlug()
        {
            return _parentCategory != null ? "|" + _parentCategory.Slug : "";
        }  
    }
}
