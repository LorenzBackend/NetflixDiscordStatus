using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetflixDiscordStatus.Misc
{
    public class MyEventHandler
    {
        public delegate void InitResultEvent(string message, bool success);
        public static event InitResultEvent onInitResult;

        public delegate void UnexpectedError(string message);
        public static event UnexpectedError onUnexpectedError;

        public static void SendInitResult(string message, bool success)
        {
            if (onInitResult != null) onInitResult(message, success);
        }

        public static void SendUnexpectedError(string message)
        {
            if (onUnexpectedError != null) onUnexpectedError(message);
        }
    }
}
