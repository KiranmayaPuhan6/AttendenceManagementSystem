using System.Runtime.CompilerServices;

namespace AMS.Services.Utility.HelperMethods
{
    public static class MethodNameExtensionHelper
    {
        public static string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
