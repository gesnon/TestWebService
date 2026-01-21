using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanApiProject
{
    public class Program
    {
        private const string PipeName = "LoodsmanPipe";
        public LoodsmanApiService loodsmanApiService = new LoodsmanApiService();
        public static int Main(string[] args)
        {
            Console.WriteLine("LoodsmanConsole start up");
            return Serve();
        }
        private static int Serve()
        {
            while (true)
            {
                using (var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.None,
                    4096,
                    4096,
                    CreatePipeSecurity()))
                {
                    Console.WriteLine("Waiting for connection");
                    server.WaitForConnection();
                    HandleClient(server);
                }
            }
        }

        private static byte[] RenderPng(string text)
        {
            LoodsmanApiService service = new LoodsmanApiService();

            string path = service.GetPath(text);

            if (path == "404")
            {
                return null;
            }

            byte[] result = service.GetPicture(path);

            return result;
        }
        private static void HandleClient(NamedPipeServerStream server)
        {
            using (var reader = new StreamReader(server, Encoding.UTF8))
            using (var writer = new StreamWriter(server, Encoding.UTF8) { AutoFlush = true })
            {
                var line = reader.ReadLine();
                Console.WriteLine($"Recieved {line}");
                if (string.IsNullOrWhiteSpace(line))
                {
                    writer.WriteLine(SerializeJson(new ImageResponse { ok = false, error = "Empty request." }));
                    return;
                }

                ImageRequest request;
                try
                {
                    request = DeserializeJson<ImageRequest>(line);
                }
                catch (Exception ex)
                {
                    writer.WriteLine(SerializeJson(new ImageResponse { ok = false, error = "Invalid JSON: " + ex.Message }));
                    return;
                }

                if (request == null || string.IsNullOrWhiteSpace(request.text))
                {
                    writer.WriteLine(SerializeJson(new ImageResponse { ok = false, error = "Missing text." }));
                    return;
                }

                try
                {
                    var bytes = RenderPng(request.text);
                    if (bytes == null)
                    {
                        writer.WriteLine(SerializeJson(new ImageResponse { ok = false, error="404" }));
                        return;
                    }
                    var base64 = Convert.ToBase64String(bytes);
                    writer.WriteLine(SerializeJson(new ImageResponse { ok = true, imageBase64 = base64 }));
                }
                catch (Exception ex)
                {
                    writer.WriteLine(SerializeJson(new ImageResponse { ok = false, error = ex.Message }));
                }
            }
        }
        private static string SerializeJson<T>(T value)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static T DeserializeJson<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        private static PipeSecurity CreatePipeSecurity()
        {
            var security = new PipeSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));
            return security;
        }

        [DataContract]
        private sealed class ImageRequest
        {
            [DataMember(Name = "text")]
            public string text;
        }

        [DataContract]
        private sealed class ImageResponse
        {
            [DataMember(Name = "ok")]
            public bool ok;

            [DataMember(Name = "imageBase64")]
            public string imageBase64;

            [DataMember(Name = "error")]
            public string error;
        }
    }
}
