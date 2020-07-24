using System;
using System.Collections.Generic;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting;
using Serilog.Sinks.DingTalkRobot;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.Email() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationRobotExtensions
    {
        const string DefaultOutputTemplate = "[{Level}] {Timestamp:yyyy-MM-dd HH:mm:ss} {SourceContext} {Message}{NewLine}{Exception}";
        const int DefaultBatchPostingLimit = 100;
        static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Adds a sink that sends log events via DingTalk Robot.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="token">DingTalk Robot Token</param>
        /// <param name="secret">DingTalk Robot Secret</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// the default is "{Timestamp} [{Level}] {Message}{NewLine}{Exception}".</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <returns>
        /// Logger configuration, allowing configuration to continue.
        /// </returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        /// <exception cref="System.ArgumentNullException">loggerConfiguration
        /// or
        /// fromEmail
        /// or
        /// toEmail</exception>
        public static LoggerConfiguration Robot(
            this LoggerSinkConfiguration loggerConfiguration,
            string token,
            string secret,
            string outputTemplate = DefaultOutputTemplate,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null
            )
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");
            if (token == null) throw new ArgumentNullException("token");
            if (secret == null) throw new ArgumentNullException("secret");

            var batchingPeriod = period ?? DefaultPeriod;
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var connectionInfo = new RobotConnectionInfo {Secret = secret, Token = token};
            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchPostingLimit,
                Period = batchingPeriod,
                EagerlyEmitFirstEvent = false,  // set default to false, not usable for emailing
                QueueLimit = 10000
            };
            var batchingSink = new PeriodicBatchingSink(new RobotSink(connectionInfo, textFormatter), batchingOptions);

            return loggerConfiguration.Sink(batchingSink, restrictedToMinimumLevel);

        }
    }
}
