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
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.WarehouseBuilding, PXDacType.Catalogue)]
	public class INSiteBuilding : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteBuilding>.By<buildingID>
		{
			public static INSiteBuilding Find(PXGraph graph, int? buildingID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, buildingID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<INSiteBuilding>.By<branchID> { }
			public class Address : CR.Address.PK.ForeignKeyOf<INSiteBuilding>.By<addressID> { }
		}
		#endregion

		#region BuildingID
		[PXDBIdentity]
		[PXReferentialIntegrityCheck]
		public virtual int? BuildingID { get; set; }
		public abstract class buildingID : PX.Data.BQL.BqlInt.Field<buildingID> { }
		#endregion
		#region BuildingCD
		[PXDBString(30, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Building ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<buildingCD>), SubstituteKey = typeof(buildingCD), CacheGlobal = true)]
		[PXDefault]
		public virtual string BuildingCD { get; set; }
		public abstract class buildingCD : PX.Data.BQL.BqlString.Field<buildingCD> { }
		#endregion
		#region Descr
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual string Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region BranchID
		[Branch]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AddressID
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual int? AddressID { get; set; }
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
