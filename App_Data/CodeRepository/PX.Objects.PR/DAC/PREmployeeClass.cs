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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores information on a class used to group employees together. The information will be displayed on the Employee Payroll Class (PR202000) form.
	/// </summary>
	[PXCacheName(Messages.PREmployeeClass)]
	[PXPrimaryGraph(typeof(PREmployeeClassMaint))]
	[Serializable]
	public class PREmployeeClass : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeClass>.By<PREmployeeClass.employeeClassID>
		{
			public static PREmployeeClass Find(PXGraph graph, string employeeClassID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, employeeClassID, options);
		}
		
		public static class FK
		{
			public class PayGroup : PRPayGroup.PK.ForeignKeyOf<PREmployeeClass>.By<payGroupID> { }
			public class Calendar : CSCalendar.PK.ForeignKeyOf<PREmployeeClass>.By<calendarID> { }
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PREmployeeClass>.By<workCodeID> { }
			public class Union : PMUnion.PK.ForeignKeyOf<PREmployeeClass>.By<unionID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<PREmployeeClass>.By<countryID> { }
		}
		#endregion

		#region EmployeeClassID
		public abstract class employeeClassID : PX.Data.BQL.BqlString.Field<employeeClassID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault]
		[PXUIField(DisplayName = "Payroll Class ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		[PXSelector(typeof(SearchFor<employeeClassID>.Where<MatchPRCountry<countryID>>), CacheGlobal = true)]
		[PXReferentialIntegrityCheck]
		public string EmployeeClassID { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public string Descr { get; set; }
		#endregion
		#region EmpType
		public abstract class empType : PX.Data.BQL.BqlString.Field<empType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Employee Type")]
		[EmployeeType.List]
		public virtual string EmpType { get; set; }
		#endregion
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Group")]
		[PXSelector(typeof(PRPayGroup.payGroupID), DescriptionField = typeof(PRPayGroup.description))]
		[PXForeignReference(typeof(FK.PayGroup))]
		public virtual string PayGroupID { get; set; }
		#endregion
		#region CalendarID
		public abstract class calendarID : BqlString.Field<calendarID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual string CalendarID { get; set; }
		#endregion
		#region HoursPerWeek
		public abstract class hoursPerWeek : BqlDecimal.Field<hoursPerWeek> { }
		[HoursPerWeek(typeof(calendarID), typeof(hoursPerYear), typeof(stdWeeksPerYear))]
		public decimal? HoursPerWeek { get; set; }
		#endregion
		#region StdWeeksPerYear
		public abstract class stdWeeksPerYear : BqlByte.Field<stdWeeksPerYear> { }
		[PXDBByte(MaxValue = 52)]
		[PXDefault(TypeCode.Byte, "52")]
		[PXUIField(DisplayName = "Working Weeks per Year")]
		public virtual byte? StdWeeksPerYear { get; set; }
		#endregion
		#region HoursPerYear
		public abstract class hoursPerYear : BqlDecimal.Field<hoursPerYear> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Working Hours per Year", Enabled = false)]
		public decimal? HoursPerYear { get; set; }
		#endregion
		#region ExemptFromOvertimeRules
		public abstract class exemptFromOvertimeRules : BqlBool.Field<exemptFromOvertimeRules> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Exempt from Overtime Rules")]
		[PXDefault(false)]
		[PXFormula(typeof(True.When<empType.IsEqual<EmployeeType.salariedExempt>>.Else<False>.When<empType.IsEqual<EmployeeType.salariedNonExempt>>.Else<exemptFromOvertimeRules>))]
		public bool? ExemptFromOvertimeRules { get; set; }
		#endregion
		#region NetPayMin
		public abstract class netPayMin : BqlDecimal.Field<netPayMin> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Net Pay Minimum")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? NetPayMin { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : BqlString.Field<workCodeID> { }
		[PXForeignReference(typeof(Field<workCodeID>.IsRelatedTo<PMWorkCode.workCodeID>))]
		[WorkCodeMatchCountry(typeof(countryID), DisplayName = "Default WCC Code", FieldClass = null)]
		public string WorkCodeID { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : BqlString.Field<unionID> { }
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PMUnion(null, null, DisplayName = "Default Union", FieldClass = null)]
		public virtual string UnionID { get; set; }
		#endregion
		#region ExemptFromCertifiedReporting
		public abstract class exemptFromCertifiedReporting : BqlBool.Field<exemptFromCertifiedReporting> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Exempt from Certified Reporting")]
		[PXDefault(false)]
		public bool? ExemptFromCertifiedReporting { get; set; }
		#endregion
		#region GrnMaxPctNet
		public abstract class grnMaxPctNet : BqlDecimal.Field<grnMaxPctNet> { }
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Maximum Percent of Net Pay for All Garnishments")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual decimal? GrnMaxPctNet { get; set; }
		#endregion
		#region WorkLocationCount
		public abstract class workLocationCount : PX.Data.BQL.BqlInt.Field<workLocationCount> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Work Location Count", Visible = false)]
		public virtual int? WorkLocationCount { get; set; }
		#endregion
		#region UsePayrollProjectWorkLocation
		public abstract class usePayrollProjectWorkLocation : BqlBool.Field<usePayrollProjectWorkLocation> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Use Payroll Work Location from Project")]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.projectModule>))]
		[PXDefault(true)]
		public bool? UsePayrollProjectWorkLocation { get; set; }
		#endregion
		#region OverrideHoursPerYearForCertified
		public abstract class overrideHoursPerYearForCertified : BqlBool.Field<overrideHoursPerYearForCertified> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Override Hours per Year for Certified Project")]
		[PXDefault(false)]
		public bool? OverrideHoursPerYearForCertified { get; set; }
		#endregion
		#region HoursPerYearForCertified
		public abstract class hoursPerYearForCertified : BqlInt.Field<hoursPerYearForCertified> { }
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Certified Project Hours per Year")]
		[HoursForCertifiedProject(typeof(hoursPerYear), typeof(overrideHoursPerYearForCertified))]
		public int? HoursPerYearForCertified { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PRCountry]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
