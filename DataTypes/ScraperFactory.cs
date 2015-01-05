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
    public static class ScraperFactory
    {
        public static IScraper CreateScraper(Article article)
        {
            //bongda.com.vn
            if (article.url.Contains("bongda.com.vn"))
                return new BongDaVnScraper();
            else if (article.url.Contains("thethao247.vn"))
                return new TheThao247Scraper();
            else if (article.url.Contains("ole.vn"))
                return new OleScraper();
            else if (article.url.Contains("bongdalives.com"))
                return new BongDaLivesScraper();
            else if (article.url.Contains("tipkeo.com"))
                return new TipKeoScraper();
            else if (article.url.Contains("vnexpress.net"))
                return new VnexpressScraper();
            else if (article.url.Contains("dantri.com.vn"))
                return new DantriScraper();
            else if (article.url.Contains("bongdaplus.vn"))
                return new BongDaPlusScraper();
            else if (article.url.Contains("ibongda.vn"))
                return new IBongdaScraper();
            else if (article.url.Contains("adult-mag.com"))
                return new AdultMagScraper();
            else if (article.url.Contains("bromygod.com"))
                return new BroMyGodScraper();
            else if (article.url.Contains("celebromance.com"))
                return new CelebromanceComScraper();
            else if (article.url.Contains("datingadvice.com"))
                return new DatingAdviceScraper();
            else if (article.url.Contains("doctornerdlove.com"))
                return new DoctorNerdLoveScraper();
            else if (article.url.Contains("evanmarckatz.com"))
                return new EvanmarckatzScraper();
            else if (article.url.Contains("heavy.com"))
                return new HeavyScraper();
            else if (article.url.Contains("huffingtonpost.com"))
                return new HuffingtonPostScraper();
            else if (article.url.Contains("news.com.au"))
                return new NewsComAuScraper();
            else if (article.url.Contains("telegraph.co.uk"))
                return new TelegraphScraper();
            else if (article.url.Contains("theguardian.com"))
                return new TheGuardianScraper();
            else if (article.url.Contains("vietgiaitri.com"))
                return new VietGiaiTriScraper();
            else if (article.url.Contains("tapchidanong.org"))
                return new TapChiDanOngScraper();
            else if (article.url.Contains("ngoisao.vn"))
                return new NgoiSaoScraper();
            else if (article.url.Contains("giovangchotso.net"))
                return new GioVangChotSoScraper();
            else if (article.url.Contains("timesoccer.com"))
                return new TimeSoccerComScraper();
            else
                return new NullScraper();
        }
    }
}
