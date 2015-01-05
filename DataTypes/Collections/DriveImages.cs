using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class DriveImages
    {
        private string _sourceUrl;

        public string SourceUrl
        {
            get { return _sourceUrl; }
            set { _sourceUrl = value; }
        }

        private string _sourcePath;

        public string SourcePath
        {
            get { return _sourcePath; }
            set { _sourcePath = value; }
        }

        private string _drivePath;

        public string DrivePath
        {
            get { return _drivePath; }
            set { _drivePath = value; }
        }

        public DriveImages()
            : this(null, null, null)
        {

        }

        public DriveImages(string sourceURL, string sourcePath, string drive)
        {
            _sourceUrl = sourceURL;
            _sourcePath = sourcePath;
            _drivePath = drive;
        }
    }
}
