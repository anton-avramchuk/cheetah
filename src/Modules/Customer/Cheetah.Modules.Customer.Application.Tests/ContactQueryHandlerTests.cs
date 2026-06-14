using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.Application.Contacts;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Customer.Application.Tests;

public class ContactQueryHandlerTests
{
    private readonly Mock<IRepository<TestContact, Guid>> _repo = new();
    private readonly TestContactProjector _projector = new();
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public async Task GetById_Missing_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestContact?)null);
        var handler = new GetContactByIdQueryHandler<TestContact, TestContactDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetContactByIdQuery<TestContactDto>(Guid.NewGuid()));

        dto.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_Found_ProjectsDto()
    {
        var contact = TestContact.Create(CustomerId, "John", "CEO", "a@b.io");
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        var handler = new GetContactByIdQueryHandler<TestContact, TestContactDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetContactByIdQuery<TestContactDto>(contact.Id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(contact.Id);
        dto.CustomerId.ShouldBe(CustomerId);
        dto.FullName.ShouldBe("John");
        dto.Position.ShouldBe("CEO");
        dto.Email.ShouldBe("a@b.io");
    }

    [Fact]
    public async Task ListByCustomer_ProjectsAllItems()
    {
        var items = new List<TestContact>
        {
            TestContact.Create(CustomerId, "A"),
            TestContact.Create(CustomerId, "B")
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestContact>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var handler = new ListContactsByCustomerQueryHandler<TestContact, TestContactDto>(_repo.Object, _projector);

        var result = await handler.HandleAsync(new ListContactsByCustomerQuery<TestContactDto>(CustomerId));

        result.Count.ShouldBe(2);
        result.Select(d => d.FullName).ShouldBe(new[] { "A", "B" });
    }
}
