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
using PX.Objects.IN.Matrix.Attributes;

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXCacheName(Messages.AdditionalAttributesDAC)]
	public class AdditionalAttributes : IBqlTable
	{
		#region Values
		public abstract class values : PX.Data.BQL.BqlByteArray.Field<values> { }
		/// <summary>
		/// Array to store values (CSAttributeDetail.valueID) of additional attributes which are not from matrix (columns)
		/// </summary>
		public virtual string[] Values
		{
			get;
			set;
		}
		#endregion

		#region Descriptions
		public abstract class descriptions : PX.Data.BQL.BqlByteArray.Field<descriptions> { }
		/// <summary>
		/// Array to store descriptions (CSAttributeDetail.description) of additional attributes which are not from matrix (columns)
		/// </summary>
		public virtual string[] Descriptions
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

		#region AttributeDisplayNames
		public abstract class attributeDisplayNames : PX.Data.BQL.BqlByteArray.Field<attributeDisplayNames> { }
		/// <summary>
		/// Array to store attribute descriptions (CSAttribute.description) of additional attributes which are not from matrix (columns)
		/// </summary>
		public virtual string[] AttributeDisplayNames
		{
			get;
			set;
		}
		#endregion

		#region ViewNames
		public abstract class viewNames : PX.Data.BQL.BqlByteArray.Field<viewNames> { }
		/// <summary>
		/// ViewName for each attribute (to show PXSelector)
		/// </summary>
		public virtual string[] ViewNames
		{
			get;
			set;
		}
		#endregion

		#region Extra
		public abstract class extra : PX.Data.BQL.BqlInt.Field<extra> { }
		/// <summary>
		/// The extra field is used to commit changes on a previous field. Should be hidden, last in a row.
		/// </summary>
		[PXUIField(DisplayName = "Template Item", Enabled = false)]
		[PXString]
		[MatrixAttributeValueSelector]
		public virtual string Extra
		{
			get;
			set;
		}
		#endregion
	}
}
