using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IndexWebApi
{
    public class SearchService : ISearchService
    {
        public Dictionary<string, IndDocument> dict = new Dictionary<string, IndDocument>
        {
            { "a", new IndDocument { Title = "Quake II : Evolved v3 [ENG] [Сборка]",  Link = "http://rutracker.org/forum/viewtopic.php?t=3228261",
  Body = "\n\tQuake 2 : Evolved v3\nГод выпуска: 1997/2009\nЖанр: Action\nРазработчик: Id Software\nИздательство: Activision\nВерсия: Игры 3.20/Мода 3.0\nТип издания: Пиратка\nЯзык интерфейса: английский\nЯзык озвучки: английский\nТаблэтка: Не нужна\nСистемные требования:Минимум:\nВидеокарта: GF3, GF4 (не поддерживается GF4MX), Radeon 8500, Radeon 9100, или лучше\nПроцессор: Intel P4 / AMD Athlon.\nПамять: 512 МБРекомендуем:\nВидеокарта: GF6600 или лучше, R9800 или лучше\nПроцессор: Intel Core / AMD Athlon64.\nПамять: 1024 МБОписание:\nТотальная конверсия культовой игры. Для этого проекта командой энтузиастов был написан движок a-la Doom III с поддержкой динамических теней, bump-mapping'а, шейдеров и т.п. Была заменена большая часть текстур и моделей.Доп. информация:\n1)Запускать через q2e.exe (в папке)\n2)Также присутствует оригинальный Quake 2 (без всяких примочек ).Просто запустите QUAKE2.EXE (в папке)\n3)Если будут проблемы ( у меня было 2 ) , пишите в комменты.\n4)Ночью я сплю , поэтому раздавать не буду.(c 11/12:00 до 21:00 раздача , c 21:00 до 11/12:00 дела и сон)\n\nСкриншоты\n\n\n\n\n\n\n\n\n\n\nСравнение(Видео&#41;\nQuake 2 : http://www.youtube.com/watch?v=dp6auQvUyBIer\nQuake 2: Evolved v3 : http://www.youtube.com/watch?v=6Ul6sOqyxT8\n\nКто скачал не уходите с раздачи!!!Я вас по человечески прошу!У меня скорость сейчас не айс (трабла какая-то)!!!Я вижу , что уже есть сиды!Спасибо огромное!Не уходите!\t\t\t\n\t\t\n\t\t\n\t\t\n\t\tDo",
  Magnet =  "magnet:?xt=urn:btih:69FB9EDA6FAD006715A3979E3BC373743486D462&tr=http%3A%2F%2Fbt4.t-ru.org%2Fann%3Fmagnet", Rank = 1 } },
            { "b", new IndDocument { Title = "Quake II : Evolved v3 [ENG] [Сборка]",  Link = "http://rutracker.org/forum/viewtopic.php?t=3228261",
  Body = "\n\tQuake 2 : Evolved v3\nГод выпуска: 1997/2009\nЖанр: Action\nРазработчик: Id Software\nИздательство: Activision\nВерсия: Игры 3.20/Мода 3.0\nТип издания: Пиратка\nЯзык интерфейса: английский\nЯзык озвучки: английский\nТаблэтка: Не нужна\nСистемные требования:Минимум:\nВидеокарта: GF3, GF4 (не поддерживается GF4MX), Radeon 8500, Radeon 9100, или лучше\nПроцессор: Intel P4 / AMD Athlon.\nПамять: 512 МБРекомендуем:\nВидеокарта: GF6600 или лучше, R9800 или лучше\nПроцессор: Intel Core / AMD Athlon64.\nПамять: 1024 МБОписание:\nТотальная конверсия культовой игры. Для этого проекта командой энтузиастов был написан движок a-la Doom III с поддержкой динамических теней, bump-mapping'а, шейдеров и т.п. Была заменена большая часть текстур и моделей.Доп. информация:\n1)Запускать через q2e.exe (в папке)\n2)Также присутствует оригинальный Quake 2 (без всяких примочек ).Просто запустите QUAKE2.EXE (в папке)\n3)Если будут проблемы ( у меня было 2 ) , пишите в комменты.\n4)Ночью я сплю , поэтому раздавать не буду.(c 11/12:00 до 21:00 раздача , c 21:00 до 11/12:00 дела и сон)\n\nСкриншоты\n\n\n\n\n\n\n\n\n\n\nСравнение(Видео&#41;\nQuake 2 : http://www.youtube.com/watch?v=dp6auQvUyBIer\nQuake 2: Evolved v3 : http://www.youtube.com/watch?v=6Ul6sOqyxT8\n\nКто скачал не уходите с раздачи!!!Я вас по человечески прошу!У меня скорость сейчас не айс (трабла какая-то)!!!Я вижу , что уже есть сиды!Спасибо огромное!Не уходите!\t\t\t\n\t\t\n\t\t\n\t\t\n\t\tDo",
  Magnet =  "magnet:?xt=urn:btih:69FB9EDA6FAD006715A3979E3BC373743486D462&tr=http%3A%2F%2Fbt4.t-ru.org%2Fann%3Fmagnet", Rank = 2 } },
            { "c", new IndDocument { Title = "Quake II : Evolved v3 [ENG] [Сборка]",  Link = "http://rutracker.org/forum/viewtopic.php?t=3228261",
  Body = "\n\tQuake 2 : Evolved v3\nГод выпуска: 1997/2009\nЖанр: Action\nРазработчик: Id Software\nИздательство: Activision\nВерсия: Игры 3.20/Мода 3.0\nТип издания: Пиратка\nЯзык интерфейса: английский\nЯзык озвучки: английский\nТаблэтка: Не нужна\nСистемные требования:Минимум:\nВидеокарта: GF3, GF4 (не поддерживается GF4MX), Radeon 8500, Radeon 9100, или лучше\nПроцессор: Intel P4 / AMD Athlon.\nПамять: 512 МБРекомендуем:\nВидеокарта: GF6600 или лучше, R9800 или лучше\nПроцессор: Intel Core / AMD Athlon64.\nПамять: 1024 МБОписание:\nТотальная конверсия культовой игры. Для этого проекта командой энтузиастов был написан движок a-la Doom III с поддержкой динамических теней, bump-mapping'а, шейдеров и т.п. Была заменена большая часть текстур и моделей.Доп. информация:\n1)Запускать через q2e.exe (в папке)\n2)Также присутствует оригинальный Quake 2 (без всяких примочек ).Просто запустите QUAKE2.EXE (в папке)\n3)Если будут проблемы ( у меня было 2 ) , пишите в комменты.\n4)Ночью я сплю , поэтому раздавать не буду.(c 11/12:00 до 21:00 раздача , c 21:00 до 11/12:00 дела и сон)\n\nСкриншоты\n\n\n\n\n\n\n\n\n\n\nСравнение(Видео&#41;\nQuake 2 : http://www.youtube.com/watch?v=dp6auQvUyBIer\nQuake 2: Evolved v3 : http://www.youtube.com/watch?v=6Ul6sOqyxT8\n\nКто скачал не уходите с раздачи!!!Я вас по человечески прошу!У меня скорость сейчас не айс (трабла какая-то)!!!Я вижу , что уже есть сиды!Спасибо огромное!Не уходите!\t\t\t\n\t\t\n\t\t\n\t\t\n\t\tDo",
  Magnet =  "magnet:?xt=urn:btih:69FB9EDA6FAD006715A3979E3BC373743486D462&tr=http%3A%2F%2Fbt4.t-ru.org%2Fann%3Fmagnet", Rank = 3 } }
        };

        public int AddDocuments(string value)
        {
            try
            {
                JObject.Parse(value);
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        IndDocument[] ISearchService.Find(string query)
        {
            List<IndDocument> res = new List<IndDocument>(); 
            if (query.Contains("a"))
            {
                res.Add(dict["a"]);
            }
            if (query.Contains("b"))
            {
                res.Add(dict["b"]);
            }
            if (query.Contains("c"))
            {
                res.Add(dict["c"]);
            }
            else
                res.Add(new IndDocument { Body = query, Rank = 0 });
            return res.ToArray();
        }
    }
}
