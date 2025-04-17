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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.MarketingListMember)]
	public partial class CRMarketingListMember : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CRMarketingListMember>.By<marketingListID, contactID>
		{
			public static CRMarketingListMember Find(PXGraph graph, int? marketingListID, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, marketingListID, contactID, options);
		}
		public static class FK
		{
			public class MarketingList : CR.CRMarketingList.PK.ForeignKeyOf<CRMarketingListMember>.By<marketingListID> { }
		}
		#endregion

		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Member Name")]
		[PXSelector(typeof(Search2<Contact.contactID,
			LeftJoin<GL.Branch,
				On<GL.Branch.bAccountID, Equal<Contact.bAccountID>,
				And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>,
			Where<Branch.bAccountID, IsNull, 
				And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			>>),
			typeof(Contact.contactType),
			typeof(Contact.memberName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.isActive),
			DescriptionField = typeof(Contact.memberName),
			Filterable = true, 
			DirtyRead = true)]
		[PXParent(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<CRMarketingListMember.contactID>>>>))]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region MarketingListID

		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRMarketingList.marketingListID))]
		[PXUIField(DisplayName = "Marketing List ID", Visible = true)]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.type, Equal<CRMarketingList.type.@static>>>),
		    DescriptionField = typeof(CRMarketingList.mailListCode))]
		public virtual Int32? MarketingListID { get; set; }

		#endregion

		#region IsSubscribed

		public abstract class isSubscribed : PX.Data.BQL.BqlBool.Field<isSubscribed> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Subscribed")]
		public virtual Boolean? IsSubscribed { get; set; }

		#endregion

		#region IsVirtual

		public abstract class isVirtual : PX.Data.BQL.BqlBool.Field<isVirtual> { }
		/// <summary>
		/// A calculated field that indicates (if set to <c>false</c>) that the record exists in the database.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Virtual", Visible = false, Enabled = false)]
		public virtual bool? IsVirtual { get; set; }

		#endregion

		#region Type
		/// <summary>
		/// Represents the type of the marketing list.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRMarketingList.type"/> class.
		/// The default value is <see cref="CRMarketingList.type.Static"/>.
		/// </value>
		[PXString]
		[PXDefault(CRMarketingList.type.Static, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "List Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual string Type { get; set; }

		#endregion


		#region Format

		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }
		[PXDBString]
		[PXDefault(NotificationFormat.Html)]
		[PXUIField(DisplayName = "Format")]
		[NotificationFormat.TemplateList]
		public virtual string Format { get; set; }

		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }

		#endregion

		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }

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

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }

		#endregion

		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }

		#endregion

		#region tstamp

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }

		#endregion
	}

	[Serializable]
	[PXHidden]
	public class CRMarketingListMember2 : CRMarketingListMember
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
	}
}
