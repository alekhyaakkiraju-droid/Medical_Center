using Serilog;
using Serilog.Formatting.Compact;

namespace AngularApi.Logging
{
    public static class SerilogConfiguration
    {
        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "MedicalCenter")
                    .Destructure.ByTransformingWhere<string>(
                        type => type == typeof(string),
                        value =>
                        {
                            if (value.Contains('@'))
                            {
                                return PiiMasking.MaskEmail(value);
                            }

                            if (value.Contains(' '))
                            {
                                return PiiMasking.MaskName(value);
                            }

                            return value;
                        })
                    .WriteTo.Console(new RenderedCompactJsonFormatter());
            });
        }
    }
}
