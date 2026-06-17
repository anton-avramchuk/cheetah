using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.NotesTimeline.Shared;

namespace Cheetah.Modules.NotesTimeline.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahNotesTimelineSharedModule))]
public class CheetahNotesTimelineContractsModule : CrmModule
{
}
