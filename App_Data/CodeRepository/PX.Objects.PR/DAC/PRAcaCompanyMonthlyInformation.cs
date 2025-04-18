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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the ACA coverage of a company for a specific month. The information will be displayed on the ACA Reporting (PR207000) form.
	/// </summary>
	[PXCacheName(Messages.PRAcaCompanyMonthlyInformation)]
	public class PRAcaCompanyMonthlyInformation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRAcaCompanyMonthlyInformation>.By<orgBAccountID, year, month>
		{
			public static PRAcaCompanyMonthlyInformation Find(PXGraph graph, int? orgBAccountID, string year, int? month, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, orgBAccountID, year, month, options);
		}

		public static class FK
		{
			public class AcaCompanyYearlyInformation : PRAcaCompanyYearlyInformation.PK.ForeignKeyOf<PRAcaCompanyMonthlyInformation>.By<orgBAccountID, year> { }
			public class Organization : GL.DAC.Organization.PK.ForeignKeyOf<PRAcaCompanyMonthlyInformation>.By<orgBAccountID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Obsolete]
		[PXDBInt]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.orgBAccountID))]
		public virtual int? OrgBAccountID { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = Messages.Year)]
		[PXParent(typeof(
			Select<PRAcaCompanyYearlyInformation,
				Where<PRAcaCompanyYearlyInformation.year, Equal<Current<PRAcaCompanyMonthlyInformation.year>>,
				And<PRAcaCompanyYearlyInformation.orgBAccountID, Equal<Current<PRAcaCompanyMonthlyInformation.orgBAccountID>>>>>))]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.year))]
		public virtual string Year { get; set; }
		#endregion
		#region Month
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = Messages.Month, Enabled = false)]
		[Month.List]
		public virtual int? Month { get; set; }
		#endregion
		#region NumberOfFte
		public abstract class numberOfFte : PX.Data.BQL.BqlInt.Field<numberOfFte> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Nbr. FTE", Enabled = false)]
		public virtual int? NumberOfFte { get; set; }
		#endregion
		#region NumberOfEmployees
		public abstract class numberOfEmployees : PX.Data.BQL.BqlInt.Field<numberOfEmployees> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Nbr. Employees", Enabled = false)]
		public virtual int? NumberOfEmployees { get; set; }
		#endregion
		#region PctEmployeesCoveredByMec
		public abstract class pctEmployeesCoveredByMec : PX.Data.BQL.BqlDecimal.Field<pctEmployeesCoveredByMec> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "% of Employees Covered by MEC", Enabled = false)]
		public virtual decimal? PctEmployeesCoveredByMec { get; set; }
		#endregion
		#region CertificationOfEligibility
		public abstract class certificationOfEligibility : PX.Data.BQL.BqlString.Field<certificationOfEligibility> { }
		[PXDBString(3)]
		[PXUIField(DisplayName = "Certification of Eligibility")]
		[AcaCertificationOfEligibility.List]
		public virtual string CertificationOfEligibility { get; set; }
		#endregion
		#region SelfInsured
		public abstract class selfInsured : PX.Data.BQL.BqlBool.Field<selfInsured> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Self-Insured")]
		[PXDefault(false)]
		public virtual bool? SelfInsured { get; set; }
		#endregion
		#region Numberof1095C
		public abstract class numberof1095C : PX.Data.BQL.BqlInt.Field<numberof1095C> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Nbr. of 1095-C Forms", Enabled = false)]
		public virtual int? Numberof1095C { get; set; }
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
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
}
