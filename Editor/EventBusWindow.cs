using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace HungNT.EventBus.Editor
{
    /// <summary>
    /// Cửa sổ debug listener của <see cref="IEventBus"/>: <b>Window → HungNT → Event Bus</b>.
    /// <para>Trước đây đây là CustomEditor cho component EventBus. Từ khi bus là plain C# do container
    /// tạo, nó không còn nằm trên GameObject nào — cửa sổ tự dò các <see cref="LifetimeScope"/> đang
    /// sống và resolve bus ra từ đó.</para>
    /// </summary>
    public class EventBusWindow : EditorWindow
    {
        private readonly Dictionary<string, bool> _foldouts = new();
        private double _lastRepaintTime;
        private Vector2 _scroll;

        // Styles (lazy-init)
        private GUIStyle _headerStyle;
        private GUIStyle _listenerStyle;
        private GUIStyle _destroyedStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _sectionBoxStyle;

        [MenuItem("Window/HungNT/Event Bus")]
        private static void Open()
        {
            GetWindow<EventBusWindow>("Event Bus").Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Listener list available in Play Mode only.", MessageType.Info);
                return;
            }

            var bus = FindBus();
            if (bus == null)
            {
                EditorGUILayout.HelpBox(
                    "Không tìm thấy IEventBus trong LifetimeScope nào đang chạy.\n" +
                    "Kiểm tra scope gốc đã gọi builder.InstallEventBus() chưa.",
                    MessageType.Warning);
                return;
            }

            InitStyles();

            var snapshot = bus.GetDebugSnapshot();
            DrawHeader(snapshot.Count);

            if (snapshot.Count == 0)
            {
                EditorGUILayout.LabelField("  (no listeners registered)", EditorStyles.miniLabel);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var grouped = GroupByEvent(snapshot);
            foreach (var kvp in grouped)
                DrawEventGroup(kvp.Key, kvp.Value);
            EditorGUILayout.EndScrollView();

            // Auto-repaint mỗi 0.5s
            if (EditorApplication.timeSinceStartup - _lastRepaintTime > 0.5)
            {
                _lastRepaintTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        /// <summary>Scope con resolve được bus của scope cha nên lấy kết quả khớp đầu tiên là đủ.</summary>
        private static IEventBus FindBus()
        {
            var scopes = FindObjectsByType<LifetimeScope>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var scope in scopes)
            {
                if (scope.Container == null)
                    continue;

                if (scope.Container.TryResolve<IEventBus>(out var bus))
                    return bus;
            }

            return null;
        }

        // ── Draw helpers ─────────────────────────────────────────────────────

        private void DrawHeader(int totalCount)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("⚡ Event Listeners", _headerStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"  {totalCount} registered  ", _badgeStyle, GUILayout.Width(110));
            EditorGUILayout.EndHorizontal();

            var rect = GUILayoutUtility.GetLastRect();
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
            EditorGUILayout.Space(8);
        }

        private void DrawEventGroup(string eventName, List<EventBusListenerInfo> listeners)
        {
            if (!_foldouts.ContainsKey(eventName))
                _foldouts[eventName] = true;

            int destroyedCount = 0;
            foreach (var l in listeners)
                if (l.IsDestroyed)
                    destroyedCount++;

            var label = destroyedCount > 0
                ? $"⚠  {eventName}  ({listeners.Count} listeners, {destroyedCount} destroyed)"
                : $"◆  {eventName}  ({listeners.Count} listener{(listeners.Count > 1 ? "s" : "")})";

            // Box bao bọc từng event group
            EditorGUILayout.BeginVertical(_sectionBoxStyle);

            _foldouts[eventName] = EditorGUILayout.Foldout(_foldouts[eventName], label, true, _headerStyle);

            if (_foldouts[eventName])
            {
                EditorGUI.indentLevel++;
                foreach (var entry in listeners)
                    DrawListenerRow(entry);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawListenerRow(EventBusListenerInfo entry)
        {
            var style = entry.IsDestroyed ? _destroyedStyle : _listenerStyle;
            var icon = entry.IsDestroyed ? "⚠ " : "→ ";
            var label = entry.IsDestroyed
                ? $"{icon}[DESTROYED]  {entry.TargetName}.{entry.MethodName}"
                : $"{icon}{entry.TargetName}.{entry.MethodName}";

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label, style, GUILayout.MinWidth(160));

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    entry.RegisteredObject,
                    typeof(UnityEngine.Object),
                    allowSceneObjects: true,
                    GUILayout.MinWidth(120), GUILayout.MaxWidth(200));
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Grouping ─────────────────────────────────────────────────────────

        private static Dictionary<string, List<EventBusListenerInfo>> GroupByEvent(List<EventBusListenerInfo> snapshot)
        {
            var grouped = new Dictionary<string, List<EventBusListenerInfo>>();
            foreach (var entry in snapshot)
            {
                if (!grouped.ContainsKey(entry.EventName))
                    grouped[entry.EventName] = new List<EventBusListenerInfo>();
                grouped[entry.EventName].Add(entry);
            }

            return grouped;
        }

        // ── Styles ───────────────────────────────────────────────────────────

        private void InitStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };
            _headerStyle.normal.textColor = new Color(0.88f, 0.88f, 0.88f);
            _headerStyle.onNormal.textColor = new Color(0.88f, 0.88f, 0.88f);

            _listenerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
            };
            _listenerStyle.normal.textColor = new Color(0.55f, 0.92f, 0.55f); // green

            _destroyedStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
            };
            _destroyedStyle.normal.textColor = new Color(1f, 0.45f, 0.35f); // red-orange

            _badgeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };

            _sectionBoxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 2, 2),
            };
        }
    }
}
