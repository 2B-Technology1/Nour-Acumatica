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

using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POAccrualBalanceInquiryFilter)]
    public class POAccrualInquiryFilter: IBqlTable
    {		
		#region OrganizationID
		[Organization(false, Required = false)]
		public virtual int? OrganizationID { get; set; }
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		#endregion

		#region BranchID
		[BranchOfOrganization(typeof(organizationID), false)]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion

		#region OrgBAccountID
		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public virtual int? OrgBAccountID { get; set; }
		public abstract class orgBAccountID : BqlInt.Field<orgBAccountID> { }
		#endregion

		#region VendorID
		[Vendor(DescriptionField = typeof(Vendor.acctName))]
		public virtual int? VendorID { get; set; }
		public abstract class vendorID : BqlInt.Field<vendorID> { }
		#endregion

		#region AcctID
		[Account(null, DescriptionField = typeof(Account.description))]
		[PXDefault]
		public virtual int? AcctID { get; set; }
		public abstract class acctID : BqlInt.Field<acctID> { }
		#endregion

		#region SubID
		[SubAccount(DisplayName = "Sub.", DescriptionField = typeof(Sub.description))]
		public virtual int? SubID { get; set; }
		public abstract class subID : BqlInt.Field<subID> { }
		#endregion

		#region SubCD
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Subaccount", FieldClass = SubAccountAttribute.DimensionName)]
		[PXDimension("SUBACCOUNT", ValidComboRequired = false)]
		public virtual string SubCD { get; set; }
		public abstract class subCD : BqlString.Field<subCD> { }
		#endregion

		#region SubCD Wildcard
		[PXDBString(30, IsUnicode = true)]
		public virtual string SubCDWildcard
		{
			[PXDependsOnFields(typeof(subCD))]
			get
			{
				return SubCDUtils.CreateSubCDWildcard(SubCD, SubAccountAttribute.DimensionName);
			}
            set { }
		}
		public abstract class subCDWildcard : BqlString.Field<subCDWildcard> { };
		#endregion

		#region UseMasterCalendar
		[PXBool]
		public bool? UseMasterCalendar { get; set; }
		public abstract class useMasterCalendar : BqlBool.Field<useMasterCalendar> { }
		#endregion

		#region FinPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(branchID),
			organizationSourceType: typeof(organizationID),
			useMasterCalendarSourceType: typeof(useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "Period")]
		[PXDefault(typeof(Coalesce<
					Search<FinPeriod.finPeriodID,
						Where<FinPeriod.organizationID, Equal<Current<organizationID>>,
							And<FinPeriod.startDate, LessEqual<Current<AccessInfo.businessDate>>>>,
						OrderBy<Desc<FinPeriod.startDate, Desc<FinPeriod.finPeriodID>>>>,
					Search<FinPeriod.finPeriodID,
						Where<FinPeriod.organizationID, Equal<Zero>,
							And<FinPeriod.startDate, LessEqual<Current<AccessInfo.businessDate>>>>,
						OrderBy<Desc<FinPeriod.startDate, Desc<FinPeriod.finPeriodID>>>>>
			))]
		public virtual string FinPeriodID { get; set; }
		public abstract class finPeriodID : BqlString.Field<finPeriodID> { }
		#endregion

		#region ShowByLines
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Show Details by Line")]
		public virtual bool? ShowByLines { get; set; }
		public abstract class showByLines : BqlBool.Field<showByLines> { }
		#endregion

		#region UnbilledAmt
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Total", Enabled = false)]
		public virtual decimal? UnbilledAmt { get; set; }
		public abstract class unbilledAmt : BqlDecimal.Field<unbilledAmt> { }
		#endregion
		#region NotReceivedAmt
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Not Received Total", Enabled = false)]
		public virtual decimal? NotReceivedAmt { get; set; }
		public abstract class notReceivedAmt : BqlDecimal.Field<notReceivedAmt> { }
		#endregion
		#region NotInvoicedAmt
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship Total Not Invoiced", Enabled = false)]
		public virtual decimal? NotInvoicedAmt { get; set; }
		public abstract class notInvoicedAmt : BqlDecimal.Field<notInvoicedAmt> { }
		#endregion
		#region NotAdjustedAmt
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Adjustment Total Not Released", Enabled = false)]
		public virtual decimal? NotAdjustedAmt { get; set; }
		public abstract class notAdjustedAmt : BqlDecimal.Field<notAdjustedAmt> { }
		#endregion
		#region Balance
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "PO Accrued Total", Enabled = false)]
		public virtual decimal? Balance { get; set; }
		public abstract class balance : BqlDecimal.Field<balance> { }
		#endregion
	}
}
