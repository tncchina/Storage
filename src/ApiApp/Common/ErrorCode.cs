using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ApiApp.Common
{
    public class ErrorCode
    {
        public const string LongOperationFailed = "LongOperationFailed";
        public const string ProperyCannotBeEmpty = "ProperyCannotBeEmpty";
        public const string BadCSVMetadata = "BadCSVMetadata";
    }
}