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

namespace PX.Objects.IN.Matrix.DAC.Projections
{
	[PXCacheName(Messages.DescriptionGenerationRuleDAC)]
	[PXBreakInheritance]
	[PXProjection(typeof(Select<INMatrixGenerationRule,
		Where<INMatrixGenerationRule.type, Equal<INMatrixGenerationRule.type.description>>>), Persistent = true)]
	public class DescriptionGenerationRule : INMatrixGenerationRule
	{
		#region Keys
		public new class PK : PrimaryKeyOf<DescriptionGenerationRule>.By<parentType, parentID, type, lineNbr>
		{
			public static DescriptionGenerationRule Find(PXGraph graph, string parentType, int? parentID, string type, int? lineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, parentType, parentID, type, lineNbr, options);
		}
		public static new class FK
		{
			public class TemplateInventoryItem : InventoryItem.PK.ForeignKeyOf<DescriptionGenerationRule>.By<parentID> { }
			public class ItemClass : INItemClass.PK.ForeignKeyOf<DescriptionGenerationRule>.By<parentID> { }
		}
		#endregion

		#region ParentID
		public abstract new class parentID : PX.Data.BQL.BqlInt.Field<parentID> { }

		/// <summary>
		/// Template Inventory Item identifier.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.TemplateInventoryItem))]
		public override int? ParentID
		{
			get => base.ParentID;
			set => base.ParentID = value;
		}
		#endregion
		#region ParentType
		public abstract new class parentType : PX.Data.BQL.BqlString.Field<parentType> { }
		#endregion
		#region Type
		public abstract new class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(1, IsKey = true, IsFixed = true, IsUnicode = false)]
		[INMatrixGenerationRule.type.List]
		[PXDefault(INMatrixGenerationRule.type.Description)]
		public override string Type
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract new class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(InventoryItem.generationRuleCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public override int? LineNbr
		{
			get => base.LineNbr;
			set => base.LineNbr = value;
		}
		#endregion
		#region SortOrder
		public abstract new class sortOrder : PX.Data.BQL.BqlString.Field<sortOrder> { }
		#endregion
		#region AttributeID
		public abstract new class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		#endregion
		#region AddSpaces
		public abstract new class addSpaces : PX.Data.BQL.BqlBool.Field<addSpaces> { }
		#endregion

	}
}
