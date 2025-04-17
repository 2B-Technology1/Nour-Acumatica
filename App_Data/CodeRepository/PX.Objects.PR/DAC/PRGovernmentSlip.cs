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
	/// Stores different Annual forms supported for Canadian Payroll for each year.
	/// </summary>
	[PXCacheName(Messages.PRGovernmentSlip)]
	[Serializable]
	public class PRGovernmentSlip : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRGovernmentSlip>.By<slipName, year>
		{
			public static PRGovernmentSlip Find(PXGraph graph, string slipName, string year, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, slipName, year, options);
		}
		#endregion

		#region SlipName
		public abstract class slipName : PX.Data.BQL.BqlString.Field<slipName> { }
		[PXDBString(50, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Slip Name")]
		public string SlipName { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Year")]
		public int? Year { get; set; }
		#endregion
		#region Timestamp
		public abstract class timestamp : PX.Data.BQL.BqlDateTime.Field<timestamp> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Timestamp")]
		public virtual DateTime? Timestamp { get; set; }
		#endregion
		#region SlipData
		public abstract class slipData : PX.Data.BQL.BqlString.Field<slipData> { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Slip Data")]
		public virtual string SlipData { get; set; }
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
