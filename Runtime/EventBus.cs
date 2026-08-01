using System;
using System.Collections.Generic;

namespace HungNT.EventBus
{
    /// <summary>
    /// Implementation của <see cref="IEventBus"/> — plain C#, do container tạo và dọn.
    /// Tự bỏ qua listener là <c>UnityEngine.Object</c> đã bị destroy khi Dispatch, không crash.
    /// </summary>
    public class EventBus : IEventBus, IDisposable
    {
        // Dùng List<Delegate> thay multicast delegate để tránh alloc trên dispatch path (không gọi GetInvocationList)
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        // Unregister trong lúc đang Dispatch: null-out phần tử (không Remove — Remove làm shift index,
        // listener kế tiếp bị skip). List bẩn được compact khi dispatch ngoài cùng kết thúc.
        private readonly HashSet<Type> _dirtyTypes = new();
        private int _dispatchDepth;

        /// <summary>Container gọi khi scope chứa bus này chết — mọi listener của scope biến mất cùng lúc.</summary>
        public void Dispose()
        {
            _handlers.Clear();
            _dirtyTypes.Clear();
        }

        #region === REGISTER / UNREGISTER ===

        public void Register<TEvent>(Action<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null)
            {
                this.LogWarning($"Register<{typeof(TEvent).Name}> — listener null, skipped.");
                return;
            }

            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }

            list.Add(listener);
        }

        public void Unregister<TEvent>(Action<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null) return;

            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list)) return;

            var index = list.IndexOf(listener);
            if (index < 0) return;

            if (_dispatchDepth > 0)
            {
                list[index] = null;
                _dirtyTypes.Add(type);
                return;
            }

            list.RemoveAt(index);
            if (list.Count == 0)
                _handlers.Remove(type);
        }

        #endregion

        #region === DISPATCH ===

        public void Dispatch<TEvent>(TEvent evt) where TEvent : IEvent
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list)) return;

            _dispatchDepth++;
            try
            {
                // Chốt count: listener register thêm trong lúc dispatch sẽ nhận từ lần sau.
                var count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    var d = list[i];
                    if (d == null)
                        continue;
                    if (d.Target is UnityEngine.Object unityObj && unityObj == null)
                        continue;

                    try
                    {
                        ((Action<TEvent>)d).Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        DebugEx.LogError($"[{nameof(EventBus)}] Exception in {d.Target?.GetType().Name}.{d.Method.Name}: {ex}");
                    }
                }
            }
            finally
            {
                _dispatchDepth--;
                if (_dispatchDepth == 0 && _dirtyTypes.Count > 0)
                    CompactDirtyLists();
            }
        }

        public void Dispatch<TEvent>() where TEvent : struct, IEvent
            => Dispatch(default(TEvent));

        #endregion

        #region === UTILITIES ===

        public void ClearEvent<TEvent>() where TEvent : IEvent
        {
            var type = typeof(TEvent);
            _handlers.Remove(type);
            _dirtyTypes.Remove(type);
        }

        public void ClearAll()
        {
            _handlers.Clear();
            _dirtyTypes.Clear();
            this.Log("All listeners cleared.".Color("orange"));
        }

        public List<EventBusListenerInfo> GetDebugSnapshot()
        {
            var result = new List<EventBusListenerInfo>();

            foreach (var kvp in _handlers)
            {
                if (kvp.Value == null) continue;

                foreach (var d in kvp.Value)
                {
                    if (d == null) continue; // slot bị null-out trong lúc dispatch, chưa compact

                    bool isDestroyed = d.Target is UnityEngine.Object uObj && uObj == null;

                    UnityEngine.Object registeredObj = null;
                    if (!isDestroyed && d.Target is UnityEngine.Object obj)
                        registeredObj = obj;

                    result.Add(new EventBusListenerInfo
                    {
                        EventName = kvp.Key.Name,
                        TargetName = d.Target != null ? d.Target.GetType().Name : "static",
                        MethodName = d.Method.Name,
                        IsDestroyed = isDestroyed,
                        RegisteredObject = registeredObj,
                    });
                }
            }

            return result;
        }

        #endregion

        /// <summary>Dọn các slot bị null-out bởi Unregister-trong-dispatch. Chỉ chạy khi không còn dispatch nào lồng nhau.</summary>
        private void CompactDirtyLists()
        {
            foreach (var type in _dirtyTypes)
            {
                if (!_handlers.TryGetValue(type, out var list))
                    continue;

                list.RemoveAll(d => d == null);
                if (list.Count == 0)
                    _handlers.Remove(type);
            }

            _dirtyTypes.Clear();
        }
    }
}
