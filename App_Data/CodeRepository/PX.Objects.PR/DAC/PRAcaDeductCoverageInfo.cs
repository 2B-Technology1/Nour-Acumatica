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
	/// Stores the information related to the ACA coverage for a specific deduction and benefit code. The information will be displayed on the Deduction and Benefit Codes (PR101060) form.
	/// </summary>
	[PXCacheName(Messages.PRAcaDeductCoverageInfo)]
	[Serializable]
	public sealed class PRAcaDeductCoverageInfo : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRAcaDeductCoverageInfo>.By<deductCodeID, coverageType>
		{
			public static PRAcaDeductCoverageInfo Find(PXGraph graph, int? deductCodeID, string coverageType, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, deductCodeID, coverageType, options);
		}

		public static class FK
		{
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRAcaDeductCoverageInfo>.By<deductCodeID> { }
		}
		#endregion

		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Code ID")]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<PRAcaDeductCoverageInfo.deductCodeID>>>>))]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		public int? DeductCodeID { get; set; }
		#endregion
		#region CoverageType
		public abstract class coverageType : PX.Data.BQL.BqlString.Field<coverageType> { }
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Coverage Type", Required = true)]
		[AcaCoverageType.List]
		public string CoverageType { get; set; }
		#endregion
		#region HealthPlanType
		public abstract class healthPlanType : PX.Data.BQL.BqlString.Field<healthPlanType> { }
		[PXDBString(3)]
		[PXUIField(DisplayName = "Health Plan Type", Required = true)]
		[AcaHealthPlanType.List]
		public string HealthPlanType { get; set; }
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
