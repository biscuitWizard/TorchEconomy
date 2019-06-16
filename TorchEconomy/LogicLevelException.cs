using System;

namespace TorchEconomy
{
    /// <summary>
    /// Ultimately harmless level exception. Usually means a graceful fail that
    /// can be reported to a user.
    /// </summary>
    public class LogicLevelException : Exception
    {
        public LogicLevelException(string friendlyMessage) : base(friendlyMessage)
        {
        }
    }
}