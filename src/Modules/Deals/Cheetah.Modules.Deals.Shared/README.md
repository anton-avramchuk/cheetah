# Cheetah.Modules.Deals.Shared

Разделяемые enum'ы, value object `Money` и константы Deals. Зависит **только от `Cheetah.Core`** —
нижний слой, на который опираются Contracts, Domain, Infrastructure и Application.

## Состав

| Тип | Назначение |
|---|---|
| `DealStatus` | `Open` / `Won` / `Lost` (Won/Lost — терминальные) |
| `StageType` | Тип стадии воронки: `Open` / `Won` / `Lost` |
| `TriggerSource` | Источник изменения: `Manual` / `Automation` / `Import` |
| `Money` | Сумма + валюта (ISO-4217). Самодостаточный `record` (не наследует `ValueObject`, чтобы не тянуть `Core.Domain`); маппится как EF owned-type |
| `DealsConstants` | Имя подключения, схема `deals`, ограничения длин, тип денежной колонки |
| `DealEntityRefKeys` | Ключ полиморфной привязки сделки — `crm.deal` |

## Зависимости

`Cheetah.Core` — и больше ничего. Не ссылается на `Domain`/`Contracts`, чтобы оставаться
переиспользуемым нижним слоем модуля.
