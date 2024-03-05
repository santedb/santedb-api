/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Diagnostics;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SanteDB.Core
{
    /// <summary>
    /// Utility for starting / stopping the SanteDB hosted services
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
            {
                if (args.Name == asm.FullName)
                {
                    return asm;
                }
            }

            // Try for an non-same number Version
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fAsmName = args.Name;
                if (fAsmName.Contains(","))
                {
                    fAsmName = fAsmName.Substring(0, fAsmName.IndexOf(","));
                }

                if (fAsmName == asm.GetName().Name)
                {
                    return asm;
                }
            }

            return null;
        }

    }
}
