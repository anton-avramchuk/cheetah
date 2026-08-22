using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahNotesSharedModule))]
public class CheetahNotesContractsModule : CrmModule
{
}
