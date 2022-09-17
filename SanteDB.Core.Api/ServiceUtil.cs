using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SanteDB.Core
{
    /// <summary>
    /// Utility for starting / stopping hosted services
    /// </summary>
    public static class ServiceUtil
    {

        /// <summary>
        /// Helper function to start the services
        /// </summary>
        public static void Start(Guid activityId, IApplicationServiceContext applicationServiceContext)
        {
            Trace.CorrelationManager.ActivityId = activityId;
            Trace.TraceInformation("Starting host context on Console Presentation System at {0}", DateTime.Now);

            // Do this because loading stuff is tricky ;)
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            try
            {
                // Initialize 
                ApplicationServiceContext.Current = applicationServiceContext;
                ApplicationServiceContext.Current.Start();
            }
            catch (Exception e)
            {
                Trace.TraceError("Fatal exception occurred: {0}", e.ToString());
                Stop();
                throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public static void Stop()
        {
            ApplicationServiceContext.Current?.Stop();
            Tracer.DisposeWriters();
        }

        /// <summary>
        /// Assembly resolution
        /// </summary>
        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                if (args.Name == asm.FullName)
                    return asm;

            /// Try for an non-same number Version
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fAsmName = args.Name;
                if (fAsmName.Contains(","))
                    fAsmName = fAsmName.Substring(0, fAsmName.IndexOf(","));
                if (fAsmName == asm.GetName().Name)
                    return asm;
            }

            return null;
        }

    }
}
