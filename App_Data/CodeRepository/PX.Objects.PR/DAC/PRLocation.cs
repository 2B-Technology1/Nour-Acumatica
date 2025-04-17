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
using PX.Objects.CR;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A physical location where work is performed.
	/// </summary>
	[PXCacheName(Messages.PRLocation)]
	[PXPrimaryGraph(typeof(WorkLocationsMaint))]
	[Serializable]
	public class PRLocation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRLocation>.By<locationID>
		{
			public static PRLocation Find(PXGraph graph, int? locationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, locationID, options);
		}

		public class UK : PrimaryKeyOf<PRLocation>.By<locationCD>
		{
			public static PRLocation Find(PXGraph graph, string locationCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, locationCD, options);
		}

		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<PRLocation>.By<branchID> { }
			public class Address : CR.Address.PK.ForeignKeyOf<PRLocation>.By<addressID> { }
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		/// <summary>
		/// The unique identifier of the work location.
		/// </summary>
		[PXDBIdentity]
		[PXReferentialIntegrityCheck]
		public int? LocationID { get; set; }
		#endregion
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the work location.
		/// </summary>
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(PRLocation.locationCD))]
		public string LocationCD { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// The description.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public string Description { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the location is used.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The unique identifier of the branch to which the work location belongs.
		/// The field is included in <see cref="FK.Branch"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(useDefaulting: false, addDefaultAttribute: false, DisplayName = "Use Address from Branch ID")]
		public int? BranchID { get; set; }
		#endregion
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		/// <summary>
		/// The unique identifier of the address associated with the work location.
		/// The field is included in <see cref="FK.Address"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Address ID", Visible = false)]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXSelector(typeof(Address.addressID))]
		public int? AddressID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
