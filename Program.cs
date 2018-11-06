using System;

namespace Workspace
{
    public class Program
    {
        private static Interpreter pythonInterpreter = null;

        public static bool VerifyPythonInterpreter()
        {
            if (pythonInterpreter == null)
            {
                string pythonInterpreterPath = Environment.GetEnvironmentVariable("PYTHON");
                if (pythonInterpreterPath == null || pythonInterpreterPath.Length == 0)
                {
                    Console.WriteLine("No selected python interpreter. Please Set the environment python path with $PYTHON");
                    Console.WriteLine("Please click any key to exit ...");
                    Console.ReadLine();
                    return false;
                }
                pythonInterpreter = new Interpreter(pythonInterpreterPath);
                Console.WriteLine("Using python interpreter: {0}", pythonInterpreterPath);
                return true;
            }
            return true;
        }

        public static void Main(string[] args)
        {
            //var projectId = "peppy-nation-221511"; // 바꿔야 할 부분
            //var bucket = "voice-device"; // 바꿔야 할 부분

            Console.Write("Input your project id: ");
            var projectId = Console.ReadLine();

            Console.Write("Input your bucket: ");
            var bucket = Console.ReadLine();


            var connector = new CloudPlatformConnector(projectId);
            var speechConnector = new SpeechConnector(connector).Connect();

            bool isStorageConnected = connector.IsConnectedStorageClient(bucket);
            if (!isStorageConnected) throw new TypeAccessException($"Can't connected {projectId}@{bucket}");

            var waveExtensionFiles = connector.GetSpecificFileExtension("flac"); // wav, flac

            var index = 0;
            Console.WriteLine("\n===== FILE LIST =====\n");
            foreach(var filename in waveExtensionFiles)
            {
                Console.WriteLine($"[{index}] filename: {filename}");
                index++;
            }
            Console.WriteLine("\n====================");

            var recognitionAudioFiles = speechConnector.ConvertRecognitionAudioFiles(waveExtensionFiles);

            Console.WriteLine("Analyzing file on Google Cloud platform ...");
            speechConnector.AnalyzeSoundToText(recognitionAudioFiles);

            Console.WriteLine("Finished. Press any key to exit the program ...");
            Console.ReadLine();
        }
    }
}