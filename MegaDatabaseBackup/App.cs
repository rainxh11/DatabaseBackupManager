using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace MegaDatabaseBackup
{
    public class App
    {
        public static async Task UploadFile(string email, string password, string backupName, string fileName, Progress<double> progress)
        {
            try
            {
                MegaApiClient client = new MegaApiClient();
                client.Login(email, password);

                var nodes = client.GetNodes();
                var rootNode = nodes.ToList().Find(x => x.Type == NodeType.Root);
                var backupNode = nodes.ToList().Find(x => x.Name == backupName);

                if (backupNode == null)
                {
                    backupNode = client.CreateFolder(backupName, rootNode);
                }

                await client.UploadFileAsync(fileName, backupNode, progress);

                client.Logout();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
        }

        public static void UploadFileSync(string email, string password, string backupName, string fileName)
        {
            try
            {
                MegaApiClient client = new MegaApiClient();
                client.Login(email, password);

                var nodes = client.GetNodes();
                var rootNode = nodes.ToList().Find(x => x.Type == NodeType.Root);
                var backupNode = nodes.ToList().Find(x => x.Name == backupName);
                if (backupNode == null)
                {
                    backupNode = client.CreateFolder(backupName, rootNode);
                }

                client.UploadFile(fileName, backupNode);

                client.Logout();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
        }

        public static void CreateBackup(string dbName, string destinationFile, string host, string workingDir)
        {
            Process mongodump = new Process()
            {
                StartInfo =
                {
                    FileName = workingDir + @"\mongodump.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = $"/host:{host} /db:{dbName} /archive:{destinationFile} /gzip"
                }
            };
            try
            {
                mongodump.Start();
                mongodump.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}