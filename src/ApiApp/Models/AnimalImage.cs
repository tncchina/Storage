using System;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class AnimalImage
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Tag { get; set; }

        public string UploadBatchId { get; set; }

        public string ImageName { get; set; }

        public string ImageBlob { get; set; }

        public string UploadBlobSASUrl { get; set; }

        public string DownloadBlobSASUrl { get; set; }

        // The following is from the original database
        // 文件编号,原始文件编号,文件格式,文件夹编号,相机编号,布设点位编号,相机安装日期,拍摄时间,工作天数,对象类别,物种名称,动物数量,性别,独立探测首张,备注
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