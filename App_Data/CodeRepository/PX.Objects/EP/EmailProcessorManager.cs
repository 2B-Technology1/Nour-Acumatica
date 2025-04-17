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
using System.Collections.Generic;
using PX.Common.Service;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EmailProcessEventArgs
	{
		private readonly PXGraph _graph;
		private readonly EMailAccount _account;
		private readonly CRSMEmail _message;

		private bool _isSuccessful;

		public EmailProcessEventArgs(PXGraph graph, EMailAccount account, CRSMEmail message)
		{
			if (graph == null) throw new ArgumentNullException("graph");
			if (account == null) throw new ArgumentNullException("account");
			if (message == null) throw new ArgumentNullException("message");

			_graph = graph;
			_account = account;
			_message = message;
		}

		public CRSMEmail Message
		{
			get { return _message; }
		}

		public PXGraph Graph
		{
			get { return _graph; }
		}

		public EMailAccount Account
		{
			get { return _account; }
		}

		public bool IsSuccessful
		{
			get 
			{
				return _isSuccessful;
			}
			set 
			{
				_isSuccessful |= value;
			}
		}
	}

	public interface IEmailProcessor
	{
		void Process(EmailProcessEventArgs e);
	}
}
