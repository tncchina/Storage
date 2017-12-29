using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class AnimalImage
    {
        private static readonly ReadOnlyDictionary<string, FieldNames> Fields = new ReadOnlyDictionary<string, FieldNames>(
            new Dictionary<string, FieldNames>()
            {
                { "文件编号", FieldNames.OriginalFileId },
                { "原始文件编号", FieldNames.OriginalImageId },
                { "文件格式", FieldNames.FileFormat },
                { "文件夹编号", FieldNames.OriginalFolderId },
                { "相机编号", FieldNames.CameraId },
                { "布设点位编号", FieldNames.LocationId },
                { "相机安装日期", FieldNames.CameraInstallationDate },
                { "拍摄时间", FieldNames.ShootingTime },
                { "工作天数", FieldNames.WorkingDays },
                { "对象类别", FieldNames.Category },
                { "物种名称", FieldNames.SpecicesName },
                { "动物数量", FieldNames.AnimalQuantity },
                { "性别", FieldNames.Sex },
                { "独立探测首张", FieldNames.IndependentProbeFirst },
                { "备注", FieldNames.Notes }
        });

        private enum FieldNames
        {
            OriginalFileId,
            OriginalImageId,
            FileFormat,
            OriginalFolderId,
            CameraId,
            LocationId,
            CameraInstallationDate,
            ShootingTime,
            WorkingDays,
            Category,
            SpecicesName,
            AnimalQuantity,
            Sex,
            IndependentProbeFirst,
            Notes
        }

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

        public string ShootingTime { get; set; }

        public int WorkingDays { get; set; }

        public string Category { get; set; }

        public string SpecicesName { get; set; }

        public int? AnimalQuantity { get; set; }

        public string Sex { get; set; }

        public string IndependentProbeFirst { get; set; }

        public string Notes { get; set; }

        public static List<AnimalImage> ReadFromCsv(string csvMatadata)
        {
            List<AnimalImage> result = new List<AnimalImage>();

            using (StringReader reader = new StringReader(csvMatadata))
            using (TextFieldParser csvParser = new TextFieldParser(reader)
            {
                Delimiters = new[] { "," },
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true,
                TextFieldType = FieldType.Delimited
            })
            {
                string[] titles = csvParser.ReadFields();
                Dictionary<FieldNames, int> fieldMapping = titles.Select((t, i) => new KeyValuePair<FieldNames, int>(Fields[t], i)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                string[] fields;
                while (!csvParser.EndOfData)
                {
                    fields = csvParser.ReadFields();
                    result.Add(new AnimalImage
                    {
                        OriginalFileId = SafeGetElement(fields, fieldMapping[FieldNames.OriginalFileId]),
                        OriginalImageId = SafeGetElement(fields, fieldMapping[FieldNames.OriginalImageId]),
                        FileFormat = SafeGetElement(fields, fieldMapping[FieldNames.FileFormat]),
                        OriginalFolderId = SafeGetElement(fields, fieldMapping[FieldNames.OriginalFolderId]),
                        CameraId = SafeGetElement(fields, fieldMapping[FieldNames.CameraId]),
                        LocationId = SafeGetElement(fields, fieldMapping[FieldNames.LocationId]),
                        CameraInstallationDate = SafeGetDateTimeElement(fields, fieldMapping[FieldNames.CameraInstallationDate]),
                        ShootingTime = SafeGetElement(fields, fieldMapping[FieldNames.ShootingTime]),
                        WorkingDays = int.Parse(SafeGetElement(fields, fieldMapping[FieldNames.WorkingDays])),
                        Category = SafeGetElement(fields, fieldMapping[FieldNames.Category]),
                        SpecicesName = SafeGetElement(fields, fieldMapping[FieldNames.SpecicesName]),
                        AnimalQuantity = SafeGetIntElement(fields, fieldMapping[FieldNames.AnimalQuantity]),
                        Sex = SafeGetElement(fields, fieldMapping[FieldNames.Sex]),
                        IndependentProbeFirst = SafeGetElement(fields, fieldMapping[FieldNames.IndependentProbeFirst]),
                        Notes = SafeGetElement(fields, fieldMapping[FieldNames.Notes])
                    });
                }
            }

            return result;
        }

        private static string SafeGetElement(string[] data, int index)
        {
            return index < data.Length ? (string.IsNullOrWhiteSpace(data[index]) ? null : data[index].Trim()) : null;
        }

        private static DateTime? SafeGetDateTimeElement(string[] data, int index)
        {
            string element = SafeGetElement(data, index);
            return element != null ? (DateTime?)DateTime.Parse(element) : null;
        }

        private static int? SafeGetIntElement(string[] data, int index)
        {
            string element = SafeGetElement(data, index);
            return element != null ? (int?)int.Parse(element) : null;
        }
    }
}