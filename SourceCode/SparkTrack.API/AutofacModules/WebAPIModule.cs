namespace SparkTrack.API.AutofacModules;

using Autofac;
using API;

public class APIModule(string apiBaseUrl) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkHttpClient>().As<HttpClient>();
        
        RegisterClient<FeaturesClient>(builder);
    }

    private void RegisterClient<TClient>(ContainerBuilder builder) where TClient : class
    {
        builder.Register<ClientWrapper<TClient>>(
            c =>
            {
                var httpClient = c.Resolve<HttpClient>();
                var client = (TClient)Activator.CreateInstance(typeof(TClient), apiBaseUrl, httpClient)!;

                return new ClientWrapper<TClient>(client, httpClient);
            }
        );
    }
}