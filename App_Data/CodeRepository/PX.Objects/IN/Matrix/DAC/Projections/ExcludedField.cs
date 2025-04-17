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
using PX.Objects.IN.Matrix.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.Matrix.DAC.Projections
{
	[PXCacheName(Messages.INMatrixExcludedField)]
	[PXBreakInheritance]
	[PXProjection(typeof(Select<INMatrixExcludedData,
		Where<INMatrixExcludedData.type, Equal<INMatrixExcludedData.type.field>>>), Persistent = true)]
	public class ExcludedField : INMatrixExcludedData
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ExcludedField>.By<templateID, type, tableName, fieldName>
		{
			public static ExcludedField Find(PXGraph graph, int? templateID, string type, string tableName, string fieldName, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, templateID, type, tableName, fieldName, options);
		}
		#endregion

		#region Type
		public abstract new class type : PX.Data.BQL.BqlString.Field<type> { }

		/// <summary>
		/// Type of row: 'F' - field, 'A' - attribute.
		/// </summary>
		[PXDBString(1, IsKey = true, IsFixed = true, IsUnicode = false)]
		[INMatrixExcludedData.type.List]
		[PXDefault(INMatrixExcludedData.type.Field)]
		public override string Type
		{
			get => base.Type;
			set => base.Type = value;
		}
		#endregion
		#region TableName
		public abstract new class tableName : PX.Data.BQL.BqlString.Field<tableName> { }

		/// <summary>
		/// References to a DAC name.
		/// </summary>
		[PXDBString(255, IsKey = true)]
		[PXUIField(DisplayName = "Table Name", Required = true)]
		[PXDefault]
		[PXStringList]
		public override string TableName
		{
			get => base.TableName;
			set => base.TableName = value;
		}
		#endregion
		#region FieldName
		public abstract new class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		/// <summary>
		/// References to field name of related DAC <see cref="TableName"/>.
		/// </summary>
		[PXDBString(255, IsKey = true)]
		[PXUIField(DisplayName = "Field Name", Required = true)]
		[PXDefault]
		[ExcludedFieldSelector(INMatrixExcludedData.type.Field)]
		public override string FieldName
		{
			get => base.FieldName;
			set => base.FieldName = value;
		}
		#endregion
		#region TemplateID
		public abstract new class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }

		/// <summary>
		/// Template Inventory Item identifier.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.TemplateInventoryItem))]
		public override int? TemplateID
		{
			get => base.TemplateID;
			set => base.TemplateID = value;
		}
		#endregion
	}
}
