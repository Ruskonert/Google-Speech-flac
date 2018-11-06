using Google.Cloud.Speech.V1;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.IO;

namespace Workspace
{
    class SpeechConnector
    {
        private CloudPlatformConnector baseConnector = null;
        private SpeechClient client = null;

        private RecognitionConfig recognitionConfig = null;


        public SpeechConnector(CloudPlatformConnector connector)
        {
            this.baseConnector = connector;
        }

        public SpeechConnector Connect()
        {
            this.client = SpeechClient.Create();
            
            // flac 파일
            this.recognitionConfig = new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                LanguageCode = "ko-KR",
            };

            /* wav 파일
            this.recognitionConfig = new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 24000,
                LanguageCode = "ko-KR",
            };
            */

            return this;
        }

        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public object AnalyzeSoundToText(List<RecognitionAudio> target)
        {
            foreach (var sound in target)
            {
                try
                {
                    var response = client.LongRunningRecognize(this.recognitionConfig, sound);
                    var responseResult = response.PollUntilCompleted();

                    var i = 0;
                    var soundFilename = JObject.Parse(sound.ToString())["uri"].ToString().Replace("gs:", "").Replace("/", "_");

                    foreach (var result in responseResult.Result.Results)
                    {
                        var timestamp = SpeechConnector.UnixTimeNow();
                        using (StreamWriter rdr = new StreamWriter($"result_{soundFilename}_{timestamp}_{i}.txt"))
                        {
                            foreach (var alternative in result.Alternatives)
                                rdr.WriteLine(alternative.Transcript);
                            rdr.Close();
                            Console.WriteLine($"The result saved the file -> result_{soundFilename}_{timestamp}_{i}_.txt");
                        }
                        i++;
                    }
                }
                catch(Grpc.Core.RpcException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Something wrong :( Skipping this file");
                }
            }
            return true;
        }

        public List<RecognitionAudio> ConvertRecognitionAudioFiles(List<string> pathlist)
        {
            List<RecognitionAudio> audioList = new List<RecognitionAudio>();
            foreach(var path in pathlist)
            {
                audioList.Add(RecognitionAudio.FromStorageUri($"gs://{this.baseConnector.BucketName}/{path}"));
            }
            return audioList;
        }
    }
}
