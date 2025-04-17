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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR.Extensions
{
	#region DACs

	[Serializable]
	[PXHidden]
	public class NotificationFilter : IBqlTable
	{
		#region NotificationName

		public abstract class notificationName : PX.Data.BQL.BqlString.Field<notificationName> { }

		[PXSelector(typeof(Search2<Notification.name, LeftJoin<SiteMap, On<SiteMap.screenID, Equal<Notification.screenID>>>>),
			fieldList: new []
			{
				typeof(Notification.name),
				typeof(SiteMap.title),
				typeof(Notification.screenID),
				typeof(Notification.subject)
			},
			Headers = new string[] { "Description", "Screen Name", "Screen ID", "Subject" },
			DescriptionField = typeof(Notification.name))]
		[PXString(255, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Template")]
		[PXDefault()]
		public virtual string NotificationName { get; set; }

		#endregion

		#region ReplaceEmailContents

		public abstract class replaceEmailContents : PX.Data.BQL.BqlBool.Field<replaceEmailContents> { }

		[PXBool]
		[PXUIField(DisplayName = "Replace Email Contents", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(false)]
		public virtual bool? ReplaceEmailContents { get; set; }

		#endregion
	}

	#endregion

	/// <exclude/>
	public abstract class CRSelectNotificatonTemplateExt<TGraph, TMaster>
		: PXGraphExtension<TGraph>
			where TGraph : PXGraph
			where TMaster : class, IBqlTable, new()
	{
		#region Views

		[PXViewName(Messages.NotificationTemplate)]
		public CRValidationFilter<NotificationFilter> NotificationInfo;

		#endregion

		#region Actions

		public PXAction<TMaster> LoadEmailSource;
		[PXUIField(DisplayName = "Select Template", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = Messages.SelectTemplateTooltip)]
		protected virtual IEnumerable loadEmailSource(PXAdapter adapter)
		{
			if (NotificationInfo.AskExtFullyValid(DialogAnswerType.Positive))
			{
				Notification notification = PXSelect<Notification, Where<Notification.name, Equal<Required<Notification.name>>>>
					.Select(Base, NotificationInfo.Current.NotificationName);

				var _attachments = new List<Guid>();
				foreach (NoteDoc noteDoc in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(Base, notification.NoteID))
				{
					_attachments.Add((Guid)noteDoc.FileID);
				}

				PXNoteAttribute.SetFileNotes(Base.Caches<TMaster>(), Base.Caches<TMaster>().Current, _attachments.ToArray());

				MapData(notification);
			}

			if (Base.IsContractBasedAPI)
				Base.Actions.PressSave();

			return adapter.Get();
		}

		#endregion

		#region Methods

		public abstract void MapData(Notification notification);

		#endregion
	}
}
