namespace SparkTrack.API.AutofacModules;

using Autofac;
using API;
using Core.Client.Services.Configuration;
using Data;
using Interceptors;
using Services.Authorization;
using Services.Features;

public class APIModule(string apiBaseUrl, string tokensConfigPath) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkHttpClient>().As<HttpClient>();
        
        RegisterClient<FeaturesClient>(builder);
        RegisterClient<AuthorizationClient>(builder);

        builder.RegisterType<FeaturesService>().AsImplementedInterfaces();
        builder.RegisterType<RetryAuthHandler>();
        builder.RegisterType<JsonConfigurationService<TokensConfiguration>>()
            .AsImplementedInterfaces()
            .WithParameter(new TypedParameter(typeof(string), tokensConfigPath));
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces();
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