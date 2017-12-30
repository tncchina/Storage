namespace ApiApp.Models
{
    public class UploadBatchResponse : IModelResult
    {
        public AnimalImage[] Animals { get; set; }

        public string NextLink { get; set; }
    }
}