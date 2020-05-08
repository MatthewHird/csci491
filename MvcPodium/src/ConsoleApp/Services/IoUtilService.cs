using System.IO;
using System.Text;

namespace MvcPodium.ConsoleApp.Services
{
    public class IoUtilService : IIoUtilService
    {
        public void WriteStringToFile(
            string outString,
            string outFilePath)
        {
            using (var outStream = File.Create(outFilePath))
            {
                outStream.Write(Encoding.UTF8.GetBytes(outString));
                outStream.Flush();
            }
        }
    }
}
