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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRDeductCodeDetail)]
	[Serializable]
	public class PRDeductCodeDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRDeductCodeDetail>.By<codeID, taxID>
		{
			public static PRDeductCodeDetail Find(PXGraph graph, int? codeID, int? taxID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, codeID, taxID, options);
		}

		public static class FK
		{
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRDeductCodeDetail>.By<codeID> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PRDeductCodeDetail>.By<taxID> { }
		}
		#endregion

		#region CodeID
		public abstract class codeID : BqlInt.Field<codeID> { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXUIField(DisplayName = "Code ID")]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<PRDeductCodeDetail.codeID>>>>))]
		public int? CodeID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : BqlInt.Field<taxID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(
			SearchFor<PRTaxCode.taxID>
				.Where<PRTaxCode.countryID.IsEqual<PRDeductCode.countryID.FromCurrent>>),
			SubstituteKey = typeof(PRTaxCode.taxCD),
			DescriptionField = typeof(PRTaxCode.description))]
		public int? TaxID { get; set; }
		#endregion
		#region IsDeductionPreTax
		[PXDBBool]
		[PXUIField(DisplayName = "Deduction decreases taxable wage")]
		[PXDefault(false)]
		[PXUIVisible(typeof(Where<Parent<PRDeductCode.contribType>, NotEqual<ContributionTypeListAttribute.employerContribution>>))]
		[PXUIEnabled(typeof(Where<Parent<PRDeductCode.benefitTypeCDCAN>, Equal<customDeductionType>>))]
		public bool? IsDeductionPreTax { get; set; }
		public abstract class isDeductionPreTax : BqlBool.Field<isDeductionPreTax> { }
		#endregion
		#region IsBenefitTaxable
		[PXDBBool]
		[PXUIField(DisplayName = "Benefit increases taxable wage")]
		[PXDefault(false)]
		[PXUIVisible(typeof(Where<Parent<PRDeductCode.contribType>, NotEqual<ContributionTypeListAttribute.employeeDeduction>>))]
		public bool? IsBenefitTaxable { get; set; }
		public abstract class isBenefitTaxable : BqlBool.Field<isBenefitTaxable> { }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}

	public class customDeductionType : PX.Data.BQL.BqlInt.Constant<customDeductionType>
	{
		public customDeductionType() : base(DeductionType.CustomDeductionType) { }
	}
}
