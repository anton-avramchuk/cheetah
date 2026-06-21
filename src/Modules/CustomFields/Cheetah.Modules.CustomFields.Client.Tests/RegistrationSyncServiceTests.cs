using Cheetah.Modules.CustomFields.Client;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Client.Tests;

public class RegistrationSyncServiceTests
{
    [Fact]
    public async Task Start_CatalogUnavailable_DoesNotThrow()
    {
        var client = new Mock<ICustomFieldsClient>();
        client.Setup(c => c.SyncTypesAsync(It.IsAny<IReadOnlyList<CustomFieldEntityTypeDescriptor>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("catalog down"));

        var contribution = new CustomFieldsRegistrationContribution([
            new CustomFieldEntityTypeDescriptor("crm.deal", "Сделка", "deals", CustomFieldEntityIdType.Guid)
        ]);

        var svc = new CustomFieldsRegistrationSyncService(
            [contribution], client.Object, NullLogger<CustomFieldsRegistrationSyncService>.Instance);

        await Should.NotThrowAsync(() => svc.StartAsync(CancellationToken.None));
        client.Verify(c => c.SyncTypesAsync(It.IsAny<IReadOnlyList<CustomFieldEntityTypeDescriptor>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Start_NoDescriptors_SkipsSync()
    {
        var client = new Mock<ICustomFieldsClient>();
        var svc = new CustomFieldsRegistrationSyncService(
            [], client.Object, NullLogger<CustomFieldsRegistrationSyncService>.Instance);

        await svc.StartAsync(CancellationToken.None);

        client.Verify(c => c.SyncTypesAsync(It.IsAny<IReadOnlyList<CustomFieldEntityTypeDescriptor>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
