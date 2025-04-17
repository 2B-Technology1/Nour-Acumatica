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
using System.Net.Mail;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.CR.BackwardCompatibility;
using PX.Objects.CR.Extensions;
using PX.Objects.CS;
using PX.Objects.FS;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
	public abstract class ProjectManagementActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID> : ActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		where TGraph : PXGraph, new ()
		where TPrimaryEntity : BaseCache, IBqlTable, INotable, new ()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		public Guid? CurrentProjectManagementEntityNoteId
		{
			get;
			set;
		}

		public override Guid? GetRefNoteID()
		{
			if (Base.IsMobile)
			{
				return CurrentProjectManagementEntityNoteId;
			}
			else
			{
				return base.GetRefNoteID();
			}
		}

		public virtual void _(Events.RowSelected<TPrimaryEntity> args)
		{
			this.CurrentProjectManagementEntityNoteId = this.CurrentProjectManagementEntityNoteId ?? (Base.Caches[typeof(TPrimaryEntity)].Current as BaseCache)?.NoteID;
		}

		protected void SetActivityDefaultSubject(string subjectPattern, params object[] subjectFields)
		{
			this.DefaultSubject = PXMessages.LocalizeFormatNoPrefixNLA(subjectPattern, subjectFields);
		}
	}
}
