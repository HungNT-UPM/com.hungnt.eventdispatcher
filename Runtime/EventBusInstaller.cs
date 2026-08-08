using VContainer;

namespace HungNT.EventBus
{
    /// <summary>Đăng ký <see cref="IEventBusService"/>. Gọi trong <c>Configure</c> của LifetimeScope.</summary>
    public static class EventBusInstaller
    {
        /// <summary>
        /// Mặc định <c>Singleton</c> — bus dùng chung toàn ứng dụng, đặt ở scope gốc.
        /// Truyền <c>Lifetime.Scoped</c> ở scope scene nếu muốn listener tự biến mất cùng scene đó.
        /// </summary>
        public static void InstallEventBus(this IContainerBuilder builder)
        {
            builder.Register<EventBusService>(Lifetime.Singleton).As<IEventBusService>();
        }
    }
}