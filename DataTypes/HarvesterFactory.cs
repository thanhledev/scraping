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
    public static class HarvesterFactory
    {
        public static IHarvest CreateHarvester(string link)
        {
            if (link.Contains("bongda.com.vn"))
                return new BongDaVnHarvester();
            else if (link.Contains("thethao247.vn"))
                return new TheThao247Harvester();
            else if (link.Contains("ole.vn"))
                return new OleHarvester();
            else if (link.Contains("bongdalives.com"))
                return new BongDaLivesHarvester();
            else if (link.Contains("tipkeo.com"))
                return new TipKeoHarvester();
            else if (link.Contains("vnexpress.net"))
                return new VnexpressHarvester();
            else if (link.Contains("dantri.com.vn"))
                return new DantriHarvester();
            else if (link.Contains("bongdaplus.vn"))
                return new BongDaPlusHarvester();
            else if (link.Contains("ibongda.vn"))
                return new IBongdaHarvester();
            else if (link.Contains("adult-mag.com"))
                return new AdultMagHarvester();
            else if (link.Contains("bromygod.com"))
                return new BroMyGodHarvester();
            else if (link.Contains("celebromance.com"))
                return new CelebromanceComHarvester();
            else if (link.Contains("datingadvice.com"))
                return new DatingAdviceHarvester();
            else if (link.Contains("doctornerdlove.com"))
                return new DoctorNerdLoveHarvester();
            else if (link.Contains("evanmarckatz.com"))
                return new EvanmarckatzHarvester();
            else if (link.Contains("heavy.com"))
                return new HeavyHarvester();
            else if (link.Contains("huffingtonpost.com"))
                return new HuffingtonPostHarvester();
            else if (link.Contains("news.com.au"))
                return new NewsComAuHarvester();
            else if (link.Contains("telegraph.co.uk"))
                return new TelegraphHarvester();
            else if (link.Contains("theguardian.com"))
                return new TheGuardianHarvester();
            else if (link.Contains("vietgiaitri.com"))
                return new VietGiaiTriHarvester();
            else if (link.Contains("tapchidanong.org"))
                return new TapChiDanOngHarvester();
            else if (link.Contains("ngoisao.vn"))
                return new NgoiSaoHarvester();
            else if (link.Contains("giovangchotso.net"))
                return new GioVangChotSoHarvester();
            else if (link.Contains("timesoccer.com"))
                return new TimeSoccerHarvester();
            else 
                return new NullHarvester();
        }
    }
}
