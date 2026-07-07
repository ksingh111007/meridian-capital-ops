using System.Runtime.CompilerServices;

namespace Meridian.Api.IntegrationTests.Support;

internal static class AssemblySetup
{
    /// <summary>
    /// Quartz bridges its internal logging through a process-global LogProvider that
    /// captures the first host's LoggerFactory. With many WebApplicationFactory hosts
    /// starting and disposing in one test run, later hosts would touch the disposed
    /// factory (ObjectDisposedException). Disable the bridge — jobs themselves log
    /// via injected ILogger and are unaffected.
    /// </summary>
    [ModuleInitializer]
    internal static void DisableQuartzGlobalLogProvider() => Quartz.Logging.LogProvider.IsDisabled = true;
}
