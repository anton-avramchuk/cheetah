using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Notes.Application.Notes;
using Cheetah.Modules.Notes.Default.Contracts;
using Cheetah.Modules.Notes.Shared;
using Mapster;

namespace Cheetah.Modules.Notes.Default.Mapping;

/// <summary>
/// Mapster-маппинги «из коробки»: Request → Command/Query. Команды/запросы заметки — generic-обёртки
/// над самим запросом, поэтому маппинг задан явным <c>MapWith</c>, а не по соглашению об именах.
/// Наследник, закрывающий шаблон своими типами, объявляет такой же профиль у себя.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class NotesMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<CreateNoteRequest, CreateNoteCommand<CreateNoteRequest>>()
            .MapWith(src => new CreateNoteCommand<CreateNoteRequest>(src));

        config.NewConfig<UpdateNoteRequest, UpdateNoteCommand<UpdateNoteRequest>>()
            .MapWith(src => new UpdateNoteCommand<UpdateNoteRequest>(src.Id, src));

        config.NewConfig<GetNoteByIdRequest, GetNoteByIdQuery<NoteDto>>()
            .MapWith(src => new GetNoteByIdQuery<NoteDto>(src.Id));

        config.NewConfig<GetNotesByEntityRequest, GetNotesByEntityQuery<NoteDto>>()
            .MapWith(src => new GetNotesByEntityQuery<NoteDto>(
                src.EntityType, src.EntityId, src.PinnedOnly ?? false,
                src.Skip ?? 0, src.Take ?? NotesConstants.DefaultPageSize));

        config.NewConfig<GetNoteRepliesRequest, GetNoteRepliesQuery<NoteDto>>()
            .MapWith(src => new GetNoteRepliesQuery<NoteDto>(
                src.Id, src.Skip ?? 0, src.Take ?? NotesConstants.DefaultPageSize));

        config.NewConfig<PinNoteRequest, PinNoteCommand>().MapWith(src => new PinNoteCommand(src.Id));
        config.NewConfig<UnpinNoteRequest, UnpinNoteCommand>().MapWith(src => new UnpinNoteCommand(src.Id));
        config.NewConfig<RemoveNoteRequest, RemoveNoteCommand>().MapWith(src => new RemoveNoteCommand(src.Id));
    }
}
