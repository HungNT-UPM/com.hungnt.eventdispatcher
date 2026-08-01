using UnityEngine;
using VContainer;

namespace HungNT.EventBus.Demo
{
    /// <summary>
    /// Demo: listener nhận <see cref="IEventBus"/> qua injection.
    /// Gán vào GameObject khác với <see cref="EventBusDemo_Dispatcher"/>, đăng ký bằng
    /// <c>builder.RegisterComponentInHierarchy&lt;EventBusDemo_Listener&gt;()</c>,
    /// rồi mở <b>Window → HungNT → Event Bus</b> để xem listener live.
    /// </summary>
    public class EventBusDemo_Listener : MonoBehaviour
    {
        private IEventBus _bus;

        [Inject]
        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        // [Inject] chạy trước OnEnable lần đầu, nhưng OnEnable còn chạy lại sau mỗi lần bật/tắt object
        // → vẫn phải cặp Register/Unregister như cũ.
        private void OnEnable()
        {
            _bus.Register<OnGameStart>(OnGameStart);
            _bus.Register<OnGameWin>(OnGameWin);
            _bus.Register<OnCoinChanged>(OnCoinChanged);
            _bus.Register<OnPlayerJump>(OnPlayerJump);
        }

        private void OnDisable()
        {
            _bus.Unregister<OnGameStart>(OnGameStart);
            _bus.Unregister<OnGameWin>(OnGameWin);
            _bus.Unregister<OnCoinChanged>(OnCoinChanged);
            _bus.Unregister<OnPlayerJump>(OnPlayerJump);
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
