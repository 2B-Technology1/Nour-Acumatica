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
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.EQUIPMENT_TYPE)]
    [PXPrimaryGraph(typeof(EquipmentTypeMaint))]
	public class FSEquipmentType : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<FSEquipmentType>.By<equipmentTypeCD>
        {
            public static FSEquipmentType Find(PXGraph graph, string equipmentTypeCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, equipmentTypeCD, options);
        }
		public class UK : PrimaryKeyOf<FSEquipmentType>.By<equipmentTypeID>
		{
			public static FSEquipmentType Find(PXGraph graph, int? equipmentTypeID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, equipmentTypeID, options);
		}
		#endregion

		#region EquipmentTypeID
		public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }		

		[PXDBIdentity]
		[PXUIField(Enabled = false)]        
		public virtual int? EquipmentTypeID { get; set; }
		#endregion
		#region EquipmentTypeCD
		public abstract class equipmentTypeCD : PX.Data.BQL.BqlString.Field<equipmentTypeCD> { }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXDefault]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Equipment Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSEquipmentType.equipmentTypeCD>), DescriptionField = typeof(FSEquipmentType.descr))]
		public virtual string EquipmentTypeCD { get; set; }
		#endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }		

		[PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr { get; set; }
		#endregion
		#region RequireBranchLocation
		public abstract class requireBranchLocation : PX.Data.BQL.BqlBool.Field<requireBranchLocation> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Branch Location")]
		public virtual bool? RequireBranchLocation { get; set; }
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
