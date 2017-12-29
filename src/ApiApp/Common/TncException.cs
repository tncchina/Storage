using System;
using System.Net;

namespace ApiApp.Common
{
    internal class TncException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }
    }
}