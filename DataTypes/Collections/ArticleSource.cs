using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTypes.Enums;

namespace DataTypes.Collections
{
    /// <summary>
    /// Represent for an article source
    /// </summary>
    public class ArticleSource
    {       
        //unique name of the source
        private string _name;

        //title of the website
        private string _title;

        //unique website's address
        private string _url;

        //website's language
        private ArticleSourceLanguage _language;
       
        //website's type
        private List<ArticleSourceType> _types;
        
        //Choosen or not
        private bool _choosen;

        private List<string> _forbiddenCategories;

        //private List<string> _newsCategories;

        private List<string> _galleryCategories;

        private string _scrapeLinkStructure;

        private List<SourceCategory> _categories;

        public List<SourceCategory> Categories
        {
            get { return _categories; }            
        }
        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public bool Choosen
        {
            get { return _choosen; }
            set { _choosen = value; }
        }

        public List<string> ForbiddenCategories
        {
            get { return _forbiddenCategories; }
            set { _forbiddenCategories = value; }
        }
        
        public List<string> GalleryCategories
        {
            get { return _galleryCategories; }
            set { _galleryCategories = value; }
        }

        public string ScrapeLinkStructure
        {
            get { return _scrapeLinkStructure; }
            set { _scrapeLinkStructure = value; }
        }

        public ArticleSourceLanguage Language
        {
            get { return _language; }
            set { _language = value; }
        }

        public List<ArticleSourceType> Types
        {
            get { return _types; }
            set { _types = value; }
        }

        public List<string> _cateList = new List<string>();

        public ArticleSource(string name, string title, string url)
        {
            Name = name;
            Title = title;
            Url = url;
            Choosen = true;
            _forbiddenCategories = new List<string>();
            _galleryCategories = new List<string>();
            _language = ArticleSourceLanguage.English;
            _types = new List<ArticleSourceType>();
            _scrapeLinkStructure = "";
            _categories = new List<SourceCategory>();
        }

        public void AddCategory(SourceCategory cate)
        {
            _categories.Add(cate);
        }

        public SourceCategory FindCategory(string host, string slug)
        {
            foreach (var i in _categories)
            {
                if ( string.Compare(i.Slug,slug,true) == 0 && i.SourceHost == host)
                    return i;
            }
            return null;
        }

        public int GetCount()
        {
            return _categories.Count;
        }

        public void LoadListCategories(SourceCategory parent)
        {
            foreach (var cate in _categories)
            {
                if (cate.ParentCategory == parent)
                {
                    string value = _title + "|" + cate.Slug + cate.GetParentCategorySlug();
                    _cateList.Add(value);
                    LoadListCategories(cate);
                }
            }
        }

        public void GetCategoryChildren(List<SourceCategory> children, SourceCategory check)
        {
            foreach (var cat in _categories)
            {
                if (cat.ParentCategory == check)
                {
                    if (!children.Contains(cat))
                        children.Add(cat);
                    GetCategoryChildren(children, cat);
                }
            }
            if (!children.Contains(check))
                children.Add(check);
        }
    }
}
