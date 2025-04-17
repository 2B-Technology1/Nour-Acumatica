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

using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
    [Serializable]
	[PXCacheName(Messages.INItemCategory, PXDacType.Catalogue)]
	public class INItemCategory : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemCategory>.By<inventoryID, categoryID>
		{
			public static INItemCategory Find(PXGraph graph, int? inventoryID, int? categoryID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, categoryID, options);
		}
		public static class FK
		{
			public class Category : IN.INCategory.PK.ForeignKeyOf<INItemCategory>.By<categoryID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemCategory>.By<inventoryID> { }
		}
		#endregion
		#region CategoryID
        public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
        protected int? _CategoryID;
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(INCategory.categoryID))]
        [PXParent(typeof(FK.Category))]
        [PXUIField(DisplayName = "Category")]
        public virtual int? CategoryID
        {
            get { return this._CategoryID; }
            set { this._CategoryID = value; }
        }
        #endregion
        
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
        [PXParent(typeof(FK.InventoryItem))]
        [AnyInventory(IsKey = true)]
        public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

        #region CategorySelected
        public abstract class categorySelected : PX.Data.BQL.BqlBool.Field<categorySelected> { }
        protected bool? _CategorySelected;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Category Selected", Visibility = PXUIVisibility.Service)]
        public virtual bool? CategorySelected
        {
            get
            {
                return this._CategorySelected;
            }
            set
            {
                this._CategorySelected = value;
            }
        }
        #endregion

		#region CreatedByID
		public abstract class createdByID : Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}

    [Serializable]
    public class INItemCategoryBuffer : PX.Data.IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [PXInt(IsKey = true)]
        [PXDefault()]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
    }
}
