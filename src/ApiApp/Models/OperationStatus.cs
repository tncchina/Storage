using System;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class OperationStatus : IModelResult
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}