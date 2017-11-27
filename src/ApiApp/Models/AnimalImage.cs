using System;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class AnimalImage
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string UploadBatchId { get; set; }

        public string ImageUrl { get; set; }

        // The following is from the original database
        public string OriginalFileId { get; set; }

        public string OriginalImageId { get; set; }

        public string FileFormat { get; set; }

        public string OriginalFolderId { get; set; }

        public string CameraId { get; set; }

        public string LocationId { get; set; }

        public DateTime? CameraInstallationDate { get; set; }

        public DateTime ShootingTime { get; set; }

        public int WorkingDays { get; set; }

        public string Category { get; set; }

        public string SpecicesName { get; set; }

        public int? AnimalQuantity { get; set; }

        public string Sex { get; set; }

        public string IndependentProbeFirst { get; set; }

        public string Notes { get; set; }
    }
}