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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Maximum insurable wages associated with the workers' compensation codes. It will limit the amount calculated on the Paychecks and Adjustments (PR302000) form. The information will be displayed on the Workers' Compensation Codes (PR209800) form.
	/// </summary>
	[PXCacheName(Messages.PRWorkCompensationBenefitRate)]
	[Serializable]
	public class PRWorkCompensationMaximumInsurableWage : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRWorkCompensationMaximumInsurableWage>.By<workCodeID, deductCodeID, effectiveDate>
		{
			public static PRWorkCompensationMaximumInsurableWage Find(PXGraph graph, string workCodeID, int? deductCodeID, DateTime? effectiveDate, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, workCodeID, deductCodeID, effectiveDate, options);
		}

		public static class FK
		{
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PRWorkCompensationMaximumInsurableWage>.By<workCodeID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRWorkCompensationMaximumInsurableWage>.By<deductCodeID> { }
		}
		#endregion

		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[MaximumInsurableWageWorkCode(IsKey = true)]
		[PXDefault(typeof(PMWorkCode.workCodeID))]
		[PXParent(typeof(FK.WorkCode))]
		public string WorkCodeID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction and Benefit Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<True>>), typeof(workCodeCountryID))]
		[PXParent(typeof(FK.DeductionCode))]
		public int? DeductCodeID { get; set; }
		#endregion
		#region MaximumInsurableWage
		public abstract class maximumInsurableWage : PX.Data.BQL.BqlDecimal.Field<maximumInsurableWage> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Wage Limit")]
		public decimal? MaximumInsurableWage { get; set; }
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		[PXDBDate(IsKey = true)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Effective Date")]
		[PXCheckUnique(typeof(workCodeID), typeof(deductCodeID), UniqueKeyIsPartOfPrimaryKey = true)]
		public virtual DateTime? EffectiveDate { get; set; }
		#endregion

		#region WorkCodeCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(SearchFor<PRxPMWorkCode.countryID>
			.Where<PMWorkCode.workCodeID.IsEqual<workCodeID.FromCurrent>>))]
		public virtual string WorkCodeCountryID { get; set; }
		public abstract class workCodeCountryID : PX.Data.BQL.BqlString.Field<workCodeCountryID> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
