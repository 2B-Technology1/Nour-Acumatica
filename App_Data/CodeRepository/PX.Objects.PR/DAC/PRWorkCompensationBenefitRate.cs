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
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// The benefit rates associated with the workers' compensation codes and used to calculate the benefit amount on the Paychecks and Adjustments (PR302000) form. The information will be displayed on the Workers' Compensation Codes (PR209800) form.
	/// </summary>
	[PXCacheName(Messages.PRWorkCompensationBenefitRate)]
	[Serializable]
	public class PRWorkCompensationBenefitRate : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRWorkCompensationBenefitRate>.By<recordID>
		{
			public static PRWorkCompensationBenefitRate Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, recordID, options);
		}

		public class UK : PrimaryKeyOf<PRWorkCompensationBenefitRate>.By<workCodeID, deductCodeID, effectiveDate, branchID>
		{
			public static PRWorkCompensationBenefitRate Find(PXGraph graph, string workCodeID, int? deductCodeID, DateTime? effectiveDate, int? branchID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, workCodeID, deductCodeID, effectiveDate, branchID, options);
		}

		public static class FK
		{
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PRWorkCompensationBenefitRate>.By<workCodeID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRWorkCompensationBenefitRate>.By<deductCodeID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<PRWorkCompensationBenefitRate>.By<branchID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlString.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public int? RecordID { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[WorkCodeMatchCountry(typeof(deductCodeCountryID), FieldClass = null, DisplayName = "WCC Code")]
		[PXDefault]
		public string WorkCodeID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction and Benefit Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<True>>), typeof(workCodeCountryID))]
		public int? DeductCodeID { get; set; }
		#endregion
		#region DeductionRate
		public abstract class deductionRate : PX.Data.BQL.BqlDecimal.Field<deductionRate> { }
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Rate")]
		[PXUIVisible(typeof(Where<WCDeductionColumnVisibilityEvaluator, Equal<True>>))]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionTypeListAttribute.employerContribution>>))]
		public decimal? DeductionRate
		{
			[PXDependsOnFields(typeof(contribType))]
			get
			{
				if (ContribType != ContributionType.EmployerContribution)
				{
					return _DeductionRate;
				}
				return null;
			}
			set
			{
				_DeductionRate = value;
			}
		}
		private decimal? _DeductionRate;
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Benefit Rate")]
		public decimal? Rate { get; set; }
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Effective Date")]
		public virtual DateTime? EffectiveDate { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[EffectiveDateActive(
			typeof(effectiveDate),
			new Type[] { typeof(workCodeID), typeof(deductCodeID), typeof(branchID), typeof(effectiveDate) },
			typeof(SelectFrom<PRDeductCode>
				.Where<PRDeductCode.codeID.IsEqual<P.AsInt>
					.And<PRDeductCode.isActive.IsEqual<True>>>),
			typeof(deductCodeID))]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(
			searchType: typeof(SearchFor<BranchWithAddress.branchID>),
			useDefaulting: false,
			addDefaultAttribute: false)]
		[PXRestrictor(typeof(Where<deductCodeCountryID.FromCurrent.IsNull.Or<BranchWithAddress.addressCountryID.IsEqual<deductCodeCountryID.FromCurrent>>>),
			Messages.PaymentBranchCountryNotMatching, new[] { typeof(deductCodeCountryID), typeof(BranchWithAddress.addressCountryID) })]
		[PXCheckUnique(typeof(workCodeID), typeof(deductCodeID), typeof(effectiveDate))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		[PXString(3)]
		[PXUIField(DisplayName = "Contribution Type", Visible = false)]
		[PXFormula(typeof(Selector<deductCodeID, PRDeductCode.contribType>))]
		public string ContribType { get; set; }
		#endregion
		#region DeductionCalcType
		public abstract class deductionCalcType : PX.Data.BQL.BqlString.Field<deductionCalcType> { }
		[PXString(3)]
		[DedCntCalculationMethod.List]
		[PXUIField(DisplayName = "Deduction Calculation Method", Enabled = false)]
		[PXUIVisible(typeof(Where<WCDeductionColumnVisibilityEvaluator, Equal<True>>))]
		[PXFormula(typeof(Switch<Case<Where<contribType.IsNotEqual<ContributionTypeListAttribute.employerContribution>>, Selector<deductCodeID, PRDeductCode.dedCalcType>>, Null>))]
		public string DeductionCalcType { get; set; }
		#endregion
		#region WorkCodeCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(SearchFor<PRxPMWorkCode.countryID>
			.Where<PMWorkCode.workCodeID.IsEqual<workCodeID.FromCurrent>>))]
		public virtual string WorkCodeCountryID { get; set; }
		public abstract class workCodeCountryID : PX.Data.BQL.BqlString.Field<workCodeCountryID> { }
		#endregion
		#region DeductCodeCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(SearchFor<PRDeductCode.countryID>
			.Where<PRDeductCode.codeID.IsEqual<deductCodeID.FromCurrent>>))]
		public virtual string DeductCodeCountryID { get; set; }
		public abstract class deductCodeCountryID : PX.Data.BQL.BqlString.Field<deductCodeCountryID> { }
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
