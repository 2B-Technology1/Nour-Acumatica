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
using System.Collections;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CR.BackwardCompatibility
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[Serializable]
	[PXHidden]
	public sealed class CRLeadBackwardCompatibility : PXCacheExtension<CRLead>
	{
		#region LeadContactID
		public abstract class leadContactID : PX.Data.BQL.BqlInt.Field<leadContactID> { }

		[PXDBInt(BqlField = typeof(Contact.contactID))]
		[PXDependsOnFields(typeof(Contact.contactID))]
		[PXUIField(Visible = false)]
		[NoUpdateDBField(NoInsert = true)]
		public Int32? LeadContactID
		{
			get
			{
				return Base.ContactID;
			}
		}
		#endregion

		#region LeadNoteID
		public abstract class leadNoteID : PX.Data.BQL.BqlInt.Field<leadNoteID> { }

		[PXDBGuid(BqlField = typeof(Contact.noteID))]
		[PXDependsOnFields(typeof(Contact.noteID))]
		[PXUIField(Visible = false)]
		[NoUpdateDBField(NoInsert = true)]
		public Guid? LeadNoteID
		{
			get
			{
				return Base.NoteID;
			}
		}
		#endregion

		#region LeadCreatedByID
		public abstract class leadCreatedByID : PX.Data.BQL.BqlGuid.Field<leadCreatedByID> { }

		[PXDBCreatedByID(BqlField = typeof(Contact.createdByID))]
		[PXDependsOnFields(typeof(Contact.createdByID))]
		[NoUpdateDBField(NoInsert = true)]
		public Guid? LeadCreatedByID
		{
			get
			{
				return Base.CreatedByID;
			}
		}
		#endregion

		#region LeadCreatedByScreenID
		public abstract class leadCreatedByScreenID : PX.Data.BQL.BqlString.Field<leadCreatedByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(Contact.createdByScreenID))]
		[PXDependsOnFields(typeof(Contact.createdByScreenID))]
		[NoUpdateDBField(NoInsert = true)]
		public String LeadCreatedByScreenID
		{
			get
			{
				return Base.CreatedByScreenID;
			}
		}
		#endregion

		#region LeadCreatedDateTime
		public abstract class leadCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<leadCreatedDateTime> { }

		[PXDBCreatedDateTime(BqlField = typeof(Contact.createdDateTime))]
		[PXDependsOnFields(typeof(Contact.createdDateTime))]
		[NoUpdateDBField(NoInsert = true)]
		public DateTime? LeadCreatedDateTime
		{
			get
			{
				return Base.CreatedDateTime;
			}
		}
		#endregion

		#region LeadLastModifiedByID
		public abstract class leadLastModifiedByID : PX.Data.BQL.BqlGuid.Field<leadLastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(Contact.lastModifiedByID))]
		[PXDependsOnFields(typeof(Contact.lastModifiedByID))]
		[NoUpdateDBField(NoInsert = true)]
		public Guid? LeadLastModifiedByID
		{
			get
			{
				return Base.LastModifiedByID;
			}
		}
		#endregion

		#region LeadLastModifiedByScreenID
		public abstract class leadLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<leadLastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(Contact.lastModifiedByScreenID))]
		[PXDependsOnFields(typeof(Contact.lastModifiedByScreenID))]
		[NoUpdateDBField(NoInsert = true)]
		public String LeadLastModifiedByScreenID
		{
			get
			{
				return Base.LastModifiedByScreenID;
			}
		}
		#endregion

		#region LeadLastModifiedDateTime
		public abstract class leadLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<leadLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(Contact.lastModifiedDateTime))]
		[PXDependsOnFields(typeof(Contact.lastModifiedDateTime))]
		[NoUpdateDBField(NoInsert = true)]
		public DateTime? LeadLastModifiedDateTime
		{
			get
			{
				return Base.LastModifiedDateTime;
			}
		}
		#endregion
	}
}
