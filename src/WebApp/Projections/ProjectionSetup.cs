using System;
using System.Linq;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApp.Projections
{
    public static class ProjectionSetup
    {
        public async static Task Run(string connString, string rootPath)
        {
            var param = ParseConnectionString(connString);

            var path = rootPath + Path.DirectorySeparatorChar + "/Projections/";
            var handler = new HttpClientHandler();
            handler.Credentials = param.Credentials;
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri($"http://{param.Host}:2113");
                
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
            var name = "ShoppingCartCreatedToCheckedOutProjection";
            StringContent content;
            using(var reader = File.OpenText($"{path}{Path.DirectorySeparatorChar}{name}.js"))
            {
                content = new StringContent(reader.ReadToEnd());
            }
            await client.PostAsync($"/projections/continuous?name={name}&type=js&enabled=true&emit=true&trackemittedstreams=true", content);
        }

        private static dynamic ParseConnectionString(string connString)
        {
            var builder = new DbConnectionStringBuilder() { ConnectionString = connString };
            object connectTo;
            if (builder.TryGetValue("ConnectTo", out connectTo))
            {
                dynamic obj = new ExpandoObject();
                var uri = new Uri(connectTo.ToString());
                var parts = uri.UserInfo.Split(':');
                obj.Credentials = new NetworkCredential(parts[0], parts[1]);
                obj.Host = uri.Host;
                obj.Port = uri.Port;

                return obj;
            }

            throw new ArgumentException("ConnString {connString} not valid", connString);
        }
    }
}