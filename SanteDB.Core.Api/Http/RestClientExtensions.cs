using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
            catch(Exception)
            {
                latencyMs = 0;
                timeDrift = MinusOne;
                return false;
            }
        }
    }
}
