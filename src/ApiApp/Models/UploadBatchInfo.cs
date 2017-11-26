using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiApp.Models
{
    public class UploadBatchRequest
    {
        public string BatchTag { get; set; }

        public string BatchDescription { get; set; }
    }

    public class UploadBatchResponse
    {
        public string ContainerUrl { get; set; }

        public string BatchTag { get; set; }
    }
}