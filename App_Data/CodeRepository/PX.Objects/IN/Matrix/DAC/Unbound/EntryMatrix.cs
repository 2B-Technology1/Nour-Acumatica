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

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXCacheName(Messages.EntityMatrixDAC)]
	public class EntryMatrix : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXInt(IsKey = true)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region RowAttributeValue
		public abstract class rowAttributeValue : PX.Data.BQL.BqlString.Field<rowAttributeValue> { }
		[PXString(10, IsUnicode = true)]
		public virtual string RowAttributeValue
		{
			get;
			set;
		}
		#endregion
		#region RowAttributeValueDescr
		public abstract class rowAttributeValueDescr : PX.Data.BQL.BqlString.Field<rowAttributeValueDescr> { }
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Attribute Value", Enabled = false)]
		public virtual string RowAttributeValueDescr
		{
			get;
			set;
		}
		#endregion

		#region ColAttributeValues
		public abstract class colAttributeValues : PX.Data.BQL.BqlByteArray.Field<colAttributeValues> { }
		public virtual string[] ColAttributeValues
		{
			get;
			set;
		}
		#endregion
		#region ColAttributeValueDescrs
		public abstract class colAttributeValueDescrs : PX.Data.BQL.BqlByteArray.Field<colAttributeValueDescrs> { }
		public virtual string[] ColAttributeValueDescrs
		{
			get;
			set;
		}
		#endregion

		#region InventoryIDs
		public abstract class inventoryIDs : PX.Data.BQL.BqlByteArray.Field<inventoryIDs> { }
		public virtual int?[] InventoryIDs
		{
			get;
			set;
		}
		#endregion

		#region Quantities
		public abstract class quantities : PX.Data.BQL.BqlByteArray.Field<quantities> { }
		public virtual decimal?[] Quantities
		{
			get;
			set;
		}
		#endregion

		#region UOMs
		public abstract class uOMs : PX.Data.BQL.BqlByteArray.Field<uOMs> { }
		/// <exclude/>
		public virtual string[] UOMs
		{
			get;
			set;
		}
		#endregion

		#region BaseQuantities
		public abstract class baseQuantities : PX.Data.BQL.BqlByteArray.Field<baseQuantities> { }
		/// <exclude/>
		public virtual decimal?[] BaseQuantities
		{
			get;
			set;
		}
		#endregion

		#region BaseUOM
		public abstract class baseUOM : PX.Data.BQL.BqlString.Field<baseUOM> { }
		/// <exclude/>
		[PXString]
		public virtual string BaseUOM
		{
			get;
			set;
		}
		#endregion

		#region Errors
		public abstract class errors : PX.Data.BQL.BqlByteArray.Field<errors> { }
		public virtual string[] Errors
		{
			get;
			set;
		}
		#endregion

		#region AllSelected
		public abstract class allSelected : PX.Data.BQL.BqlBool.Field<allSelected> { }
		[PXBool]
		public virtual bool? AllSelected
		{
			get;
			set;
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlByteArray.Field<selected> { }
		public virtual bool?[] Selected
		{
			get;
			set;
		}
		#endregion

		#region IsPreliminary
		public abstract class isPreliminary : PX.Data.BQL.BqlBool.Field<isPreliminary> { }
		[PXBool]
		public virtual bool? IsPreliminary
		{
			get;
			set;
		}
		#endregion

		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal> { }
		[PXBool]
		public virtual bool? IsTotal
		{
			get;
			set;
		}
		#endregion

		#region MatrixAvailability
		public abstract class matrixAvailability : PX.Data.BQL.BqlInt.Field<matrixAvailability> { }
		[PXString(IsUnicode = true)]
		public virtual string MatrixAvailability
		{
			get;
			set;
		}
		#endregion

		#region SelectedColumn
		public abstract class selectedColumn : PX.Data.BQL.BqlInt.Field<selectedColumn> { }
		[PXInt]
		public virtual int? SelectedColumn
		{
			get;
			set;
		}
		#endregion
	}
}
