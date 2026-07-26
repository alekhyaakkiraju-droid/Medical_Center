using Microsoft.Extensions.Configuration;

namespace AngularApi.Infrastructure;

public sealed class DockerSecretConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new DockerSecretConfigurationProvider();
}
