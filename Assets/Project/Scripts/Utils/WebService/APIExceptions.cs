using System;

namespace UnityArchitectureShowcase.Network
{
    public class GetResponseException : Exception
    {
        public GetResponseException() : base() { }
        public GetResponseException(string message) : base(message) { }
        public GetResponseException(string message, Exception innerException) : base(message, innerException) { }
    }
}