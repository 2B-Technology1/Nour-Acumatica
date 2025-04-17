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
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.IN.Turnover
{
	[PXCacheName(Messages.INTurnoverCalcFilter)]
	public class INTurnoverCalcFilter : IBqlTable
	{
		#region Action
		public abstract class action : BqlString.Field<action>
		{
			public const string None = "NONE";
			public const string Calculate = "CALC";
			public const string Delete = "DEL";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					(None, Messages.NonePlaceholder),
					(Calculate, Messages.CalculateTurnover),
					(Delete, Messages.DeleteTurnover))
				{ }
			}

			public class calculate: BqlString.Constant<calculate>
			{
				public calculate(): base(Calculate) { }
			}
		}
		[action.List]
		[PXDBString(4)]
		[PXUIField(DisplayName = "Action", Required = true)]
		[PXDefault(action.None)]
		public virtual string Action { get; set; }
		#endregion

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
		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false, Required = true)]
		public virtual int? OrgBAccountID { get; set; }
		public abstract class orgBAccountID : BqlInt.Field<orgBAccountID> { }
		#endregion

		#region UseMasterCalendar
		[PXDBBool]
		public bool? UseMasterCalendar { get; set; }
		public abstract class useMasterCalendar : BqlBool.Field<useMasterCalendar> { }
		#endregion

		#region FromPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(branchID),
			organizationSourceType: typeof(organizationID),
			useMasterCalendarSourceType: typeof(useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "From Period", Required = false)]
		public virtual string FromPeriodID { get; set; }
		public abstract class fromPeriodID : BqlString.Field<fromPeriodID> { }
		#endregion

		#region ToPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(branchID),
			organizationSourceType: typeof(organizationID),
			useMasterCalendarSourceType: typeof(useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "To Period", Required = false)]
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
		public virtual string ToPeriodID { get; set; }
		public abstract class toPeriodID : BqlString.Field<toPeriodID> { }
		#endregion

		#region NumberOfPeriods
		[PXDBInt]
		[PXUIField(DisplayName = "Calculate for Last N Period(s)", Visible = false)]
		public virtual int? NumberOfPeriods { get; set; }
		public abstract class numberOfPeriods : BqlInt.Field<numberOfPeriods> { }
		#endregion

		#region CalculateBy
		public abstract class calculateBy : BqlString.Field<action>
		{
			public const string None = "NONE";
			public const string Period = "PERIOD";
			public const string Year = "YEAR";
			public const string Range = "RANGE";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					(None, Messages.NonePlaceholder),
					(Period, Messages.CalculateByPeriod),
					(Year, Messages.CalculateByYear),
					(Range, Messages.CalculateByRange))
				{ }
			}

			public class range : BqlString.Constant<range>
			{
				public range() : base(Range) { }
			}
		}
		[calculateBy.List]
		[PXDBString(6)]
		[PXUIField(DisplayName = "Calculate By")]
		[PXDefault(typeof(Switch<Case<Where<action.IsEqual<action.calculate>>, calculateBy.range>, Null>))]
		public virtual string CalculateBy { get; set; }
		#endregion
	}
}
