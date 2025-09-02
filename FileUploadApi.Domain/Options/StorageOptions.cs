namespace FileUploadApi.Domain.Options
{
    public class StorageOptions
    {
        public string BucketNamePhotos { get; set; }
        public string BucketNameDocuments { get; set; }
        public string Region { get; set; }
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}