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
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INTote, PXDacType.Catalogue)]
	public class INTote : IBqlTable
	{
		public const int UnassignedToteID = 0;

		#region Keys
		public class PK : PrimaryKeyOf<INTote>.By<siteID, toteID>
		{
			public static INTote Find(PXGraph graph, int? siteID, int? toteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, toteID, options);
		}
		public class UK : PrimaryKeyOf<INTote>.By<siteID, toteCD>
		{
			public static INTote Find(PXGraph graph, int? siteID, string toteCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, toteCD, options);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INTote>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<INTote>.By<siteID, assignedCartID> { }
		}
		#endregion

		#region SiteID
		[PXDBDefault(typeof(INSite.siteID))]
		[Site(IsKey = true, Visible = false)]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region ToteID
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public int? ToteID { get; set; }
		public abstract class toteID : PX.Data.BQL.BqlInt.Field<toteID> { }
		#endregion
		#region ToteCD
		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Tote ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<toteCD, Where<active, Equal<True>>>), DescriptionField = typeof(descr))]
		[PXCheckUnique]
		[PX.Data.EP.PXFieldDescription]
		public string ToteCD { get; set; }
		public abstract class toteCD : PX.Data.BQL.BqlString.Field<toteCD> { }
		#endregion
		#region Descr
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public string Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region AssignedCartID
		[PXDBInt]
		[PXSelector(typeof(SearchFor<INCart.cartID>.In<SelectFrom<INCart>.Where<INCart.siteID.IsEqual<siteID.FromCurrent>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXUIField(DisplayName = "Assigned Cart ID")]
		[PXForeignReference(typeof(FK.Cart), ReferenceBehavior.SetNull)]
		[PXParent(typeof(FK.Cart), LeaveChildren = true)]
		[PXFormula(null, typeof(CountCalc<INCart.assignedNbrOfTotes>))]
		public int? AssignedCartID { get; set; }
		public abstract class assignedCartID : PX.Data.BQL.BqlInt.Field<assignedCartID> { }
		#endregion
		#region Active
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region NoteID
		[PXNote(DescriptionField = typeof(toteCD), Selector = typeof(toteCD))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion

		[PXDBInt]
		[PXDefault(INTote.UnassignedToteID)]
		[PXUIField(DisplayName = "Tote ID")]
		[PXSelector(typeof(INTote.toteID), SubstituteKey = typeof(INTote.toteCD), DescriptionField = typeof(INTote.descr), ValidateValue = false)]
		public class UnassignableToteAttribute : PXEntityAttribute, IPXFieldSelectingSubscriber
		{
			protected override bool ChildrenAttributesComeFirstFor<ISubscriber>()
			{
				if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
					return true;

				return base.ChildrenAttributesComeFirstFor<ISubscriber>();
			}

			void IPXFieldSelectingSubscriber.FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (e.ReturnValue is null || e.ReturnValue is string strValue && strValue == UnassignedToteID.ToString() || e.ReturnValue is int intValue && intValue == UnassignedToteID)
					e.ReturnValue = PXMessages.LocalizeNoPrefix(Messages.Unassigned);
			}
		}
	}
}
