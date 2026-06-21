using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class KanbanTests : BunitContext
{
    private IRenderedComponent<Kanban<string, string>> RenderBoard(
        Action<KanbanDropArgs<string, string>>? onDrop = null,
        Func<string, bool>? canDrag = null)
        => Render<Kanban<string, string>>(p =>
        {
            p.Add(k => k.Columns, new[] { "A", "B" });
            p.Add(k => k.CardsSelector, (string c) => c == "A" ? ["a1", "a2"] : Array.Empty<string>());
            p.Add(k => k.CardTemplate, (string card) => $"<span class=\"kc\">{card}</span>");
            if (canDrag is not null)
                p.Add(k => k.CanDragCard, canDrag);
            if (onDrop is not null)
                p.Add(k => k.OnCardDropped, onDrop);
        });

    [Fact]
    public void RendersColumns_CardsAndCounts()
    {
        var cut = RenderBoard();

        cut.FindAll(".crm-kanban-column").Count.ShouldBe(2);
        cut.FindAll(".crm-kanban-card").Count.ShouldBe(2);
        cut.Markup.ShouldContain("a1");
        cut.Markup.ShouldContain("a2");

        var counts = cut.FindAll(".crm-kanban-count");
        counts[0].TextContent.ShouldBe("2"); // колонка A
        counts[1].TextContent.ShouldBe("0"); // колонка B
    }

    [Fact]
    public void LockedCard_IsNotDraggable_AndHasLockedClass()
    {
        var cut = RenderBoard(canDrag: c => c != "a2");

        var cards = cut.FindAll(".crm-kanban-card");
        cards[0].GetAttribute("draggable").ShouldBe("true");
        cards[1].GetAttribute("draggable").ShouldBe("false");
        cards[1].ClassList.ShouldContain("crm-kanban-card-locked");
    }

    [Fact]
    public void DropCardToAnotherColumn_RaisesOnCardDropped_WithSourceAndTarget()
    {
        KanbanDropArgs<string, string>? dropped = null;
        var cut = RenderBoard(onDrop: a => dropped = a);

        cut.FindAll(".crm-kanban-card")[0].DragStart();
        cut.FindAll(".crm-kanban-column")[1].Drop();

        dropped.ShouldNotBeNull();
        dropped!.Card.ShouldBe("a1");
        dropped.SourceColumn.ShouldBe("A");
        dropped.TargetColumn.ShouldBe("B");
    }

    [Fact]
    public void DropCardOnSameColumn_DoesNotRaiseEvent()
    {
        KanbanDropArgs<string, string>? dropped = null;
        var cut = RenderBoard(onDrop: a => dropped = a);

        cut.FindAll(".crm-kanban-card")[0].DragStart();
        cut.FindAll(".crm-kanban-column")[0].Drop();

        dropped.ShouldBeNull();
    }
}
