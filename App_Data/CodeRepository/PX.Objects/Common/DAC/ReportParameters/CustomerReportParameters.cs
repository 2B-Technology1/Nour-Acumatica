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
using PX.Objects.AR;
using PX.Objects.CR;
using System.Collections.Generic;

namespace PX.Objects.Common.DAC.ReportParameters
{
	[PXHidden]
	public class CustomerReportParameters : IBqlTable
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
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlInt.Field<customerClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CustomerClass.customerClassID>))]
		public string CustomerClassID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[Customer()]
		[PXDefault()]
		public int? CustomerID { get; set; }
		#endregion

		#region CustomerIDByCustomerClass
		public abstract class customerIDByCustomerClass : PX.Data.BQL.BqlInt.Field<customerIDByCustomerClass> { }

		[PXDBInt]
		[PXDimensionSelector(CustomerAttribute.DimensionName, typeof(Search<
			Customer.bAccountID, 
			Where<Customer.customerClassID, Equal<Optional<CustomerReportParameters.customerClassID>>,
				Or<Optional<CustomerReportParameters.customerClassID>, IsNull>>>),
				typeof(BAccountR.acctCD),
				typeof(BAccountR.acctCD),
				typeof(Customer.acctName),
				typeof(Customer.customerClassID),
				typeof(Customer.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID))]
		public int? CustomerIDByCustomerClass { get; set; }
		#endregion
	}
}
