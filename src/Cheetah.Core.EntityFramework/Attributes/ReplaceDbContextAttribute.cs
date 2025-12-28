namespace Cheetah.Core.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ReplaceDbContextAttribute(params Type[] replacedDbContextTypes) : Attribute
{
    public Type[] ReplacedDbContextTypes { get; } = replacedDbContextTypes;
}