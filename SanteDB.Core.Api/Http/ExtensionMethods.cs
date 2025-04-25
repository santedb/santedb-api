/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2025-2-3
 */
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Common extension methods 
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Determines if <paramref name="exception"/> represents an HTTP error condition
        /// </summary>
        /// <param name="exception">The exception which is to be checked</param>
        /// <param name="statusCode">The status code if the <paramref name="exception"/> is a <see cref="WebException"/></param>
        /// <returns>True if <paramref name="exception"/> is an <see cref="WebException"/></returns>
        public static bool IsHttpException(this Exception exception, out HttpStatusCode statusCode)
        {
            while (exception != null)
            {
                if (exception is WebException we && we.Response is HttpWebResponse hwr)
                {
                    statusCode = hwr.StatusCode;
                    return true;
                }
                exception = exception.InnerException;
            }
            statusCode = 0;
            return false;
        }

        /// <summary>
        /// Iterates through the exception causes and returns true if any exception in the hierarchy is a <see cref="TimeoutException"/>
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if any exception is an instance of <see cref="TimeoutException"/></returns>
        public static bool IsTimeoutException(this Exception exception)
        {
            while (exception != null)
            {
                if (exception is TimeoutException)
                {
                    return true;
                }

                exception = exception.InnerException;
            }
            return false;
        }

        /// <summary>
        /// Will iterate through the <paramref name="exception"/> and determine whether the exception was caused by a communication/network issue
        /// </summary>
        /// <returns>True if the exception was caused by a communication exception</returns>
        /// <remarks>
        /// We need to know if an exception was a business/application error (i.e. the message was sent to the server and the server rejected it) to place it into the 
        /// dead-letter queue. However, if the exception is merely an indication of a communication exception (timeout due to slow send, proxy error, etc.) then we 
        /// don't want to pollute the outbound queue, rather we want to keep it - pause for a period of time - and retry
        /// </remarks>
        public static bool IsCommunicationException(this Exception exception)
        {
            var isCommunicationException = false;
            while (exception != null)
            {
                isCommunicationException |= exception is SocketException ||  // Socket error
                    exception is WebException we && (we.Status != WebExceptionStatus.ProtocolError) || // Web exception with a non-protocol error
                    exception is NetworkInformationException;
                exception = exception.InnerException;
            }
            return isCommunicationException;
        }
    }
}
