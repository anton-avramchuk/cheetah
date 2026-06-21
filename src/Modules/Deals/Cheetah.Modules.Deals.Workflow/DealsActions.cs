namespace Cheetah.Modules.Deals.Workflow;

/// <summary>Имена действий, которые модуль Deals предоставляет Workflow (значения = ActionType в правилах).</summary>
public static class DealsActions
{
    public const string ChangeStage = "ChangeDealStage";
    public const string Win = "WinDeal";
    public const string AssignOwner = "AssignDealOwner";
}
