using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ApiApp.Common
{
    public class OperationStage
    {
        public const string New = "New";
        public const string InProgress = "InProgress";
        public const string Success = "Success";
        public const string Fail = "Fail";
    }
}