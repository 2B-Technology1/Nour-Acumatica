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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.AP
{
	public class AP1099PayerTreeSelect : PXSelectOrganizationTree
	{
		public AP1099PayerTreeSelect(PXGraph graph) : base(graph) { }

		public AP1099PayerTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public override IEnumerable tree([PXString] string AcctCD)
		{
			List<BranchItem> result = new List<BranchItem>();
			foreach (PXResult<BAccountR, Branch, Organization> row in
				SelectFrom<BAccountR>
					.LeftJoin<Branch>
						.On<BAccountR.bAccountID.IsEqual<Branch.bAccountID>>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.Or<BAccountR.bAccountID.IsEqual<Organization.bAccountID>>>
					.Where<Brackets<Branch.branchID.IsNull
							.Or<Branch.bAccountID.IsEqual<Organization.bAccountID>>
							.Or<Branch.branchID.IsNotNull
								.And<Organization.reporting1099ByBranches.IsEqual<True>>>>
						.And<MatchWithBranch<Branch.branchID>>
						.And<MatchWithOrganization<Organization.organizationID>>
						.And<Branch.branchID.IsNull.Or<Branch.active.IsEqual<True>>>
						.And<Organization.organizationID.IsNull.Or<Organization.active.IsEqual<True>>>>
					.View
					.Select(_Graph))
			{
				BAccountR bAccount = row;
				Branch branch = row;
				Organization organization = row;

				BranchItem item = new BranchItem
				{
					BAccountID = bAccount.BAccountID,
					AcctCD = bAccount.AcctCD,
					AcctName = bAccount.AcctName,
					CanSelect = true
				};

				if (branch?.BAccountID != null && organization.BAccountID != branch.BAccountID)
				{
					item.ParentBAccountID = PXAccess.GetParentOrganization(branch.BranchID).BAccountID;
				}

				item.CanSelect = !(organization.Reporting1099ByBranches == true && item.BAccountID == organization.BAccountID);

				result.Add(item);
			}
			return result;
		}
	}

	public class AP1099ReportingPayerTreeSelect : PXSelectOrganizationTree
	{
		public AP1099ReportingPayerTreeSelect(PXGraph graph) : base(graph) {}

		public AP1099ReportingPayerTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) {}

		public override IEnumerable tree([PXString] string AcctCD)
		{
			List<BranchItem> result = new List<BranchItem>();
			List<(Branch Branch, BAccountR BAccount)> branches =
				SelectFrom<Branch>
					.InnerJoin<BAccountR>
						.On<Branch.bAccountID.IsEqual<BAccountR.bAccountID>>
					.Where<Branch.active.IsEqual<True>
						.And<MatchWithBranch<Branch.branchID>>>
				.View
				.Select(_Graph)
				.AsEnumerable()
				.Cast<PXResult<Branch, BAccountR>>()
				.Select(row => ((Branch)row, (BAccountR)row))
				.ToList();

			foreach (PXResult<Organization, BAccountR> orgBAccountPair in
				SelectFrom<Organization>
					.InnerJoin<BAccountR>
						.On<Organization.bAccountID.IsEqual<BAccountR.bAccountID>>
					.Where<Organization.active.IsEqual<True>
						.And<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithOrganization<Organization.organizationID>>>
				.View
				.Select(_Graph))
			{
				Organization organization = orgBAccountPair;
				BAccountR orgBAccount = orgBAccountPair;
				bool addOrganization = true;

				if (organization.Reporting1099ByBranches == true)
				{
					addOrganization = false;
					foreach((Branch Branch, BAccountR BAccount) branchBAccountPair in branches
						.Where(pair => pair.Branch.OrganizationID == organization.OrganizationID && pair.Branch.Reporting1099 == true))
					{
						addOrganization = true;
						result.Add(new BranchItem
						{
							BAccountID = branchBAccountPair.BAccount.BAccountID,
							AcctCD = branchBAccountPair.BAccount.AcctCD,
							AcctName = branchBAccountPair.BAccount.AcctName,
							ParentBAccountID = organization.BAccountID
						});
					}
				}

				if(addOrganization)
				{
					result.Add(new BranchItem
					{
						BAccountID = orgBAccount.BAccountID,
						AcctCD = orgBAccount.AcctCD,
						AcctName = orgBAccount.AcctName,
						CanSelect = !(organization.Reporting1099ByBranches == true && orgBAccount.BAccountID == organization.BAccountID)
					});
				}
			}
			return result;
		}
	}

	[Serializable]
	public class MISC1099EFileFilter : IBqlTable
	{
		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		// Selector need for slot
		[Organization(
			onlyActive: true,
			defaultingSource: typeof(Coalesce<
				SearchFor<Organization.organizationID>
				.In<
					SelectFrom<Organization>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Branch.branchID.IsEqual<AccessInfo.branchID.FromCurrent>
						.And<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithBranch<Branch.branchID>>>>,
				SearchFor<Organization.organizationID>
				.In<
					SelectFrom<Organization>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Brackets<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithBranch<Branch.branchID>>>>>),
			DisplayName = "Transmitter Company")]
		[PXRestrictor(
			typeof(Where<Organization.reporting1099.IsEqual<True>
				.Or<Organization.reporting1099ByBranches.IsEqual<True>>>), 
			Messages.EFilingIsAvailableOnlyCompaniesWithEnabled1099)]
		public virtual int? OrganizationID { get; set; }

		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		// Selector need for slot
		[Branch(
			sourceType: null,
			searchType: typeof(SearchFor<Branch.branchID>
			.In<
				SelectFrom<Branch>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.And<Organization.reporting1099ByBranches.IsEqual<True>>>
					.Where<Branch.organizationID.IsEqual<MISC1099EFileFilter.organizationID.FromCurrent>
						.And<MatchWithBranch<Branch.branchID>>>>),
			useDefaulting: false,
			DisplayName = "Transmitter Branch")]
		[PXRestrictor(typeof(Where<Branch.reporting1099.IsEqual<True>>), Messages.EFilingIsAvailableOnlyBranchWithEnabled1099)]
		[PXUIEnabled(typeof(Where<Selector<organizationID, Organization.reporting1099ByBranches>, Equal<True>>))]
		[PXUIRequired(typeof(Where<Selector<organizationID, Organization.reporting1099ByBranches>, Equal<True>>))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(
			sourceOrganizationID: typeof(organizationID),
			sourceBranchID: typeof(branchID),
			treeDataMember: typeof(AP1099ReportingPayerTreeSelect),
			onlyActive: true,
			DisplayName = "Transmitter",
			SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region FinYear

		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> 
		{
			public const string NECAvailable1099Year = "2020";
			public class nECAvailable1099Year : BqlString.Constant<nECAvailable1099Year>
			{
				public nECAvailable1099Year() : base(NECAvailable1099Year) { }
			}

		}
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "1099 Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear,
			Where<AP1099Year.organizationID, Equal<Optional<MISC1099EFileFilter.organizationID>>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
		#region FileFormat
		public abstract class fileFormat : BqlString.Field<fileFormat>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { MISC, NEC },
					new string[] { Messages.MISC, Messages.NEC })
				{ }
			}
			public const string MISC = "M";
			public const string NEC = "N";

			public class mISC : BqlString.Constant<mISC>
			{
				public mISC() : base(MISC) { }
			}

			public class nEC : BqlString.Constant<nEC>
			{
				public nEC() : base(NEC) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "File Format")]
		[fileFormat.List]
		[PXDefault(fileFormat.MISC)]
		public virtual string FileFormat { get; set; }
		#endregion
		#region Include
		public abstract class include : PX.Data.BQL.BqlString.Field<include>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { TransmitterOnly, AllMarkedOrganizations },
					new string[] { Messages.TransmitterOnly, Messages.AllMarkedOrganizations })
				{ }
			}
			public const string TransmitterOnly = "T";
			public const string AllMarkedOrganizations = "A";

			public class transmitterOnly : PX.Data.BQL.BqlString.Constant<transmitterOnly>
			{
				public transmitterOnly() : base(TransmitterOnly) { }
			}

			public class allMarkedOrganizations : PX.Data.BQL.BqlString.Constant<allMarkedOrganizations>
			{
				public allMarkedOrganizations() : base(AllMarkedOrganizations) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Prepare For")]
		[include.List]
		[PXDefault(include.TransmitterOnly)]
		public virtual string Include { get; set; }
		#endregion
		#region Box7
		public abstract class box7 : PX.Data.BQL.BqlString.Field<box7>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Box7All, Box7Equal, Box7NotEqual },
					new string[] { Messages.Box7All, Messages.Box7Equal, Messages.Box7NotEqual }) { }
			}

			public const string Box7All = "AL";
			public const string Box7Equal = "EQ";
			public const string Box7NotEqual = "NE";

			public class box7All : PX.Data.BQL.BqlString.Constant<box7All>
			{
				public box7All() : base(Box7All) { }
			}

			public class box7Equal : PX.Data.BQL.BqlString.Constant<box7Equal>
			{
				public box7Equal() : base(Box7Equal) { }
			}

			public class box7NotEqual : PX.Data.BQL.BqlString.Constant<box7NotEqual>
			{
				public box7NotEqual() : base(Box7NotEqual) { }
			}

			public class box7Nbr : PX.Data.BQL.BqlShort.Constant<box7Nbr>
			{
				public box7Nbr() : base(7) { }
			}
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Include")]
		[box7.List]
		[PXDefault(box7.Box7All)]
		[PXUIEnabled(typeof(Where<finYear.IsLess<finYear.nECAvailable1099Year>>))]
		public virtual string Box7 { get; set; }
		#endregion

		#region IsPriorYear

		public abstract class isPriorYear : PX.Data.BQL.BqlBool.Field<isPriorYear> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prior Year")]
		public virtual bool? IsPriorYear { get; set; }

		#endregion

		#region IsCorrectionReturn

		public abstract class isCorrectionReturn : PX.Data.BQL.BqlBool.Field<isCorrectionReturn> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Correction File")]
		public virtual bool? IsCorrectionReturn { get; set; }

		#endregion

		#region IsLastFiling

		public abstract class isLastFiling : PX.Data.BQL.BqlBool.Field<isLastFiling> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Last Filing")]
		public virtual bool? IsLastFiling { get; set; }

		#endregion

		#region ReportingDirectSalesOnly

		public abstract class reportingDirectSalesOnly : PX.Data.BQL.BqlBool.Field<reportingDirectSalesOnly> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIEnabled(typeof(Where<fileFormat.IsEqual<fileFormat.mISC>.Or<finYear.IsGreater<finYear.nECAvailable1099Year>>>))]
		[PXFormula(typeof(Switch<Case<Where<fileFormat.IsEqual<fileFormat.nEC>.And<finYear.IsLessEqual<finYear.nECAvailable1099Year>>>, False>, reportingDirectSalesOnly>))]
		[PXUIField(DisplayName = "Direct Sales Only")]
		public virtual bool? ReportingDirectSalesOnly { get; set; }

		#endregion

		#region IsTestMode

		public abstract class isTestMode : PX.Data.BQL.BqlBool.Field<isTestMode> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Test File")]
		public virtual bool? IsTestMode { get; set; }

		#endregion

		#region CountryID
		public abstract class countryID : BqlString.Field<countryID> { }

		[PXDBString(100)]
		public virtual string CountryID { get; set; }
		#endregion
	}
}
