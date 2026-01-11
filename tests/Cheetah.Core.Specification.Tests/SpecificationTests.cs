using System.Linq.Expressions;
using FluentAssertions;

namespace Cheetah.Core.Specification.Tests;

public class SpecificationTests
{
    private readonly IQueryable<Customer> _customers;

    public SpecificationTests()
    {
        _customers = new List<Customer>
            {
                new Customer("John", 17, 47000, "England"),
                new Customer("Tuana", 2, 500, "Turkey"),
                new Customer("Martin", 43, 16000, "USA"),
                new Customer("Lee", 32, 24502, "China"),
                new Customer("Douglas", 42, 42000, "England"),
                new Customer("Abelard", 14, 2332, "German"),
                new Customer("Neo", 16, 120000, "USA"),
                new Customer("Daan", 39, 6000, "Netherlands"),
                new Customer("Alessandro", 22, 8271, "Italy"),
                new Customer("Noah", 33, 82192, "Belgium")
            }.AsQueryable();
    }

    [Fact]
    public void Any_Should_Return_All()
    {
        _customers
            .Where(new AnySpecification<Customer>()) //Implicitly converted to Expression!
            .Count()
            .Should().Be(_customers.Count());
    }

    [Fact]
    public void None_Should_Return_Empty()
    {
        _customers
            .Where(new NoneSpecification<Customer>().ToExpression())
            .Count()
            .Should().Be(0);
    }

    [Fact]
    public void Not_Should_Return_Reverse_Result()
    {
        _customers
            .Where(new EuropeanCustomerSpecification().Not().ToExpression())
            .Count()
            .Should().Be(3);
    }

    [Fact]
    public void Should_Support_Native_Expressions_And_Combinations()
    {
        _customers
            .Where(new ExpressionSpecification<Customer>(c => c.Age >= 18).ToExpression())
            .Count()
            .Should().Be(6);

        _customers
            .Where(new EuropeanCustomerSpecification().And(new ExpressionSpecification<Customer>(c => c.Age >= 18)).ToExpression())
            .Count()
            .Should().Be(4);
    }

    [Fact]
    public void CustomSpecification_Test()
    {
        _customers
            .Where(new EuropeanCustomerSpecification().ToExpression())
            .Count()
            .Should().Be(7);

        _customers
            .Where(new Age18PlusCustomerSpecification().ToExpression())
            .Count()
            .Should().Be(6);

        _customers
            .Where(new BalanceCustomerSpecification(10000, 30000).ToExpression())
            .Count()
            .Should().Be(2);

        _customers
            .Where(new PremiumCustomerSpecification().ToExpression())
            .Count()
            .Should().Be(3);
    }

    [Fact]
    public void IsSatisfiedBy_Tests()
    {
        new PremiumCustomerSpecification().IsSatisfiedBy(new Customer("David", 49, 55000, "USA")).Should().BeTrue();

        new PremiumCustomerSpecification().IsSatisfiedBy(new Customer("David", 49, 200, "USA")).Should().BeFalse();
        new PremiumCustomerSpecification().IsSatisfiedBy(new Customer("David", 12, 55000, "USA")).Should().BeFalse();
    }

    [Fact]
    public void CustomSpecification_Composite_Tests()
    {
        _customers
            .Where(new EuropeanCustomerSpecification().And(new Age18PlusCustomerSpecification()).ToExpression())
            .Count()
            .Should().Be(4);

        _customers
           .Where(new EuropeanCustomerSpecification().Not().And(new Age18PlusCustomerSpecification()).ToExpression())
           .Count()
           .Should().Be(2);

        _customers
            .Where(new Age18PlusCustomerSpecification().AndNot(new EuropeanCustomerSpecification()).ToExpression())
            .Count()
            .Should().Be(2);
    }

    private class Customer
    {
        public string Name { get; private set; }

        public byte Age { get; private set; }

        public long Balance { get; private set; }

        public string Location { get; private set; }

        public Customer(string name, byte age, long balance, string location)
        {
            Name = name;
            Age = age;
            Balance = balance;
            Location = location;
        }
    }

    private class EuropeanCustomerSpecification : Specification<Customer>
    {
        public override Expression<Func<Customer, bool>> ToExpression()
        {
            return c => c.Location == "England" ||
                        c.Location == "Turkey" ||
                        c.Location == "German" ||
                        c.Location == "Netherlands" ||
                        c.Location == "Italy" ||
                        c.Location == "Belgium";
        }
    }

    private class Age18PlusCustomerSpecification : Specification<Customer>
    {
        public override Expression<Func<Customer, bool>> ToExpression()
        {
            return c => c.Age >= 18;
        }
    }

    private class BalanceCustomerSpecification : Specification<Customer>
    {
        public int MinBalance { get; }

        public int MaxBalance { get; }

        public BalanceCustomerSpecification(int minBalance, int maxBalance)
        {
            MinBalance = minBalance;
            MaxBalance = maxBalance;
        }

        public override Expression<Func<Customer, bool>> ToExpression()
        {
            return c => c.Balance >= MinBalance && c.Balance <= MaxBalance;
        }
    }

    private class PremiumCustomerSpecification : AndSpecification<Customer>
    {
        public PremiumCustomerSpecification()
            : base(new Age18PlusCustomerSpecification(), new BalanceCustomerSpecification(20000, int.MaxValue))
        {
        }
    }
}