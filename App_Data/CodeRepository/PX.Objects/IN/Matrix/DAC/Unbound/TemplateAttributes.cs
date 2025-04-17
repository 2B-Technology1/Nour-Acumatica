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
	[PXCacheName(Messages.TemplateAttributesDAC)]
	public class TemplateAttributes : IBqlTable
	{
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		/// <summary>
		/// References to parent Inventory Item.
		/// </summary>
		[PXUIField(DisplayName = "Template Item")]
		[TemplateInventory(IsKey = true)]
		public virtual int? TemplateItemID
		{
			get;
			set;
		}
		#endregion
		#region AttributeIdentifiers
		public abstract class attributeIdentifiers : PX.Data.BQL.BqlByteArray.Field<attributeIdentifiers> { }
		/// <summary>
		/// Array to store attribute identifiers (CSAttribute.attributeID) of additional attributes which are not from matrix (columns)
		/// </summary>
		public virtual string[] AttributeIdentifiers
		{
			get;
			set;
		}
		#endregion
	}
}
