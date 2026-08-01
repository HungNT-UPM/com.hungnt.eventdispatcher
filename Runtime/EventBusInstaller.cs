using VContainer;

namespace HungNT.EventBus
{
    /// <summary>Đăng ký <see cref="IEventBus"/>. Gọi trong <c>Configure</c> của LifetimeScope.</summary>
    public static class EventBusInstaller
    {
        /// <summary>
        /// <paramref name="lifetime"/> mặc định <c>Singleton</c> (bus toàn ứng dụng, đặt ở scope gốc).
        /// Truyền <c>Lifetime.Scoped</c> ở scope scene nếu muốn bus riêng cho scene đó —
        /// listener sẽ tự biến mất cùng scene.
        /// </summary>
        public static IContainerBuilder InstallEventBus(
            this IContainerBuilder builder,
            Lifetime lifetime = Lifetime.Singleton)
        {
            builder.Register<EventBus>(lifetime).As<IEventBus>();
            return builder;
        }
    }
}
