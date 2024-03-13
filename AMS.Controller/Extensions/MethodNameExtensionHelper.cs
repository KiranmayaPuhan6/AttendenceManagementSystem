using System.Runtime.CompilerServices;

namespace AMS.Controller.Extensions
{
    public static class MethodNameExtensionHelper
    {
        public static string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
