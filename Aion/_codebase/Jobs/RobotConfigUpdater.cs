﻿using System;
using System.Linq;
using Aion.Data.Configuration;
using Aion.Services;
using Quartz;
using Reusable.Logging;

namespace Aion.Jobs
{
    [DisallowConcurrentExecution]
    internal class RobotConfigUpdater : RobotJob, IJob
    {
        private static readonly ILogger Logger;

        static RobotConfigUpdater()
        {
            Logger = LoggerFactory.CreateLogger(nameof(RobotConfigUpdater));
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var schemes = SchemeReader.ReadSchemes(Program.Configuration.Load<Program, Global>().RobotsDirectoryName).ToArray();
                Scheduler.ScheduleRobots(schemes);
            }
            catch (Exception ex)
            {
                LogEntry.New().Error().Exception(ex).Message("Error scheduling robots.").Log(Logger);
            }
        }
    }
}
