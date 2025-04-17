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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;
using System.Diagnostics;

namespace PX.Objects.EP
{
	/// <summary>
	/// Stores information on the rate associated with different shifts. The information will be displayed on the Shift Codes (EP103000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.EPShiftCodeRate)]
	[DebuggerDisplay("{GetType().Name,nq}: ShiftID = {ShiftID,nq}, EffectiveDate = {EffectiveDate,nq}, CuryID = {CuryID,nq}")]
	public class EPShiftCodeRate : IBqlTable
	{
		public class PK : PrimaryKeyOf<EPShiftCodeRate>.By<shiftID, effectiveDate, curyID>
		{
			public static EPShiftCodeRate Find(PXGraph graph, int? shiftID, DateTime? effectiveDate, string curyID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, shiftID, effectiveDate, curyID, options);
		}

		public static class FK
		{
			public class ShiftCode : EPShiftCode.PK.ForeignKeyOf<EPShiftCodeRate>.By<shiftID> { }
		}

		#region ShiftID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(EPShiftCode.shiftID))]
		[PXParent(typeof(FK.ShiftCode))]
		public virtual int? ShiftID { get; set; }
		public abstract class shiftID : BqlInt.Field<shiftID> { }
		#endregion

		#region EffectiveDate
		[PXDBDate(IsKey = true)]
		[PXUIField(DisplayName = "Effective Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EffectiveDate { get; set; }
		public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(10, IsKey = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Required = false)]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		public virtual String CuryID { get; set; }
		#endregion

		#region Type
		[PXDBString]
		[PXUIField(DisplayName = "Type")]
		[EPShiftCodeType.List]
		[PXDefault(EPShiftCodeType.Amount)]
		public virtual string Type { get; set; }
		public abstract class type : BqlString.Field<type> { }
		#endregion

		#region Percent
		[PXDBDecimal(2, MinValue = 0)]
		[PXUIField(DisplayName = "Percent")]
		[ShowValueWhen(typeof(Where<type.IsEqual<EPShiftCodeType.percent>>))]
		public virtual decimal? Percent { get; set; }
		public abstract class percent : BqlDecimal.Field<percent> { }
		#endregion

		#region WageAmount
		[PXDBDecimal(2, MinValue = 0)]
		[PXUIField(DisplayName = "Wage Amount" , FieldClass = nameof(FeaturesSet.PayrollModule))]
		[ShowValueWhen(typeof(Where<type.IsEqual<EPShiftCodeType.amount>>))]
		public virtual decimal? WageAmount { get; set; }
		public abstract class wageAmount : BqlDecimal.Field<wageAmount> { }
		#endregion

		#region CostingAmount
		[PXDBDecimal(2, MinValue = 0)]
		[ShiftAmountName]
		[ShowValueWhen(typeof(Where<type.IsEqual<EPShiftCodeType.amount>>))]
		public virtual decimal? CostingAmount { get; set; }
		public abstract class costingAmount : BqlDecimal.Field<costingAmount> { }
		#endregion

		#region BurdenAmount
		[PXDecimal(2)]
		[PXUIField(DisplayName = "Burden Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollModule))]
		[ShowValueWhen(typeof(Where<type.IsEqual<EPShiftCodeType.amount>>))]
		[PXDependsOnFields(typeof(costingAmount), typeof(wageAmount))]
		public virtual decimal? BurdenAmount => CostingAmount - WageAmount;
		public abstract class burdenAmount : BqlDecimal.Field<burdenAmount> { }
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

	public class ShiftAmountNameAttribute : PXUIFieldAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			bool payrollModuleInstalled = PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
			DisplayName = payrollModuleInstalled ? Messages.ShiftAmountNameWithPayroll : Messages.ShiftAmountNameWithoutPayroll;

			base.CacheAttached(sender);
		}
	}
}
