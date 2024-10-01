using Aspire.CommunityToolkit.OllamaSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace Microsoft.Extensions.Hosting;

public static class AspireOllamaSharpExtensions
{
    private const string DefaultConfigSectionName = "Aspire:OllamaSharp";

    /// <summary>
    /// Adds <see cref="OllamaApiClient"/> services to the container.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to read config from and add services to.</param>
    /// <param name="connectionName">A name used to retrieve the connection string from the ConnectionStrings configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It's invoked after the settings are read from the configuration.</param>
    /// <exception cref="UriFormatException">Thrown when no Ollama endpoint is provided.</exception>
    public static void AddOllamaApiClient(this IHostApplicationBuilder builder, string connectionName, Action<OllamaSharpSettings>? configureSettings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName, nameof(connectionName));
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        OllamaSharpSettings settings = new();
        builder.Configuration.GetSection($"{DefaultConfigSectionName}:{connectionName}").Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ConnectionString = connectionString;
        }

        configureSettings?.Invoke(settings);

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new UriFormatException("No endpoint for Ollama defined.");
        }

        OllamaApiClient client = new(new HttpClient { BaseAddress = new Uri(settings.ConnectionString) });

        if (!string.IsNullOrWhiteSpace(settings.SelectedModel))
        {
            client.SelectedModel = settings.SelectedModel;
        }

        builder.Services.AddSingleton<IOllamaApiClient>(client);
    }
}