using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public static class TextFeatureMapper
    {
        static char[] toReplace = "-()\"\';. ,\n\t\r".ToArray();

        static string Clear(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.ToUpper().ReplaceAll(toReplace);
        }
        public static double LangMap(string s)
        {
            s = Clear(s);
            double accum = -1;
            if (s.ContainsAny("ДУБЛ"))
                accum += 5;
            if (s.ContainsAny("ПРОФ"))
                accum += 4;
            if (s.ContainsAny("МНОГ"))
                accum += 3;
            if (s.ContainsAny("РУССКИ", "RUS", "НЕТРЕБ", "НЕВАЖН"))
                accum += 2;
            if (s.ContainsAny("ДВУХГОЛ"))
                accum += 2;
            if (s.ContainsAny("УБТИТ", "САБ", "SUB"))
                accum += 1;
            if (s.ContainsAny("ПОНСК", "JAP"))
                accum += 1;
            if (s.ContainsAny("ОДНОГОЛОС", "АВТОРСК"))
                accum += 1;
            if (s.ContainsAny("ENG", "НГЛ", "ОТСУТСТВУ", "ОРИГИН", "НЕТ"))
                accum += 0;
            if (s.ContainsAny("ЛЮБИТ"))
                accum += -1;
            return (accum + 2) / 18;
        }

        public static double QualMap(string s)
        {
            s = Clear(s);
            int start = -1;
            if (s.Contains("CAM"))
                start += -5;
            if (s.Contains("TS"))
                start += -3;
            if (s.Contains("VHS"))
                start += -1;
            if (s.ContainsAny("TVRIP", "SATRIP", "DVB", "WEBDV", "WEBRIP"))
                start += 0;
            if (s.Contains("DVDRIP"))
                start += 1;
            if (s.ContainsAny("HDRIP", "HDTVRIP", "BDRIP"))
                start += 2;
            if (s.ContainsAny("DVD5", "DVD9", "WEBDL"))
                start += 3;
            if (s.ContainsAny("BLURAY", "REMUX"))
                start += 4;
            return (start + 10.0) / 19;
        }

        public static double KeygenMap(string s)
        {
            s = Clear(s);
            if (s.ContainsAny("ЕТРЕБ", "DRMFREE", "НЕНУЖ", "ЭТРЭБУЕ"))
                return 1;
            if (s.ContainsAny("ЛЕЧ", "ШИТА"))
                return 0.75;
            if (s.ContainsAny("РИСУТ", "CODEX", "RELOAD", "ЕСТЬ", "СЕРИ", "STEAM"))
                return 0.5;
            if (s.ContainsAny("ЭМУЛ", "ОБРАЗ"))
                return 0.25;
            if (s.ContainsAny("BLURAY", "REMUX"))
                return 0;
            return 0.1;
        }
    }
}
