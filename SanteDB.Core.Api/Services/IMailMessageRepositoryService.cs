/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-7-31
 */
using SanteDB.Core.Mail;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{
	/// <summary>
	/// Represents an alerting service.
	/// </summary>
	public interface IMailMessageRepositoryService
	{
		/// <summary>
		/// Fired when an alert is received.
		/// </summary>
		event EventHandler<MailMessageEventArgs> Committed;

		/// <summary>
		/// Fired when an alert was raised and is being processed.
		/// </summary>
		event EventHandler<MailMessageEventArgs> Received;

		/// <summary>
		/// Broadcasts an alert.
		/// </summary>
		/// <param name="message">The message.</param>
		void Broadcast(MailMessage message);

		/// <summary>
		/// Searches for alerts.
		/// </summary>
		/// <param name="predicate">The predicate to use to search for alerts.</param>
		/// <param name="offset">The offset of the search.</param>
		/// <param name="count">The count of the search results.</param>
		/// <param name="totalCount">The total count of the alerts.</param>
		/// <returns>Returns a list of alerts.</returns>
		IEnumerable<MailMessage> Find(Expression<Func<MailMessage, bool>> predicate, int offset, int? count, out int totalCount);

		/// <summary>
		/// Gets an alert.
		/// </summary>
		/// <param name="id">The id of the alert to be retrieved.</param>
		/// <returns>Returns an alert.</returns>
		MailMessage Get(Guid id);

		/// <summary>
		/// Inserts an alert message.
		/// </summary>
		/// <param name="message">The alert message to be inserted.</param>
		/// <returns>Returns the inserted alert.</returns>
		MailMessage Insert(MailMessage message);

		/// <summary>
		/// Saves an alert.
		/// </summary>
		/// <param name="message">The alert message to be saved.</param>
		/// <returns>Returns the saved alert.</returns>
		MailMessage Save(MailMessage message);
	}
}