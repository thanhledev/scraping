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
    public static class SpawnerFactory
    {
        public static ISpawning CreateSpawner(ArticleSource source)
        {
            //bongda.com.vn
            if (source.Url.Contains("bongda.com.vn"))
                return new BongDaVnSpawner();
            else if (source.Url.Contains("thethao247.vn"))
                return new TheThao247Spawner();
            else if (source.Url.Contains("ole.vn"))
                return new OleSpawner();
            else if (source.Url.Contains("bongdalives.com"))
                return new BongDaLivesSpawner();
            else if (source.Url.Contains("tipkeo.com"))
                return new TipKeoSpawner();
            else if (source.Url.Contains("vnexpress.net"))
                return new VnexpressSpawner();
            else if (source.Url.Contains("dantri.com.vn"))
                return new DantriSpawner();
            else if (source.Url.Contains("bongdaplus.vn"))
                return new BongDaPlusSpawner();
            else if (source.Url.Contains("ibongda.vn"))
                return new IBongdaSpawner();
            else if (source.Url.Contains("adult-mag.com"))
                return new AdultMagSpawner();
            else if (source.Url.Contains("bromygod.com"))
                return new BroMyGodSpawner();
            else if (source.Url.Contains("celebromance.com"))
                return new CelebromanceComSpawner();
            else if (source.Url.Contains("datingadvice.com"))
                return new DatingAdviceSpawner();
            else if (source.Url.Contains("doctornerdlove.com"))
                return new DoctorNerdLoveSpawner();
            else if (source.Url.Contains("evanmarckatz.com"))
                return new EvanmarckatzSpawner();
            else if (source.Url.Contains("heavy.com"))
                return new HeavySpawner();
            else if (source.Url.Contains("huffingtonpost.com"))
                return new HuffingtonPostSpawner();
            else if (source.Url.Contains("news.com.au"))
                return new NewsComAuSpawner();
            else if (source.Url.Contains("telegraph.co.uk"))
                return new TelegraphSpawner();
            else if (source.Url.Contains("theguardian.com"))
                return new TheGuardianSpawner();
            else if (source.Url.Contains("vietgiaitri.com"))
                return new VietGiaiTriSpawner();
            else if (source.Url.Contains("tapchidanong.org"))
                return new TapChiDanOngSpawner();
            else if (source.Url.Contains("ngoisao.vn"))
                return new NgoiSaoSpawner();
            else if (source.Url.Contains("giovangchotso.net"))
                return new GioVangChotSoSpawner();
            else if (source.Url.Contains("timesoccer.com"))
                return new TimeSoccerComSpawner();
            else
                return new NullSpawner();
        }
    }
}
