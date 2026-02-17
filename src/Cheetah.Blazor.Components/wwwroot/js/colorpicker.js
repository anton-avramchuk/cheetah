window.CrmColorPicker = {
    _instances: new Map(),

    init(dotNetRef, containerId) {
        const state = { dotNetRef, containerId, activeTarget: null };
        this._instances.set(containerId, state);

        const onPointerMove = (e) => {
            if (!state.activeTarget) return;
            e.preventDefault();
            this._handleMove(state, e);
        };

        const onPointerUp = (e) => {
            if (!state.activeTarget) return;
            state.activeTarget = null;
        };

        state.onPointerMove = onPointerMove;
        state.onPointerUp = onPointerUp;
        document.addEventListener('pointermove', onPointerMove);
        document.addEventListener('pointerup', onPointerUp);
    },

    dispose(containerId) {
        const state = this._instances.get(containerId);
        if (state) {
            document.removeEventListener('pointermove', state.onPointerMove);
            document.removeEventListener('pointerup', state.onPointerUp);
            this._instances.delete(containerId);
        }
    },

    startDrag(containerId, target, e) {
        const state = this._instances.get(containerId);
        if (!state) return;
        state.activeTarget = target;
        this._handleMove(state, e);
    },

    _handleMove(state, e) {
        const target = state.activeTarget;
        const el = document.getElementById(state.containerId)
            ?.querySelector(`[data-picker="${target}"]`);
        if (!el) return;

        const rect = el.getBoundingClientRect();
        const x = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
        const y = Math.max(0, Math.min(1, (e.clientY - rect.top) / rect.height));

        state.dotNetRef.invokeMethodAsync('OnPointerInput', target, x, y);
    }
};
