namespace Voat
{
    using System.Data.Entity;
    using System.Diagnostics;
    using Autofac;
    using Models;

    public static class DependencyInjection
    {
        public static void RegisterComponents(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var dbContext = new whoaverseEntities();
#if DEBUG
                dbContext.Database.Log += s => Debug.WriteLine(s);
#endif
                return dbContext;
            }).As<DbContext>().InstancePerLifetimeScope();
        }
    }
}