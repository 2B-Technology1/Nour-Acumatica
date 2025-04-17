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
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.SM;
using PX.Objects.CS;
using PX.Api;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
	public sealed class NotificationUtility
	{
		private readonly PXGraph _graph;

		public NotificationUtility(PXGraph graph)
		{
			_graph = graph;
		}

		public Guid? SearchSetupID(string source, string notificationCD)
		{
			return SearchSetup(source,notificationCD)?.SetupID;
		}

		public NotificationSetup SearchSetup(string source, string notificationCD)
		{
			if (source == null || notificationCD == null) return null;
			NotificationSetup setup =
				PXSelect<NotificationSetup,
						Where<NotificationSetup.sourceCD, Equal<Required<NotificationSetup.sourceCD>>,
							And<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
							And < NotificationSetup.nBranchID, IsNull,
							And <NotificationSetup.active, Equal<True>>>>>>
					.SelectWindowed(_graph, 0, 1, source, notificationCD);
			return setup;
		}

		public NotificationSetup SearchSetup(string source, string notificationCD, int? branchID, Func<object> getRowFunc)
		{
			if (source == null || notificationCD == null || branchID == null)
				return SearchSetup(source, notificationCD);

			NotificationSetup setup =
				PXSelect<NotificationSetup,
						Where<NotificationSetup.sourceCD, Equal<Required<NotificationSetup.sourceCD>>,
							And<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
							And<NotificationSetup.nBranchID, Equal<Required<NotificationSetup.nBranchID>>,
							And<NotificationSetup.active, Equal<True>>>>>>
					.SelectWindowed(_graph, 0, 1, source, notificationCD, branchID);

			if (setup == null)
			{
				return SearchSetup(source, notificationCD);
			}
			else
			{
				object row = getRowFunc();
				NotificationSource notification = GetSource(source, row, (Guid)setup.SetupID, branchID);

				if (notification == null)
				{
					return SearchSetup(source, notificationCD);
				}
			}
			return setup;
		}

		/// <summary>
		/// Search for report ID in Notification settings for particular entity. To use if entity is not suported by <see cref="SearchReport{TSourceEntity}(string, int?, int?)"/>
		/// </summary>
		/// <typeparam name="TSourceEntity">Entity to search notifaction settings for</typeparam>
		/// <typeparam name="TNoteField">Note field of the entity</typeparam>
		/// <typeparam name="TIDField">ID field of the entity to search it by</typeparam>
		/// <param name="source">entity name to search it in NotificationSetup table (usualy equals to cluss name) </param>
		/// <param name="reportID">Default report ID</param>
		/// <param name="entityID">Value in column TIDField to search by</param>
		/// <param name="branchID">Branch to search by</param>
		/// <returns>Replacement ReportID specified in Notification settings</returns>
		public string SearchReport<TSourceEntity, TNoteField, TIDField>(string source, string reportID, int? entityID, int? branchID)
			where TSourceEntity : class, IBqlTable, new()
			where TNoteField : class, IBqlField
			where TIDField : class, IBqlField
		{
			NotificationSetup notificationSetup = GetSetup(source, reportID, branchID);
			if (notificationSetup == null) return reportID;

			IEnumerable<NotificationSource> notificationSources = PXSelectJoin<TSourceEntity,
				LeftJoin<NotificationSource,
				On<TNoteField, Equal<NotificationSource.refNoteID>>>,
				Where<TIDField, Equal<Required<TIDField>>, And<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>>>>
				.Select(_graph, entityID, notificationSetup.SetupID).RowCast<NotificationSource>();

			return ChooseReportIDFromNotificationSource(notificationSources, branchID, reportID);
		}

		/// <summary>
		/// Search for report ID in Notification settings for particular entity. To use if entity is not suported by <see cref="SearchReport{TSourceEntity}(string, int?, int?)"/>
		/// </summary>
		/// <typeparam name="TSourceEntity">Entity to search notifaction settings for</typeparam>
		/// <typeparam name="TNoteField">Note field of the entity</typeparam>
		/// <typeparam name="TIDField">ID field of the entity to search it by</typeparam>
		/// <typeparam name="TClassField">Class ID field of the entity (class is alternative sdource for the setting)</typeparam>
		/// <param name="source">entity name to search it in NotificationSetup table (usualy equals to cluss name) </param>
		/// <param name="reportID">Default report ID</param>
		/// <param name="entityID">Value in column TIDField to search by</param>
		/// <param name="branchID">Branch to search by</param>
		/// <returns>Replacement ReportID specified in Notification settings</returns>
		public string SearchReport<TSourceEntity, TNoteField, TIDField, TClassField>(string source, string reportID, int? entityID, int? branchID)
			where TSourceEntity : class, IBqlTable, new()
			where TNoteField : class, IBqlField
			where TIDField : class, IBqlField
			where TClassField : class, IBqlField
		{
			NotificationSetup notificationSetup = GetSetup(source, reportID, branchID);
			if (notificationSetup == null) return reportID;

			IEnumerable<NotificationSource> notificationSources = PXSelectJoin<TSourceEntity,
				LeftJoin<NotificationSource,
				On<TNoteField, Equal<NotificationSource.refNoteID>, Or<TClassField, Equal<NotificationSource.classID>>>>,
				Where<TIDField, Equal<Required<TIDField>>, And<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>>>>
				.Select(_graph, entityID, notificationSetup.SetupID).RowCast<NotificationSource>();

			return ChooseReportIDFromNotificationSource(notificationSources, branchID, reportID);
		}

		/// <summary>
		/// Search for report ID in Notification settings for particular entity
		/// </summary>
		/// <typeparam name="TSourceEntity">Entity to search notifaction settings for. Project, Baccount, Vendor and Customer are currently supported. Use other overloads for other classes</typeparam>
		/// <param name="entityID">Value in column TIDField to search by</param>
		/// <param name="branchID">Branch to search by</param>
		/// <returns>Replacement ReportID specified in Notification settings</returns>
		internal string SearchReport<TSourceEntity>(string reportID, int? entityID, int? branchID)
			where TSourceEntity : class, IBqlTable, new()
		{
			switch (typeof(TSourceEntity))
			{
				case Type t when t == typeof(PMProject):
					return SearchProjectReport(reportID, entityID, branchID);
				case Type t when t == typeof(Customer):
					return SearchCustomerReport(reportID, entityID, branchID);
				case Type t when t == typeof(Vendor):
					return SearchVendorReport(reportID, entityID, branchID);
				default: return SearchReport<TSourceEntity, BAccount.noteID, BAccount.bAccountID>(nameof(TSourceEntity), reportID, entityID, branchID);
			}
		}

		public string SearchVendorReport(string reportID, int? vendorID, int? branchID)
		{
			return SearchReport<Vendor, Vendor.noteID, Vendor.bAccountID, Vendor.vendorClassID>(nameof(Vendor), reportID, vendorID, branchID);
		}

		public string SearchCustomerReport(string reportID, int? customerID, int? branchID)
		{
			return SearchReport<AR.Light.Customer, AR.Light.Customer.noteID, AR.Light.Customer.bAccountID, AR.Light.Customer.customerClassID>(nameof(Customer), reportID, customerID, branchID);
		}

		public string SearchProjectReport(string reportID, int? projectID, int? branchID)
		{
			return SearchReport<PMProject, PMProject.noteID, PMProject.contractID>(PMNotificationSource.Project, reportID, projectID, branchID);
		}

		internal string ChooseReportIDFromNotificationSource(IEnumerable<NotificationSource> notificationSources, int? branchID, string originalReportID)
		{
			return ChooseNotificationSource(notificationSources, branchID)?.ReportID ?? originalReportID;
		}

		private NotificationSource ChooseNotificationSource(IEnumerable<NotificationSource> notificationSources, int? branchID)
		{
			NotificationSource foundNotification =
				//notification, specified for Customer and particular branch
				notificationSources.FirstOrDefault(_ => _.ClassID == null && _.NBranchID == branchID)
				//notification, specified for Customer for any branch
				?? notificationSources.FirstOrDefault(_ => _.ClassID == null && _.NBranchID == null)
				//notification, specified for Customer Class and particular branch
				?? notificationSources.FirstOrDefault(_ => _.ClassID != null && _.NBranchID == branchID)
				//notification, specified for Customer Class and any branch
				?? notificationSources.FirstOrDefault(_ => _.ClassID != null && _.NBranchID == null);
			return foundNotification;
		}

		public Guid? SearchPrinter(string source, string reportID, int? branchID)
		{
			NotificationSetupUserOverride userSetup =
				SelectFrom<NotificationSetupUserOverride>
				.InnerJoin<NotificationSetup>.On<NotificationSetupUserOverride.FK.DefaultSetup>
				.Where<NotificationSetupUserOverride.userID.IsEqual<AccessInfo.userID.FromCurrent>
					.And<NotificationSetupUserOverride.active.IsEqual<True>>
					.And<NotificationSetupUserOverride.shipVia.IsNull>
					.And<NotificationSetup.active.IsEqual<True>>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.Select(_graph, source, reportID, branchID);
			if (userSetup?.DefaultPrinterID != null)
				return userSetup.DefaultPrinterID;

			UserPreferences userPreferences = SelectFrom<UserPreferences>.Where<UserPreferences.userID.IsEqual<@P.AsGuid>>.View.Select(_graph, _graph.Accessinfo.UserID);
			if (userPreferences?.DefaultPrinterID != null)
				return userPreferences.DefaultPrinterID;

			if (source != null && reportID != null)
			{
				NotificationSetup setup = GetSetup(source, reportID, branchID);
				if (setup?.DefaultPrinterID != null)
					return setup.DefaultPrinterID;
			}

			GL.Branch branch = SelectFrom<GL.Branch>.Where<GL.Branch.branchID.IsEqual<@P.AsInt>>.View.Select(_graph, branchID ?? _graph.Accessinfo.BranchID);
			if (branch != null)
			{
				if (branch.DefaultPrinterID != null)
					return branch.DefaultPrinterID;

				GL.DAC.Organization organization = SelectFrom<GL.DAC.Organization>.Where<GL.DAC.Organization.organizationID.IsEqual<@P.AsInt>>.View.Select(_graph, branch.OrganizationID);
					if (organization != null)
						return organization.DefaultPrinterID;
				}

			return null;
		}

		public NotificationSetup GetSetup(string source, string reportID, int? branchID)
		{
			NotificationSetup setup =
				SelectFrom<NotificationSetup>
				.Where<NotificationSetup.active.IsEqual<True>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.shipVia.IsNull>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.SelectWindowed(_graph, 0, 1, source, reportID, branchID);
			return setup;
		}

		public NotificationSource GetSource(NotificationSetup setup)
		{
			return new NotificationSource
			{
				Active = setup.Active,
				EMailAccountID = setup.EMailAccountID,
				ReportID = setup.ReportID,
				NotificationID = setup.NotificationID,
				Format = setup.Format,
				SetupID = setup.SetupID
			};
		}

		public NotificationSource GetSource(string sourceType, object row, Guid setupID, int? branchID)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(sourceType, row);
			NavigateRow(graph, row);

			PXView notificationView = null;
			graph.Views.TryGetValue("NotificationSources", out notificationView);

			if (notificationView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof(NotificationSource).IsAssignableFrom(view.GetItemType())))
				{
					notificationView = view;
					break;
				}
			}
			if (notificationView == null) return null;
			NotificationSource result = null;
			foreach (NotificationSource rec in notificationView.SelectMulti().RowCast<NotificationSource>())
			{
				if (rec.SetupID == setupID && rec.NBranchID == branchID)
					return rec;
				if (rec.SetupID == setupID && rec.NBranchID == null)
					result = rec;
			}
			return result;
		}

		protected List<object> GetDefaultRecipients(PXCache cache, NotificationSource source)
		{
			List<object> recipient = new List<object>();

			foreach (NotificationSetupRecipient setupRecipient in
					PXSelect<NotificationSetupRecipient,
					Where<NotificationSetupRecipient.setupID, Equal<Required<NotificationSetupRecipient.setupID>>>>
					.Select(cache.Graph, source.SetupID))
			{
				try
				{
					NotificationRecipient rec = (NotificationRecipient)cache.CreateInstance();
					rec.SetupID = source.SetupID;
					rec.ContactType = setupRecipient.ContactType;
					rec.ContactID = setupRecipient.ContactID;
					rec.Active = setupRecipient.Active;
					rec.AddTo = setupRecipient.AddTo;
					rec.Format = setupRecipient.Format;

					recipient.Add(rec);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}

			return recipient;
		}

		public RecipientList GetRecipients(string type, object row, NotificationSource source)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(type, row);
			NavigateRow(graph, row);
			NavigateRow(graph, source, false);


			PXView recipientView;
			graph.Views.TryGetValue("NotificationRecipients", out recipientView);

			if (recipientView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof(NotificationRecipient).IsAssignableFrom(view.GetItemType())))
				{
					recipientView = view;
					break;
				}
			}
			if (recipientView != null)
			{
				RecipientList recipient = null;
				Dictionary<string, string> errors = new Dictionary<string, string>();
				int count = 0;
				var recipientList = recipientView.SelectMulti();
				if (recipientList.Any_() == false)
				{
					recipientList = GetDefaultRecipients(graph.Caches[typeof(NotificationRecipient)], source);
				}

				foreach (NotificationRecipient item in recipientList)
				{
					NavigateRow(graph, item, false);
					if (item.Active == true)
					{
						count++;
						if (string.IsNullOrWhiteSpace(item.Email))
						{
							string currEmail = ((NotificationRecipient)graph.Caches[typeof(NotificationRecipient)].Current).Email;
							if (string.IsNullOrWhiteSpace(currEmail))
							{
								Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Current<NotificationRecipient.contactID>>>>.SelectSingleBound(_graph, new object[] { item });
								NotificationContactType.ListAttribute list = new NotificationContactType.ListAttribute();
								StringBuilder display = new StringBuilder(list.ValueLabelDic[item.ContactType]);
								if (contact != null)
								{
									display.Append(" ");
									display.Append(contact.DisplayName);
								}
								errors.Add(count.ToString(CultureInfo.InvariantCulture), PXMessages.LocalizeFormatNoPrefix(Messages.EmptyEmail, display));
							}
							else
							{
								item.Email = currEmail;
							}
						}
						if (!string.IsNullOrWhiteSpace(item.Email))
						{
							if (recipient == null)
								recipient = new RecipientList();
							recipient.Add(item);
						}
					}
				}
				if (errors.Any())
				{
					NotificationSetup nsetup = PXSelect<NotificationSetup, Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>.SelectSingleBound(_graph, new object[] { source });
					throw new PXOuterException(errors, _graph.GetType(), row, Messages.InvalidRecipients, errors.Count, count, nsetup.NotificationCD, nsetup.Module);
				}
				else
				{
					return recipient;
				}
			}
			return null;
		}

		private PXGraph CreatePrimaryGraph(string source, object row)
		{
			Type graphType = null;
			if (source == ARNotificationSource.Customer)
				graphType = typeof(CustomerMaint);
			else if (source == APNotificationSource.Vendor)
			{
				graphType = typeof(VendorMaint);
				if (row != null)
				{
					PXCache cache = _graph.Caches[row.GetType()];
					if(cache.GetValue<BAccount.type>(row) as string == BAccountType.EmployeeType)
						graphType = typeof(EmployeeMaint);
				}
			}
			else if (source == DAC.CRNotificationSource.BAccount)
			{
				graphType = typeof(BusinessAccountMaint);
			}
			else
				graphType = new EntityHelper(_graph).GetPrimaryGraphType(row, false);

			if (graphType == null)
				throw new PXException(PX.SM.Messages.NotificationGraphNotFound);

			var res = graphType == _graph.GetType()
				? _graph
				: (PXGraph)PXGraph.CreateInstance(graphType);
			return res;
		}

		private static void NavigateRow(PXGraph graph, object row, bool primaryView = true)
		{
			Type type = row.GetType();
			PXCache primary = graph.Views[graph.PrimaryView].Cache;
			if (primary.GetItemType().IsAssignableFrom(row.GetType()))
			{
				graph.Caches[type].Current = row;
				graph.Caches[primary.GetItemType()].Current = row;
			}
			else if (row.GetType().IsAssignableFrom(primary.GetItemType()))
			{
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(row.GetType()), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
			else
			if (primaryView)
			{
				object[] searches = new object[primary.Keys.Count];
				string[] sortcolumns = new string[primary.Keys.Count];
				for (int i = 0; i < primary.Keys.Count; i++)
				{
					searches[i] = graph.Caches[type].GetValue(row, primary.Keys[i]);
					sortcolumns[i] = primary.Keys[i];
				}
				int startRow = 0, totalRows = 0;
				var list = graph.Views[graph.PrimaryView].Select(null, null, searches, sortcolumns,null, null, ref startRow, 1, ref totalRows);				
				graph.Views[graph.PrimaryView].Cache.Current = 
					(list != null && list.Count > 0)
					? PXResult.Unwrap(list[0], graph.Views[graph.PrimaryView].Cache.GetItemType())
					: null;				
			}
			else
			{
				primary = graph.Caches[type];
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(type), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
		}
	}
}


