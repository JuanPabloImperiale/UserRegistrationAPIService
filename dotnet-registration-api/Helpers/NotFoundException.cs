using System.Globalization;

namespace dotnet_registration_api.Helpers
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }

        public NotFoundException(string message) : base(message) { }        
    }
}
