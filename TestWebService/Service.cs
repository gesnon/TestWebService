namespace TestWebService
{
    public class Service
    {
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
