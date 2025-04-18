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
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using System.Collections;
using System.IO;
using PX.Api;
using PX.Common;
using PX.Objects.AP.Overrides.APDocumentRelease;
using PX.Objects.CS.DAC;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.Attributes;
using System.Text.RegularExpressions;
using static PX.Objects.AP.APSetup;

namespace PX.Objects.AP
{

	#region Payer1099SelectorAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Company/Branch")]
	[PXDimensionSelector(
		BAccountAttribute.DimensionName, 
		typeof(SearchFor<BAccountR.bAccountID>
			.In<
				SelectFrom<BAccountR>
					.LeftJoin<Branch>
						.On<BAccountR.bAccountID.IsEqual<Branch.bAccountID>>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.Or<BAccountR.bAccountID.IsEqual<Organization.bAccountID>>>
					.Where<Brackets<Branch.branchID.IsNotNull.And<Organization.reporting1099ByBranches.IsEqual<True>>
							.Or<Branch.branchID.IsNull.And<Organization.reporting1099ByBranches.IsNotEqual<True>>>
							.Or<Branch.bAccountID.IsEqual<Organization.bAccountID>>>
						.And<MatchWithBranch<Branch.branchID>>
						.And<MatchWithOrganization<Organization.organizationID>>>>),
		typeof(BAccountR.acctCD),
		typeof(BAccountR.acctCD),
		typeof(BAccountR.acctName),
		typeof(BAccountR.type))]
	public class Payer1099SelectorAttribute : PXEntityAttribute
	{
		public Payer1099SelectorAttribute()
		{
			this.DescriptionField = typeof(BAccountR.acctName);
			Initialize();
		}
	}
	#endregion

	[PXProjection(typeof(SelectFrom<AP1099History>
		.InnerJoin<Branch>
			.On<AP1099History.branchID.IsEqual<Branch.branchID>>
		.InnerJoin<Organization>
			.On<Branch.organizationID.IsEqual<Organization.organizationID>>
		.Where<MatchWithBranch<AP1099History.branchID>>))]
	[PXHidden]
	public class AP1099BAccountHistory : IBqlTable
	{
		#region BranchID
		public abstract class branchID : BqlInt.Field<branchID> { }
		[Branch(useDefaulting: false, IsKey = true, BqlTable = typeof(AP1099History))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : BqlInt.Field<vendorID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(AP1099History))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region FinYear
		public abstract class finYear : BqlString.Field<finYear> { }
		[PXDBString(4, IsKey = true, IsFixed = true, BqlTable = typeof(AP1099History))]
		public virtual String FinYear
		{
			get;
			set;
		}
		#endregion
		#region BoxNbr
		public abstract class boxNbr : BqlShort.Field<boxNbr> { }
		[PXDBShort(IsKey = true, BqlTable = typeof(AP1099History))]
		public virtual Int16? BoxNbr
		{
			get;
			set;
		}
		#endregion
		#region HistAmt
		public abstract class histAmt : BqlDecimal.Field<histAmt> { }
		[CM.PXDBBaseCury(BqlTable = typeof(AP1099History))]
		public virtual decimal? HistAmt
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }

		[PXInt]
		[PXDBCalced(
			typeof(IIf<Where<Organization.reporting1099ByBranches.IsEqual<True>>, Branch.bAccountID, Organization.bAccountID>),
			typeof(int))]
		public virtual int? BAccountID
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(SelectFrom<AP1099BAccountHistory>
		.Aggregate<To<
			GroupBy<AP1099BAccountHistory.bAccountID>,
			GroupBy<AP1099BAccountHistory.vendorID>,
			GroupBy<AP1099BAccountHistory.finYear>,
			GroupBy<AP1099BAccountHistory.boxNbr>,
			Sum<AP1099BAccountHistory.histAmt>>>))]
	[PXCacheName("AP 1099 History by Payer")]
	public class AP1099HistoryByPayer : AP1099BAccountHistory
	{
		#region BAccountID

		public new abstract class bAccountID : BqlInt.Field<bAccountID> { }
		[Payer1099Selector(BqlField = typeof(AP1099BAccountHistory.bAccountID), IsKey = true)]
		public override int? BAccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public new abstract class branchID : BqlInt.Field<branchID> { }

		[Branch(
			useDefaulting: false, 
			BqlTable = typeof(AP1099BAccountHistory),
			Visible = false,
			Visibility = PXUIVisibility.Invisible)]
		public override int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public new abstract class vendorID : BqlInt.Field<vendorID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(AP1099BAccountHistory))]
		public override int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region FinYear
		public new abstract class finYear : BqlString.Field<finYear> { }
		[PXDBString(4, IsKey = true, IsFixed = true, BqlTable = typeof(AP1099BAccountHistory))]
		public override string FinYear
		{
			get;
			set;
		}
		#endregion
		#region BoxNbr
		public new abstract class boxNbr : BqlShort.Field<boxNbr> { }
		[PXDBShort(IsKey = true, BqlTable = typeof(AP1099BAccountHistory))]
		public override short? BoxNbr
		{
			get;
			set;
		}
		#endregion
		#region HistAmt
		public new abstract class histAmt : BqlDecimal.Field<histAmt> { }
		[CM.PXDBBaseCury(BqlTable = typeof(AP1099BAccountHistory))]
		public override decimal? HistAmt
		{
			get;
			set;
		}
		#endregion

	}

	public class MISC1099EFileProcessing : PXGraph<MISC1099EFileProcessing>
	{
		#region Declaration
		public PXCancel<MISC1099EFileFilter> Cancel;

		public PXFilter<MISC1099EFileFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessingOrderBy<MISC1099EFileProcessingInfo, MISC1099EFileFilter, 
			OrderBy<Asc<MISC1099EFileProcessingInfo.payerOrganizationID, 
				Asc<MISC1099EFileProcessingInfo.payerBranchID, 
				Asc<MISC1099EFileProcessingInfo.vendorID>>>>> Records;

		protected int RecordCounter;

		protected Organization TransmitterOrganization
		{
			get
			{
				OrganizationSlot.All1099.TryGetValue(Filter.Current.OrganizationID ?? 0, out Organization organization);
				return organization;
			}
		}

		protected Branch TransmitterBranch
		{
			get
			{
				AvailableBranches.TryGetValue(Filter.Current.BranchID ?? 0, out Branch branch);
				return branch;
			}
		}

		protected YearFormat CalculateYearFormat(string year)
		{		
			return (string.Compare(year, "2021") < 0)
				? YearFormat.F2020
				: YearFormat.F2021;			
		}

		protected AP1099OrganizationDefinition OrganizationSlot => PXDatabase.GetSlot<AP1099OrganizationDefinition, PXFilter<MISC1099EFileFilter>>(
			typeof(AP1099OrganizationDefinition).FullName,
			Filter,
			typeof(Organization));

		protected virtual IDictionary<int, Organization> AvailableOrganizations =>	OrganizationSlot.ForReporting;

		protected virtual int?[] MarkedOrganizationIDs
		{
			get
			{
				if (Filter.Current != null && Filter.Current.OrganizationID != null)
				{
					if (Filter.Current.Include == MISC1099EFileFilter.include.AllMarkedOrganizations)
					{
						return null;
					}
					return new int?[] { TransmitterOrganization?.OrganizationID };
				}
				return new int?[] { };
			}
		}

		protected virtual IDictionary<int, Branch> AvailableBranches =>
			PXDatabase.GetSlot<AP1099BranchDefinition, PXFilter<MISC1099EFileFilter>>(
				typeof(AP1099BranchDefinition).FullName,
				Filter,
				typeof(Organization),
				typeof(Branch))
				.Available;

		protected virtual int?[] MarkedBranchIDs
		{
			get
			{
				if (Filter.Current != null && Filter.Current.OrganizationID != null)
				{
					List<int?> markedIDs;
					if (Filter.Current.Include == MISC1099EFileFilter.include.AllMarkedOrganizations)
					{
						HashSet<int> IDs = new HashSet<int>(AvailableBranches.Values.Select(b => (int)b.BranchID));
						IDs.AddRange(AvailableOrganizations.Keys.Select(orgID => PXAccess.GetChildBranchIDs(orgID, false)).SelectMany(b => b));
						markedIDs = IDs.Cast<int?>().ToList();
					}
					else
					{
						markedIDs = new List<int?> { TransmitterBranch?.BranchID };
					}

					return markedIDs.Any(id => id != null)
						? markedIDs.ToArray()
						: null;
				}
				return null;
			}
		}

		protected Dictionary<string, string> combinedFederalOrStateCodes;

		#endregion


		protected virtual MISC1099EFileProcessingInfoRaw AdjustOrganizationBranch(MISC1099EFileProcessingInfoRaw info)
		{
			int? organizationID = info.PayerOrganizationID;
			int? branchID = info.PayerBranchID;

			I1099Settings settings = AdjustOrganizationBranch(ref organizationID, ref branchID);

			info.PayerOrganizationID = organizationID;
			info.PayerBranchID = branchID;
			info.PayerBAccountID = settings.BAccountID;

			return info;
		}

		protected virtual I1099Settings AdjustOrganizationBranch(ref int? organizationID, ref int? branchID)
		{
			AvailableOrganizations.TryGetValue(organizationID ?? 0, out Organization payerOrganization);
			AvailableBranches.TryGetValue(branchID ?? 0, out Branch payerBranch);

			organizationID = payerOrganization?.OrganizationID ?? 0;
			branchID = payerBranch?.BranchID ?? 0;

			if (organizationID == null && branchID == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty);
			}

			return payerOrganization as I1099Settings ?? payerBranch as I1099Settings;

		}

		public IEnumerable records()
		{
			this.Caches<MISC1099EFileProcessingInfoRaw>().Clear();
			this.Caches<MISC1099EFileProcessingInfoRaw>().ClearQueryCache();

			if (Filter.Current?.OrganizationID == null
				|| TransmitterOrganization?.Reporting1099ByBranches == true && Filter.Current?.BranchID == null)
			{
				yield break;
			}

			using (new PXReadBranchRestrictedScope(MarkedOrganizationIDs, MarkedBranchIDs))
			{
				// TODO: Workaround awaiting AC-64107
				// -
				IEnumerable<MISC1099EFileProcessingInfo> list = PXSelect<
					MISC1099EFileProcessingInfoRaw,
					Where<
						MISC1099EFileProcessingInfoRaw.finYear, Equal<Current<MISC1099EFileFilter.finYear>>,
						And<Current<MISC1099EFileFilter.organizationID>, IsNotNull>>>
					.Select(this)
					.RowCast<MISC1099EFileProcessingInfoRaw>()
					.Select(infoRaw => AdjustOrganizationBranch(infoRaw))
					.GroupBy(rawInfo => new { rawInfo.PayerOrganizationID, rawInfo.PayerBranchID, rawInfo.VendorID })
					.Select(group => new MISC1099EFileProcessingInfo
					{
						PayerOrganizationID = group.Key.PayerOrganizationID,
						PayerBranchID = group.Key.PayerBranchID,
						PayerBAccountID = group.First().PayerBAccountID,
						DisplayOrganizationID = group.Key.PayerOrganizationID > 0 ? group.Key.PayerOrganizationID : null,
						DisplayBranchID = group.Key.PayerBranchID > 0 ? group.Key.PayerBranchID : null,
						BoxNbr = group.First().BoxNbr,
						FinYear = group.First().FinYear,
						VendorID = group.Key.VendorID,
						VAcctCD = group.First().VAcctCD,
						VAcctName = group.First().VAcctName,
						LTaxRegistrationID = group.First().LTaxRegistrationID,
						HistAmt = group.Sum(h => h.HistAmt),
						CountryID = group.First().CountryID,
						State = group.First().State
					})
					.ToArray();

				foreach (MISC1099EFileProcessingInfo record in list)
				{
					Filter.Current.CountryID = record.CountryID;
					MISC1099EFileProcessingInfo located = Records.Cache.Locate(record) as MISC1099EFileProcessingInfo;
					yield return (located ?? Records.Cache.Insert(record));
				}
				//There is "masterBranch is not null" here because select parameters must be changed after changing masterBranch. 
				//If select parametrs is unchanged since last use this select will not be executed and return previous result.
			}
		}

		public MISC1099EFileProcessing()
		{
			Records.SetProcessTooltip(Messages.EFiling1099SelectedVendorsTooltip);
			Records.SetProcessAllTooltip(Messages.EFiling1099AllVendorsTooltip);
		}


		protected virtual void MISC1099EFileFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			MISC1099EFileFilter oldRow = (MISC1099EFileFilter)e.OldRow;
			MISC1099EFileFilter newRow = (MISC1099EFileFilter)e.Row;
			if (oldRow.FinYear != newRow.FinYear || 
				oldRow.OrganizationID != newRow.OrganizationID || 
				oldRow.Box7 != newRow.Box7)
			{
				Records.Cache.Clear();
			}
		}

		protected virtual void _(Events.FieldUpdated<MISC1099EFileFilter, MISC1099EFileFilter.finYear> e)
		{
			if (e.Row.FinYear != null
				&& string.Compare(e.Row.FinYear, MISC1099EFileFilter.finYear.NECAvailable1099Year) < 0
				&& e.Row.FileFormat == MISC1099EFileFilter.fileFormat.NEC)
			{
				e.Cache.SetDefaultExt<MISC1099EFileFilter.fileFormat>(e.Row);
			}
		}

		protected virtual void MISC1099EFileFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			MISC1099EFileFilter rowfilter = e.Row as MISC1099EFileFilter;
			if (rowfilter == null) return;

			Records.SetProcessDelegate(
				delegate (List<MISC1099EFileProcessingInfo> list)
				{
					MISC1099EFileProcessing graph = CreateInstance<MISC1099EFileProcessing>();
					graph.Process(list, rowfilter);
				});

			if (rowfilter.Include == MISC1099EFileFilter.include.AllMarkedOrganizations)
			{
				bool HaveBranches = false;
				string unmarkedOrganizationsBranchs = SelectFrom<Organization>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Brackets<Organization.reporting1099.IsNotEqual<True>.Or<Organization.reporting1099.IsNull>>
						.And<Organization.reporting1099ByBranches.IsNotEqual<True>.Or<Organization.reporting1099ByBranches.IsNull>>
						.And<MatchWithBranch<Branch.branchID>>>
					.OrderBy<Desc<NullIf<Organization.organizationType, @P.AsString>>>
					.View
					.SelectWindowed(this, 0, 10, OrganizationTypes.WithoutBranches).AsEnumerable()
					.Cast<PXResult<Organization, Branch>>()
					.Select(delegate (PXResult<Organization, Branch> row)
					{
						Organization organization = row;
						Branch branch = row;
						HaveBranches |= organization.OrganizationType != OrganizationTypes.WithoutBranches;
						return branch.BranchCD;
					})
					.JoinToString(", ");

				if (HaveBranches)
				{
					sender.RaiseExceptionHandling<MISC1099EFileFilter.include>(rowfilter, rowfilter.Include,
						new PXSetPropertyException(Messages.Unefiled1099OrganizationsBranchs, PXErrorLevel.Warning, unmarkedOrganizationsBranchs));
				}
				else
				{
					sender.RaiseExceptionHandling<MISC1099EFileFilter.include>(rowfilter, rowfilter.Include,
						!string.IsNullOrEmpty(unmarkedOrganizationsBranchs)
							? new PXSetPropertyException(Messages.Unefiled1099Organizations, PXErrorLevel.Warning, unmarkedOrganizationsBranchs)
							: null);
				}
			}
			else
			{
				sender.RaiseExceptionHandling<MISC1099EFileFilter.include>(rowfilter, rowfilter.Include, null);
			}

			if(string.Compare(rowfilter.FinYear, MISC1099EFileFilter.finYear.NECAvailable1099Year) < 0)
			{
				PXStringListAttribute.SetList<MISC1099EFileFilter.fileFormat>(sender, rowfilter, (MISC1099EFileFilter.fileFormat.MISC, Messages.MISC));
			}
			else
			{
				PXStringListAttribute.SetList<MISC1099EFileFilter.fileFormat>(
					sender, 
					rowfilter, 
					(MISC1099EFileFilter.fileFormat.MISC, Messages.MISC),
					(MISC1099EFileFilter.fileFormat.NEC, Messages.NEC));
			}
		}

		protected virtual void _(Events.FieldUpdated<MISC1099EFileFilter, MISC1099EFileFilter.organizationID> e)
		{
			e.Row.BranchID = null;
		}

		public class Reporting1099Entity: I1099Settings
		{
			public I1099Settings Settings;
			public BAccount BAccount;
			public Contact Contact;
			public Address Address;
			public LocationExtAddress Location;

			public int? BAccountID { get => Settings.BAccountID; set => Settings.BAccountID = value; }
			public string TCC { get => Settings.TCC; set => Settings.TCC = value; }
			public bool? ForeignEntity { get => Settings.ForeignEntity; set => Settings.ForeignEntity = value; }
			public bool? CFSFiler { get => Settings.CFSFiler; set => Settings.CFSFiler = value; }
			public string ContactName { get => Settings.ContactName; set => Settings.ContactName = value; }
			public string CTelNumber { get => Settings.CTelNumber; set => Settings.CTelNumber = value; }
			public string CEmail { get => Settings.CEmail; set => Settings.CEmail = value; }
			public string NameControl { get => Settings.NameControl; set => Settings.NameControl = value; }

			public static implicit operator BAccount(Reporting1099Entity entity) => entity.BAccount;
			public static implicit operator Contact(Reporting1099Entity entity) => entity.Contact;
			public static implicit operator Address(Reporting1099Entity entity) => entity.Address;
			public static implicit operator LocationExtAddress(Reporting1099Entity entity) => entity.Location;
		}

		protected virtual Reporting1099Entity GetReportingEntity(int? organizationID, int? branchID)
		{
			Reporting1099Entity entity = new Reporting1099Entity
			{
				Settings = AdjustOrganizationBranch(ref organizationID, ref branchID)
			};
			foreach (PXResult<BAccount, Contact, Address, LocationExtAddress> bAccountData in SelectFrom<BAccount>
				.LeftJoin<Contact>
					.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>
						.And<BAccount.defContactID.IsEqual<Contact.contactID>>>
				.LeftJoin<Address>
					.On<BAccount.bAccountID.IsEqual<Address.bAccountID>
						.And<BAccount.defAddressID.IsEqual<Address.addressID>>>
				.LeftJoin<LocationExtAddress>
					.On<BAccount.bAccountID.IsEqual<LocationExtAddress.bAccountID>
						.And<BAccount.defLocationID.IsEqual<LocationExtAddress.locationID>>>
				.Where<BAccount.bAccountID.IsEqual<@P.AsInt>>
				.View
				.SelectSingleBound(this, new object[] { }, entity.Settings.BAccountID))
			{
				entity.BAccount = bAccountData;
				entity.Address = bAccountData;
				entity.Contact = bAccountData;
				entity.Location = bAccountData;
			}

			return entity;
		}

		public void Process(List<MISC1099EFileProcessingInfo> records, MISC1099EFileFilter filter)
		{
			if (filter.FileFormat == MISC1099EFileFilter.fileFormat.MISC || CalculateYearFormat(filter.FinYear) == YearFormat.F2021)
			{
				FillCombinedFederalOrStateCodes(filter.FinYear);
			}

			using (new PXReadBranchRestrictedScope(MarkedOrganizationIDs, MarkedBranchIDs, requireAccessForAllSpecified: true))
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.Unicode))
					{
						{
							TransmitterTRecord trecord = CreateTransmitterRecord(GetReportingEntity(filter.OrganizationID, filter.BranchID), filter, 0);
							List<object> data1099Misc = new List<object> { trecord };

							int payeesCount = 0;
							List<IGrouping<(int? OrganizationID, int? BranchID), MISC1099EFileProcessingInfo>> groups = records.GroupBy(rec => (rec.PayerOrganizationID, rec.PayerBranchID )).ToList();
							foreach (IGrouping<(int? OrganizationID, int? BranchID), MISC1099EFileProcessingInfo> group in groups)
							{
								Reporting1099Entity payer = GetReportingEntity(group.Key.OrganizationID, group.Key.BranchID);
								{
									Contact rowShipContact = PXSelectJoin<Contact,
										InnerJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
											And<Contact.contactID, Equal<Location.defContactID>>>>,
										Where<Location.bAccountID, Equal<Required<BAccount.bAccountID>>,
											And<Location.locationID, Equal<Required<BAccount.defLocationID>>>>>
											.Select(this, payer.BAccountID, ((LocationExtAddress)payer).LocationID);

									PayerRecordA recordA = CreatePayerARecord(payer, rowShipContact, filter);
									// The condition must be moved to the CreatePayerARecord parameter.
									// These are breaking changes and should be included in a major update.
									// The fix comes in a minor update.
									// Implemented in such way as to avoid breaking changes.
									if (recordA.CombinedFederalORStateFiler == "1" && @group.All(info => GetCombinedFederalOrStateCode(info.State) == string.Empty))
									{
										recordA.CombinedFederalORStateFiler = string.Empty;
									}
									data1099Misc.Add(recordA);

									List<PayeeRecordB> payeeRecs = new List<PayeeRecordB>();
									foreach (MISC1099EFileProcessingInfo rec in @group)
									{
										PXProcessing<MISC1099EFileProcessingInfo>.SetCurrentItem(rec);
										payeeRecs.Add(CreatePayeeBRecord(payer, rec, filter));
										PXProcessing<MISC1099EFileProcessingInfo>.SetProcessed();
									}
									payeeRecs = payeeRecs.WhereNotNull().ToList();
									payeesCount += payeeRecs.Count;
									trecord.TotalNumberofPayees = payeeRecs.Count.ToString();
									data1099Misc.AddRange(payeeRecs);
									data1099Misc.Add(CreateEndOfPayerRecordC(payeeRecs));

									//If combined State Filer then only generate K Record.
									if (payer.CFSFiler == true 
										&& (filter.FileFormat == MISC1099EFileFilter.fileFormat.MISC || CalculateYearFormat(filter.FinYear) == YearFormat.F2021))
									{
										data1099Misc.AddRange(payeeRecs
											.Where(x => !string.IsNullOrWhiteSpace(x.PayeeState))
											.GroupBy(x => x.PayeeState.Trim(), StringComparer.CurrentCultureIgnoreCase)
											.Select(y => CreateStateTotalsRecordK(y.ToList()))
											.Where(kRecord => kRecord != null));
									}
								}
							}

							data1099Misc.Add(CreateEndOfTransmissionRecordF(groups.Count, payeesCount));

							//Write to file
							FixedLengthFile flatFile = new FixedLengthFile();

							foreach (object rec in data1099Misc)
							{
								((I1099Record)rec).WriteToFile(sw, CalculateYearFormat(filter.FinYear));
							}
							sw.Flush();

							string formatLabel = PXStringListAttribute.GetLocalizedLabel<MISC1099EFileFilter.fileFormat>(this.Caches<MISC1099EFileFilter>(), filter);
							string path = $"1099-{formatLabel}.txt";
							PX.SM.FileInfo info = new PX.SM.FileInfo(path, null, stream.ToArray());

							throw new PXRedirectToFileException(info, true);
						}
					}
				}
			}
		}

		public virtual TransmitterTRecord CreateTransmitterRecord(Reporting1099Entity entity, MISC1099EFileFilter filter, int totalPayeeB)
		{
			return CreateTransmitterRecord(entity, entity, entity, entity, filter, totalPayeeB);
		}

		protected TransmitterTRecord CreateTransmitterRecord(
			I1099Settings settings1099,
			BAccount bAccount, 
			Contact rowMainContact,
			Address rowMainAddress,
			MISC1099EFileFilter filter,
			int totalPayeeB)
		{
			return new TransmitterTRecord
			{
				RecordType = "T",
				PaymentYear = filter.FinYear,
				PriorYearDataIndicator = filter.IsPriorYear == true ? "P" : string.Empty,
				TransmitterTIN = bAccount.TaxRegistrationID,
				TransmitterControlCode = settings1099.TCC,
				TestFileIndicator = filter.IsTestMode == true ? "T" : string.Empty,
				ForeignEntityIndicator = settings1099.ForeignEntity == true ? "1" : string.Empty,

				TransmitterName = bAccount.LegalName.Trim(),
				CompanyName = bAccount.LegalName.Trim(),

				CompanyMailingAddress = string.Concat(rowMainAddress.AddressLine1, rowMainAddress.AddressLine2),
				CompanyCity = rowMainAddress.City,
				CompanyState = rowMainAddress.State,
				CompanyZipCode = rowMainAddress.PostalCode,
				//Setup at the end - dependent of Payee B records
				TotalNumberofPayees = totalPayeeB.ToString(),
				ContactName = settings1099.ContactName,
				ContactTelephoneAndExt = settings1099.CTelNumber,
				ContactEmailAddress = settings1099.CEmail,
				RecordSequenceNumber = (++RecordCounter).ToString(),

				VendorIndicator = "V",
				VendorName = TRecordVendorInfo.VendorName,
				VendorMailingAddress = TRecordVendorInfo.VendorMailingAddress,
				VendorCity = TRecordVendorInfo.VendorCity,
				VendorState = TRecordVendorInfo.VendorState,
				VendorZipCode = TRecordVendorInfo.VendorZipCode,
				VendorContactName = TRecordVendorInfo.VendorContactName,
				VendorContactTelephoneAndExt = TRecordVendorInfo.VendorContactTelephoneAndExt,

				#region Check - Vendor or Branch?
				VendorForeignEntityIndicator = TRecordVendorInfo.VendorForeignEntityIndicator,
				#endregion
			};
		}

		public virtual PayerRecordA CreatePayerARecord(Reporting1099Entity entity, Contact rowShipContact, MISC1099EFileFilter filter)
		{
			return CreatePayerARecord(entity, entity, entity, entity, entity, rowShipContact, filter);
		}

		public PayerRecordA CreatePayerARecord(
			I1099Settings settings1099, 
			BAccount bAccount,
			Contact rowMainContact,
			Address rowMainAddress,
			LocationExtAddress rowShipInfo,
			Contact rowShipContact,
			MISC1099EFileFilter filter)
		{
			string companyName1 = bAccount.LegalName.Trim();
			string companyName2 = string.Empty;
			if (companyName1.Length > 40)
			{
				companyName2 = companyName1.Substring(40);
				companyName1 = companyName1.Substring(0, 40);
			}

			string typeOfReturn;
			string amountCodes;
			switch (filter.FileFormat)
			{
				case MISC1099EFileFilter.fileFormat.MISC:
					typeOfReturn = "A";
					if (string.Compare(filter.FinYear, "2021") < 0)
					{
						amountCodes = (filter.ReportingDirectSalesOnly == true) ? "1" : "1234568ABCDE";
						if (string.Compare(filter.FinYear, MISC1099EFileFilter.finYear.NECAvailable1099Year) < 0)
						{
							amountCodes = $"{amountCodes}G";
						}
					}
					else
					{
						amountCodes = (filter.ReportingDirectSalesOnly == true) ? "1" : "1234568ABCDEF";
					}
					break;
				case MISC1099EFileFilter.fileFormat.NEC:
					typeOfReturn = "NE";
					amountCodes = (filter.ReportingDirectSalesOnly == true) ? "1" : "14";
					break;
				default:
					string unknownFormatLabel = PXStringListAttribute.GetLocalizedLabel<MISC1099EFileFilter.fileFormat>(this.Caches<MISC1099EFileFilter>(), filter);
					throw new PXException(Messages.Unknown1099EFileFormat, unknownFormatLabel);
			}
			
			return new PayerRecordA
			{
				RecordType = "A",
				PaymentYear = filter.FinYear,
				CombinedFederalORStateFiler = settings1099.CFSFiler == true && (CalculateYearFormat(filter.FinYear) == YearFormat.F2021 || filter.FileFormat == MISC1099EFileFilter.fileFormat.MISC)
					? "1" 
					: string.Empty,
				PayerTaxpayerIdentificationNumberTIN = bAccount.TaxRegistrationID,

				PayerNameControl = settings1099.NameControl,

				LastFilingIndicator = filter.IsLastFiling == true ? "1" : string.Empty,

				TypeofReturn = typeOfReturn,
				AmountCodes = amountCodes,

				ForeignEntityIndicator = settings1099.ForeignEntity == true ? "1" : string.Empty,
				FirstPayerNameLine = companyName1,
				SecondPayerNameLine = companyName2,

				#region Check with Gabriel, we need Transfer Agent or no
				TransferAgentIndicator = "0",
				#endregion

				PayerShippingAddress = string.Concat(rowShipInfo.AddressLine1, rowShipInfo.AddressLine2),
				PayerCity = rowShipInfo.City,
				PayerState = rowShipInfo.State,
				PayerZipCode = rowShipInfo.PostalCode,

				PayerTelephoneAndExt = rowShipContact.Phone1,

				RecordSequenceNumber = (++RecordCounter).ToString()
			};
		}

		public PayeeRecordB CreatePayeeBRecord(I1099Settings settings1099, MISC1099EFileProcessingInfo record1099, MISC1099EFileFilter filter)
		{
			PayeeRecordB bRecord;
			this.Caches<AP1099History>().ClearQueryCache();
			using (new PXReadBranchRestrictedScope(
				record1099.DisplayOrganizationID?.SingleToArray().Cast<int?>().ToArray(),
				record1099.DisplayBranchID?.SingleToArray().Cast<int?>().ToArray(), 
				requireAccessForAllSpecified:true))
			{
				VendorR rowVendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Required<VendorR.bAccountID>>>>.Select(this, record1099.VendorID);

				Address rowVendorAddress = PXSelect<Address,
					Where<Address.bAccountID, Equal<Required<BAccount.bAccountID>>,
						And<Address.addressID, Equal<Required<BAccount.defAddressID>>>>>.Select(this, rowVendor.BAccountID, rowVendor.DefAddressID);

				LocationExtAddress rowVendorShipInfo = PXSelect<LocationExtAddress,
					Where<LocationExtAddress.locationBAccountID, Equal<Required<BAccount.bAccountID>>,
						And<LocationExtAddress.locationID, Equal<Required<BAccount.defLocationID>>>>>.Select(this, rowVendor.BAccountID, rowVendor.DefLocationID);

				List<AP1099History> amtList1099 = PXSelectJoinGroupBy<AP1099History,
					InnerJoin<AP1099Box, On<AP1099History.boxNbr, Equal<AP1099Box.boxNbr>>>,
					Where<AP1099History.vendorID, Equal<Required<AP1099History.vendorID>>,
						And<AP1099History.finYear, Equal<Required<AP1099History.finYear>>,
						And<Where<
							Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7All>,
							Or<Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7Equal>,
								And<AP1099History.boxNbr, Equal<MISC1099EFileFilter.box7.box7Nbr>,
							Or<Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7NotEqual>,
								And<AP1099History.boxNbr, NotEqual<MISC1099EFileFilter.box7.box7Nbr>>>>>>>>>,
					Aggregate<
						GroupBy<AP1099History.boxNbr,
						Sum<AP1099History.histAmt>>>>
						.Select(this, record1099.VendorID, filter.FinYear, filter.Box7, filter.Box7, filter.Box7).AsEnumerable()
						.Where(res => res.GetItem<AP1099History>().HistAmt >= res.GetItem<AP1099Box>().MinReportAmt)
						.RowCast<AP1099History>()
						.ToList();

				APSetup aPSetup = PXSetup<APSetup>.Select(this);

				if ((amtList1099.Sum(hist => hist.HistAmt) ?? 0m) == 0m) return null;

				decimal paymentAmount1 = 0m;
				decimal paymentAmount2 = 0m;
				decimal paymentAmount3 = 0m;
				decimal paymentAmount4 = 0m;
				decimal paymentAmount5 = 0m;
				decimal paymentAmount6 = 0m;
				decimal paymentAmount7 = 0m;
				decimal paymentAmount8 = 0m;
				decimal paymentAmount9 = 0m;
				decimal paymentAmountA = 0m;
				decimal paymentAmountB = 0m;
				decimal paymentAmountC = 0m;
				decimal paymentAmountD = 0m; 
				decimal paymentAmountE = 0m;
				decimal paymentAmountF = 0m;
				decimal paymentAmountG = 0m;
				bool NEC_Exists = (amtList1099.Where(hist => hist.BoxNbr == 4 || hist.BoxNbr == 7).Sum(hist => hist.HistAmt) ?? 0m) > 0;
				bool MISC_Exists = (amtList1099.Where(hist => hist.BoxNbr == 1 || hist.BoxNbr == 2 || hist.BoxNbr == 3 || hist.BoxNbr == 5 || hist.BoxNbr == 6 || (CalculateYearFormat(filter.FinYear) == YearFormat.F2020 && hist.BoxNbr == 7) || hist.BoxNbr == 8 || hist.BoxNbr == 10 || hist.BoxNbr == 11 || hist.BoxNbr == 13 || hist.BoxNbr == 14).Sum(hist => hist.HistAmt) ?? 0m) > 0;
				bool FiledInMISC = aPSetup.PrintDirectSalesOn == printDirectSalesOn.MISC_Always
					|| aPSetup.PrintDirectSalesOn == printDirectSalesOn.MISC_if_Filed && (MISC_Exists || !NEC_Exists)
					|| aPSetup.PrintDirectSalesOn == printDirectSalesOn.NEC_if_Filed && MISC_Exists && !NEC_Exists;
				bool FiledInNEC = !FiledInMISC;

				switch (filter.FileFormat)
				{
					case MISC1099EFileFilter.fileFormat.MISC:
						paymentAmount1 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 1)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount2 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 2)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount3 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 3)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount4 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 4)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount5 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 5)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount6 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 6)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount7 = 0m;
						paymentAmount8 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 8)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount9 = Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 9)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountA = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 10)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountB = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 13)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountC = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 14)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountD = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 151)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountE = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 152)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountF = filter.ReportingDirectSalesOnly == true || CalculateYearFormat(filter.FinYear) == YearFormat.F2020 ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 11)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmountG = string.Compare(filter.FinYear, MISC1099EFileFilter.finYear.NECAvailable1099Year) < 0
								? filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 7)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2)
								: 0m;

						decimal amountExcludeBox7 = paymentAmount1 + paymentAmount2 + paymentAmount3 + paymentAmount4
							+ paymentAmount5 + paymentAmount6 + paymentAmount8 + (FiledInMISC ? paymentAmount9 : 0m) + paymentAmountA + paymentAmountB
							+ paymentAmountC + paymentAmountD + paymentAmountE + paymentAmountF + paymentAmountG;
						if (string.Compare(filter.FinYear, MISC1099EFileFilter.finYear.NECAvailable1099Year) >= 0
							&& amountExcludeBox7 == 0m)
						{
							return null; // skip vendor
						}

						paymentAmount9 = 0m;

						break;
					case MISC1099EFileFilter.fileFormat.NEC:
						paymentAmount1 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 7)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount4 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 4)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						paymentAmount9 = CalculateYearFormat(filter.FinYear) == YearFormat.F2020 ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 9)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2);
						decimal amountsInNEC = paymentAmount1 + paymentAmount4 + (FiledInNEC ? paymentAmount9 : 0m);
						if (amountsInNEC == 0m)
						{
							return null; // skip vendor
						}

						paymentAmount9 = 0m;

						break;
					default:
						string unknownFormatLabel = PXStringListAttribute.GetLocalizedLabel<MISC1099EFileFilter.fileFormat>(this.Caches<MISC1099EFileFilter>(), filter);
						throw new PXException(Messages.Unknown1099EFileFormat, unknownFormatLabel);
				}

				string typeOfTin = string.Empty;

				switch (rowVendor.TinType)
				{
					case Vendor.tinType.EIN:
						typeOfTin = "1";
						break;
					case Vendor.tinType.SSN:
					case Vendor.tinType.ITIN:
					case Vendor.tinType.ATIN:
						typeOfTin = "2";
						break;
				}

				bRecord = new PayeeRecordB
				{
					RecordType = "B",
					PaymentYear = filter.FinYear,

					//ALWAYS G since we have one Payee record per file.
					CorrectedReturnIndicator = filter.IsCorrectionReturn == true ? "G" : string.Empty,

					NameControl = string.Empty,

					TypeOfTIN = typeOfTin,

					PayerTaxpayerIdentificationNumberTIN = rowVendorShipInfo.TaxRegistrationID,

					PayerAccountNumberForPayee = rowVendor.AcctCD,

					#region Check with Gabriel, not sure about this
					PayerOfficeCode = string.Empty,
					#endregion

					PaymentAmount1 = paymentAmount1,
					PaymentAmount2 = paymentAmount2,
					PaymentAmount3 = paymentAmount3,
					PaymentAmount4 = paymentAmount4,
					PaymentAmount5 = paymentAmount5,
					PaymentAmount6 = paymentAmount6,
					PaymentAmount7 = paymentAmount7,
					PaymentAmount8 = paymentAmount8,
					PaymentAmount9 = paymentAmount9,
					PaymentAmountA = paymentAmountA,
					PaymentAmountB = paymentAmountB,
					PaymentAmountC = paymentAmountC,
					Payment = paymentAmountD,
					PaymentAmountE = paymentAmountE,
					PaymentAmountF = paymentAmountF,
					PaymentAmountG = paymentAmountG,

					ForeignCountryIndicator = rowVendor.ForeignEntity == true ? "1" : string.Empty,
					PayeeNameLine = rowVendor.LegalName,

					PayeeMailingAddress = string.Concat(rowVendorAddress.AddressLine1, rowVendorAddress.AddressLine2),

					PayeeCity = rowVendorAddress.City,
					PayeeState = rowVendorAddress.State,
					PayeeZipCode = rowVendorAddress.PostalCode,

					RecordSequenceNumber = (++RecordCounter).ToString(),

					#region Confirmed with Gabriel, Skip for now
					SecondTINNotice = string.Empty,
					#endregion,

					#region Check - Dependent on Box 9 - check in 3rd party
					DirectSalesIndicator = CalculateYearFormat(filter.FinYear) == YearFormat.F2020 && string.Equals(filter.FileFormat, MISC1099EFileFilter.fileFormat.MISC)
							|| CalculateYearFormat(filter.FinYear) == YearFormat.F2021 && string.Equals(filter.FileFormat, MISC1099EFileFilter.fileFormat.MISC) && FiledInMISC
							|| CalculateYearFormat(filter.FinYear) == YearFormat.F2021 && string.Equals(filter.FileFormat, MISC1099EFileFilter.fileFormat.NEC) && FiledInNEC
						? GetDirectSaleIndicator(record1099.VendorID.Value, filter.FinYear)
						: string.Empty,
					#endregion

					FATCA = rowVendor.FATCA == true && (CalculateYearFormat(filter.FinYear) == YearFormat.F2020 || string.Equals(filter.FileFormat, MISC1099EFileFilter.fileFormat.MISC))
						? "1"
						: string.Empty,

					#region Confirmed with Gabriel, skip for now
					SpecialDataEntries = string.Empty,
					StateIncomeTaxWithheld = string.Empty,
					LocalIncomeTaxWithheld = string.Empty,
					#endregion

					CombineFederalOrStateCode = settings1099.CFSFiler == true && (filter.FileFormat == MISC1099EFileFilter.fileFormat.MISC || CalculateYearFormat(filter.FinYear) == YearFormat.F2021)
						? GetCombinedFederalOrStateCode(rowVendorAddress.State) 
						: string.Empty,

				};
			}
			return bRecord;
		}

		public EndOfPayerRecordC CreateEndOfPayerRecordC(List<PayeeRecordB> listPayeeB)
		{
			return new EndOfPayerRecordC
			{
				RecordType = "C",
				// At the End, Total# of B Records
				NumberOfPayees = Convert.ToString(listPayeeB.Count),

				#region Totals
				ControlTotal1 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount1), 2),
				ControlTotal2 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount2), 2),
				ControlTotal3 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount3), 2),
				ControlTotal4 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount4), 2),
				ControlTotal5 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount5), 2),
				ControlTotal6 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount6), 2),
				ControlTotal7 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount7), 2),
				ControlTotal8 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount8), 2),
				ControlTotal9 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount9), 2),
				ControlTotalA = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountA), 2),
				ControlTotalB = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountB), 2),
				ControlTotalC = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountC), 2),
				ControlTotalD = Math.Round(listPayeeB.Sum(brec => brec.Payment), 2),
				ControlTotalE = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountE), 2),
				ControlTotalF = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountF), 2),
				ControlTotalG = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountG), 2),
				#endregion

				RecordSequenceNumber = (++RecordCounter).ToString(),
			};
		}

		public StateTotalsRecordK CreateStateTotalsRecordK(List<PayeeRecordB> listPayeeB)
		{
			if (listPayeeB == null) return null;

			string stateInfo = (listPayeeB.FirstOrDefault() ?? new PayeeRecordB()).PayeeState;
			string CSFCCode = GetCombinedFederalOrStateCode(stateInfo);

			//Do not include K Record if State do not participate in CF/SF Program
			if (string.IsNullOrEmpty(CSFCCode)) return null;

			return new StateTotalsRecordK
			{
				RecordType = "K",

				NumberOfPayees = Convert.ToString(listPayeeB.Count),

				#region Totals
				ControlTotal1 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount1), 2),
				ControlTotal2 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount2), 2),
				ControlTotal3 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount3), 2),
				ControlTotal4 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount4), 2),
				ControlTotal5 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount5), 2),
				ControlTotal6 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount6), 2),
				ControlTotal7 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount7), 2),
				ControlTotal8 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount8), 2),
				ControlTotal9 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount9), 2),
				ControlTotalA = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountA), 2),
				ControlTotalB = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountB), 2),
				ControlTotalC = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountC), 2),
				ControlTotalD = Math.Round(listPayeeB.Sum(brec => brec.Payment), 2),
				ControlTotalE = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountE), 2),
				ControlTotalF = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountF), 2),
				ControlTotalG = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountG), 2),
				#endregion

				RecordSequenceNumber = (++RecordCounter).ToString(),

				#region State and Local
				StateIncomeTaxWithheldTotal = 0m,
				LocalIncomeTaxWithheldTotal = 0m,
				#endregion

				//Check if new field needed
				CombinedFederalOrStateCode = CSFCCode,
			};
		}

		public EndOfTransmissionRecordF CreateEndOfTransmissionRecordF(int totalPayerA, int totalPayeeB)
		{
			return new EndOfTransmissionRecordF()
			{
				RecordType = "F",
				// At the End, Total# of B Records
				NumberOfARecords = totalPayerA.ToString(),

				TotalNumberOfPayees = totalPayeeB.ToString(),

				RecordSequenceNumber = (++RecordCounter).ToString(),
			};
		}

		public PXAction<MISC1099EFileFilter> View1099Summary;
		[PXUIField(DisplayName = "View 1099 Vendor History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable view1099Summary(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				AP1099DetailEnq graph = CreateInstance<AP1099DetailEnq>();
				graph.YearVendorHeader.Current.FinYear = Records.Current.FinYear;
				graph.YearVendorHeader.Current.VendorID = Records.Current.VendorID;	
				PXFieldState state = Records.Cache.GetValueExt<MISC1099EFileProcessingInfo.payerBAccountID>(Records.Cache.Current) as PXFieldState;
				graph.YearVendorHeader.Cache.SetValueExt<AP1099YearMaster.orgBAccountID>(
					graph.YearVendorHeader.Current, state.Value);
				throw new PXRedirectRequiredException(graph, true, "1099 Year Vendor History");
			}
			return adapter.Get();
		}

		protected virtual string GetCombinedFederalOrStateCode(string stateAbbrCode)
		{
			stateAbbrCode = stateAbbrCode ?? string.Empty;
			string stateCFSFCode;
			if (!combinedFederalOrStateCodes.TryGetValue(stateAbbrCode.Trim().ToUpper(), out stateCFSFCode))
			{
				stateCFSFCode = string.Empty;
			}

			return stateCFSFCode;
		}

		protected virtual void FillCombinedFederalOrStateCodes(string finYear)
		{
			combinedFederalOrStateCodes = new Dictionary<string, string>();

			if (string.Compare(finYear, "2023") < 0)
			{
				combinedFederalOrStateCodes.Add("AL", "01");
				combinedFederalOrStateCodes.Add("AZ", "04");
				combinedFederalOrStateCodes.Add("AR", "05");
				combinedFederalOrStateCodes.Add("CA", "06");
				combinedFederalOrStateCodes.Add("CO", "07");
				combinedFederalOrStateCodes.Add("CT", "08");
				combinedFederalOrStateCodes.Add("DE", "10");
				combinedFederalOrStateCodes.Add("GA", "13");
				combinedFederalOrStateCodes.Add("HI", "15");
				combinedFederalOrStateCodes.Add("ID", "16");
				combinedFederalOrStateCodes.Add("IN", "18");
				combinedFederalOrStateCodes.Add("KS", "20");
				combinedFederalOrStateCodes.Add("LA", "22");
				combinedFederalOrStateCodes.Add("ME", "23");
				combinedFederalOrStateCodes.Add("MD", "24");
				combinedFederalOrStateCodes.Add("MA", "25");
				combinedFederalOrStateCodes.Add("MI", "26");
				combinedFederalOrStateCodes.Add("MN", "27");
				combinedFederalOrStateCodes.Add("MS", "28");
				combinedFederalOrStateCodes.Add("MO", "29");
				combinedFederalOrStateCodes.Add("MT", "30");
				combinedFederalOrStateCodes.Add("NE", "31");
				combinedFederalOrStateCodes.Add("NJ", "34");
				combinedFederalOrStateCodes.Add("NM", "35");
				combinedFederalOrStateCodes.Add("NC", "37");
				combinedFederalOrStateCodes.Add("ND", "38");
				combinedFederalOrStateCodes.Add("OH", "39");
				combinedFederalOrStateCodes.Add("OK", "40");
				combinedFederalOrStateCodes.Add("SC", "45");
				combinedFederalOrStateCodes.Add("WI", "55");
			}
			else
			{
				combinedFederalOrStateCodes.Add("AL", "01");
				combinedFederalOrStateCodes.Add("AZ", "04");
				combinedFederalOrStateCodes.Add("AR", "05");
				combinedFederalOrStateCodes.Add("CA", "06");
				combinedFederalOrStateCodes.Add("CO", "07");
				combinedFederalOrStateCodes.Add("CT", "08");
				combinedFederalOrStateCodes.Add("DE", "10");
				combinedFederalOrStateCodes.Add("PA", "11");
				combinedFederalOrStateCodes.Add("GA", "13");
				combinedFederalOrStateCodes.Add("HI", "15");
				combinedFederalOrStateCodes.Add("ID", "16");
				combinedFederalOrStateCodes.Add("IN", "18");
				combinedFederalOrStateCodes.Add("KS", "20");
				combinedFederalOrStateCodes.Add("LA", "22");
				combinedFederalOrStateCodes.Add("ME", "23");
				combinedFederalOrStateCodes.Add("MD", "24");
				combinedFederalOrStateCodes.Add("MA", "25");
				combinedFederalOrStateCodes.Add("MI", "26");
				combinedFederalOrStateCodes.Add("MN", "27");
				combinedFederalOrStateCodes.Add("MS", "28");
				combinedFederalOrStateCodes.Add("MO", "29");
				combinedFederalOrStateCodes.Add("MT", "30");
				combinedFederalOrStateCodes.Add("NE", "31");
				combinedFederalOrStateCodes.Add("NJ", "34");
				combinedFederalOrStateCodes.Add("NM", "35");
				combinedFederalOrStateCodes.Add("NC", "37");
				combinedFederalOrStateCodes.Add("ND", "38");
				combinedFederalOrStateCodes.Add("OH", "39");
				combinedFederalOrStateCodes.Add("OK", "40");
				combinedFederalOrStateCodes.Add("DC", "42");
				combinedFederalOrStateCodes.Add("SC", "45");
				combinedFederalOrStateCodes.Add("WI", "55");
			}
		}

		protected virtual string GetDirectSaleIndicator(int VendorID, string FinYear)
		{
			using (new PXReadBranchRestrictedScope(MarkedOrganizationIDs, MarkedBranchIDs, requireAccessForAllSpecified:true))
			{
				foreach (PXResult<AP1099History, AP1099Box> dataRec in PXSelectJoinGroupBy<AP1099History,
					InnerJoin<AP1099Box, On<AP1099Box.boxNbr, Equal<AP1099History.boxNbr>>>,
					Where<AP1099History.vendorID, Equal<Required<AP1099History.vendorID>>,
						And<AP1099History.boxNbr, Equal<Required<AP1099History.boxNbr>>,
						And<AP1099History.finYear, Equal<Required<AP1099History.finYear>>>>>,
					Aggregate<GroupBy<AP1099History.boxNbr, Sum<AP1099History.histAmt>>>>.Select(this, VendorID, 9, FinYear))
				{
					return (((AP1099History)dataRec).HistAmt >= ((AP1099Box)dataRec).MinReportAmt) ? "1" : string.Empty;
				}
			}
			return string.Empty;
		}
		protected class AP1099OrganizationDefinition : IPrefetchable<PXFilter<MISC1099EFileFilter>>
		{
			public Dictionary<int, Organization> All1099 = null;
			public Dictionary<int, Organization> ForReporting = null;

			public void Prefetch(PXFilter<MISC1099EFileFilter> filter)
			{
				List<Organization> organizations = PXSelectorAttribute.SelectAll<MISC1099EFileFilter.organizationID>(filter.Cache, filter.Current)
					.RowCast<Organization>()
					.ToList();

				All1099 = organizations.ToDictionary(o => (int)o.OrganizationID);
				ForReporting = organizations
					.Where(o => o.Reporting1099 == true)
					.ToDictionary(o => (int)o.OrganizationID);
			}
		}

		protected class AP1099BranchDefinition : IPrefetchable<PXFilter<MISC1099EFileFilter>>
		{
			public Dictionary<int, Branch> Available = null;

			public void Prefetch(PXFilter<MISC1099EFileFilter> filter)
			{
				Available = SelectFrom<Branch>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>>
					.Where<Organization.reporting1099ByBranches.IsEqual<True>
						.And<Branch.reporting1099.IsEqual<True>>
						.And<Organization.active.IsEqual<True>>
						.And<Branch.active.IsEqual<True>>
						.And<MatchWithBranch<Branch.branchID>>>
					.View
					.Select(filter.Cache.Graph)
					.RowCast<Branch>()
					.ToDictionary(b => (int)b.BranchID);
			}
		}
	}

	public interface I1099Settings
	{
		int? BAccountID { get; set; }
		string TCC { get; set; }
		bool? ForeignEntity { get; set; }
		bool? CFSFiler { get; set; }
		string ContactName { get; set; }
		string CTelNumber { get; set; }
		string CEmail { get; set; }
		string NameControl { get; set; }
	}

	public interface I1099Record
	{
		void WriteToFile(StreamWriter writer, YearFormat yearFormat);
	}

	public static class StringBuilderExtensions
	{
		public static StringBuilder Append(this StringBuilder str, string value, int startPosition, int fieldLength, PaddingEnum paddingStyle = PaddingEnum.None, char paddingChar = ' ', AlphaCharacterCaseEnum alphaCharacterCaseStyle = AlphaCharacterCaseEnum.Upper, string regexReplacePattern = "")
		{
			string outputString = value ?? String.Empty;

			if(str.Length+1 != startPosition)
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.RecordCompilationError, str.ToString(0,1), startPosition));
			}

			//Remove Special Characters
			if (!string.IsNullOrEmpty(regexReplacePattern))
			{
				outputString = Regex.Replace(@outputString, regexReplacePattern, string.Empty);
			}

			//Read value as per length and read position
			outputString = outputString.Length > fieldLength ? outputString.Substring(0, fieldLength) : outputString;

			switch (alphaCharacterCaseStyle)
			{
				case AlphaCharacterCaseEnum.Lower:
					outputString = outputString.ToLower();
					break;
				case AlphaCharacterCaseEnum.Upper:
					outputString = outputString.ToUpper();
					break;
			}

			if (outputString.Length < fieldLength)
			{
				outputString = paddingStyle == PaddingEnum.Left ? outputString.PadLeft(fieldLength, paddingChar) : outputString.PadRight(fieldLength, paddingChar);
			}

			return str.Append(outputString);
		}

		public static StringBuilder Append(this StringBuilder str, decimal value, int startPosition, int fieldLength, PaddingEnum paddingStyle = PaddingEnum.None, char paddingChar = ' ', AlphaCharacterCaseEnum alphaCharacterCaseStyle = AlphaCharacterCaseEnum.Upper, string regexReplacePattern = "")
		{
			string outputString = Convert.ToString(value);
			outputString = outputString.Replace(".", "");

			return Append(str, outputString, startPosition, fieldLength, paddingStyle, paddingChar, alphaCharacterCaseStyle, regexReplacePattern);
		}
	}

	public enum YearFormat
	{
		F2020,
		F2021
	}
}
