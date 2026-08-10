# com.hungnt.eventbus

Pub/sub type-safe dùng struct event, không magic string.

## Yêu cầu

`com.hungnt.core` 2.0.0 và **VContainer** (cài thủ công qua Git URL — xem README của core).

## Cài đặt vào container

```csharp
builder.InstallEventBus();
```

Mặc định `Lifetime.Singleton`, đặt ở scope gốc là bus dùng chung toàn ứng dụng.

Truyền `Lifetime.Scoped` ở scope của một scene để scene đó có bus riêng — mọi listener tự biến mất khi scene unload, xoá sổ nhóm bug quên `Unregister`:

```csharp
builder.InstallEventBus(Lifetime.Scoped);
```

## Định nghĩa event

Dùng struct để dispatch không sinh rác:

```csharp
public struct OnCoinChanged : IEvent
{
    public int OldValue;
    public int NewValue;
}

public struct OnGameStarted : IEvent { }   // signal, không mang dữ liệu
```

## Sử dụng

```csharp
public class CoinLabel : MonoBehaviour
{
    [Inject] private IEventBusService _eventBus;

    private void OnEnable() => _eventBus.Register<OnCoinChanged>(HandleCoinChanged);

    private void OnDisable() => _eventBus.Unregister<OnCoinChanged>(HandleCoinChanged);

    private void HandleCoinChanged(OnCoinChanged e) { }
}
```

Dispatch:

```csharp
_eventBus.Dispatch(new OnCoinChanged { OldValue = 50, NewValue = 150 });
_eventBus.Dispatch<OnGameStarted>();
```

Component nhận inject phải được đăng ký ở scope: `builder.RegisterComponentInHierarchy<CoinLabel>();`

## Đảm bảo an toàn

- Listener là `UnityEngine.Object` đã destroy sẽ bị bỏ qua khi dispatch, không ném lỗi.
- `Unregister` ngay trong lúc đang dispatch là an toàn: slot được null-out rồi dọn sau, listener kế tiếp không bị nhảy cóc.
- `Register` giữa lúc dispatch sẽ nhận event từ lần dispatch kế tiếp.
- Handler ném exception được bắt và log, không chặn các listener còn lại.

Dù vậy vẫn nên `Unregister` trong `OnDisable`/`OnDestroy` để danh sách listener không phình.

## Debug

**Window → HungNT → Event Bus** liệt kê listener đang đăng ký theo từng event, kèm cảnh báo khi có listener trỏ tới object đã destroy. Cửa sổ tự dò các `LifetimeScope` đang chạy để lấy bus nên chỉ hoạt động trong Play mode.
