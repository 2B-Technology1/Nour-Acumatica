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
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
     [System.SerializableAttribute]
     [PXCacheName(TX.TableName.MODEL_TEMPLATE_COMPONENT)]
	public class FSModelTemplateComponent : PX.Data.IBqlTable
	{
        #region Key
        public class PK : PrimaryKeyOf<FSModelTemplateComponent>.By<componentID>
        {
            public static FSModelTemplateComponent Find(PXGraph graph, int? componentID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, componentID, options);
        }
        public class UK : PrimaryKeyOf<FSModelTemplateComponent>.By<modelTemplateID, componentCD>
        {
            public static FSModelTemplateComponent Find(PXGraph graph, int? modelTemplateID, string componentCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, modelTemplateID, componentCD, options);
        }

        public static class FK
        {
            public class ModelTemplate : IN.INItemClass.PK.ForeignKeyOf<FSModelTemplateComponent>.By<modelTemplateID> { }
            public class ItemClass : IN.INItemClass.PK.ForeignKeyOf<FSModelTemplateComponent>.By<classID> { }
        }
        #endregion

        #region ModelTemplateID
        public abstract class modelTemplateID : PX.Data.BQL.BqlInt.Field<modelTemplateID> { }

		[PXDBInt(IsKey = true)]
		[PXDBDefaultAttribute(typeof(INItemClass.itemClassID))]
        [PXParent(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<FSModelTemplateComponent.modelTemplateID>>>>))]
        public virtual int? ModelTemplateID { get; set; }
		#endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBIdentity]
        public virtual int? ComponentID { get; set; }

        #endregion
		#region ComponentCD
		public abstract class componentCD : PX.Data.BQL.BqlString.Field<componentCD> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Component ID")]
        public virtual string ComponentCD { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(250, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        #endregion
        #region ClassID
        public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Item Class ID")]
        [PXDefault]
        [PXSelector(typeof(
            Search<INItemClass.itemClassID,
            Where<
                FSxEquipmentModelTemplate.equipmentItemClass, Equal<ListField_EquipmentItemClass.Component>>>),
            SubstituteKey = typeof(INItemClass.itemClassCD),
            DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ClassID { get; set; }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlInt.Field<qty> { }

        [PXDBInt(MinValue = 1)]
        [PXDefault(TypeCode.Int32, "1")]
        [PXUIField(DisplayName = "Quantity")]
        public virtual int? Qty { get; set; }
        #endregion
        #region Optional
        public abstract class optional : PX.Data.BQL.BqlBool.Field<optional> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Optional", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? Optional { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
