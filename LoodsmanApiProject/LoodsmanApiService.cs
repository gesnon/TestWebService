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
                return "Занято";
            }

            return null;


        }
    }
}
