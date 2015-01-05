using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;

namespace DataTypes
{
    public static class PathCreaterFactory
    {
        public static IPathCreater GetPathCreater(string link)
        {
            if (link.Contains("bongdalives.com"))
                return new BongDaLivesPathCreater();
            else if (link.Contains("bongda.com.vn"))
                return new BongDaVnPathCreater();
            else if (link.Contains("dantri"))
                return new DantriPathCreater();
            else if (link.Contains("ole.vn"))
                return new OlePathCreater();
            else if (link.Contains("thethao247.vn"))
                return new TheThao247PathCreater();
            else if (link.Contains("tipkeo.com"))
                return new TipKeoPathCreater();
            else if (link.Contains("vnexpress"))
                return new VnexpressPathCreater();
            else if (link.Contains("bongdaplus.vn"))
                return new BongDaPlusPathCreater();
            else if (link.Contains("ibongda.vn"))
                return new IBongdaPathCreater();
            else if (link.Contains("adult-mag.com"))
                return new AdultMagPathCreater();
            else if (link.Contains("bromygod.com"))
                return new BroMyGodPathCreater();
            else if (link.Contains("celebromance.com"))
                return new CelebromanceComPathCreater();
            else if (link.Contains("datingadvice.com"))
                return new DatingAdvicePathCreater();
            else if (link.Contains("doctornerdlove.com"))
                return new DoctorNerdLovePathCreater();
            else if (link.Contains("evanmarckatz.com"))
                return new EvanmarckatzPathCreater();
            else if (link.Contains("heavy.com"))
                return new HeavyPathCreater();
            else if (link.Contains("huffingtonpost.com"))
                return new HuffingtonPostPathCreater();
            else if (link.Contains("news.com.au"))
                return new NewsComAuPathCreater();
            else if (link.Contains("telegraph.co.uk"))
                return new TelegraphPathCreater();
            else if (link.Contains("theguardian.com"))
                return new TheGuardianPathCreater();
            else if (link.Contains("vietgiaitri.com"))
                return new VietGiaiTriPathCreater();
            else if (link.Contains("tapchidanong.org"))
                return new TapChiDanOngPathCreater();
            else if (link.Contains("ngoisao.vn"))
                return new NgoiSaoPathCreater();
            else
                return new NullPathCreater();
        }
    }
}
