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
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A benefit which increases the applicable wage for another deduction or benefit.
	/// </summary>
	[PXCacheName(Messages.PRDeductCodeBenefitIncreasingWage)]
	[Serializable]
	public class PRDeductCodeBenefitIncreasingWage : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRDeductCodeBenefitIncreasingWage>.By<deductCodeID, applicableBenefitCodeID>
		{
			public static PRDeductCodeBenefitIncreasingWage Find(PXGraph graph, int? deductCodeID, int? applicableBenefitCodeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, deductCodeID, applicableBenefitCodeID, options);
		}

		public static class FK
		{
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRDeductCodeBenefitIncreasingWage>.By<deductCodeID> { }
			public class ApplicableBenefitCode : PRDeductCode.PK.ForeignKeyOf<PRDeductCodeBenefitIncreasingWage>.By<applicableBenefitCodeID> { }
		}
		#endregion

		#region DeductCodeID
		public abstract class deductCodeID : BqlInt.Field<deductCodeID> { }
		/// <summary>
		/// The unique identifier of the deduction or benefit code having its applicable wage increased.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<deductCodeID>>>>))]
		public virtual int? DeductCodeID { get; set; }
		#endregion
		#region ApplicableBenefitCodeID
		public abstract class applicableBenefitCodeID : BqlInt.Field<applicableBenefitCodeID> { }
		/// <summary>
		/// The unique identifier of the benefit code increasing the applicable wage.
		/// The field is included in <see cref="FK.ApplicableBenefitCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Benefit Code")]
		[PXSelector(typeof(SearchFor<PRDeductCode.codeID>
			.Where<PRDeductCode.contribType.IsNotEqual<ContributionTypeListAttribute.employeeDeduction>
				.And<PRDeductCode.affectsTaxes.IsEqual<True>>
				.And<PRDeductCode.countryID.IsEqual<PRDeductCode.countryID.FromCurrent>>
				.And<PRDeductCode.codeID.IsNotEqual<PRDeductCode.codeID.FromCurrent>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXForeignReference(typeof(Field<applicableBenefitCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		public virtual int? ApplicableBenefitCodeID { get; set; }
		#endregion

		#region ApplicableBenefitCodeCountryID
		public abstract class applicableBenefitCodeCountryID : BqlString.Field<applicableBenefitCodeCountryID> { }
		[PXString(2)]
		[PXDBScalar(typeof(SearchFor<PRDeductCode.countryID>.Where<PRDeductCode.codeID.IsEqual<applicableBenefitCodeID>>))]
		[PXUnboundDefault(typeof(Selector<applicableBenefitCodeID, PRDeductCode.countryID>))]
		[PXFormula(typeof(Default<applicableBenefitCodeID>))]
		public virtual string ApplicableBenefitCodeCountryID { get; set; }
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
