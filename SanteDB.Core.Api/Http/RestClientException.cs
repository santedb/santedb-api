﻿/*
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
 * Date: 2023-6-21
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Net;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Rest client exception.
    /// </summary>
    public class RestClientException<TResult> : System.Net.WebException, IHasToDisplay
    {
        /// <summary>
        /// The result
        /// </summary>
        /// <value>The result.</value>
        public TResult Result
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the HTTP status of the result
        /// </summary>
        public HttpStatusCode HttpStatus
        {
            get;
        }

        /// <inheritdoc/>
        public String ToDisplay() => this.Result is IHasToDisplay ihd ? ihd.ToDisplay() : this.Result.ToString();

        /// <summary>
        /// Create a new rest client exception
        /// </summary>
        public RestClientException(String message, Exception inner) : this(default(TResult), inner, (inner as RestClientException<TResult>)?.Status ?? WebExceptionStatus.UnknownError, (inner as RestClientException<TResult>)?.Response)
        {
            if ((inner as RestClientException<TResult>) != null)
            {
                this.Result = (inner as RestClientException<TResult>).Result;
            }
        }

        /// <summary>
        /// Create the client exception
        /// </summary>
        public RestClientException(TResult result, Exception inner, WebExceptionStatus status, WebResponse response) : base(inner?.Message ?? ErrorMessages.COMMUNICATION_RESPONSE_FAILURE, null, status, response)
        {
            this.Result = result;
            if (response is HttpWebResponse hwr)
            {
                this.HttpStatus = hwr.StatusCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("[RestClientException: {0}, Status={2}, HttpResult={3}]\r\n--SERVER FAULT--\r\n{1}\r\n--END SERVER FAULT--\r\n-- STACK TRACE--\r\n{4}", this.Message, Result, this.Status, (this.Response as HttpWebResponse)?.StatusCode, this.StackTrace);
        }


    }
}