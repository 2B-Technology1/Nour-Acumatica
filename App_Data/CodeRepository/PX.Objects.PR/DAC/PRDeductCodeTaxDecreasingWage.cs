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
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A tax which decreases the applicable wage for a certain deduction or benefit.
	/// </summary>
	[PXCacheName(Messages.PRDeductCodeTaxDecreasingWage)]
	[Serializable]
	public class PRDeductCodeTaxDecreasingWage : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRDeductCodeTaxDecreasingWage>.By<deductCodeID, applicableTaxID>
		{
			public static PRDeductCodeTaxDecreasingWage Find(PXGraph graph, int? deductCodeID, int? applicableTaxID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, deductCodeID, applicableTaxID, options);
		}

		public static class FK
		{
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRDeductCodeTaxDecreasingWage>.By<deductCodeID> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PRDeductCodeTaxDecreasingWage>.By<applicableTaxID> { }
		}
		#endregion

		#region DeductCodeID
		public abstract class deductCodeID : BqlInt.Field<deductCodeID> { }
		/// <summary>
		/// The unique identifier of the deduction or benefit code having its applicable wage decreased.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<deductCodeID>>>>))]
		public virtual int? DeductCodeID { get; set; }
		#endregion
		#region ApplicableTaxID
		public abstract class applicableTaxID : BqlInt.Field<applicableTaxID> { }
		/// <summary>
		/// The unique identifier of the tax code.
		/// The field is included in <see cref="FK.TaxCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax Code")]
		[PXSelector(typeof(SearchFor<PRTaxCode.taxID>
				.Where<PRTaxCode.taxCategory.IsEqual<TaxCategory.employeeWithholding>
					.And<PRTaxCode.countryID.IsEqual<PRDeductCode.countryID.FromCurrent>>>),
			SubstituteKey = typeof(PRTaxCode.taxCD),
			DescriptionField = typeof(PRTaxCode.description))]
		[PXForeignReference(typeof(Field<applicableTaxID>.IsRelatedTo<PRTaxCode.taxID>))]
		public virtual int? ApplicableTaxID { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
