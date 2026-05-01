using System;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Project-local facade for HCITrilogy.Core.ServiceLocator.
    /// All calls forward to the shared implementation.
    /// </summary>
    public static class ServiceLocator
    {
        public static void Register<T>(T service) where T : class
            => HCITrilogy.Core.ServiceLocator.Register(service);
        public static void Unregister<T>() where T : class
            => HCITrilogy.Core.ServiceLocator.Unregister<T>();
        public static T Get<T>() where T : class
            => HCITrilogy.Core.ServiceLocator.Get<T>();
        public static bool TryGet<T>(out T service) where T : class
            => HCITrilogy.Core.ServiceLocator.TryGet(out service);
        public static void Clear()
            => HCITrilogy.Core.ServiceLocator.Clear();
    }
}
