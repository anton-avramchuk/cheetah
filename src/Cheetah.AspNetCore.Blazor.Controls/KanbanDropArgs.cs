namespace Cheetah.AspNetCore.Blazor.Controls;

/// <summary>Событие переноса карточки канбана из одной колонки в другую.</summary>
/// <param name="Card">Перенесённая карточка.</param>
/// <param name="SourceColumn">Колонка, из которой перетащили.</param>
/// <param name="TargetColumn">Колонка, в которую отпустили.</param>
public sealed record KanbanDropArgs<TColumn, TCard>(TCard Card, TColumn SourceColumn, TColumn TargetColumn);
