using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.FileStorage.S3;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFileStorageModule))]
public partial class CrmFileStorageS3Module : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<S3FileStorageOptions>(services.GetConfiguration().GetSection("FileStorage:S3"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<S3FileStorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ForcePathStyle = opts.UsePathStyle
            };
            if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
                config.ServiceURL = opts.ServiceUrl;
            else
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(opts.Region);

            // Credentials: либо явные, либо из стандартной AWS-цепочки (env, profile, IAM role).
            if (!string.IsNullOrWhiteSpace(opts.AccessKey) && !string.IsNullOrWhiteSpace(opts.SecretKey))
            {
                var creds = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
                return new AmazonS3Client(creds, config);
            }
            return new AmazonS3Client(config);
        });

        RegisterServices(services);
    }
}
