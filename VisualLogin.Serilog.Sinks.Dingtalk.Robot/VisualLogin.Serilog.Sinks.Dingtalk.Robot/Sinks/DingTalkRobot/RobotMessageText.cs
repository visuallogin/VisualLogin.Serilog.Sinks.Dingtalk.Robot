namespace Serilog.Sinks.DingTalkRobot
{
    public class RobotMessageText
    {
        public RobotMessageExtensionsText text { get; set; } = new RobotMessageExtensionsText();
        public string msgtype { get; set; } = "text";
    }
    public class RobotMessageExtensionsText
    {
        public string content { get; set; }
    }
}