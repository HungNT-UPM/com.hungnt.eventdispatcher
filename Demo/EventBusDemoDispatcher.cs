using UnityEngine;
using VContainer;

namespace HungNT.EventBus.Demo
{
    /// <summary>
    /// Demo: dispatch event qua <see cref="IEventBusService"/> được inject.
    /// Gán vào một GameObject, đăng ký bằng <c>RegisterComponentInHierarchy</c>,
    /// rồi bấm các mục trong context menu của component (Play Mode).
    /// </summary>
    public class EventBusDemoDispatcher : MonoBehaviour
    {
        private IEventBusService _eventBus;

        [Inject]
        public void Construct(IEventBusService eventBus)
        {
            _eventBus = eventBus;
        }

        [ContextMenu("Dispatch OnGameStart")]
        public void DispatchGameStart()
            => _eventBus.Dispatch<OnGameStart>();

        [ContextMenu("Dispatch OnGameWin")]
        public void DispatchGameWin()
            => _eventBus.Dispatch<OnGameWin>();

        [ContextMenu("Dispatch OnCoinChanged")]
        public void DispatchCoinChanged()
            => _eventBus.Dispatch(new OnCoinChanged { OldValue = 50, NewValue = 150 });

        [ContextMenu("Dispatch OnPlayerJump")]
        public void DispatchPlayerJump()
            => _eventBus.Dispatch(new OnPlayerJump { JumpHeight = 3.5f });
    }
}