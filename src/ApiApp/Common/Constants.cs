using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ApiApp.Common
{
    public class Constants
    {
        public const string Success = "Success";

        public const int RetryIntervalInSeconds = 15;

        public const string CustomResponseHeaders = "CustomResponseHeaders";

        public const string LocationHeader = "Location";

        public const string RetryAfterHeader = "Retry-After";

        public const string SkipToken = "$skiptoken";
    }
}