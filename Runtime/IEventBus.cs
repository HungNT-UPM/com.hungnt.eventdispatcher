using System;
using System.Collections.Generic;

namespace HungNT.EventBus
{
    /// <summary>
    /// Pub/sub type-safe dùng struct event. Inject vào nơi cần thay vì gọi singleton toàn cục.
    /// <para><b>Bus theo scope:</b> đăng ký ở scope gốc → bus toàn ứng dụng; đăng ký ở scope scene
    /// (<c>Lifetime.Scoped</c>) → mọi listener tự biến mất khi scene unload, xoá sổ nhóm bug
    /// "quên Unregister trong OnDestroy".</para>
    /// </summary>
    public interface IEventBus
    {
        /// <summary>Đăng ký lắng nghe <typeparamref name="TEvent"/>. Register trong lúc dispatch: nhận event từ lần dispatch sau.</summary>
        void Register<TEvent>(Action<TEvent> listener) where TEvent : IEvent;

        /// <summary>Hủy đăng ký. An toàn khi gọi ngay trong listener đang được dispatch.</summary>
        void Unregister<TEvent>(Action<TEvent> listener) where TEvent : IEvent;

        /// <summary>Gửi event có data tới mọi listener đã đăng ký.</summary>
        void Dispatch<TEvent>(TEvent evt) where TEvent : IEvent;

        /// <summary>Gửi signal event không có data.</summary>
        void Dispatch<TEvent>() where TEvent : struct, IEvent;

        /// <summary>Xóa mọi listener của một event type.</summary>
        void ClearEvent<TEvent>() where TEvent : IEvent;

        /// <summary>Xóa toàn bộ listener.</summary>
        void ClearAll();

        /// <summary>Snapshot listener đang đăng ký — phục vụ cửa sổ debug trong Editor.</summary>
        List<EventBusListenerInfo> GetDebugSnapshot();
    }
}
