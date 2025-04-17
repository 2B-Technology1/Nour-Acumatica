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
	/// Stores the information of different companies (in the Part of an Aggregate Group check box) for ACA reporting purposes. The information will be displayed on the ACA Reporting (PR207000) form.
	/// </summary>
	[PXCacheName(Messages.PRAcaAggregateGroupMember)]
	public class PRAcaAggregateGroupMember : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRAcaAggregateGroupMember>.By<orgBAccountID, year, memberEin>
		{
			public static PRAcaAggregateGroupMember Find(PXGraph graph, int? orgBAccountID, string year, string memberEin, PKFindOptions options = PKFindOptions.None) => 
				FindBy(graph, orgBAccountID, year, memberEin, options);
		}

		public static class FK
		{
			public class AcaCompanyYearlyInformation : PRAcaCompanyYearlyInformation.PK.ForeignKeyOf<PRAcaAggregateGroupMember>.By<orgBAccountID, year> { }
			public class Organization : GL.DAC.Organization.PK.ForeignKeyOf<PRAcaAggregateGroupMember>.By<orgBAccountID> { }
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
				Where<PRAcaCompanyYearlyInformation.year, Equal<Current<PRAcaAggregateGroupMember.year>>,
				And<PRAcaCompanyYearlyInformation.orgBAccountID, Equal<Current<PRAcaAggregateGroupMember.orgBAccountID>>,
				And<PRAcaCompanyYearlyInformation.isPartOfAggregateGroup, Equal<True>>>>>))]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.year))]
		public virtual string Year { get; set; }
		#endregion
		#region MemberCompanyName
		public abstract class memberCompanyName : PX.Data.BQL.BqlString.Field<memberCompanyName> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Account Name", Required = true)]
		public virtual string MemberCompanyName { get; set; }
		#endregion
		#region MemberEin
		public abstract class memberEin : PX.Data.BQL.BqlString.Field<memberEin> { }
		[PXDBString(9, InputMask = "##-#######", IsKey = true)]
		[PXUIField(DisplayName = "Member EIN", Required = true)]
		public virtual string MemberEin { get; set; }
		#endregion
		#region HighestMonthlyFteNumber
		public abstract class highestMonthlyFteNumber : PX.Data.BQL.BqlInt.Field<highestMonthlyFteNumber> { }
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Highest Monthly FTE Number")]
		public virtual int? HighestMonthlyFteNumber { get; set; }
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
