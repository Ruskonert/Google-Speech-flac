using System;
using Google.Cloud.Storage.V1;
using System.Collections.Generic;

namespace Workspace
{
    class CloudPlatformConnector
    {

        //  The value that is your project id. It has following format: xxxxx-xxxxx-xxxxxx
        protected string projectName = "peppy-nation-221511";

        // The value that is your service account key to access your project.
        // The rule must be project owner. To generate the key following by:
        // https://console.cloud.google.com/apis/credentials/serviceaccountkey?project=id
        private string jsonKey = null;

        // The client for access your project storage.
        private StorageClient storageClient = null;



        private string bucketName = null;
        public string BucketName { get => bucketName; }

        private bool _KeyChecksum()
        {
            if (jsonKey == null)
            {
                jsonKey = this.GetGoogleCredentials();
                return jsonKey != null;
            }
            else return true;
        }

        protected string GetGoogleCredentials()
        {
            return Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        }

        public bool IsConnectedStorageClient(string bucket = "RESERVED")
        {
            this._KeyChecksum();
            if (storageClient == null)
                this.storageClient = StorageClient.Create();

            if (this.storageClient == null)
            {
                Console.WriteLine("Storage Client was not initialized or null.");
                return false;
            }

            try
            {
                if (bucket == "RESERVED") bucket = this.bucketName;
                this.storageClient.CreateBucket(this.projectName, bucket);
                Console.WriteLine($"Bucket {bucket} created.");
            }
            catch (Google.GoogleApiException e)
            when (e.Error.Code == 409)
            {
                // already existed.

            }
            this.bucketName = bucket;
            return true;
        }



        public CloudPlatformConnector(String targetProjectName) : this(targetProjectName, null)
        {

        }

        public CloudPlatformConnector(String targetProjectName, String accessJsonKeyPath) : this(targetProjectName, accessJsonKeyPath, null)
        {

        }

        public CloudPlatformConnector(String targetProjectName, String accessJsonKeyPath, String googleStorage)
        {
            this.projectName = targetProjectName;
            if (accessJsonKeyPath == null)
            {
                Console.WriteLine("The Google Credentials JSON Key was undefined, Checking your environment values.");
                this.jsonKey = this.GetGoogleCredentials();
                if (this.jsonKey == null)
                {
                    throw new NullReferenceException("GOOGLE_APPLICATION_CREDENTIALS was not found! Please configure your access key at first.");
                }
                else
                {
                    Console.WriteLine($"GOOGLE_CREDENTIALS_PATH={this.jsonKey}");
                }
            }
            else
            {
                if (accessJsonKeyPath.EndsWith(".json"))
                {
                    throw new FormatException("GOOGLE_APPLICATION_CREDENTIALS must be JSON");
                }

                if (this.GetGoogleCredentials() != null)
                    Console.WriteLine("Environment value was already defined, replacing value this time.");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", accessJsonKeyPath);
            }

            this.bucketName = googleStorage;
        }

        public List<string> GetSpecificFileExtension(string fileExtension)
        {
            if (fileExtension == null || fileExtension.Length == 0) return null;
            if (!fileExtension.StartsWith(".")) fileExtension = "." + fileExtension;
            if (this.storageClient == null || this.bucketName == null)
                throw new NullReferenceException("Not connected client.");

            _extension = fileExtension;
            return this.GetListObjects().FindAll(_ExtensionMatch);
        }

        private volatile static string _extension = null;

        private static bool _ExtensionMatch(string s)
        {
            if(s.EndsWith(_extension))
                return true;
            else
                return false;
        }

        public List<string> GetListObjects()
        {
            var list = new List<string>();
            var storage = this.storageClient;
            foreach (var bucket in this.storageClient.ListObjects(this.bucketName, ""))
            {
                list.Add(bucket.Name);
            }
            return list;
        }
    }
}
