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
using PX.Data.BQL;
using PX.Data.EP;
using PX.TM;
using PX.Data.Maintenance.GI;
using PX.Objects.CM;
using PX.SM;
using PX.Data.ReferentialIntegrity.Attributes;
using System.Diagnostics;

namespace PX.Objects.CR
{
	[Serializable]
	[CRCacheIndependentPrimaryGraph(typeof(CRMarketingListMaint),
		typeof(Select<CRMarketingList,
			Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>))]
	[PXCacheName(Messages.MailList)]
	[DebuggerDisplay("{GetType().Name,nq}: MarketingListID = {MarketingListID,nq}, MailListCode = {MailListCode}, Name = {Name}")]
	public class CRMarketingList : IBqlTable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CRMarketingList>.By<marketingListID>
		{
			public static CRMarketingList Find(PXGraph graph, int? marketingListID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, marketingListID, options);
		}

		public class UK : PrimaryKeyOf<CRMarketingList>.By<mailListCode>
		{
			public static CRMarketingList Find(PXGraph graph, string mailListCode, PKFindOptions options = PKFindOptions.None) => FindBy(graph, mailListCode, options);
		}
		public new static class FK
		{
			public class Owner : CR.Contact.PK.ForeignKeyOf<CRMarketingList>.By<ownerID> { }
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<CRMarketingList>.By<workgroupID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region MarketingListID
		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
		[PXDBIdentity()]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? MarketingListID { get; set; }
		#endregion
		#region MailListCode

		public abstract class mailListCode : PX.Data.BQL.BqlString.Field<mailListCode>
		{
			public const string DimensionName = "MLISTCD";
		}
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Marketing List ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(mailListCode.DimensionName, typeof(Search<CRMarketingList.marketingListID>), typeof(CRMarketingList.mailListCode))]
		[PXFieldDescription]
		public virtual String MailListCode { get; set; }
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "List Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Name { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBText(IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description { get; set; }
		#endregion
		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class List : PXStringListAttribute
			{
				public List() : base(
					(Active, "Active"),
					(Inactive, "Inactive")
				)
				{ }
			}

			public const string Active = "A";
			public const string Inactive = "I";

			public class active : BqlString.Constant<active>
			{
				public active() : base(Active) { }
			}

			public class inactive : BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) { }
			}
		}
		[PXDBString]
		[PXDefault(status.Inactive)]
		[status.List]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Status { get; set; }
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		[Owner(typeof(CRMarketingList.workgroupID))]
		public virtual int? OwnerID { get; set; }
		#endregion
		#region Method

		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }

		private string _method;

		[PXDBString(1, IsFixed = true)]
		[CRContactMethods]
		[PXDefault(CRContactMethodsAttribute.Any)]
		[PXUIField(DisplayName = "Contact Method")]
		public virtual String Method
		{
			get { return _method ?? CRContactMethodsAttribute.Any; }
			set { _method = value; }
		}

		#endregion
		#region Type

		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
			public class List : PXStringListAttribute
			{
				public List() : base(
					(Static, "Static"),
					(Dynamic, "Dynamic")
				)
				{ }
			}

			public const string Static = "S";
			public const string Dynamic = "D";

			public class @static : BqlString.Constant<@static>
			{
				public @static() : base(Static) { }
			}

			public class dynamic : BqlString.Constant<dynamic>
			{
				public dynamic() : base(Dynamic) { }
			}
		}

		[PXDBString]
		[PXDefault(type.Static)]
		[type.List]
		[PXUIField(DisplayName = "List Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual string Type { get; set; }

		#endregion

		#region GIDesignID
		public abstract class gIDesignID : PX.Data.BQL.BqlGuid.Field<gIDesignID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Generic Inquiry")]
		[ContactGISelector]
		[PXForeignReference(typeof(Field<gIDesignID>.IsRelatedTo<GIDesign.designID>))]
		public Guid? GIDesignID { get; set; }
		#endregion

		#region SharedGIFilter
		public abstract class sharedGIFilter : PX.Data.BQL.BqlGuid.Field<sharedGIFilter> { }
		[PXDBGuid]
		[PXUIField(DisplayName = "Shared Filter")]
		[FilterList(typeof(gIDesignID), IsSiteMapIdentityScreenID = false, IsSiteMapIdentityGIDesignID = true)]
		[PXFormula(typeof(Default<gIDesignID>))]
		public virtual Guid? SharedGIFilter { get; set; }

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(
			DescriptionField = typeof(CRMarketingList.mailListCode),
			Selector = typeof(CRMarketingList.marketingListID),
			ShowInReferenceSelector = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = "Modified Date")]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
	}

	/// <exclude/>
	[CRCacheIndependentPrimaryGraph(typeof(CRMarketingListMaint),
		typeof(Select<CRMarketingList,
			Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>))]
	[PXCacheName(Messages.MailList)]
	[PXBreakInheritance]
	public class CRMarketingListAlias : CRMarketingList
	{
		public new abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		public new abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class gIDesignID : PX.Data.BQL.BqlGuid.Field<gIDesignID> { }
		public new abstract class sharedGIFilter : PX.Data.BQL.BqlGuid.Field<sharedGIFilter> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
	}
}
