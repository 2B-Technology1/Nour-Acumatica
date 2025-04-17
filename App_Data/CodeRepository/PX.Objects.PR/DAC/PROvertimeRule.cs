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
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the rules that trigger overtime calculation in the paycheck. The information will be displayed on the Overtime Rules (PR104000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PROvertimeRule)]
	[PXPrimaryGraph(typeof(PROvertimeRuleMaint))]
	public class PROvertimeRule : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PROvertimeRule>.By<overtimeRuleID>
		{
			public static PROvertimeRule Find(PXGraph graph, string overtimeRuleID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, overtimeRuleID, options);
		}

		public static class FK
		{
			public class DisbursingEarningType : EPEarningType.PK.ForeignKeyOf<PROvertimeRule>.By<disbursingTypeCD> { }
			public class State : CS.State.PK.ForeignKeyOf<PROvertimeRule>.By<countryID, state> { }
			public class Country : CS.Country.PK.ForeignKeyOf<PROvertimeRule>.By<countryID> { }
			public class Union : PMUnion.PK.ForeignKeyOf<PROvertimeRule>.By<unionID> { }
			public class Project : PMProject.PK.ForeignKeyOf<PROvertimeRule>.By<projectID> { }
		}
		#endregion

		#region OvertimeRuleID
		public abstract class overtimeRuleID : BqlString.Field<overtimeRuleID> { }
		/// <summary>
		/// The unique identifier of the overtime rule.
		/// </summary>
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Overtime Rule")]
		[PXReferentialIntegrityCheck]
		public virtual string OvertimeRuleID { get; set; }
		#endregion
		#region Description
		public abstract class description : BqlString.Field<description> { }
		/// <summary>
		/// The description of the overtime rule.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion
		#region IsActive //ToDo AC-149516: Check that the Disbursing Earning Type is still correct when the Overtime Rule is re-activated.
		public abstract class isActive : BqlBool.Field<isActive> { }
		/// <summary>
		/// Indicates that (if set to <see langword="true" />) the overtime rule is active.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region DisbursingTypeCD
		public abstract class disbursingTypeCD : BqlString.Field<disbursingTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the disbursing earning type.
		/// The field is included in <see cref="FK.DisbursingEarningType"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEarningType.typeCD"/> field.
		/// </value>
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Disbursing Earning Type")]
		[PXDefault]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.isOvertime.IsEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXForeignReference(typeof(Field<disbursingTypeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		public virtual string DisbursingTypeCD { get; set; }
		#endregion
		#region OvertimeMultiplier
		public abstract class overtimeMultiplier : BqlDecimal.Field<overtimeMultiplier> { }
		/// <summary>
		/// The overtime multiplier.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEarningType.overtimeMultiplier"/> field.
		/// </value>
		[PXDecimal]
		[PXFormula(typeof(Selector<disbursingTypeCD, EPEarningType.overtimeMultiplier>))]
		[PXUIField(DisplayName = "Multiplier", Enabled = false)]
		public virtual decimal? OvertimeMultiplier { get; set; }
		#endregion
		#region RuleType
		public abstract class ruleType : BqlString.Field<ruleType> { }
		/// <summary>
		/// The type of the overtime rule.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PROvertimeRuleType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsUnicode = false, IsFixed = true, InputMask = ">LLL")]
		[PXDefault]
		[PROvertimeRuleType.List]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXCheckUnique(typeof(isActive), typeof(disbursingTypeCD), typeof(weekDay), typeof(overtimeThreshold), typeof(numberOfConsecutiveDays),
			typeof(state), typeof(unionID), typeof(projectID), typeof(countryID), IgnoreNulls = false, ClearOnDuplicate = false, ErrorMessage = Messages.DuplicateOvertimeRule)]
		public virtual string RuleType { get; set; }
		#endregion
		#region OvertimeThreshold
		public abstract class overtimeThreshold : BqlDecimal.Field<overtimeThreshold> { }
		/// <summary>
		/// The threshold (in hours) to apply the overtime rule to earning detail(s).
		/// </summary>
		/// <value>
		/// The minimum 0 hours, the maximum 999.99 hours.
		/// </value>
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXDBDecimal(2, MinValue = 0, MaxValue = 999.99)]
		[PXUIField(DisplayName = "Threshold for Overtime (Hours)", Required = true)]
		public virtual decimal? OvertimeThreshold { get; set; }
		#endregion
		#region WeekDay
		public abstract class weekDay : BqlByte.Field<weekDay> { }
		/// <summary>
		/// A specific day of the week to apply the 'Daily' overtime rule.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EP.DayOfWeekAttribute"/>.
		/// </value>
		[PXDBByte]
		[EP.DayOfWeek]
		[PXUIField(DisplayName = "Day of Week")]
		[PXUIEnabled(typeof(ruleType.IsEqual<PROvertimeRuleType.daily>))]
		[PXFormula(typeof(Switch<Case<Where<ruleType, NotEqual<PROvertimeRuleType.daily>>, Null>>))]
		public virtual byte? WeekDay { get; set; }
		#endregion
		#region NumberOfConsecutiveDays
		public abstract class numberOfConsecutiveDays : BqlByte.Field<numberOfConsecutiveDays> { }
		/// <summary>
		/// The number of consecutive days an employee should work to apply the 'Consecutive' overtime rule during the Paycheck calculation.
		/// </summary>
		/// <value>
		/// The minimum 0 days, the maximum 7 days.
		/// </value>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBByte(MinValue = 0, MaxValue = 7)]
		[PXUIField(DisplayName = "Number of Consecutive Days")]
		[PXUIEnabled(typeof(ruleType.IsEqual<PROvertimeRuleType.consecutive>))]
		[PXFormula(typeof(Switch<Case<Where<ruleType, NotEqual<PROvertimeRuleType.consecutive>>, Null>>))]
		public virtual byte? NumberOfConsecutiveDays { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		/// <summary>
		/// The unique identifier of the country to which the overtime rule corresponds.
		/// The field is included in <see cref="FK.State"/> and <see cref="FK.Country"/>.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PRCountry]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion
		#region State
		public abstract class state : BqlString.Field<state> { }
		/// <summary>
		/// The state or province to which the overtime rule is applied.
		/// The field is included in <see cref="FK.State"/>.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[State(typeof(countryID))]
		[PXUIField(DisplayName = "State")]
		public virtual string State { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : BqlString.Field<unionID> { }
		/// <summary>
		/// The unique identifier of the union local, if any.
		/// The field is included in <see cref="FK.Union"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMUnion.UnionID"/> field.
		/// </value>
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PMUnion(null, null, FieldClass = null)]
		public virtual string UnionID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		/// <summary>
		/// The unique identifier of the associated project.
		/// The field is included in <see cref="FK.Project"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[ProjectBase(DisplayName = "Project")]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public virtual byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
