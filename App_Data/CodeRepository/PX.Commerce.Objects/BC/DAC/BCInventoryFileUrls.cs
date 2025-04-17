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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Represents a file associated with an inventory item.
	/// </summary>
	[Serializable]
	[PXCacheName("BC Inventory File Urls")]
	public class BCInventoryFileUrls : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCInventoryFileUrls>.By<BCInventoryFileUrls.fileID, BCInventoryFileUrls.inventoryID>
		{
			public static BCInventoryFileUrls Find(PXGraph graph, int? fileID, int? inventoryID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, fileID, inventoryID, options);
		}
		public static class FK
		{
			public class Item : InventoryItem.PK.ForeignKeyOf<BCInventoryFileUrls>.By<inventoryID> { }
		}
		#endregion
		#region FileID
		/// <summary>
		/// The identity of this record.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		//[PXUIField(DisplayName = "File ID", Visible = false)]
		public int? FileID { get; set; }
		/// <inheritdoc cref="FileID" />
		public abstract class fileID : PX.Data.BQL.BqlInt.Field<fileID> { }
		#endregion

		#region InventoryID
		/// <summary>
		/// The ID of the inventory item this file url belongs to.
		/// </summary>
		[PXDBInt]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		public virtual int? InventoryID { get; set; }
		/// <inheritdoc cref="InventoryID" />
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion

		#region FileURL
		/// <summary>
		/// The URL to the file.
		/// </summary>
		[PXDBString(IsUnicode =true)]
		[PXUIField(DisplayName = "URL")]
	    [PXDefault]
		[PXFieldDescription]
		public virtual string FileURL { get; set; }
		/// <inheritdoc cref="FileURL" />
		public abstract class fileURL : PX.Data.BQL.BqlString.Field<fileURL> { }
		#endregion

		#region FileType
		/// <summary>
		/// The type of the file.
		/// </summary>
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Type")]
		[BCFileType]
		[PXDefault(BCFileTypeAttribute.Image)]
		public virtual string FileType { get; set; }
		/// <inheritdoc cref="FileType" />
		public abstract class fileType : PX.Data.BQL.BqlString.Field<fileType> { }
		#endregion

		#region NoteID
		/// <summary>
		/// The NoteID of this record.
		/// </summary>
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		/// <inheritdoc cref="NoteID" />
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
	}
}
