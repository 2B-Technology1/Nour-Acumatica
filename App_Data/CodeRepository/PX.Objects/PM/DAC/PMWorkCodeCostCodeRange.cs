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
using System;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.PMWorkCodeCostCodeRange)]
	[Serializable]
	public class PMWorkCodeCostCodeRange : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PMWorkCodeCostCodeRange>.By<workCodeID, lineNbr>
		{
			public static PMWorkCodeCostCodeRange Find(PXGraph graph, string workCodeID, int? lineNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, workCodeID, lineNbr, options);
		}

		public static class FK
		{
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PMWorkCodeCostCodeRange>.By<workCodeID> { }
			public class CostCodeFrom : PMCostCode.PK.ForeignKeyOf<PMWorkCodeCostCodeRange>.By<costCodeFrom> { }
			public class CostCodeTo : PMCostCode.PK.ForeignKeyOf<PMWorkCodeCostCodeRange>.By<costCodeTo> { }

		}
		#endregion

		#region WorkCodeID
		public abstract class workCodeID : BqlString.Field<workCodeID> { }
		[PXDBString(PMWorkCode.workCodeID.Length, IsKey = true)]
		[PXDBDefault(typeof(PMWorkCode.workCodeID))]
		[PXParent(typeof(FK.WorkCode))]
		public string WorkCodeID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMWorkCode))]
		public int? LineNbr { get; set; }
		#endregion
		#region CostCodeFrom
		public abstract class costCodeFrom : BqlString.Field<costCodeFrom> { }
		[PXRestrictor(typeof(Where<PMCostCode.isActive, Equal<True>>), Objects.PM.Messages.CostCodeInactiveWithFormat, typeof(PMCostCode.costCodeCD))]
		[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(PMCostCode.costCodeCD))]
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Cost Code From")]
		[CheckWorkCodeCostCodeRange]
		[PXForeignReference(typeof(FK.CostCodeFrom))]
		public virtual string CostCodeFrom { get; set; }
		#endregion
		#region CostCodeTo
		public abstract class costCodeTo : BqlString.Field<costCodeTo> { }
		[PXRestrictor(typeof(Where<PMCostCode.isActive, Equal<True>>), Objects.PM.Messages.CostCodeInactiveWithFormat, typeof(PMCostCode.costCodeCD))]
		[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(PMCostCode.costCodeCD))]
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Cost Code To")]
		[PXForeignReference(typeof(FK.CostCodeTo))]
		public virtual String CostCodeTo { get; set; }
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
