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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.PO;
using System.Collections.Generic;

namespace PX.Objects.Common.DAC.ReportParameters
{
	[PXHidden]
	public class VendorReportParameters : IBqlTable
	{
		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public const string Summary = "S";
				public const string Details = "D";

				public ListAttribute() : base(GetAllowedValues(), GetAllowedLabels()) { }

				public static string[] GetAllowedValues()
				{
					List<string> allowedValues = new List<string>
					{
						Summary,
						Details
					};
					return allowedValues.ToArray();
				}

				public static string[] GetAllowedLabels()
				{
					List<string> allowedLabels = new List<string>
					{
						AP.Messages.Summary,
						AP.Messages.Details
					};
					return allowedLabels.ToArray();
				}
			}
		}

		[format.List()]
		[PXDBString(2)]
		[PXUIField(DisplayName = AP.Messages.Format, Visibility = PXUIVisibility.SelectorVisible)]
		public string Format { get; set; }
		#endregion

		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlInt.Field<vendorClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<
			VendorClass.vendorClassID,
			LeftJoin<EPEmployeeClass,
				On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>,
			Where<EPEmployeeClass.vendorClassID, IsNull>>))]
		public string VendorClassID { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[Vendor]
		[PXRestrictor(typeof(Where<Vendor.vStatus, NotEqual<VendorStatus.inactive>>),
			AP.Messages.VendorIsInStatus,
			typeof(Vendor.vStatus))]
		public int? VendorID { get; set; }
		#endregion

		#region VendorActiveID
		public abstract class vendorActiveID : PX.Data.BQL.BqlInt.Field<vendorActiveID> { }

		[VendorActive(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true)]
		public int? VendorActiveID { get; set; }
		#endregion

		#region VendorIDPOReceipt
		public abstract class vendorIDPOReceipt : PX.Data.BQL.BqlInt.Field<vendorIDPOReceipt> { }
		[Vendor(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>),
			Visibility = PXUIVisibility.SelectorVisible,
			CacheGlobal = true,
			Filterable = true)]
		[VerndorNonEmployeeOrOrganizationRestrictor(typeof(POReceipt.receiptType))]
		[PXRestrictor(
			typeof(Where<
				Vendor.vStatus, IsNull,
				Or<Vendor.vStatus, Equal<VendorStatus.active>,
				Or<Vendor.vStatus, Equal<VendorStatus.oneTime>>>>),
			AP.Messages.VendorIsInStatus,
			typeof(Vendor.vStatus))]

		public virtual int? VendorIDPOReceipt {	get; set; }
		#endregion

		#region VendorIDNonEmployeeActive
		public abstract class vendorIDNonEmployeeActive : PX.Data.BQL.BqlInt.Field<vendorIDNonEmployeeActive> { }

		[VendorNonEmployeeActive(
			Visibility = PXUIVisibility.SelectorVisible, 
			DescriptionField = typeof(Vendor.acctName), 
			CacheGlobal = true, 
			Filterable = true)]
		public virtual int? VendorIDNonEmployeeActive { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[APActiveProject]
		public int? ProjectID { get; set; }
		#endregion ProjectID
	}
}
