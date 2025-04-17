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
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Commerce.Shopify
{
	/// <summary>
	/// Represents a Role Assignment for a contact to a Customer with Customer Category of Organization.
	/// </summary>
	[Serializable]
	[PXCacheName("Customer Contact Role Assignment")]
	public class BCRoleAssignment : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCRoleAssignment>.By<BCRoleAssignment.roleAssignmentID>
		{
			public static BCRoleAssignment Find(PXGraph graph, int? roleAssignmentID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, roleAssignmentID, options);
		}
		public static class FK
		{
			public class Contact : PX.Objects.CR.Contact.PK.ForeignKeyOf<BCRoleAssignment>.By<contactID> { }
		}
		#endregion

		#region BAccountID
		/// <summary>
		/// The Business Account ID for the role assignment.
		/// </summary>
		[PXDBInt()]
		[PXDefault(typeof(Contact.bAccountID))]
		public int? BAccountID { get; set; }

		/// <inheritdoc cref="BAccountID"/>
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		#endregion

		#region RoleAssignmentID
		/// <summary>
		/// The Identity of the role assignment.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public int? RoleAssignmentID { get; set; }
		/// <inheritdoc cref="RoleAssignmentID"/>
		public abstract class roleAssignmentID : BqlInt.Field<roleAssignmentID> { }
		#endregion

		#region NoteID
		/// <summary>
		/// The Note ID of the roleAssignment.
		/// </summary>
		[PXNote(BqlField = typeof(BCRoleAssignment.noteID))]
		public virtual Guid? NoteID { get; set; }

		///<inheritdoc cref="NoteID"/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion

		#region ContactID
		/// <summary>
		/// The ID of the contact for this role assignment.
		/// </summary>
		[PXUIField(DisplayName = "Contact")]
		[PXDBDefault(typeof(Contact.contactID))]
		[PXParent(typeof(SelectFrom<Contact>.
			Where<Contact.contactID.IsEqual<BCRoleAssignment.contactID.FromCurrent>>))]
		[ContactRaw(typeof(Contact.bAccountID))]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public int? ContactID { get; set; }

		///<inheritdoc cref="ContactID"/>
		public abstract class contactID : BqlInt.Field<contactID> { }
		#endregion

		#region LocationID
		/// <summary>
		/// The ID of the location for this role assignment.
		/// </summary>
		[PXDBDefault(typeof(Location.locationID))]
		[PXParent(typeof(SelectFrom<Location>.
			Where<Location.locationID.IsEqual<BCRoleAssignment.locationID.FromCurrent>>))]
		[PXUIField(DisplayName = "Location")]
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<BCRoleAssignment.bAccountID>>,
			And<MatchWithBranch<Location.cBranchID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		public int? LocationID { get; set; }

		///<inheritdoc cref="LocationID"/>
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		 
		#region Role
		/// <summary>
		/// The code representing the Role type for this Role Assignment. See <see cref="BCRoleListAttribute"/> for all values.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BCRoleListAttribute.OrderOnlyValue)]
		[PXUIField(DisplayName = "Role")]
		[BCRoleList]
		public string Role { get; set; }

		///<inheritdoc cref="Role"/>
		public abstract class role : BqlString.Field<role> { }
		#endregion

		#region CreatedByID

		[PXDBCreatedByID(BqlField = typeof(BCRoleAssignment.createdByID))]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID

		[PXDBCreatedByScreenID(BqlField = typeof(BCRoleAssignment.createdByScreenID))]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime(BqlField = typeof(BCRoleAssignment.createdDateTime))]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID(BqlField = typeof(BCRoleAssignment.lastModifiedByID))]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID(BqlField = typeof(BCRoleAssignment.lastModifiedByScreenID))]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime(BqlField = typeof(BCRoleAssignment.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		#region Tstamp
		[PXDBTimestamp(BqlField = typeof(BCRoleAssignment.tstamp))]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion
	}
}
