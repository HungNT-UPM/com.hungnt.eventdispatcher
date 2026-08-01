using UnityEngine;
using VContainer;

namespace HungNT.EventBus.Demo
{
    /// <summary>
    /// Demo: dispatch event qua <see cref="IEventBus"/> được inject.
    /// Gán vào một GameObject, đăng ký bằng <c>RegisterComponentInHierarchy</c>,
    /// rồi bấm các mục trong context menu của component (Play Mode).
    /// </summary>
    public class EventBusDemo_Dispatcher : MonoBehaviour
    {
        private IEventBus _bus;

        [Inject]
        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        [ContextMenu("Dispatch OnGameStart")]
        public void DispatchGameStart()
            => _bus.Dispatch<OnGameStart>();

        [ContextMenu("Dispatch OnGameWin")]
        public void DispatchGameWin()
            => _bus.Dispatch<OnGameWin>();

        [ContextMenu("Dispatch OnCoinChanged")]
        public void DispatchCoinChanged()
            => _bus.Dispatch(new OnCoinChanged { OldValue = 50, NewValue = 150 });

        [ContextMenu("Dispatch OnPlayerJump")]
        public void DispatchPlayerJump()
            => _bus.Dispatch(new OnPlayerJump { JumpHeight = 3.5f });
    }
}
