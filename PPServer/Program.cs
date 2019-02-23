using osu.Game.Beatmaps.Formats;
using PerformanceCalculator;
using PerformanceCalculator.Simulate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace PPServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LegacyDifficultyCalculatorBeatmapDecoder.Register();
            if (args.Length > 0)
            {
                if(Directory.Exists(args[0]))
                    ProcessorWorkingBeatmap.FilePath = args[0];
            }
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://127.0.0.1:5555/");
            server.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            server.Start();
            while (true)
            {
                new Thread(new ParameterizedThreadStart(run)){ IsBackground = true }.Start(server.GetContext());
            }
        }

        private static void run(object obj)
        {
            HttpListenerContext context = obj as HttpListenerContext;
            try
            {
                getPP(context);
            }
            catch
            {
                error(context);
            }
        }

        private static void getPP(HttpListenerContext context)
        {
            string RawUrl = context.Request.RawUrl;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (RawUrl.Length > 2)
                RawUrl = RawUrl.Substring(2);

            string[] arr = RawUrl.Split('&');
            foreach (string item in arr)
            {
                string[] a = item.Split('=');
                if (a.Length == 2)
                {
                    dic.Add(a[0], a[1]);
                }
            }

            string bid = GetValue("b", dic);
            string mode = GetValue("m", dic);
            string mods = GetValue("mod", dic);

            SimulateCommand cmd = null;
            switch (mode.ToLower().Trim())
            {
                case "osu":
                case "0":
                    cmd = new OsuSimulateCommand();
                    break;
                case "3":
                case "mania":
                    cmd = new ManiaSimulateCommand();
                    break;
                case "1":
                case "taiko":
                    cmd = new TaikoSimulateCommand();
                    break;
                default:
                    cmd = new OsuSimulateCommand();
                    break;
            }
            if(int.TryParse(bid,out int id))
            {
                cmd.BeatmapID = id;
            }
            else
            {
                cmd.BeatmapID = 0;
            }

            cmd.Mods = GetMods(mods);
            cmd.Accuracy = 100;
            cmd.Execute();
            writeHeader(context);

            StreamWriter sw = new StreamWriter(context.Response.OutputStream);
            sw.Write(cmd.GetJson());
            sw.Dispose();
            context.Response.Abort();
        }

        private static void error(HttpListenerContext context)
        {
            if(context != null)
            {
                try
                {
                    writeHeader(context);
                    StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write("[]");
                    writer.Flush();
                    writer.Dispose();
                    context.Response.Abort();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void writeHeader(HttpListenerContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Headers["Server-Name"] = "PPServer/1.0 By wanjia";
        }


        public static string GetValue(string key, Dictionary<string, string> dic)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                return string.Empty;
            }
        }

        public static string[] GetMods(string mod)
        {
            if (mod.Length >= 2)
            {
                List<string> mods = new List<string>();
                List<string> list = new List<string>()
                {
                    "EZ","NF","HT","HR","HD","DT","FL"
                };
                string tmp = string.Empty;
                while (mod.Length >= 2)
                {
                    tmp = mod.Substring(0, 2).ToUpper();
                    if (list.Contains(tmp))
                    {
                        mods.Add(tmp);
                        list.Remove(tmp);
                    }
                    mod = mod.Substring(2);
                }
                return mods.ToArray();
            }
            else
            {
                return new string[0];
            }
        }

        public static string GetIndex()
        {
            return "<h1>Welcome to osu PP API!</h1>";
        }
    }
}
