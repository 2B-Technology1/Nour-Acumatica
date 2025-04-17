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
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores information on the deduction and benefit code associated with a payroll batch. The information will be displayed on the Payroll Batches (PR301000) form.
	/// </summary>
	[PXCacheName(Messages.PRBatchDeduct)]
	[Serializable]
	public class PRBatchDeduct : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRBatchDeduct>.By<batchNbr, codeID>
		{
			public static PRBatchDeduct Find(PXGraph graph, string batchNbr, int? codeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, batchNbr, codeID, options);
		}

		public static class FK
		{
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PRBatchDeduct>.By<batchNbr> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRBatchDeduct>.By<codeID> { }
		}
		#endregion

		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr))]
		[PXParent(typeof(FK.PayrollBatch))]
		public string BatchNbr { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Deduction Code", Enabled = false)]
		[PXSelector(typeof(SearchFor<PRDeductCode.codeID>
			.Where<PRDeductCode.isActive.IsEqual<True>
				.And<PRDeductCode.isWorkersCompensation.IsEqual<False>>
				.And<PRDeductCode.isCertifiedProject.IsEqual<False>>
				.And<PRDeductCode.isUnion.IsEqual<False>>
				.And<MatchPRCountry<PRDeductCode.countryID>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXDefault]
		public int? CodeID { get; set; }
		#endregion
		#region IsEnabled
		public abstract class isEnabled : PX.Data.BQL.BqlBool.Field<isEnabled> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Enabled")]
		[PXDefault(true)]
		public bool? IsEnabled { get; set; }
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
