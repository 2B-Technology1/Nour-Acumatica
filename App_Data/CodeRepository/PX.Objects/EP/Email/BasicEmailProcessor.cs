/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public abstract class BasicEmailProcessor : IEmailProcessor
	{
		protected sealed class Package
		{
			private readonly PXGraph _graph;
			private readonly EMailAccount _account;
			private readonly CRSMEmail _message;
			private string _address;
			private string _description;
			private bool _isProcessed;

			private Package(PXGraph graph, EMailAccount account, CRSMEmail message)
			{
				if (graph == null) throw new ArgumentNullException("graph");
				if (account == null) throw new ArgumentNullException("account");
				if (message == null) throw new ArgumentNullException("message");

				_graph = graph;
				_account = account;
				_message = message;
			}

			public PXGraph Graph
			{
				get { return _graph; }
			}

			public CRSMEmail Message
			{
				get { return _message; }
			}

			public string Address
			{
				get { return _address; }
			}

			public string Description
			{
				get { return _description; }
			}

			public EMailAccount Account
			{
				get { return _account; }
			}

			public bool IsProcessed
			{
				get { return _isProcessed; }
			}

			public static Package Extract(EmailProcessEventArgs e)
			{
				var graph = e.Graph;
				var account = e.Account;
				var message = e.Message;

				string address;
				string description;
				if (!Mailbox.TryParse(message.MailFrom, out address, out description) ||
					address == null || address.Trim().Length == 0)
				{
					return null;
				}
				return new Package(graph, account, message)
						{
							_address = address,
							_description = description,
							_isProcessed = e.IsSuccessful
						};
			}
		}

		public void Process(EmailProcessEventArgs e)
		{
			var package = Package.Extract(e);
			e.IsSuccessful = package != null && Process(package);
		}

		protected abstract bool Process(Package package);

		protected void PersistRecord(Package package, object record)
		{
			if (record == null) return;

			var cache = package.Graph.Caches[record.GetType()];
			var status = cache.GetStatus(record);
			try
			{
				using(var ts = new PXTransactionScope())
				{
					switch (status)
					{
						case PXEntryStatus.Updated:
							cache.Persist(record, PXDBOperation.Update);
							break;
						case PXEntryStatus.Inserted:
							cache.Persist(record, PXDBOperation.Insert);
							break;
						case PXEntryStatus.Deleted:
							cache.Persist(record, PXDBOperation.Delete);
							break;
						default:
							throw new InvalidOperationException();
					}
					ts.Complete();
				}
			}
			catch (Exception)
			{
				cache.Remove(record);
				throw;
			}
		}
	}
}
