using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApp.Projections
{
    public static class ProjectionSetup
    {
        public async static Task Run(string rootPath)
        {
            var path = rootPath + Path.DirectorySeparatorChar + "/Projections/";
            var handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential("admin", "changeit");
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://localhost:2113");
                
                await EnableProjection(client, "$by_category");
                await EnableProjection(client, "$by_event_type");
                await EnableProjection(client, "$stream_by_category");
                await EnableProjection(client, "$streams");

                await CreateCustomProjection(client, path);
            }
        }

        private static async Task EnableProjection(HttpClient client, string name)
        {
            await client.PostAsync($"/projection/{name}/command/enable?enableRunAs=true", null);
        }

        private static async Task CreateCustomProjection(HttpClient client, string path)
        {
            var name = "Sample";
            StringContent content;
            using(var reader = File.OpenText($"{path}{Path.DirectorySeparatorChar}{name}.js"))
            {
                content = new StringContent(reader.ReadToEnd());
            }
            await client.PostAsync($"/projections/continuous?name={name}&type=js&enabled=true&emit=true&trackemittedstreams=true", content);
        }
    }
}