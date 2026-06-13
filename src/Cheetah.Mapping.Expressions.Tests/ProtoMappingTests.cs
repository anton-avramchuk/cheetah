using Cheetah.Mapping.Expressions;
using Cheetah.Mapping.Expressions.Protobuf;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Shouldly;

namespace Cheetah.Mapping.Expressions.Tests;

public class ProtoMappingTests
{
    // proto-поддержка теперь opt-in — включаем её для тестов (как сделал бы CrmCustomMappingProtobufModule).
    static ProtoMappingTests() => ProtobufMappingSupport.Register();

    private readonly ExpressionObjectMapper _mapper = new();

    // --- скалярные конверсии ---

    [Fact]
    public void Maps_Guid_To_String_And_Back_Through_Message()
    {
        var id = Guid.NewGuid();

        var reply = _mapper.Map<ModelWithGuid, ProtoWithStringId>(new ModelWithGuid(id, "n"));
        reply.Id.ShouldBe(id.ToString());
        reply.Name.ShouldBe("n");

        var model = _mapper.Map<ProtoWithStringId, ModelWithGuid>(reply);
        model.Id.ShouldBe(id);
    }

    [Fact]
    public void Maps_DateTime_To_Timestamp()
    {
        var when = new DateTime(2026, 6, 13, 10, 0, 0, DateTimeKind.Utc);

        var reply = _mapper.Map<ModelWithDate, ProtoWithTimestamp>(new ModelWithDate(when));

        reply.When.ShouldNotBeNull();
        reply.When.ToDateTime().ShouldBe(when);
    }

    // --- маппинг в позиционный record через конструктор ---

    [Fact]
    public void Maps_ProtoRequest_To_PositionalRecord_ViaConstructor()
    {
        var id = Guid.NewGuid();
        var request = new ProtoWithStringId { Id = id.ToString(), Name = "Acme" };

        var command = _mapper.Map<ProtoWithStringId, CreateRecordCommand>(request);

        command.Id.ShouldBe(id);
        command.Name.ShouldBe("Acme");
    }

    [Fact]
    public void Maps_OptionalString_To_NullableGuid()
    {
        var req = new ProtoOptional { Name = "x", IndustryId = "" }; // не задано -> ""
        var cmd = _mapper.Map<ProtoOptional, OptionalCommand>(req);
        cmd.Name.ShouldBe("x");
        cmd.IndustryId.ShouldBeNull();

        var id = Guid.NewGuid();
        var req2 = new ProtoOptional { Name = "x", IndustryId = id.ToString() };
        _mapper.Map<ProtoOptional, OptionalCommand>(req2).IndustryId.ShouldBe(id);
    }

    // --- proto-сообщение: null не присваивается в string-поле ---

    [Fact]
    public void DoesNotAssignNull_To_ProtoStringField()
    {
        // Email == null в источнике -> proto-сеттер кинул бы, маппер должен пропустить присваивание
        var model = new ModelWithNullable("Bob", null);

        var reply = Should.NotThrow(() => _mapper.Map<ModelWithNullable, ProtoStrictStrings>(model));

        reply.Name.ShouldBe("Bob");
        reply.Email.ShouldBe(""); // осталось значение по умолчанию
    }

    // --- обёртка скаляра в одно-полевое сообщение ---

    [Fact]
    public void Wraps_Scalar_Into_SingleField_Message()
    {
        var id = Guid.NewGuid();

        var reply = _mapper.Map<Guid, ProtoSingleField>(id);

        reply.Value.ShouldBe(id.ToString());
    }

    [Fact]
    public void Wraps_Scalar_Via_ObjectOverload()
    {
        var id = Guid.NewGuid();

        var reply = _mapper.Map<ProtoSingleField>((object)id);

        reply.Value.ShouldBe(id.ToString());
    }
}

// ---------- модели ----------

public sealed record ModelWithGuid(Guid Id, string Name);
public sealed record ModelWithDate(DateTime When);
public sealed record ModelWithNullable(string Name, string? Email);

public sealed record CreateRecordCommand(Guid Id, string Name);
public sealed record OptionalCommand(string Name, Guid? IndustryId);

// ---------- фейковые proto-сообщения (минимальный IMessage) ----------

public abstract class FakeMessage : IMessage
{
    public MessageDescriptor Descriptor => null!;
    public int CalculateSize() => 0;
    public void MergeFrom(CodedInputStream input) { }
    public void WriteTo(CodedOutputStream output) { }
}

public sealed class ProtoWithStringId : FakeMessage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class ProtoWithTimestamp : FakeMessage
{
    public Timestamp? When { get; set; }
}

public sealed class ProtoOptional : FakeMessage
{
    public string Name { get; set; } = "";
    public string IndustryId { get; set; } = "";
}

// строковые поля кидают на null — как настоящие protobuf-сеттеры
public sealed class ProtoStrictStrings : FakeMessage
{
    private string _name = "";
    private string _email = "";
    public string Name { get => _name; set => _name = value ?? throw new ArgumentNullException(nameof(value)); }
    public string Email { get => _email; set => _email = value ?? throw new ArgumentNullException(nameof(value)); }
}

public sealed class ProtoSingleField : FakeMessage
{
    public string Value { get; set; } = "";
}
