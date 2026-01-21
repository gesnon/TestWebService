using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanApiProject
{
    public class LoodsmanApiService
    {

        public string GetPath(string name)
        {

            //Connection
            Random random = new Random();

            int num = random.Next(1, 3);

            if (num == 1)
            {
                return $@"C:\Users\user\Desktop\Pictures\{name}.jpg";
            }
            if (num == 2)
            {
                return "404";
            }

            return null;


        }

        public byte[] GetPicture(string path)
        {           

            byte[] bData = GetBytes(path);

            return bData;
        }

        public static byte[] GetBytes(string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                byte[] bData = System.IO.File.ReadAllBytes(Path);

                return bData;
            }
            return null;
        }
    }
}
