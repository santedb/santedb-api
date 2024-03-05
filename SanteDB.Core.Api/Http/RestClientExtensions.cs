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
 * Date: 2023-9-14
 */
using System;
using System.Diagnostics;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Extensions methods that target <see cref="IRestClient"/>
    /// </summary>
    public static class RestClientExtensions
    {
        private const string HTTP_PING = "PING";
        private const string HTTP_ROOT_PATH = "/";
        private static readonly TimeSpan MinusOne = TimeSpan.FromMilliseconds(-1);
        private const int PING_TIMEOUT = 3000; // MS for a PING timeout

        /// <summary>
        /// Attempts to send an HTTP PING to the rest client service. If successful, the reported time drift and latency are returned.
        /// </summary>
        /// <param name="restClient">The rest client to use to send the PING request.</param>
        /// <param name="latencyMs">When successful, contains the latency in whole milliseconds that the request took. If the function fails, the result will be -1.</param>
        /// <param name="timeDrift">When successful, contains the reported time difference between the client and server. No processing takes place to increase the accuracy of this value. If the request fails, the result will be -1.</param>
        /// <returns>True when the PING request succeeds, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="restClient"/> is null and the function can not execute.</exception>
        public static bool TryPing(this IRestClient restClient, out long latencyMs, out TimeSpan timeDrift)
        {
            if (null == restClient)
            {
                throw new ArgumentNullException(nameof(restClient));
            }

            try
            {
                DateTime servertime = DateTime.Now;

                void handler(object sender, RestResponseEventArgs args)
                {
                    if (null != args.Headers)
                    {
                        if (args.Headers.TryGetValue("X-GeneratedOn", out var xgoheader))
                        {
                            _ = DateTime.TryParse(xgoheader, out servertime);
                        }
                        else if (args.Headers.TryGetValue("Date", out var dateheader))
                        {
                            _ = DateTime.TryParse(dateheader, out servertime);
                        }
                    }
                };

                restClient.Responded += handler;
                restClient.SetTimeout(PING_TIMEOUT);
                var sw = Stopwatch.StartNew();
                restClient.Invoke<object, object>(HTTP_PING, HTTP_ROOT_PATH, null);
                sw.Stop();

                restClient.Responded -= handler;

                timeDrift = servertime.Subtract(DateTime.Now);
                latencyMs = sw.ElapsedMilliseconds;

                return true;
            }
            catch (TimeoutException)
            {
                latencyMs = -1;
                timeDrift = MinusOne;
                return false;
            }
            catch (Exception)
            {
                latencyMs = 0;
                timeDrift = MinusOne;
                return false;
            }
        }
    }
}
