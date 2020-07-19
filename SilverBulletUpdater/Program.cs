using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;

namespace SilverBulletUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.Title = "SilverBullet Updater";
            Console.SetWindowSize(70, 20);
            Console.WriteLine("");
            Thread.Sleep(1000);
            stopwatch.Start();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                string address = "https://c-cracking.org/ApplicationVeri/SilverBullet/SilverBullet.zip";
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                WebClient webClient = new WebClient();
                if (!File.Exists("Update.zip"))
                {
                    Console.WriteLine("Downloading Update...");
                    webClient.DownloadFile(address, baseDirectory + "Update.zip");
                    Console.WriteLine("");
                }
                else
                {
                    File.Delete("Update.zip");
                    Console.WriteLine("Previous Update Files Deleted.");
                    Console.WriteLine("");
                    webClient.DownloadFile(address, baseDirectory + "Update.zip");
                    Console.WriteLine("Downloading Update...");
                    Console.WriteLine("");
                }
                if (File.Exists("Update.zip"))
                {
                    ZipFile zipFile = ZipFile.Read("Update.zip");
                    Console.WriteLine("UnZipping Archive...");
                    zipFile.ExtractAll(baseDirectory, ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();
                    Console.WriteLine("");
                    stopwatch.Stop();
                    string str = stopwatch.Elapsed.TotalSeconds.ToString();
                    Console.WriteLine("SUCCESS, Update Completed Succesfully in: " + str + " Seconds");
                    File.Delete("Update.zip");
                }
                else
                {
                    Console.WriteLine("Update Files not found or Corrupted! Does Update.zip exist?");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine($"Error Extracting Archive\n{ex.Message}");
            }
            Console.WriteLine("");
            Console.WriteLine("Press Any key to Close.");
            Console.ReadKey();
        }
    }
}
