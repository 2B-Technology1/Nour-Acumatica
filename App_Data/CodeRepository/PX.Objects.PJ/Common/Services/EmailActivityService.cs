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
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.PJ.Common.Services
{
	public abstract class EmailActivityService<TCache>
		where TCache : BaseCache, IProjectManagementDocumentBase, IBqlTable
	{
		protected readonly PXGraph Graph;
		protected TCache Entity;
		private readonly int? entityOwnerContactId;

		protected EmailActivityService(PXGraph graph, int? entityOwnerContactId)
		{
			Graph = graph;
			this.entityOwnerContactId = entityOwnerContactId;
		}

		protected PXGraph GetEmailActivityGraph<TNoteIdField>()
			where TNoteIdField : IBqlOperand, IBqlField
		{
			string recipientEmails = GetRecipientEmails();

			CreateEntityNoteIfNotExist<TNoteIdField>();

			var emailActivityGraph = CreateEmailActivityGraph(recipientEmails);

			return emailActivityGraph;
		}

		protected PXGraph CreateEmailActivityGraph(string recipientEmails = null)
		{
			var emailActivityGraph = PXGraph.CreateInstance<CREmailActivityMaint>();
			var email = GetEmailEntity(recipientEmails);
			emailActivityGraph.Caches<CRSMEmail>().Insert(email);
			return emailActivityGraph;
		}

		protected virtual CRSMEmail GetEmailEntity(string recipientEmails)
		{
			return new CRSMEmail
			{
				MailAccountID = GetOwnerIfExist(),
				Subject = GetSubject(),
				RefNoteIDType = GetEntityType(),
				RefNoteID = GetNoteID(),
				MailTo = recipientEmails
			};
		}

		public virtual string GetRecipientEmails()
		{
			return null;
		}

		protected int? GetOwnerIfExist()
		{
			var userEmail = GetUserEmail();
			var systemAccount = GetSystemAccount(userEmail);
			return systemAccount?.EmailAccountID;
		}

		protected string GetProjectNumber()
		{
			return new PXSelect<Contract,
					Where<Contract.contractID, Equal<Required<Contract.contractID>>>>(Graph)
				.SelectSingle(GetProjectID()).ContractCD;
		}

		protected abstract string GetSubject();

		protected virtual Guid? GetNoteID() => Entity.NoteID;

		protected virtual string GetEntityType() => Entity?.GetType().FullName;

		protected virtual int? GetProjectID() => Entity.ProjectId;

		private void CreateEntityNoteIfNotExist<TNoteIdField>()
			where TNoteIdField : IBqlField
		{
			var cache = Graph.GetPrimaryCache();
			PXNoteAttribute.GetNoteID<TNoteIdField>(cache, Entity);
			Graph.Actions.PressSave();
		}

		private string GetUserEmail()
		{
			if (entityOwnerContactId != null)
			{
				Guid? userID = PXAccess.GetUserID(entityOwnerContactId);
				if (userID != null)
				{
					return Users.PK.Find(Graph, userID)?.Email;
				}
			}
			return null;
		}

		private EMailAccount GetSystemAccount(string email)
		{
			return new PXSelect<EMailAccount,
				Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>(Graph).SelectSingle(email);
		}
	}
}
