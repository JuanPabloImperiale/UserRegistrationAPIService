using System.Globalization;

namespace dotnet_registration_api.Helpers
{
    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }        
    }
}
