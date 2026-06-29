// Позиционирует выпадающее меню CrmSelect фиксированно (в координатах вьюпорта), чтобы оно не
// обрезалось overflow-контейнерами (диалоги, скролл-области). Направление (вниз/вверх) и высота
// выбираются по доступному месту. Вызывается из CrmSelect при открытии меню.
export function place(menu, toggle) {
    if (!menu || !toggle) return;

    const rect = toggle.getBoundingClientRect();
    const gap = 4;
    const capPx = 256; // 16rem — тот же предел, что в CSS
    const spaceBelow = window.innerHeight - rect.bottom - gap;
    const spaceAbove = rect.top - gap;

    // Сброс перед измерением естественной высоты.
    menu.style.position = 'fixed';
    menu.style.left = `${rect.left}px`;
    menu.style.width = `${rect.width}px`;
    menu.style.right = 'auto';
    menu.style.maxHeight = '';

    const needed = menu.scrollHeight;
    const openUp = spaceBelow < needed && spaceAbove > spaceBelow;
    const space = openUp ? spaceAbove : spaceBelow;
    const maxH = Math.min(needed, capPx, Math.max(48, space));

    menu.style.maxHeight = `${maxH}px`;
    menu.style.top = openUp ? `${rect.top - gap - maxH}px` : `${rect.bottom + gap}px`;
}
