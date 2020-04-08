using System;
using System.Threading;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditUserIdProvider
    {
        string GetUserId();
    }

    public class EnvironmentAuditUserIdProvider : IAuditUserIdProvider
    {
        private EnvironmentAuditUserIdProvider()
        {
        }

        public static Lazy<EnvironmentAuditUserIdProvider> Instance = new Lazy<EnvironmentAuditUserIdProvider>(() => new EnvironmentAuditUserIdProvider(), true);

        public string GetUserId() => Environment.UserName;
    }

    public class ThreadPrincipalUserIdProvider : IAuditUserIdProvider
    {
        public static Lazy<ThreadPrincipalUserIdProvider> Instance = new Lazy<ThreadPrincipalUserIdProvider>(() => new ThreadPrincipalUserIdProvider(), true);

        private ThreadPrincipalUserIdProvider()
        {
        }

        public string GetUserId() => Thread.CurrentPrincipal?.Identity?.Name;
    }
}
