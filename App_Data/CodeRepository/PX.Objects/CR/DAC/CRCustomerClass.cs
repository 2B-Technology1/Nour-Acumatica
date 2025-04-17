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

using PX.SM;
using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.SM.Email;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents a business account class in CRM.
	/// </summary>
	/// <remarks>
	/// A <i>business account class</i> is a special entity that contains different default sets of additional information about business accounts
	/// and may help a user to easily group business accounts into classes.
	/// The records of this type are created and edited on the <i>Business Account Classes (CR208000)</i> form,
	/// which corresponds to the <see cref="CRCustomerClassMaint"/> graph.
	/// </remarks>
	[PXCacheName(Messages.BAccountClass)]
	[Serializable]
	[PXPrimaryGraph(typeof(CRCustomerClassMaint))]
	public partial class CRCustomerClass : CRBaseClass, PX.Data.IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<CRCustomerClass>.By<cRCustomerClassID>
		{
			public static CRCustomerClass Find(PXGraph graph, string cRCustomerClassID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cRCustomerClassID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Default Assignment Map
			/// </summary>
			public class DefaultAssignmentMap : EP.EPAssignmentMap.PK.ForeignKeyOf<CRCustomerClass>.By<defaultAssignmentMapID> { }

			/// <summary>
			/// Default Email Account
			/// </summary>
			public class DefaultEmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<CRCustomerClass>.By<defaultEMailAccountID> { }
		}
		#endregion

		#region CRCustomerClassID
		public abstract class cRCustomerClassID : PX.Data.BQL.BqlString.Field<cRCustomerClassID> { }

		/// <summary>
		/// The user-friendly unique identifier of the business account class.
		/// This field is the primary key field.
		/// </summary>
		/// <value>
		/// The value can be entered only manually.
		/// </value>
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
		[PXUIField(DisplayName = "Business Account Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String CRCustomerClassID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The brief description of the business account class.
		/// </summary>
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual String Description { get; set; }
		#endregion
		#region DefaultOwner
		public abstract class defaultOwner : PX.Data.BQL.BqlInt.Field<defaultOwner> { }

		/// <summary>
		/// The field defines a way that a default owner should be determined for a newly created business accounts of this class.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRDefaultOwnerAttribute.CRDefaultOwnerAttribute"/> class.
		/// The default value is <see cref="CRDefaultOwnerAttribute.DoNotChange"/>.
		/// </value>
		[PXDBString]
		[PXUIField(DisplayName = "Default Owner")]
		[PXDefault(CRDefaultOwnerAttribute.DoNotChange, PersistingCheck = PXPersistingCheck.Nothing)]
		[CRDefaultOwner]
		public override string DefaultOwner { get; set; }
		#endregion
		#region DefaultAssignmentMapID
		public abstract class defaultAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultAssignmentMapID> { }

		/// <summary>
		/// The identifier of the default assignment map that is used to assign the default owner
		/// and the workgroup to a newly created business accounts of this class.
		/// </summary>
		/// <value>
		/// The value of this field corresonds to the value of the <see cref="EPAssignmentMap.AssignmentMapID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Search<
				EPAssignmentMap.assignmentMapID,
				Where<
					EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeProspect>>>),
			typeof(EPAssignmentMap.assignmentMapID),
			typeof(EPAssignmentMap.name),
			DescriptionField = typeof(EPAssignmentMap.name)
		)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIEnabled(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIField(DisplayName = "Assignment Map")]
		public override int? DefaultAssignmentMapID { get; set; }
		#endregion
		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class EmailAccountRule
				: EMailAccount.userID.PreventMakingPersonalIfUsedAsSystem<
					SelectFrom<CRCustomerClass>.Where<FK.DefaultEmailAccount.SameAsCurrent>> {}
		}

		/// <summary>
		/// The identifier of the default email account for this business account class.
		/// </summary>
		/// <value>
		/// The value of the this field corresponds to the value of the <see cref="EMailAccount.EmailAccountID"/> field.
		/// </value>
		[EmailAccountRaw(emailAccountsToShow: EmailAccountsToShowOptions.OnlySystem, DisplayName = "Default Email Account")]
		[PXForeignReference(typeof(FK.DefaultEmailAccount), Data.ReferentialIntegrity.ReferenceBehavior.SetNull)]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion
		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
		protected Boolean? _IsInternal;

		/// <summary>
		/// This field indicates that the business accounts of this class are hidden from user of the Self-Service Portal
		/// so that only Acumatica ERP users can view the business accounts.
		/// </summary>
		/// <value>
		/// The default value is <see langword="true"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal
		{
			get
			{
				return this._IsInternal;
			}
			set
			{
				this._IsInternal = value;
			}
		}

		#endregion

		#region CuryID

		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		[PXDefault(typeof(
			SelectFrom<CRCustomerClass>
			.InnerJoin<CRSetup>
				.On<CRSetup.defaultCustomerClassID.IsEqual<CRCustomerClass.cRCustomerClassID>>
			.SearchFor<CRCustomerClass.curyID>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string CuryID { get; set; }

		#endregion
		#region AllowOverrideCury

		public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Enable Currency Override")]
		[PXDefault(true, typeof(
			SelectFrom<CRCustomerClass>
			.InnerJoin<CRSetup>
				.On<CRSetup.defaultCustomerClassID.IsEqual<CRCustomerClass.cRCustomerClassID>>
			.SearchFor<CRCustomerClass.allowOverrideCury>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AllowOverrideCury { get; set; }

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
