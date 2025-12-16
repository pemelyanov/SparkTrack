namespace SparkTrack.API.AutofacModules;

using Autofac;
using API;
using Services.Features;

public class APIModule(string apiBaseUrl) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkHttpClient>().As<HttpClient>();
        
        RegisterClient<FeaturesClient>(builder);

        builder.RegisterType<FeaturesService>().AsImplementedInterfaces();
    }

    private void RegisterClient<TClient>(ContainerBuilder builder) where TClient : class
    {
        builder.Register<ClientWrapper<TClient>>(
            c =>
            {
                var httpClient = c.Resolve<HttpClient>(new TypedParameter(typeof(string), apiBaseUrl));
                var client = (TClient)Activator.CreateInstance(typeof(TClient), httpClient)!;

                return new ClientWrapper<TClient>(client, httpClient);
            }
        );
    }
}