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
                    _ = DateTime.TryParse(args.Headers["X-GeneratedOn"], out servertime) || DateTime.TryParse(args.Headers["Date"], out servertime);
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
        }
    }
}
