
using System.Text;

namespace MyApp
{
    public static class Util {
        public static void SaveStringToFile(string input, string path){
            using (FileStream fs = File.Create(path)) 
            {
                    // writing data in string
                    byte[] info = new UTF8Encoding(true).GetBytes(input);
                    fs.Write(info, 0, info.Length);

                    // writing data in bytes already
                    byte[] data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
            }

        }

    }
    
}