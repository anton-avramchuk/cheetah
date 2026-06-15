using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Deals.Application.Tests;

public class DealQueryHandlerTests
{
    [Fact]
    public async Task GetBoard_MapsAggregatesToColumns()
    {
        var pipelineId = Guid.NewGuid();
        var s1 = Guid.NewGuid();
        var s2 = Guid.NewGuid();

        var deals = new Mock<IDealRepository>();
        deals.Setup(d => d.GetOpenBoardAsync(pipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new StageAggregate(s1, 3, 3000m),
                new StageAggregate(s2, 1, 500m)
            });

        var handler = new GetDealBoardQueryHandler(deals.Object);
        var board = await handler.HandleAsync(new GetDealBoardQuery(pipelineId), CancellationToken.None);

        board.PipelineId.ShouldBe(pipelineId);
        board.Columns.Count.ShouldBe(2);
        board.Columns.First(c => c.StageId == s1).Sum.ShouldBe(3000m);
        board.Columns.First(c => c.StageId == s1).Count.ShouldBe(3);
    }
}
