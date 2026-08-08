using UnityEngine;
using VContainer;

namespace HungNT.EventBus.Demo
{
    /// <summary>
    /// Demo: listener nhận <see cref="IEventBusService"/> qua injection.
    /// Gán vào GameObject khác với <see cref="EventBusDemoDispatcher"/>, đăng ký ở LifetimeScope,
    /// rồi mở <b>Window → HungNT → Event Bus</b> để xem listener live.
    /// </summary>
    public class EventBusDemoListener : MonoBehaviour
    {
        private IEventBusService _eventBus;

        [Inject]
        public void Construct(IEventBusService eventBus)
        {
            _eventBus = eventBus;
        }

        // [Inject] chạy trước OnEnable lần đầu, nhưng OnEnable còn chạy lại sau mỗi lần bật/tắt object
        // → vẫn phải cặp Register/Unregister như cũ.
        private void OnEnable()
        {
            _eventBus.Register<OnGameStart>(OnGameStart);
            _eventBus.Register<OnGameWin>(OnGameWin);
            _eventBus.Register<OnCoinChanged>(OnCoinChanged);
            _eventBus.Register<OnPlayerJump>(OnPlayerJump);
        }

        private void OnDisable()
        {
            _eventBus.Unregister<OnGameStart>(OnGameStart);
            _eventBus.Unregister<OnGameWin>(OnGameWin);
            _eventBus.Unregister<OnCoinChanged>(OnCoinChanged);
            _eventBus.Unregister<OnPlayerJump>(OnPlayerJump);
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void OnGameStart(OnGameStart _)
            => Debug.Log($"[{name}] Game Started!");

        private void OnGameWin(OnGameWin _)
            => Debug.Log($"[{name}] Game Won!");

        private void OnCoinChanged(OnCoinChanged e)
            => Debug.Log($"[{name}] Coin: {e.OldValue} → {e.NewValue} (Δ{e.Delta})");

        private void OnPlayerJump(OnPlayerJump e)
            => Debug.Log($"[{name}] Player jumped {e.JumpHeight}m");
    }
}