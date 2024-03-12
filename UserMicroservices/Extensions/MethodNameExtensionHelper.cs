using System.Runtime.CompilerServices;

namespace UserMicroservices.Extensions
{
    public static class MethodNameExtensionHelper
    {
        public static string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
