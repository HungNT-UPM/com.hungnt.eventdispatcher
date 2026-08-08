using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace HungNT.EventBus.Editor
{
    /// <summary>
    /// Xem các listener đang đăng ký lúc chạy, để soi rò rỉ (listener của object đã destroy).
    /// <para>Bus là plain C# do container tạo nên không nằm trên GameObject nào — cửa sổ tự dò
    /// các <see cref="LifetimeScope"/> đang sống để lấy ra.</para>
    /// </summary>
    public sealed class EventBusWindow : OdinEditorWindow
    {
        private const double RefreshIntervalSeconds = 0.5;

        [MenuItem("HungNT/Event Bus/Event Bus Window")]
        private static void Open()
        {
            var window = GetWindow<EventBusWindow>();
            window.titleContent = new GUIContent("Event Bus");
            window.minSize = new Vector2(560f, 320f);
        }

        private readonly List<EventBusListenerInfo> _listeners = new();
        private double _lastRefreshTime;

        [ShowInInspector, ReadOnly, HideLabel]
        [TableList(IsReadOnly = true, AlwaysExpanded = true, DefaultMinColumnWidth = 90)]
        [InfoBox("Danh sách listener chỉ có trong Play mode.", InfoMessageType.Info, nameof(IsNotPlaying))]
        [InfoBox("Không tìm thấy IEventBusService — scope gốc đã gọi builder.InstallEventBus() chưa?",
            InfoMessageType.Warning, nameof(IsPlayingWithoutBus))]
        private List<EventBusListenerInfo> Listeners => _listeners;

        [ShowInInspector, ReadOnly, PropertyOrder(-10)]
        private int TotalListeners => _listeners.Count;

        [ShowInInspector, ReadOnly, PropertyOrder(-9)]
        [InfoBox("Có listener trỏ tới object đã bị destroy — Dispatch vẫn an toàn (tự bỏ qua) nhưng nên Unregister.",
            InfoMessageType.Warning, nameof(HasDestroyedListener))]
        private int DestroyedListeners
        {
            get
            {
                var count = 0;
                foreach (var entry in _listeners)
                {
                    if (entry.IsDestroyed)
                        count++;
                }

                return count;
            }
        }

        // Điều kiện cho InfoBox — Odin đọc field/property bool theo tên.
        private bool IsNotPlaying => !Application.isPlaying;
        private bool IsPlayingWithoutBus => Application.isPlaying && FindBus() == null;
        private bool HasDestroyedListener => DestroyedListeners > 0;

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void RefreshPeriodically()
        {
            if (EditorApplication.timeSinceStartup - _lastRefreshTime < RefreshIntervalSeconds)
                return;

            _lastRefreshTime = EditorApplication.timeSinceStartup;

            _listeners.Clear();
            var bus = FindBus();
            if (bus != null)
                _listeners.AddRange(bus.GetDebugSnapshot());

            Repaint();
        }

        /// <summary>Scope con resolve được bus của scope cha nên lấy kết quả khớp đầu tiên là đủ.</summary>
        private static IEventBusService FindBus()
        {
            if (!Application.isPlaying)
                return null;

            var scopes = FindObjectsByType<LifetimeScope>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var scope in scopes)
            {
                if (scope.Container == null)
                    continue;

                if (scope.Container.TryResolve<IEventBusService>(out var bus))
                    return bus;
            }

            return null;
        }
    }
}
