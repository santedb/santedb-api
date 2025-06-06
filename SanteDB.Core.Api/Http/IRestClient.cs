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
using SanteDB.Core.Http.Description;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a RESTFul client which encapsulates some of the functions of the request
    /// </summary>
    public interface IRestClient : IDisposable, IReportProgressChanged
    {
        /// <summary>
        /// Gets or sets the credentials to be used for this client
        /// </summary>
        RestRequestCredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the format to accept data in
        /// </summary>
        /// <remarks>The <see cref="Description"/> is used for the default</remarks>
        String Accept { get; set; }

        /// <summary>
        /// Gets the specified item
        /// </summary>
        /// <typeparam name="TResult">The type of model item to retrieve</typeparam>
        TResult Get<TResult>(String url);


        /// <summary>
        /// Get the specified <typeparamref name="TResult"/> using <paramref name="query"/>
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        TResult Get<TResult>(String url, NameValueCollection query);

        /// <summary>
        /// Invokes the specified method against the URL provided
        /// </summary>
        /// <param name="method">The HTTP method wich should be executed</param>
        /// <param name="url">The URL which should be invoked</param>
        /// <param name="body">The contents of the body</param>
        /// <typeparam name="TBody">The type of object being submitted to the server</typeparam>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        TResult Invoke<TBody, TResult>(String method, String url, TBody body);

        /// <summary>
        /// Invokes the specified method against the URL provided
        /// </summary>
        /// <param name="method">The HTTP method wich should be executed</param>
        /// <param name="url">The URL which should be invoked</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The contents of the body</param>
        /// <typeparam name="TBody">The type of object being submitted to the server</typeparam>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        TResult Invoke<TBody, TResult>(String method, String url, String contentType, TBody body);

        /// <summary>
        /// Invokes the specified method against the url provided
        /// </summary>
        /// <param name="method">The HTTP method to invoke on th server</param>
        /// <param name="url">The URL which should be invoked</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The body of the object to be submitted</param>
        /// <param name="query">The query parmaeters to pass in the request</param>
        /// <typeparam name="TBody">Indicates the type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">Indicates the expected return type</typeparam>
        TResult Invoke<TBody, TResult>(String method, String url, String contentType, TBody body, NameValueCollection query);

        /// <summary>
        /// Instructs the server to perform a PATCH operation
        /// </summary>
        /// <typeparam name="TPatch">The type of patch being applied</typeparam>
        /// <param name="url">The path on which the patch should be applied</param>
        /// <param name="ifMatch">Target version/etag to patch</param>
        /// <param name="patch">The patch to apply</param>
        String Patch<TPatch>(string url, String ifMatch, TPatch patch);

        /// <summary>
        /// Instructs the server to perform a PATCH operation
        /// </summary>
        /// <typeparam name="TPatch">The type of patch being applied</typeparam>
        /// <param name="url">The path on which the patch should be applied</param>
        /// <param name="contentType">The content/type of th epatch</param>
        /// <param name="ifMatch">Target version/etag to patch</param>
        /// <param name="patch">The patch to apply</param>
        String Patch<TPatch>(string url, string contentType, String ifMatch, TPatch patch);

        /// <summary>
        /// Executes a post against the url
        /// </summary>
        /// <param name="url">The URL to which the data should be posted</param>
        /// <param name="body">The contents/object submitted to the server</param>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected result type</typeparam>
        TResult Post<TBody, TResult>(String url, TBody body);

        /// <summary>
        /// Executes a post against the url
        /// </summary>
        /// <param name="url">The URL to which the data should be posted</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The contents/object submitted to the server</param>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected result type</typeparam>
        TResult Post<TBody, TResult>(String url, String contentType, TBody body);

        /// <summary>
        /// Deletes the specified object
        /// </summary>
        /// <param name="url">The URL to execute the HTTP DELETE command against</param>
        /// <typeparam name="TResult">The type of result expected from the server</typeparam>
        TResult Delete<TResult>(String url);

        /// <summary>
        /// Executes a PUT (update) for the specified object
        /// </summary>
        /// <param name="url">The URL of the object to be updated</param>
        /// <param name="body">The actual object/resource being updated</param>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected return resource type from the server</typeparam>
        TResult Put<TBody, TResult>(String url, TBody body);

        /// <summary>
        /// Executes a PUT (update) for the specified object
        /// </summary>
        /// <param name="url">The URL of the object to be updated</param>
        /// <param name="contentType">The content/type of the <paramref name="body"/></param>
        /// <param name="body">The actual object/resource being updated</param>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected return resource type from the server</typeparam>
        TResult Put<TBody, TResult>(String url, String contentType, TBody body);

        /// <summary>
        /// Executes an Options against the URL
        /// </summary>
        /// <param name="url">The URL to execute HTTP OPTIONS against</param>
        /// <typeparam name="TResult">The expected return type from the server</typeparam>
        TResult Options<TResult>(String url);

        /// <summary>
        /// Executes a HEAD operation against the URL
        /// </summary>
        /// <param name="url">The name of the resource to perform a HEAD operation on</param>
        /// <returns>A key/value pair of the HTTP headers sent in response to the HEAD</returns>
        IDictionary<String, String> Head(String url);

        /// <summary>
        /// Executes a HEAD operation against the URL
        /// </summary>
        /// <param name="url">The name of the resource to perform a HEAD operation on</param>
        /// <param name="query">The queyr parameters to use onte HEAD</param>
        /// <returns>A key/value pair of the HTTP headers sent in response to the HEAD</returns>
        IDictionary<String, String> Head(String url, NameValueCollection query);

        /// <summary>
        /// Lock the specified resource
        /// </summary>
        /// <param name="url">The resource URL to lock</param>
        /// <typeparam name="TResult">The type of result expected from the server</typeparam>
        TResult Lock<TResult>(String url);

        /// <summary>
        /// Unlock the specified resource
        /// </summary>
        /// <param name="url">The resource URL to unlock</param>
        /// <typeparam name="TResult">The type of result expected from the server</typeparam>
        TResult Unlock<TResult>(String url);

        /// <summary>
        /// Perform a raw get
        /// </summary>
        /// <param name="url">The url to get from</param>
        /// <returns>The bytes retrieved from the service</returns>
        byte[] Get(String url);

        /// <summary>
        /// Perform a raw get
        /// </summary>
        /// <param name="url">The resource URL to execute the GET against</param>
        /// <param name="query">The query (as key=value) to include on the get</param>
        /// <returns>The raw bytestream response</returns>
        byte[] Get(String url, NameValueCollection query);

        /// <summary>
        /// Gets the service client description
        /// </summary>
        IRestClientDescription Description { get; }

        /// <summary>
        /// Fired prior to rest client invoking a method
        /// </summary>
        event EventHandler<RestRequestEventArgs> Requesting;

        /// <summary>
        /// Fired after the request has been finished
        /// </summary>
        event EventHandler<RestResponseEventArgs> Responded;

        /// <summary>
        /// Fired when the response is initiated
        /// </summary>
        event EventHandler<RestResponseEventArgs> Responding;
    }
}