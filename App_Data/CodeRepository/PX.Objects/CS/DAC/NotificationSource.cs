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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.SM;
using PX.SM;
using static PX.Objects.CR.CRContactClass;
using PX.SM.Email;

namespace PX.Objects.CS
{
    [Serializable]
	[PXCacheName(Messages.NotificationSource)]
	public partial class NotificationSource : IBqlTable
	{
		public static class FK
		{
			/// <summary>
			/// Default Email Account
			/// </summary>
			public class EmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<NotificationSource>.By<eMailAccountID> { }
		}

		#region SourceID
		public abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }
		protected int? _SourceID;
		[PXDBIdentity(IsKey=true)]
		[PXParent(typeof(Select<NotificationSetup,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>))]
		public virtual int? SourceID
		{
			get
			{
				return this._SourceID;
			}
			set
			{
				this._SourceID = value;
			}
		}
		#endregion

		public abstract class setupID_description : PX.Data.BQL.BqlString.Field<setupID_description> { }
		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
		protected Guid? _SetupID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXDBDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBGuid]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		protected string _ClassID;
		[PXDBString(10)]
		[PXUIField(DisplayName = "Class ID")]
		public virtual string ClassID
		{
			get
			{
				return this._ClassID;
			}
			set
			{
				this._ClassID = value;
			}
		}
		#endregion		

		#region NBranchID
		public abstract class nBranchID : PX.Data.BQL.BqlInt.Field<nBranchID> { }
		protected Int32? _NBranchID;
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.classID, Equal<Current<NotificationSource.classID>>>))]
		[PXDefault(typeof(Search<NotificationSetup.nBranchID,
				Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? NBranchID
		{
			get
			{
				return this._NBranchID;
			}
			set
			{
				this._NBranchID = value;
			}
		}
		#endregion

		#region EMailAccount
		public abstract class eMailAccountID : PX.Data.BQL.BqlInt.Field<eMailAccountID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class EmailAccountRule
				: EMailAccount.userID.PreventMakingPersonalIfUsedAsSystem<
					SelectFrom<NotificationSource>.Where<FK.EmailAccount.SameAsCurrent>> {}
		}

		protected Int32 _EMailAccountID;
		[EmailAccountRaw(emailAccountsToShow: EmailAccountsToShowOptions.OnlySystem, DisplayName = "Email Account")]
		[PXDefault(typeof(Search2<NotificationSetup.eMailAccountID,
			InnerJoin<EMailAccount, On<NotificationSetup.eMailAccountID, Equal<EMailAccount.emailAccountID>>>,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		public virtual Int32? EMailAccountID{ get; set; }
		#endregion

		#region ReportID
		public abstract class reportID : PX.Data.BQL.BqlString.Field<reportID> { }
		protected String _ReportID;
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report ID")]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		public virtual String ReportID
		{
			get
			{
				return this._ReportID;
			}
			set
			{
				this._ReportID = value;
			}
		}
		#endregion
		
		#region NotificationID
		public abstract class notificationID : PX.Data.BQL.BqlInt.Field<notificationID> { }
		protected Int32? _NotificationID;
		[PXDBInt]
		[PXUIField(DisplayName = "Email Template")]
		[PXDefault(typeof(Search<NotificationSetup.notificationID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<Notification.notificationID>),
			SubstituteKey = typeof(Notification.name),
			DescriptionField = typeof(Notification.name))]
		public virtual Int32? NotificationID
		{
			get
			{
				return this._NotificationID;
			}
			set
			{
				this._NotificationID = value;
			}
		}
		#endregion

		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }
		protected string _Format;
		[PXDefault(typeof(Search<NotificationSetup.format,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Format")]
		[NotificationFormat.List]
		[PXNotificationFormat(typeof(NotificationSource.reportID), typeof(NotificationSource.notificationID))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		public virtual string Format
		{
			get
			{
				return this._Format;
			}
			set
			{
				this._Format = value;
			}
		}
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active;
		[PXDBBool()]
		[PXDefault(typeof(Search<NotificationSetup.active,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Active")]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		public virtual bool? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion

		#region RecipientsBehavior
		public abstract class recipientsBehavior : PX.Data.BQL.BqlString.Field<recipientsBehavior> { }

		[PXDBString(1, IsFixed = true)]
		[RecipientsBehavior]
		[PXDefault(RecipientsBehaviorAttribute.Add)]
		[PXUIField(DisplayName = "Recipients")]
		public virtual string RecipientsBehavior { get; set; }
		#endregion

		#region OverrideSource
		public abstract class overrideSource : PX.Data.BQL.BqlBool.Field<overrideSource> { }
		protected bool? _OverrideSource;
		[PXBool()]
		[PXUIField(DisplayName = "Overridden")]
		public virtual bool? OverrideSource
		{
			[PXDependsOnFields(typeof(refNoteID))]
			get
			{
				return this._OverrideSource ??  this.RefNoteID !=  null;
			}
			set
			{
				this._OverrideSource = value;
			}
		}
		#endregion

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
	}
}
