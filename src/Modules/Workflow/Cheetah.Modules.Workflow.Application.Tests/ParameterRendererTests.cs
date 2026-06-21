using Shouldly;

namespace Cheetah.Modules.Workflow.Application.Tests;

public class ParameterRendererTests
{
    private readonly ParameterRenderer _renderer = new();

    [Fact]
    public void Substitutes_whole_string_variables_and_keeps_literals()
    {
        var payload = new Dictionary<string, object?>
        {
            ["DealId"] = "d-1",
            ["OwnerId"] = "u-9"
        };
        var template = """
        { "dealId": "{{trigger.DealId}}", "assigneeId": "{{trigger.OwnerId}}", "entityType": "crm.deal", "dueInHours": 24, "embedded": "Deal {{trigger.DealId}}" }
        """;

        var result = _renderer.Render(template, payload);

        result["dealId"].ShouldBe("d-1");                       // подстановка по whole-string
        result["assigneeId"].ShouldBe("u-9");
        result["entityType"].ShouldBe("crm.deal");              // литерал
        result["dueInHours"].ShouldBe(24L);                     // число
        result["embedded"].ShouldBe("Deal {{trigger.DealId}}"); // встроенная подстановка не поддерживается → литерал
    }

    [Fact]
    public void Unknown_variable_renders_null()
    {
        var result = _renderer.Render("{ \"x\": \"{{trigger.Missing}}\" }", new Dictionary<string, object?>());
        result["x"].ShouldBeNull();
    }

    [Fact]
    public void Empty_template_returns_empty()
    {
        _renderer.Render("", new Dictionary<string, object?>()).ShouldBeEmpty();
    }
}
