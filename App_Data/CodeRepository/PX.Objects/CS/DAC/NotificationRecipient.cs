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

namespace PX.Objects.CS
{
    [Serializable]
	[PXCacheName(Messages.NotificationRecipient)]
	public partial class NotificationRecipient : IBqlTable
	{
		#region NotificationID
		public abstract class notificationID : PX.Data.BQL.BqlInt.Field<notificationID> { }
		protected int? _NotificationID;
		[PXDBIdentity(IsKey = true)]
		[PXParent(typeof(Select<NotificationSource,
			Where<NotificationSource.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
		[PXUIField(DisplayName = "NotificationID")]
		public virtual int? NotificationID
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
		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
		protected Guid? _SetupID;
		[PXDBGuid]
		[PXDefault(typeof(NotificationSource.setupID))]
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
		#region SourceID
		public abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }
		protected int? _SourceID;
		[PXDBInt()]
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
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		protected string _ClassID;
		[PXDBString(10)]
		[PXDefault(typeof(NotificationSource.classID), PersistingCheck=PXPersistingCheck.Nothing)]
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
		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
		protected string _ContactType;		
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Contact Type")]		
		public virtual string ContactType
		{
			get
			{
				return this._ContactType;
			}
			set
			{
				this._ContactType = value;
			}
		}
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]	
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion					
		#region OriginalContactID
		public abstract class originalContactID : PX.Data.BQL.BqlInt.Field<originalContactID> { }
		[PXInt]
		[PXUIField(Visible = false)]
		public Int32? OriginalContactID
		{
			get
			{
				return this.ContactID;
			}
		}
		#endregion
		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }
		protected string _Format;
		[PXDefault(typeof(Search<NotificationSource.format,
			Where<NotificationSource.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Format")]
		[NotificationFormat.List]
		[PXNotificationFormat(
			typeof(NotificationSource.reportID), 
			typeof(NotificationSource.notificationID),
			typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationSource.sourceID>>>))]
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
		[PXDefault(typeof(Search<NotificationSource.active,
			Where<NotificationSource.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
		[PXUIField(DisplayName = "Active")]
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
		#region AddTo
		public abstract class addTo : PX.Data.BQL.BqlString.Field<addTo> { }

		[PXDBString(1, IsFixed = true)]
		[RecipientAddTo]
		[PXDefault(RecipientAddToAttribute.To)]
		[PXUIField(DisplayName = "Add To")]
		public virtual string AddTo { get; set; }
		#endregion
		#region Hidden
		[Obsolete]
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }

		[PXBool]
		[PXUIField(DisplayName = "Bcc", Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDependsOnFields(typeof(addTo))]
		[Obsolete]
		public virtual bool? Hidden
		{
			get
			{
				return this.AddTo == RecipientAddToAttribute.Bcc;
			}
		}
		#endregion
		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		protected string _Email;
		[PXString()]		
		public virtual string Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}
		#endregion
		#region OrderID
		public abstract class orderID : PX.Data.BQL.BqlInt.Field<orderID> { }
		protected int? _OrderID;
		[PXInt()]		
		public virtual int? OrderID
		{
			get
			{
				return this._OrderID;
			}
			set
			{
				this._OrderID = value;
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

	public class NotificationContactType
	{
		public const string Employee = "E";
		public const string Contact = "C";
		public const string Primary = "P";
		public const string Remittance = "R";
		public const string Shipping = "S";
		public const string Billing = "B";

		public class employee : PX.Data.BQL.BqlString.Constant<employee> { public employee() : base(Employee) { } }
		public class contact : PX.Data.BQL.BqlString.Constant<contact> { public contact() : base(Contact) { } }
		public class primary : PX.Data.BQL.BqlString.Constant<primary> { public primary() : base(Primary) { } }
		public class remittance : PX.Data.BQL.BqlString.Constant<remittance> { public remittance() : base(Remittance) { } }
		public class shipping : PX.Data.BQL.BqlString.Constant<shipping> { public shipping() : base(Shipping) { } }
		public class billing : PX.Data.BQL.BqlString.Constant<billing> { public billing() : base(Billing) { } }

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Primary, Billing, Shipping, Remittance, Employee, Contact },
							 new string[] { CR.Messages.AccountEmail, AR.Messages.Billing, CR.Messages.AccountLocationEmail, AP.Messages.Remittance, EP.Messages.Employee, CR.Messages.Contact })
			{
			}
		}

		public class ProjectListAttribute : PXStringListAttribute
		{
			public ProjectListAttribute()
				: base(new string[] { Primary, Employee, Contact },
							 new string[] { CR.Messages.AccountEmail, EP.Messages.Employee, CR.Messages.Contact })
			{
			}
		}

		public class ProjectTemplateListAttribute : PXStringListAttribute
		{
			public ProjectTemplateListAttribute()
				: base(new string[] { Primary, Employee },
							 new string[] { CR.Messages.AccountEmail, EP.Messages.Employee })
			{
			}
		}
	}	
}
