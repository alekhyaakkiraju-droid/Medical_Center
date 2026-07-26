using Microsoft.Extensions.Configuration;

namespace AngularApi.Infrastructure;

public static class DockerSecretConfigurationExtensions
{
    public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder builder)
    {
        builder.Add(new DockerSecretConfigurationSource());
        return builder;
    }
}
