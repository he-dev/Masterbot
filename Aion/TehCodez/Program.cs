﻿using System;
using Aion.Data.Configuration;
using Aion.Jobs;
using Aion.Services;
using Reusable.ConfigWhiz;
using Reusable.ConfigWhiz.Datastores.AppConfig;
using Reusable.Logging;

namespace Aion
{
    internal class Program
    {
        public const string InstanceName = "Aion";
        public const string InstanceVersion = "4.0.0";

        private static readonly ILogger Logger;

        public static Configuration Configuration { get; }

        static Program()
        {
            InitializeLogging();
            Logger = LoggerFactory.CreateLogger(nameof(Program));
            Configuration = InitializeConfiguraiton();
        }

        private static void Main()
        {
            LogEntry.New().Info().Message($"*** {InstanceName} v{InstanceVersion} started ***").Log(Logger);

            try
            {
                var cronService = new CronService();
                RobotJob.Scheduler = cronService.Scheduler;

                cronService.Scheduler.ScheduleJob<RobotScheduleUpdater>(
                    name: nameof(RobotScheduleUpdater),
                    cronExpression: Configuration.Load<Program, Global>().RobotConfigUpdaterSchedule,
                    startImmediately: false
                );

                ServiceStarter.Start(cronService);
            }
            catch (Exception ex)
            {
                LogEntry.New().Error().Exception(ex).Message("Error starting service.").Log(Logger);
            }

            if (Environment.UserInteractive)
            {
                Console.ReadKey();
            }
        }

        private static void InitializeLogging()
        {
            Reusable.Logging.NLog.Tools.LayoutRenderers.InvariantPropertiesLayoutRenderer.Register();

            Reusable.Logging.Logger.ComputedProperties.Add(new Reusable.Logging.ComputedProperties.AppSetting(name: "Environment", key: $"Aion.Program.Global.Environment"));
            Reusable.Logging.Logger.ComputedProperties.Add(new Reusable.Logging.ComputedProperties.ElapsedSeconds());
            Reusable.Logging.Logger.ComputedProperties.Add(new Reusable.Logging.ComputedProperties.ElapsedHours());

            Reusable.Logging.LoggerFactory.Initialize<Reusable.Logging.Adapters.NLogFactory>();
        }

        private static Configuration InitializeConfiguraiton()
        {
            var configuration = new Reusable.ConfigWhiz.Configuration(new AppSettings());
            configuration.Load<Program, Aion.Data.Configuration.Global>();
            return configuration;
        }
    }

    internal abstract class RobotJob
    {
        // This is a workaround for the job-data-map because it really sucks.
        public static CronScheduler Scheduler { get; set; }
    }
}
