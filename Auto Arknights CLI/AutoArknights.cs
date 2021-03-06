﻿using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public class AutoArknights : IDisposable
    {
        private const string ConfigJson = ".\\Config.json";
        private static IConfiguration _config;
        private readonly Automation _automation;

        public AutoArknights()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.BackgroundColor = ConsoleColor.Black;

            if (!Library.CheckIfSupported())
            {
                Log.Error("你的CPU不支持当前OpenCV构建，无法正常运行本程序，抱歉。");
                Console.ReadKey(true);
                return;
            }

            InitConfig();

            Log.LogLevel = Log.Level.Get(Conf("Log:Level")) ?? throw new Exception("设置 Log.Level 的值无效");
            _automation = new Automation(Conf("Adb:Path"), Conf("Adb:Remote"));
        }

        public void Dispose()
        {
            _automation.Dispose();
        }

        public static void InitConfig()
        {
            if (!File.Exists(ConfigJson)) File.Create(ConfigJson);
            _config = new ConfigurationBuilder().AddJsonFile(ConfigJson).Build();
        }

        public void Run()
        {
            var cin = new Cin();
            RepeatLevelAction.Mode mode = cin.Get<RepeatLevelAction.Mode>("输入模式");
            int repeatTime = -1;
            if (mode == RepeatLevelAction.Mode.SpecifiedTimes || mode == RepeatLevelAction.Mode.SpecTimesWithWait)
                repeatTime = cin.Get<int>("输入刷关次数");

            _automation.Actions.Enqueue(new RepeatLevelAction(mode, repeatTime));
            _automation.DoAll();

            Console.Beep();
            XConsole.AnyKey("所有任务完成");
        }

        private static string Conf(string key)
        {
            return _config[key] ?? throw new Exception($"需要设置值 {key}");
        }
    }
}