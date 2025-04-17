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
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.Objects.EP;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Common.Mail;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Data.SQLTree;
using PX.Objects.CR.Workflows;
using PX.Objects.GDPR;
using PX.Objects.PM;
using PX.Reports;
using PX.TM;
using PX.Objects.AR.Standalone;

namespace PX.Objects.CR
{

	/// <summary>
	/// Represents an entity that provides the email address and display name
	/// to an <see cref="SMEmail">email</see> when the latter is opened on the form that contains this entity.
	/// <see cref="SMEmail.MailTo">SMEmail.MailTo</see> will be filled with the email address representation
	/// made by the <see cref="PXDBEmailAttribute.FormatAddressesWithSingleDisplayName"/> method
	/// on the basis of <see cref="Address"/> and <see cref="DisplayName"/>.
	/// </summary>
	public interface IEmailMessageTarget
	{
		/// <summary>
		/// Gets the display name of the entity.
		/// </summary>
		/// <value>
		/// The display name. Can be <see langword="null"/>.
		/// </value>
		string DisplayName { get; }

		/// <summary>
		/// Gets the email address of the entity.
		/// </summary>
		/// <value>
		/// The email address. If it is <see langword="null"/>,
		/// the <see cref="SMEmail.MailTo"/> property will not be defaulted.
		/// </value>
		string Address { get; }
	}

	/// <summary>
	/// Represents the graph extension that is used for Activities creation. Interface is used to
	/// get the type of the corresponding activities in the Activities grid. 
	/// </summary>
	public interface IActivityDetailsExt
	{
		PXView ActivitiesView { get; }

		int? DefaultEmailAccountID { get; set; }
		string DefaultActivityType { get; }
		string DefaultSubject { get; set; }
		void AdjustActivitiesView();
		void AttachEvents();
		IList<Type> GetAllActivityTypes();
		IEnumerable newMailActivity(PXAdapter adapter);
		void CreateNewActivityAndRedirect(int classID, string activityType);
		PXGraph CreateNewActivity(int classID, string activityType);
		void CreatePrimaryActivity(PXGraph targetGraph, int classID, string activityType);
		void CreateTimeActivity(PXGraph targetGraph, int classID, string activityType);
		void InitializeActivity(CRActivity row);
		Guid? GetRefNoteID();
		void InitializeEmail(CRSMEmail row);
		string GetMailReply(CRSMEmail message, string currentMailReply);
		string GetMailTo(CRSMEmail message);
		string GetMailCc(CRSMEmail message, Guid? refNoteId);
		string GetSubject(CRSMEmail message);
		string GetBody();
		void SendNotification(string sourceType, string notifications, int? branchID, IDictionary<string, string> parameters, bool massProcess = false, IList<Guid?> attachments = null);
		NotificationGenerator CreateNotificationProvider(string sourceType, IList<string> notificationCDs, int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null);
		string GetPrimaryRecipientFromContext(NotificationUtility utility, string type, object row, NotificationSource source);
		Type GetActivityType();
	}
}
