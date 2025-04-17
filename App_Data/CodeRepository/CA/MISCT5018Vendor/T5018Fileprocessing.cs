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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PX.Objects.Localizations.CA.Messages;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL.DAC;
using PX.SM;
using PX.Data.BQL.Fluent;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Localizations.CA.MISCT5018Vendor;
using System.Collections.Generic;
using PX.Objects.CS;
using System.Linq;
using PX.Common;
using PX.Data.BQL;

namespace PX.Objects.Localizations.CA {
	public class T5018Fileprocessing : PXGraph<T5018Fileprocessing>
	{
		public PXSave<T5018MasterTable> Save;

		public PXAction<T5018MasterTable> Cancel;

		public PXInsert<T5018MasterTable> Insert;

		public PXDelete<T5018MasterTable> Delete;

		public PXAction<T5018MasterTable> PrepareOriginal;

		public PXAction<T5018MasterTable> PrepareAmendment;

		public PXAction<T5018MasterTable> Validate;

		public PXAction<T5018MasterTable> Generate;

		public SelectFrom<T5018MasterTable>.View MasterView;

		public SelectFrom<T5018MasterTable>.
			Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.FromCurrent>.
			And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.FromCurrent>>.
			And<T5018MasterTable.revision.IsEqual<T5018MasterTable.revision.FromCurrent>>>.
			View MasterViewSummary;

		public SelectFrom<T5018EFileRow>.
			Where<T5018EFileRow.organizationID.IsEqual<T5018MasterTable.organizationID.FromCurrent>.
			And<T5018EFileRow.year.IsEqual<T5018MasterTable.year.FromCurrent>>.
			And<T5018EFileRow.revision.IsEqual<T5018MasterTable.revision.FromCurrent>>.
			And<T5018EFileRow.amount.IsGreaterEqual<T5018MasterTable.thresholdAmount.FromCurrent>>.
			And<T5018MasterTable.filingType.FromCurrent.IsEqual<T5018MasterTable.filingType.original>.
				Or<T5018EFileRow.amendmentRow.IsEqual<True>>>>.
			View DetailsView;

		public SelectFrom<APAdjustEFileRevision>.View Transactions;

		public T5018Fileprocessing()
		{
			//workaround because overriden Delete action does not format the message properly
			string prefix;
			string localizedMessage = PXMessages.Localize(ActionsMessages.ConfirmDeleteExplicit, out prefix);
			string ConfirmationMessage = String.Format(localizedMessage, MasterView.Cache.GetName());
			this.Delete.SetConfirmationMessage(ConfirmationMessage);
		}

		protected void _(Events.FieldUpdated<T5018MasterTable.organizationID> e)
		{
			T5018MasterTable row = e.Row as T5018MasterTable;
			FinYear[] currentYears =
					Select<FinYear>().
					Where
					(
						FY =>
						FY.OrganizationID == row.OrganizationID &&
						FY.StartDate <= Accessinfo.BusinessDate.Value
					).OrderByDescending
					(
						FY => FY.Year
					).Take(2).ToArray();

			FinYear defaultYear = currentYears.Count() > 1 ? currentYears[1] : currentYears[0];
			e.Cache.SetValueExt<T5018MasterTable.year>(e.Row, defaultYear.Year);
		}

		protected virtual void _(Events.FieldUpdated<T5018MasterTable.year> e)
		{
			T5018MasterTable row = e.Row as T5018MasterTable;
			if (string.IsNullOrEmpty(e.NewValue as string))
			{
				e.Cache.SetValue<T5018MasterTable.revision>(e.Cache.Current, "");
			}
			else
			{
				string latestRevision =
					SelectFrom<T5018MasterTable>.
					Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
					And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
					OrderBy<T5018MasterTable.createdDateTime.Desc>.
					View.ReadOnly.Select(this, new object[] { row.OrganizationID, e.NewValue }).TopFirst?.Revision;
				e.Cache.SetValueExt<T5018MasterTable.revision>(e.Row, latestRevision ?? T5018Messages.NewValue);
			}
			switch (GetReportingYear(row))
			{
				case T5018OrganizationSettings.t5018ReportingYear.CalendarYear:
					row.FromDate = Int32.TryParse(row.Year, out int year) ? new DateTime?(new DateTime(year, 1, 1)) : null;
					row.ToDate = Int32.TryParse(row.Year, out year) ? new DateTime?(new DateTime(year, 12, 31)) : null;
					break;

				case T5018OrganizationSettings.t5018ReportingYear.FiscalYear:
				default:
					FinYear finYear = FinYear.PK.Find(this, row.OrganizationID, row.Year);
					row.FromDate = finYear?.StartDate;
					row.ToDate = finYear?.EndDate - TimeSpan.FromDays(1);
					break;
			}
		}

		#region Key fields defaulting

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.organizationID> e)
		{
			e.NewValue = PXAccess.GetParentOrganizationID(PXAccess.GetBranchID());
			e.Cancel = true;
		}

		protected void _(Events.FieldDefaulting<T5018MasterTable.year> e)
		{
			T5018MasterTable row = e.Row as T5018MasterTable;
			if (row == null) return;
			switch (GetReportingYear(row))
			{
				case T5018OrganizationSettings.t5018ReportingYear.CalendarYear:
					e.NewValue = Accessinfo.BusinessDate.Value.AddYears(-1).Year.ToString();
					break;

				case T5018OrganizationSettings.t5018ReportingYear.FiscalYear:
				default:
					var currentYears = SelectFrom<FinYear>.
					Where<FinYear.organizationID.IsEqual<FinYear.organizationID.AsOptional>.
					And<FinYear.startDate.IsLessEqual<FinYear.startDate.AsOptional>>>.
					OrderBy<FinYear.endDate.Desc>.View.ReadOnly.Select(this, row.OrganizationID, Accessinfo.BusinessDate.Value);

					FinYear defaultYear = currentYears.Count() > 1 ? currentYears[1] : currentYears[0];

					e.NewValue = defaultYear.Year;
					break;
			}

			e.Cancel = true;
		}

		#endregion

		#region Summary Fields Defaulting

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.filingType> e)
		{
			T5018MasterTable table =
				SelectFrom<T5018MasterTable>.
				Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
				And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
				View.Select(this, (e.Row as T5018MasterTable)?.OrganizationID, (e.Row as T5018MasterTable)?.Year);

			e.NewValue = table == null ? T5018MasterTable.filingType.Original : T5018MasterTable.filingType.Amendment;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.addressLine1> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			e.NewValue = address?.AddressLine1;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.addressLine2> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			e.NewValue = address?.AddressLine2;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.city> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			e.NewValue = address?.City;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.province> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			e.NewValue = address?.State;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.country> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			switch (address?.CountryID)
			{
				case "CA":
					e.NewValue = "CAN";
					break;

				case "US":
					e.NewValue = "USA";
					break;

				default:
					e.NewValue = "";
					break;
			}
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.postalCode> e)
		{
			Organization organization = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			Address address =
					SelectFrom<Address>.
					InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>.
					Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, new object[] { organization?.BAccountID });

			e.NewValue = address?.PostalCode;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.acctName> e) => e.NewValue = OrganizationMaint.FindOrganizationByID(this, ((T5018MasterTable)e.Row)?.OrganizationID)?.OrganizationName;

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.programNumber> e)
		{
			T5018OrganizationSettings t5018OrganizationSettings = T5018OrganizationSettings.PK.Find(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			e.NewValue = t5018OrganizationSettings?.ProgramNumber;
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.transmitterNumber> e)
		{
			T5018OrganizationSettings t5018OrganizationSettings = T5018OrganizationSettings.PK.Find(this, ((T5018MasterTable)e.Row)?.OrganizationID);
			e.NewValue = t5018OrganizationSettings?.TransmitterNumber ?? "MM555555";
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.name> e) => e.NewValue = OrganizationMaint.GetDefaultContact(this, ((T5018MasterTable)e.Row)?.OrganizationID)?.Attention;

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.areaCode> e) => e.NewValue = PhoneNumberMatch(((T5018MasterTable)e.Row)?.OrganizationID)?.Groups[1].Value.Replace("(", "").Replace(")", "");

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.phone> e)
		{
			Match match = PhoneNumberMatch(((T5018MasterTable)e.Row)?.OrganizationID);
			if (match.Success)
			{
				e.NewValue = match.Groups[2].Value.Contains("-") ? match.Groups[2].Value : match.Groups[2].Value.Substring(0, 3) + "-" + match.Groups[2].Value.Substring(3);
			}
			else
			{
				e.NewValue = "";
			}
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.extensionNbr> e)
		{
			Match match = PhoneNumberMatch(((T5018MasterTable)e.Row)?.OrganizationID);
			if (match.Success)
				e.NewValue = match.Groups[3].Value;
			else
				e.NewValue = "";
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.email> e) => e.NewValue = OrganizationMaint.GetDefaultContact(this, ((T5018MasterTable)e.Row)?.OrganizationID)?.EMail;

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.fromDate> e)
		{
			T5018MasterTable row = e.Row as T5018MasterTable;

			switch (GetReportingYear(row))
			{
				case T5018OrganizationSettings.t5018ReportingYear.CalendarYear:
					e.NewValue = Int32.TryParse(row.Year, out int year) ? new DateTime?(new DateTime(year, 1, 1)) : null;
					break;

				case T5018OrganizationSettings.t5018ReportingYear.FiscalYear:
				default:
					FinYear finYear = FinYear.PK.Find(this, row.OrganizationID, row.Year);
					e.NewValue = finYear?.StartDate;
					break;
			}
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.toDate> e)
		{
			T5018MasterTable row = e.Row as T5018MasterTable;

			switch (GetReportingYear(row))
			{
				case T5018OrganizationSettings.t5018ReportingYear.CalendarYear:
					e.NewValue = Int32.TryParse(row.Year, out int year) ? new DateTime?(new DateTime(year, 12, 31)) : null;
					break;

				case T5018OrganizationSettings.t5018ReportingYear.FiscalYear:
				default:
					FinYear finYear = FinYear.PK.Find(this, row.OrganizationID, row.Year);
					e.NewValue = finYear?.EndDate - TimeSpan.FromDays(1);
					break;
			}
		}

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.revisionSubmitted> e) => e.NewValue = false;

		protected virtual void _(Events.FieldDefaulting<T5018MasterTable.language> e) => e.NewValue = T5018MasterTable.language.English;

		#endregion

		protected virtual void _(Events.RowInserting<T5018MasterTable> e)
		{
			e.Cache.SetValue<T5018MasterTable.revision>(e.Row, PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));
		}

		protected virtual void _(Events.RowPersisting<T5018MasterTable> e)
		{
			if (e.Operation != PXDBOperation.Delete && e.Row != MasterView.Current)
			{
				e.Cancel = true;
				return;
			}

			T5018MasterTable row = e.Row;

			if (String.Equals(row.Revision, T5018Messages.NewValue))
			{

				string latestRevision = SelectFrom<T5018MasterTable>.
									Where<
									T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.And<
									T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
									Order<By<T5018MasterTable.createdDateTime.Desc>>.View.ReadOnly.Select(this, new object[] { row.OrganizationID, row.Year })?.TopFirst?.Revision;

				if (String.IsNullOrEmpty(latestRevision))
					latestRevision = "0";

				latestRevision = (Int32.Parse(latestRevision) + 1).ToString();

				row.Revision = latestRevision;
				e.Cache.Normalize();

				string NewSubmissionNumber = "";
				NewSubmissionNumber += row.OrganizationID.ToString().PadLeft(2).Replace(" ", "0");
				NewSubmissionNumber += row.Year;
				NewSubmissionNumber += row.Revision.PadLeft(2).Replace(" ", "0");
				e.Row.SubmissionNo = NewSubmissionNumber;
			}
		}

		protected virtual void _(Events.RowPersisted<T5018MasterTable> e)
		{
			if (e.TranStatus == PXTranStatus.Aborted)
			{
				T5018MasterTable latestRevision =
					SelectFrom<T5018MasterTable>.Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.FromCurrent>.
					And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.FromCurrent>>>.
					Order<By<T5018MasterTable.createdDateTime.Desc>>.
					View.ReadOnly.Select(this).TopFirst;

				MasterView.Cache.Current = latestRevision ?? MasterView.Cache.Insert();
			}
		}

		protected void _(Events.RowSelected<T5018MasterTable> e)
		{
			Insert.SetVisible(false);
			T5018MasterTable row = e.Row;
			if (row == null) return;

			if (!row.OrganizationID.HasValue || String.IsNullOrEmpty(row.Revision) || String.IsNullOrEmpty(row.Year))
			{
				Validate.SetEnabled(false);
				Generate.SetEnabled(false);
				PrepareAmendment.SetEnabled(false);
				PrepareOriginal.SetEnabled(false);
				Delete.SetEnabled(false);
				return;
			}

			T5018MasterTable t5018MasterTable =
				SelectFrom<T5018MasterTable>.
				Where<T5018MasterTable.organizationID.
				IsEqual<T5018MasterTable.organizationID.FromCurrent>.
				And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.FromCurrent>>>.View.ReadOnly.Select(this);

			PrepareAmendment.SetVisible(t5018MasterTable != null);
			PrepareOriginal.SetVisible(t5018MasterTable == null);

			Save.SetEnabled(row.Revision != null && row.Revision != PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));
			Delete.SetEnabled(row.Revision != null && row.Revision != PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));
			Validate.SetEnabled(row.Revision != null && row.Revision != PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));
			Generate.SetEnabled(row.Revision != null && row.Revision != PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));

			T5018MasterTable table =
				SelectFrom<T5018MasterTable>.
				Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.FromCurrent>.
				And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.FromCurrent>>.
				And<T5018MasterTable.createdDateTime.IsGreater<T5018MasterTable.createdDateTime.FromCurrent>>>.View.ReadOnly.Select(this);

			PXUIFieldAttribute.SetEnabled<T5018MasterTable.revisionSubmitted>(e.Cache, row, row.Revision != null && row.Revision != PXMessages.LocalizeNoPrefix(T5018Messages.NewValue) && (table == null || !row.RevisionSubmitted.Value));
			PrepareOriginal.SetEnabled(row.Revision != null && row.Revision == PXMessages.LocalizeNoPrefix(T5018Messages.NewValue));
		}

		protected void _(Events.RowDeleting<T5018MasterTable> e)
		{
			if (e.Row == null || !e.Row.OrganizationID.HasValue || String.IsNullOrEmpty(e.Row.Revision) || String.IsNullOrEmpty(e.Row.Year))
			{
				e.Cancel = true;
				return;
			}

			T5018MasterTable latestRevision =
					SelectFrom<T5018MasterTable>.Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
					And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
					Order<By<T5018MasterTable.createdDateTime.Desc>>.
					View.ReadOnly.Select(this, e.Row.OrganizationID, e.Row.Year).TopFirst;

			if (latestRevision == null || !e.Row.Revision.Trim().Equals(latestRevision.Revision.Trim()))
			{
				e.Cancel = true;
				throw new PXException(T5018Messages.NotLatestRevision);
			}
		}

		/// <summary>
		/// Greps a phone number into it's applicable sections
		/// </summary>
		/// <returns>Regex Match collection</returns>
		private Match PhoneNumberMatch(int? id)
		{
			if (id == null) return null;
			string phonePattern = @"^\+?1?\(?(\d{3})\)?[\s\-]?(\d{3}[\s\-]?\d{4})(.*)$";
			Regex phoneRegex = new Regex(phonePattern);
			return phoneRegex.Match(OrganizationMaint.GetDefaultContact(this, id)?.Phone1 ?? "");
		}

		private int? GetReportingYear(T5018MasterTable table)
		{
			T5018OrganizationSettings t5018OrganizationSettings = T5018OrganizationSettings.PK.Find(this, table?.OrganizationID);
			return t5018OrganizationSettings?.T5018ReportingYear;
		}

		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXDeleteButton()]
		public virtual IEnumerable delete(PXAdapter a)
		{
			//after delete we should show the last existing revision for current organization and current year
			T5018MasterTable current = a.View.Cache.Current as T5018MasterTable;
			object res = null;
			foreach (T5018MasterTable header in new PXDelete<T5018MasterTable>(this, "Delete").Press(a))
			{
				res = header;
			}
			//For Revision equal to "1" and inserted record, we don't need to change the behavior
			if (current != null)
			{
				int? orgId = current.OrganizationID;
				string year = current.Year;
				if (!String.Equals(current.Revision, "1")
					&& !String.Equals(current.Revision, PXMessages.LocalizeNoPrefix(T5018Messages.NewValue)))
				{
					T5018MasterTable latestRevision;
					latestRevision = SelectFrom<T5018MasterTable>.
						Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
						And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
						Order<By<T5018MasterTable.createdDateTime.Desc>>.
						View.ReadOnly.Select(this, new object[] { orgId, year })?.TopFirst;

					if (latestRevision != null)
					{
						yield return latestRevision;
						yield break;
					}
					yield return current;
					yield break;
				}
			}
			yield return res;
		}

		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable cancel(PXAdapter a)
		{
			T5018MasterTable current = null;
			string organizationCD = null;
			string year = null;
			string revision = null;
			bool cancelOnInsert = false;

			if (a.Searches != null)
			{
				if (a.Searches.Length > 0)
					organizationCD = (string)a.Searches[0];
				if (a.Searches.Length > 1)
					year = (string)a.Searches[1];
				if (a.Searches.Length > 2)
					revision = (string)a.Searches[2];
			}

			Organization org = OrganizationMaint.FindOrganizationByCD(this, organizationCD);
			FinYear finYear = null;

			if (org == null)
			{
				if (a.Searches != null)
				{
					if (a.Searches.Length > 1)
					{
						a.Searches[1] = null;
						year = null;
					}
					if (a.Searches.Length > 2)
						a.Searches[2] = null;
				}
			}
			else
				finYear = FinYear.PK.Find(this, org.OrganizationID, year);

			if (finYear == null && a.Searches != null && a.Searches.Length > 2)
			{
				a.Searches[2] = null;
				year = null;
			}

			if (org != null && finYear != null
				&& String.Equals(revision, PXMessages.LocalizeNoPrefix(T5018Messages.NewValue)))
			{
				cancelOnInsert = true;
			}

			T5018MasterTable t5018MasterTable = null;

			if (org != null)
				t5018MasterTable = SelectFrom<T5018MasterTable>.
					Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
					And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>.
					And<T5018MasterTable.revision.IsEqual<T5018MasterTable.revision.AsOptional>>>.
					View.ReadOnly.Select(this, new object[] { org.OrganizationID, year, revision });
			bool insertNewRevision = false;
			if (t5018MasterTable == null)
			{
				if (a.Searches != null && a.Searches.Length > 2)
					a.Searches[2] = null;
				if (year != null)
					insertNewRevision = true;
			}

			if (year != null && (MasterView.Current != null && (!MasterView.Current.Year.Equals(year)) || cancelOnInsert))
			{
				T5018MasterTable latestRevision;
				latestRevision = SelectFrom<T5018MasterTable>.
					Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
					And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>>.
					Order<By<T5018MasterTable.createdDateTime.Desc>>.
					View.ReadOnly.Select(this, new object[] { org.OrganizationID, year })?.TopFirst;

				if (latestRevision != null && a.Searches.Length > 2)
				{
					a.Searches[2] = latestRevision.Revision;
					insertNewRevision = false;
				}
			}

			foreach (T5018MasterTable headerCanceled in (new PXCancel<T5018MasterTable>(this, "Cancel").Press(a)))
			{
				current = headerCanceled;
			}

			if (insertNewRevision)
			{
				MasterView.Cache.Remove(current);

				T5018MasterTable newRevision = new T5018MasterTable();
				newRevision.OrganizationID = org?.OrganizationID;
				newRevision.Year = year;

				current = MasterView.Insert(newRevision);

				current.Revision = PXMessages.LocalizeNoPrefix(T5018Messages.NewValue);

				MasterView.Cache.Normalize();
			}
			else
				MasterView.Cache.IsDirty = false;

			yield return current;
		}

		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = T5018Messages.ReportButton)]
		public virtual IEnumerable validate(PXAdapter a)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			string reportID = "AP607600";

			PXReportRequiredException ex = null;
			Dictionary<PrintSettings, PXReportRequiredException> reportsToPrint =
				new Dictionary<PrintSettings, PXReportRequiredException>();

			parameters["Transmitter"] = a.Searches[0] as string;
			parameters["T5018Year"] = a.Searches[1] as string;
			parameters["Revision"] = a.Searches[2] as string;

			ex = PXReportRequiredException.CombineReport(ex, reportID, parameters);
			ex.Mode = PXBaseRedirectException.WindowMode.New;

			if (ex != null)
			{
				throw ex;
			}

			return a.Get<T5018MasterTable>();
		}

		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = T5018Messages.GenerateButton)]
		public void generate()
		{
			if (DetailsView.Select().Count == 0)
				return;

			decimal? num = default(decimal);

			T5018OrganizationSettings t5018OrganizationSettings = T5018OrganizationSettings.PK.Find(this, (MasterView.Current)?.OrganizationID);

			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "");
			XmlElement xmlElement = xmlDocument.CreateElement("Submission");
			xmlElement.SetAttribute("xmlns:ccms", "http://www.cra-arc.gc.ca/xmlns/ccms/1-0-0");
			xmlElement.SetAttribute("xmlns:sdt", "http://www.cra-arc.gc.ca/xmlns/sdt/2-2-0");
			xmlElement.SetAttribute("xmlns:ols",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols/1-0-1");
			xmlElement.SetAttribute("xmlns:ols1",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols1/1-0-1");
			xmlElement.SetAttribute("xmlns:ols10",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols10/1-0-1");
			xmlElement.SetAttribute("xmlns:ols100",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols100/1-0-1");
			xmlElement.SetAttribute("xmlns:ols12",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols12/1-0-1");
			xmlElement.SetAttribute("xmlns:ols125",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols125/1-0-1");
			xmlElement.SetAttribute("xmlns:ols140",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols140/1-0-1");
			xmlElement.SetAttribute("xmlns:ols141",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols141/1-0-1");
			xmlElement.SetAttribute("xmlns:ols2",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols2/1-0-1");
			xmlElement.SetAttribute("xmlns:ols5",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols5/1-0-1");
			xmlElement.SetAttribute("xmlns:ols50",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols50/1-0-1");
			xmlElement.SetAttribute("xmlns:ols52",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols52/1-0-1");
			xmlElement.SetAttribute("xmlns:ols6",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols6/1-0-1");
			xmlElement.SetAttribute("xmlns:ols8",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols8/1-0-1");
			xmlElement.SetAttribute("xmlns:ols8-1",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols8-1/1-0-1");
			xmlElement.SetAttribute("xmlns:ols9",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/ols9/1-0-1");
			xmlElement.SetAttribute("xmlns:olsbr",
									"http://www.cra-arc.gc.ca/enov/ol/interfaces/efile/partnership/olsbr/1-0-1");
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			XmlAttribute xmlAttribute =
				xmlDocument.CreateAttribute("xsi", "noNamespaceSchemaLocation",
											"http://www.w3.org/2001/XMLSchema-instance");
			xmlAttribute.Value = "layout-topologie.xsd";
			xmlElement.SetAttributeNode(xmlAttribute);
			XmlNode xmlNode = xmlDocument.CreateElement("T619");
			XmlNode xmlNode2 = xmlDocument.CreateElement("sbmt_ref_id");
			xmlNode2.InnerText = MasterView.Current.SubmissionNo;
			xmlNode.AppendChild(xmlNode2);
			XmlNode xmlNode3 = xmlDocument.CreateElement("rpt_tcd");
			xmlNode3.InnerText = MasterView.Current.FilingType;
			xmlNode.AppendChild(xmlNode3);
			XmlNode xmlNode4 = xmlDocument.CreateElement("trnmtr_nbr");
			xmlNode4.InnerText = t5018OrganizationSettings?.TransmitterNumber;
			xmlNode.AppendChild(xmlNode4);
			XmlNode xmlNode5 = xmlDocument.CreateElement("trnmtr_tcd");
			xmlNode5.InnerText = "1";
			xmlNode.AppendChild(xmlNode5);
			XmlNode xmlNode6 = xmlDocument.CreateElement("summ_cnt");
			xmlNode6.InnerText = "1";
			xmlNode.AppendChild(xmlNode6);
			XmlNode xmlNode7 = xmlDocument.CreateElement("lang_cd");
			xmlNode7.InnerText = MasterView.Current.Language;
			xmlNode.AppendChild(xmlNode7);
			XmlNode xmlNode8 = xmlDocument.CreateElement("TRNMTR_NM");
			int num2 = 0;
			string text = null;
			string text2 = null;
			if (!string.IsNullOrEmpty(MasterView.Current.AcctName))
			{
				string acctName = MasterView.Current.AcctName;
				for (int i = 0; i < acctName.Length; i++)
				{
					char c = acctName[i];
					if (num2 <= 30)
					{
						text += c;
					}

					num2++;
					if (num2 > 31)
					{
						text2 += c;
					}
				}
			}

			XmlNode xmlNode9 = xmlDocument.CreateElement("l1_nm");
			xmlNode9.InnerText = text;
			xmlNode8.AppendChild(xmlNode9);
			if (num2 > 31)
			{
				XmlNode xmlNode10 = xmlDocument.CreateElement("l2_nm");
				xmlNode10.InnerText = text2;
				xmlNode8.AppendChild(xmlNode10);
			}

			xmlNode.AppendChild(xmlNode8);
			XmlNode xmlNode11 = xmlDocument.CreateElement("TRNMTR_ADDR");
			XmlNode xmlNode12 = xmlDocument.CreateElement("addr_l1_txt");
			xmlNode12.InnerText = MasterView.Current.AddressLine1;
			xmlNode11.AppendChild(xmlNode12);
			XmlNode xmlNode13 = xmlDocument.CreateElement("addr_l2_txt");
			xmlNode13.InnerText = MasterView.Current.AddressLine2;
			xmlNode11.AppendChild(xmlNode13);
			XmlNode xmlNode14 = xmlDocument.CreateElement("cty_nm");
			xmlNode14.InnerText = MasterView.Current.City;
			xmlNode11.AppendChild(xmlNode14);
			XmlNode xmlNode15 = xmlDocument.CreateElement("prov_cd");
			xmlNode15.InnerText = MasterView.Current.Province;
			xmlNode11.AppendChild(xmlNode15);
			XmlNode xmlNode16 = xmlDocument.CreateElement("cntry_cd");
			xmlNode16.InnerText = MasterView.Current.Country;
			xmlNode11.AppendChild(xmlNode16);
			XmlNode xmlNode17 = xmlDocument.CreateElement("pstl_cd");
			xmlNode17.InnerText = MasterView.Current.PostalCode;
			xmlNode11.AppendChild(xmlNode17);
			xmlNode.AppendChild(xmlNode11);
			XmlNode xmlNode18 = xmlDocument.CreateElement("CNTC");
			XmlNode xmlNode19 = xmlDocument.CreateElement("cntc_nm");
			xmlNode19.InnerText = MasterView.Current.Name;
			xmlNode18.AppendChild(xmlNode19);
			XmlNode xmlNode20 = xmlDocument.CreateElement("cntc_area_cd");
			xmlNode20.InnerText = MasterView.Current.AreaCode;
			xmlNode18.AppendChild(xmlNode20);
			XmlNode xmlNode21 = xmlDocument.CreateElement("cntc_phn_nbr");
			xmlNode21.InnerText = MasterView.Current.Phone;
			xmlNode18.AppendChild(xmlNode21);
			XmlNode xmlNode22 = xmlDocument.CreateElement("cntc_extn_nbr");
			xmlNode22.InnerText = MasterView.Current.ExtensionNbr;
			xmlNode18.AppendChild(xmlNode22);
			XmlNode xmlNode23 = xmlDocument.CreateElement("cntc_email_area");
			xmlNode23.InnerText = MasterView.Current.Email;
			xmlNode18.AppendChild(xmlNode23);
			XmlNode xmlNode24 = xmlDocument.CreateElement("sec_cntc_email_area");
			xmlNode24.InnerText = MasterView.Current.SecondEmail;
			xmlNode18.AppendChild(xmlNode24);
			xmlNode.AppendChild(xmlNode18);
			xmlElement.AppendChild(xmlNode);
			XmlNode xmlNode25 = xmlDocument.CreateElement("Return");
			XmlNode xmlNode26 = xmlDocument.CreateElement("T5018");
			xmlNode25.AppendChild(xmlNode26);
			int num3 = 0;
			foreach (T5018EFileRow item in DetailsView.Select())
			{
				BAccountR bAccount2 =
					SelectFrom<BAccountR>.
					Where<BAccountR.acctCD.IsEqual<BAccountR.acctCD.AsOptional>>.View
						.Select(this, item.VAcctCD.Trim());
				T5018VendorExt VendorExtension = PXCache<BAccount>.GetExtension<T5018VendorExt>(bAccount2);
				Contact contact2 =
					SelectFrom<Contact>.
					Where<Contact.contactID.IsEqual<BAccountR.primaryContactID.AsOptional>>.View
					.Select(this, bAccount2.PrimaryContactID);

				Address address2 =
					SelectFrom<Address>.
					Where<Address.bAccountID.IsEqual<BAccountR.bAccountID.AsOptional>>.View
						.Select(this, bAccount2.BAccountID);

				num += item.Amount;
				num3++;
				XmlNode xmlNode27 = xmlDocument.CreateElement("T5018Slip");
				xmlNode26.AppendChild(xmlNode27);

				XmlNode xmlNode33 = xmlDocument.CreateElement("sin");
				switch (VendorExtension.BoxT5018)
				{
					case T5018VendorExt.boxT5018.Corporation:
						xmlNode33.InnerText = "";
						break;
					case T5018VendorExt.boxT5018.Partnership:
						xmlNode33.InnerText = "";
						break;
					case T5018VendorExt.boxT5018.Individual:
						xmlNode33.InnerText = VendorExtension.SocialInsNum;
						break;
					default:
						xmlNode33.InnerText = "";
						break;
				}

				xmlNode27.AppendChild(xmlNode33);
				XmlNode xmlNode34 = xmlDocument.CreateElement("rcpnt_bn");
				if (VendorExtension.BusinessNumber != null)
				{
					xmlNode34.InnerText = VendorExtension.BusinessNumber;
				}
				else
				{
					xmlNode34.InnerText = "";
				}

				xmlNode27.AppendChild(xmlNode34);
				XmlNode xmlNode38 = xmlDocument.CreateElement("rcpnt_tcd");
				switch (VendorExtension.BoxT5018)
				{
					case T5018VendorExt.boxT5018.Corporation:
					case T5018VendorExt.boxT5018.Partnership:
						xmlNode38.InnerText = VendorExtension.BoxT5018 == T5018VendorExt.boxT5018.Corporation ? "3" : "4";
						XmlNode xmlNode35 = xmlDocument.CreateElement("CORP_PTNRP_NM");
						XmlNode xmlNode36 = xmlDocument.CreateElement("l1_nm");
						string corpName = item.VendorName.Replace("&", "&amp;");
						xmlNode36.InnerText = corpName.Length > 30 ? corpName.Substring(0, 30) : corpName;
						xmlNode35.AppendChild(xmlNode36);
						XmlNode xmlNode37 = xmlDocument.CreateElement("l2_nm");
						xmlNode37.InnerText = (corpName ?? "").Length > 30 ?
							(corpName.Substring(30).Length > 30 ?
								corpName.Substring(30, 30) :
								corpName.Substring(30)) :
							"";
						xmlNode35.AppendChild(xmlNode37);
						xmlNode27.AppendChild(xmlNode35);
						break;
					case T5018VendorExt.boxT5018.Individual:
						xmlNode38.InnerText = "1";
						XmlNode xmlNode28 = xmlDocument.CreateElement("RCPNT_NM");
						XmlNode xmlNode29 = xmlDocument.CreateElement("snm");
						XmlNode xmlNode30 = xmlDocument.CreateElement("gvn_nm");
						XmlNode xmlNode32 = xmlDocument.CreateElement("init");

						string givenName = contact2.FirstName != null ? contact2.FirstName.Split(' ')[0] : "";
						givenName = givenName.Length > 12 ? givenName.Substring(0, 12) : givenName;

						string surname = contact2.LastName ?? "";
						surname = surname.Length > 20 ? surname.Substring(0, 20) : surname;

						xmlNode29.InnerText = surname;
						xmlNode28.AppendChild(xmlNode29);

						xmlNode30.InnerText = givenName;
						xmlNode28.AppendChild(xmlNode30);

						xmlNode32.InnerText = "";
						xmlNode28.AppendChild(xmlNode32);
						xmlNode27.InsertBefore(xmlNode28, xmlNode33);
						break;
					default:
						xmlNode38.InnerText = "";
						break;
				}

				xmlNode27.AppendChild(xmlNode38);
				XmlNode xmlNode39 = xmlDocument.CreateElement("RCPNT_ADDR");
				XmlNode xmlNode40 = xmlDocument.CreateElement("addr_l1_txt");
				xmlNode40.InnerText = address2.AddressLine1;
				xmlNode39.AppendChild(xmlNode40);
				XmlNode xmlNode41 = xmlDocument.CreateElement("addr_l2_txt");
				if (address2.AddressLine2 != null)
				{
					xmlNode41.InnerText = address2.AddressLine2;
				}
				else
				{
					xmlNode41.InnerText = address2.AddressLine2;
				}

				xmlNode39.AppendChild(xmlNode41);
				XmlNode xmlNode42 = xmlDocument.CreateElement("cty_nm");
				xmlNode42.InnerText = address2.City;
				xmlNode39.AppendChild(xmlNode42);
				XmlNode xmlNode43 = xmlDocument.CreateElement("prov_cd");
				xmlNode43.InnerText = address2.State;
				xmlNode39.AppendChild(xmlNode43);
				XmlNode xmlNode44 = xmlDocument.CreateElement("cntry_cd");
				switch (address2?.CountryID)
				{
					case "CA":
						xmlNode44.InnerText = "CAN";
						break;

					case "US":
						xmlNode44.InnerText = "USA";
						break;

					default:
						xmlNode44.InnerText = "";
						break;
				}
				xmlNode39.AppendChild(xmlNode44);
				XmlNode xmlNode45 = xmlDocument.CreateElement("pstl_cd");
				xmlNode45.InnerText = address2.PostalCode;
				xmlNode39.AppendChild(xmlNode45);
				xmlNode27.AppendChild(xmlNode39);
				XmlNode xmlNode46 = xmlDocument.CreateElement("bn");
				xmlNode46.InnerText = item.TaxRegistrationID;
				xmlNode27.AppendChild(xmlNode46);
				XmlNode xmlNode47 = xmlDocument.CreateElement("sbctrcr_amt");
				decimal value = item.Amount.Value;
				xmlNode47.InnerText = Math.Round(value, 2).ToString();
				xmlNode27.AppendChild(xmlNode47);
				XmlNode xmlNode48 = xmlDocument.CreateElement("ptnrp_filr_id");
				switch (MasterView.Current.FilingType)
				{
					default:
						xmlNode48.InnerText = "";
						break;
				}

				xmlNode27.AppendChild(xmlNode48);
				XmlNode xmlNode49 = xmlDocument.CreateElement("rpt_tcd");
				xmlNode49.InnerText = MasterView.Current.FilingType;
				xmlNode27.AppendChild(xmlNode49);
				PXProcessing.SetProcessed();
			}

			XmlNode xmlNode50 = xmlDocument.CreateElement("T5018Summary");
			xmlNode26.AppendChild(xmlNode50);
			XmlNode xmlNode51 = xmlDocument.CreateElement("bn");
			xmlNode51.InnerText = MasterView.Current.ProgramNumber;
			xmlNode50.AppendChild(xmlNode51);
			XmlNode xmlNode52 = xmlDocument.CreateElement("PAYR_NM");
			XmlNode xmlNode53 = xmlDocument.CreateElement("l1_nm");
			xmlNode53.InnerText = text;
			xmlNode52.AppendChild(xmlNode53);
			XmlNode xmlNode54 = xmlDocument.CreateElement("l2_nm");
			xmlNode54.InnerText = text2;
			xmlNode52.AppendChild(xmlNode54);
			XmlNode xmlNode55 = xmlDocument.CreateElement("l3_nm");
			xmlNode55.InnerText = "";
			xmlNode52.AppendChild(xmlNode55);
			xmlNode50.AppendChild(xmlNode52);
			XmlNode xmlNode56 = xmlDocument.CreateElement("PAYR_ADDR");
			XmlNode xmlNode57 = xmlDocument.CreateElement("addr_l1_txt");
			xmlNode57.InnerText = MasterView.Current.AddressLine1;
			xmlNode56.AppendChild(xmlNode57);
			XmlNode xmlNode58 = xmlDocument.CreateElement("addr_l2_txt");
			xmlNode58.InnerText = MasterView.Current.AddressLine2;
			xmlNode56.AppendChild(xmlNode58);
			XmlNode xmlNode59 = xmlDocument.CreateElement("cty_nm");
			xmlNode59.InnerText = MasterView.Current.City;
			xmlNode56.AppendChild(xmlNode59);
			XmlNode xmlNode60 = xmlDocument.CreateElement("prov_cd");
			xmlNode60.InnerText = MasterView.Current.Province;
			xmlNode56.AppendChild(xmlNode60);
			XmlNode xmlNode61 = xmlDocument.CreateElement("cntry_cd");
			xmlNode61.InnerText = MasterView.Current.Country;
			xmlNode56.AppendChild(xmlNode61);
			XmlNode xmlNode62 = xmlDocument.CreateElement("pstl_cd");
			xmlNode62.InnerText = MasterView.Current.PostalCode;
			xmlNode56.AppendChild(xmlNode62);
			xmlNode50.AppendChild(xmlNode56);
			XmlNode xmlNode63 = xmlDocument.CreateElement("CNTC");
			XmlNode xmlNode64 = xmlDocument.CreateElement("cntc_nm");
			xmlNode64.InnerText = MasterView.Current.Name;
			xmlNode63.AppendChild(xmlNode64);
			XmlNode xmlNode65 = xmlDocument.CreateElement("cntc_area_cd");
			xmlNode65.InnerText = MasterView.Current.AreaCode;
			xmlNode63.AppendChild(xmlNode65);
			XmlNode xmlNode66 = xmlDocument.CreateElement("cntc_phn_nbr");
			xmlNode66.InnerText = MasterView.Current.Phone;
			xmlNode63.AppendChild(xmlNode66);
			XmlNode xmlNode67 = xmlDocument.CreateElement("cntc_extn_nbr");
			xmlNode67.InnerText = MasterView.Current.ExtensionNbr;
			xmlNode63.AppendChild(xmlNode67);
			xmlNode50.AppendChild(xmlNode63);
			XmlNode xmlNode68 = xmlDocument.CreateElement("PRD_END_DT");
			XmlNode xmlNode69 = xmlDocument.CreateElement("dy");

			DateTime dateTime = MasterView.Current.ToDate.Value;

			xmlNode69.InnerText = dateTime.Day.ToString();
			xmlNode68.AppendChild(xmlNode69);
			XmlNode xmlNode70 = xmlDocument.CreateElement("mo");
			xmlNode70.InnerText = dateTime.Month.ToString();
			xmlNode68.AppendChild(xmlNode70);
			XmlNode xmlNode71 = xmlDocument.CreateElement("yr");
			xmlNode71.InnerText = dateTime.Year.ToString();
			xmlNode68.AppendChild(xmlNode71);
			xmlNode50.AppendChild(xmlNode68);
			XmlNode xmlNode72 = xmlDocument.CreateElement("slp_cnt");
			xmlNode72.InnerText = num3.ToString();
			xmlNode50.AppendChild(xmlNode72);
			XmlNode xmlNode73 = xmlDocument.CreateElement("tot_sbctrcr_amt");
			decimal value2 = num.Value;
			xmlNode73.InnerText = Math.Round(value2, 2).ToString();
			xmlNode50.AppendChild(xmlNode73);
			XmlNode xmlNode74 = xmlDocument.CreateElement("rpt_tcd");
			xmlNode74.InnerText = MasterView.Current.FilingType;
			xmlNode50.AppendChild(xmlNode74);
			xmlElement.AppendChild(xmlNode25);
			xmlDocument.AppendChild(xmlElement);
			string text9 = DateTime.Now.ToString("MMddyy");
			string text10 = "T5018-" + text9 + "_R" + MasterView.Current.Revision;

			if (MasterView.Current.OrganizationID.HasValue && !string.IsNullOrEmpty(MasterView.Current.Year))
			{
				FileInfo fileInfo = new FileInfo(Guid.NewGuid(), text10 + ".xml", null,
												 Encoding.UTF8.GetBytes(xmlDocument.OuterXml));

				UploadFileMaintenance fileGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
				if (fileGraph.SaveFile(fileInfo, FileExistsAction.CreateVersion))
				{
					PXNoteAttribute.SetFileNotes(MasterView.Cache, MasterView.Current, fileInfo.UID.Value);
				}
			}
		}

		private void Prepare(T5018MasterTable table, bool amendment = false)
		{
			using (var ts = new PXTransactionScope())
			{
				List<int> branches = new List<int>();
				if (OrganizationMaint.FindOrganizationByID(this, table.OrganizationID).OrganizationType == OrganizationTypes.WithoutBranches)
					branches.Add(PXAccess.GetBranchByBAccountID(PXAccess.GetOrganizationBAccountID(table.OrganizationID)).BranchID);
				else
					branches.AddRange(PXAccess.GetChildBranchIDs(table.OrganizationID));

				PXResultset<T5018VendorTransaction> T5018VendorTransactions = new PXResultset<T5018VendorTransaction>();

				foreach (int branchID in branches)
				{
					T5018VendorTransactions.AddRange(
						SelectFrom<T5018VendorTransaction>.
							Where<T5018VendorTransaction.branchID.IsEqual<T5018VendorTransaction.branchID.AsOptional>.
							And<T5018VendorTransaction.adjdDocDate.IsGreaterEqual<T5018VendorTransaction.adjdDocDate.AsOptional>>.
							And<T5018VendorTransaction.adjdDocDate.IsLessEqual<T5018VendorTransaction.adjdDocDate.AsOptional>>>.
							AggregateTo<GroupBy<T5018VendorTransaction.vendorID>, Sum<T5018VendorTransaction.transactionAmt>>.
							View.Select(this, branchID, table.FromDate, table.ToDate));
				}

				string previousRevision =
						SelectFrom<T5018MasterTable>.
						Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
						And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.AsOptional>>.
						And<T5018MasterTable.createdDateTime.IsLess<T5018MasterTable.createdDateTime.AsOptional>>>.
						OrderBy<T5018MasterTable.createdDateTime.Desc>.View.ReadOnly.Select(this, table.OrganizationID, table.Year, table.CreatedDateTime).TopFirst?.Revision;

				foreach (T5018VendorTransaction T5018Trans in T5018VendorTransactions)
				{
					decimal? TranAmt = PXCurrencyAttribute.BaseRound(this, T5018Trans.TransactionAmt);

					string taxRegistrationID;
					if (T5018Trans.GetExtension<T5018VendorExt>().BoxT5018 == T5018VendorExt.boxT5018.Individual)
						taxRegistrationID = T5018Trans.GetExtension<T5018VendorExt>().SocialInsNum;
					else
						taxRegistrationID = T5018Trans.GetExtension<T5018VendorExt>().BusinessNumber;

					bool amendmentRow = false;

					if (amendment)
					{
						T5018EFileRow previousSubmissionRow =
							SelectFrom<T5018EFileRow>.
							Where<T5018EFileRow.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>.
							And<T5018EFileRow.year.IsEqual<T5018MasterTable.year.AsOptional>>.
							And<T5018EFileRow.revision.IsEqual<T5018MasterTable.revision.AsOptional>>.
							And<T5018EFileRow.bAccountID.IsEqual<T5018EFileRow.bAccountID.AsOptional>>>.View.Select(this, table.OrganizationID, table.Year, previousRevision, T5018Trans.BAccountID);

						amendmentRow = previousSubmissionRow == null || TranAmt != previousSubmissionRow.Amount;
					}

					if (TranAmt.HasValue)
					{
						T5018EFileRow newRecord = (T5018EFileRow)this.Caches[nameof(T5018EFileRow)].Insert(new T5018EFileRow
						{
							OrganizationID = table.OrganizationID,
							Amount = TranAmt,
							BAccountID = T5018Trans.BAccountID,
							Year = table.Year,
							Revision = table.Revision,
							VendorName = T5018Trans.AcctName,
							OrganizationName = OrganizationMaint.FindOrganizationByID(this, table.OrganizationID).OrganizationName,
							VAcctCD = T5018Trans.AcctCD,
							TaxRegistrationID = taxRegistrationID,
							AmendmentRow = amendmentRow
						});
					}
				}
				this.Save.Press();

				foreach (APAdjust tran in
						SelectFrom<APAdjust>.
						InnerJoin<T5018EFileRow>.
						On<APAdjust.vendorID.IsEqual<T5018EFileRow.bAccountID>>.
						Where<APAdjust.adjgBranchID.IsIn<@P.AsInt>.
						And<APAdjust.adjdDocType.IsNotEqual<APDocType.prepayment>>.
						And<APAdjust.adjdDocType.IsNotEqual<APDocType.debitAdj>>.
						And<APAdjust.released.IsEqual<True>>.
						And<APAdjust.adjdDocDate.IsGreaterEqual<T5018VendorTransaction.adjdDocDate.AsOptional>>.
						And<APAdjust.adjdDocDate.IsLessEqual<T5018VendorTransaction.adjdDocDate.AsOptional>>.
						And<T5018EFileRow.organizationID.IsEqual<T5018EFileRow.organizationID.AsOptional>>.
						And<T5018EFileRow.year.IsEqual<T5018EFileRow.year.AsOptional>>.
						And<T5018EFileRow.revision.IsEqual<T5018EFileRow.revision.AsOptional>>>.
						View.Select(this, branches, table.FromDate, table.ToDate, table.OrganizationID, table.Year, table.Revision))
				{
					APAdjustEFileRevision newTran = this.Transactions.Insert(new APAdjustEFileRevision
					{
						AdjgDocType = tran.AdjgDocType,
						AdjgRefNbr = tran.AdjgRefNbr,
						AdjdLineNbr = tran.AdjdLineNbr,
						AdjdDocType = tran.AdjdDocType,
						AdjdRefNbr = tran.AdjdRefNbr,
						AdjNbr = tran.AdjNbr,
						OrganizationID = table.OrganizationID,
						Year = table.Year,
						Revision = table.Revision
					});
				}
				this.Save.Press();
				ts.Complete();
			}
		}

		[PXButton]
		[PXUIField(DisplayName = "Prepare Report")]
		public virtual IEnumerable prepareOriginal(PXAdapter adapter)
		{
			List<int> branches = new List<int>();
			if (OrganizationMaint.FindOrganizationByID(this, MasterView.Current.OrganizationID).OrganizationType == OrganizationTypes.WithoutBranches)
				branches.Add(PXAccess.GetBranchByBAccountID(PXAccess.GetOrganizationBAccountID(MasterView.Current.OrganizationID)).BranchID);
			else
				branches.AddRange(PXAccess.GetChildBranchIDs(MasterView.Current.OrganizationID));

			APAdjust NewTransactions = null;
			foreach (int branchID in branches)
			{
				if (NewTransactions == null)
				{
					NewTransactions = SelectFrom<APAdjust>.
						InnerJoin<BAccountR>.
							On<APAdjust.vendorID.IsEqual<BAccountR.bAccountID>>.
						Where<APAdjust.adjgBranchID.IsEqual<Branch.branchID.AsOptional>.
							And<APAdjust.adjdDocType.IsNotEqual<APDocType.prepayment>>.
							And<APAdjust.adjdDocType.IsNotEqual<APDocType.debitAdj>>.
							And<APAdjust.released.IsEqual<True>>.
							And<APAdjust.voided.IsEqual<False>>.
							And<T5018VendorRExt.vendorT5018.IsEqual<True>>.
							And<APAdjust.adjdDocDate.IsGreaterEqual<T5018MasterTable.fromDate.AsOptional>>.
							And<APAdjust.adjdDocDate.IsLessEqual<T5018MasterTable.toDate.AsOptional>>>.
						View.Select(this, branchID, MasterView.Current.FromDate, MasterView.Current.ToDate).TopFirst;
				}
			}

			if (NewTransactions == null)
				throw new PXException(T5018Messages.NoNewRows);

			Save.Press();

			T5018MasterTable table = MasterView.Current;

			PXLongOperation.StartOperation(this, delegate
			{
				T5018Fileprocessing graph = CreateInstance<T5018Fileprocessing>();

				graph.Prepare(table);
			});

			yield return MasterView.Current;
		}

		[PXButton]
		[PXUIField(DisplayName = "Amend Report")]
		public virtual IEnumerable prepareAmendment(PXAdapter adapter)
		{
			Save.Press();
			T5018MasterTable preRevision = SelectFrom<T5018MasterTable>.
				Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.FromCurrent>.
				And<T5018MasterTable.year.IsEqual<T5018MasterTable.year.FromCurrent>>>.
				OrderBy<T5018MasterTable.createdDateTime.Desc>.View.ReadOnly.Select(this).TopFirst;

			if (preRevision == null || !preRevision.RevisionSubmitted.HasValue || !preRevision.RevisionSubmitted.Value)
				throw new PXException(T5018Messages.NoPreviousSubmissions);

			List<int> branches = new List<int>();
			if (OrganizationMaint.FindOrganizationByID(this, MasterView.Current.OrganizationID).OrganizationType == OrganizationTypes.WithoutBranches)
				branches.Add(PXAccess.GetBranchByBAccountID(PXAccess.GetOrganizationBAccountID(MasterView.Current.OrganizationID)).BranchID);
			else
				branches.AddRange(PXAccess.GetChildBranchIDs(MasterView.Current.OrganizationID));

			APAdjust NewTransactions = NewTransactions = SelectFrom<APAdjust>.
				InnerJoin<BAccountR>.
					On<APAdjust.vendorID.IsEqual<BAccountR.bAccountID>>.
				LeftJoin<APAdjustEFileRevision>.
					On<APAdjust.adjgDocType.IsEqual<APAdjustEFileRevision.adjgDocType>.
					And<APAdjust.adjgRefNbr.IsEqual<APAdjustEFileRevision.adjgRefNbr>>.
					And<APAdjust.adjNbr.IsEqual<APAdjustEFileRevision.adjNbr>>.
					And<APAdjust.adjdDocType.IsEqual<APAdjustEFileRevision.adjdDocType>>.
					And<APAdjust.adjdRefNbr.IsEqual<APAdjustEFileRevision.adjdRefNbr>>.
					And<APAdjust.adjdLineNbr.IsEqual<APAdjustEFileRevision.adjdLineNbr>>>.
				Where<APAdjust.adjdBranchID.IsEqual<Branch.branchID.AsOptional>.
					And<APAdjust.adjdDocType.IsNotEqual<APDocType.prepayment>>.
					And<APAdjust.adjdDocType.IsNotEqual<APDocType.debitAdj>>.
					And<T5018VendorRExt.vendorT5018.IsEqual<True>>.
					And<APAdjust.released.IsEqual<True>>.
					And<APAdjustEFileRevision.organizationID.IsNull>.
					And<APAdjust.adjdDocDate.IsGreaterEqual<APAdjust.adjdDocDate.AsOptional>>.
					And<APAdjust.adjdDocDate.IsLessEqual<APAdjust.adjdDocDate.AsOptional>>>.View.ReadOnly.Select(this,
				branches[0],
				MasterView.Current.FromDate,
				MasterView.Current.ToDate).TopFirst;

			for (int i = 1; i < branches.Count && NewTransactions == null; i++)
			{
				NewTransactions = NewTransactions = SelectFrom<APAdjust>.
					InnerJoin<BAccountR>.
						On<APAdjust.vendorID.IsEqual<BAccountR.bAccountID>>.
					LeftJoin<APAdjustEFileRevision>.
						On<APAdjust.adjgDocType.IsEqual<APAdjustEFileRevision.adjgDocType>.
						And<APAdjust.adjgRefNbr.IsEqual<APAdjustEFileRevision.adjgRefNbr>>.
						And<APAdjust.adjNbr.IsEqual<APAdjustEFileRevision.adjNbr>>.
						And<APAdjust.adjdDocType.IsEqual<APAdjustEFileRevision.adjdDocType>>.
						And<APAdjust.adjdRefNbr.IsEqual<APAdjustEFileRevision.adjdRefNbr>>.
						And<APAdjust.adjdLineNbr.IsEqual<APAdjustEFileRevision.adjdLineNbr>>>.
					Where<APAdjust.adjdBranchID.IsEqual<Branch.branchID.AsOptional>.
						And<APAdjust.adjdDocType.IsNotEqual<APDocType.prepayment>>.
						And<APAdjust.adjdDocType.IsNotEqual<APDocType.debitAdj>>.
						And<T5018VendorRExt.vendorT5018.IsEqual<True>>.
						And<APAdjust.released.IsEqual<True>>.
						And<APAdjustEFileRevision.organizationID.IsNull>.
						And<APAdjust.adjdDocDate.IsGreaterEqual<APAdjust.adjdDocDate.AsOptional>>.
						And<APAdjust.adjdDocDate.IsLessEqual<APAdjust.adjdDocDate.AsOptional>>>.View.ReadOnly.Select(this,
					branches[i],
					MasterView.Current.FromDate,
					MasterView.Current.ToDate).TopFirst;
			}

			if (NewTransactions == null)
				throw new PXException(T5018Messages.NoNewRows);

			T5018MasterTable table = (T5018MasterTable)MasterView.Cache.Insert(new T5018MasterTable()
			{
				OrganizationID = MasterView.Current.OrganizationID.Value,
				Year = MasterView.Current.Year,
				Revision = T5018Messages.NewValue,
				FilingType = T5018MasterTable.filingType.Amendment
			});

			if (table == null)
				table = MasterView.Cache.Inserted.FirstOrDefault_() as T5018MasterTable;

			MasterView.Current = table;

			Save.Press();

			PXLongOperation.StartOperation(this, delegate
			{
				T5018Fileprocessing graph = CreateInstance<T5018Fileprocessing>();

				graph.Prepare(table, true);
			});


			yield return MasterView.Current;
		}
	}
}
