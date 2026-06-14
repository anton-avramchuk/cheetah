using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.Application.Customers;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Customer.Application.Tests;

public class CustomerQueryHandlerTests
{
    private readonly Mock<IRepository<TestCustomer, Guid>> _repo = new();
    private readonly TestCustomerProjector _projector = new();

    [Fact]
    public async Task GetById_Missing_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestCustomer?)null);
        var handler = new GetCustomerByIdQueryHandler<TestCustomer, TestCustomerDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetCustomerByIdQuery<TestCustomerDto>(Guid.NewGuid()));

        dto.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_Found_ProjectsDto()
    {
        var customer = TestCustomer.Create("Acme", "a@b.io");
        _repo.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new GetCustomerByIdQueryHandler<TestCustomer, TestCustomerDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetCustomerByIdQuery<TestCustomerDto>(customer.Id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(customer.Id);
        dto.DisplayName.ShouldBe("Acme");
        dto.Email.ShouldBe("a@b.io");
    }

    [Fact]
    public async Task List_ProjectsAllItems()
    {
        var items = new List<TestCustomer> { TestCustomer.Create("A"), TestCustomer.Create("B") };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestCustomer>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var handler = new ListCustomersQueryHandler<TestCustomer, TestCustomerDto>(_repo.Object, _projector);

        var result = await handler.HandleAsync(new ListCustomersQuery<TestCustomerDto>());

        result.Count.ShouldBe(2);
        result.Select(d => d.DisplayName).ShouldBe(new[] { "A", "B" });
    }
}
