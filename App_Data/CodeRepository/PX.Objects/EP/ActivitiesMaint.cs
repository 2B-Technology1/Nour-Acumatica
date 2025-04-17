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
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;

namespace PX.Objects.EP
{
	[Serializable]
	[PXHidden]
	public partial class ActivitySource : IBqlTable, INotable
	{
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		[PXNote]
		public Guid? NoteID { get; set; }
	}

	public class ActivitiesMaint : PXGraph<ActivitiesMaint>
	{
		#region Extensions

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ActivitiesMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<ActivitiesMaint_ActivityDetailsExt, ActivitiesMaint, ActivitySource, ActivitySource.noteID> { }

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ActivitiesMaint_ActivityDetailsExt : ActivityDetailsExt<ActivitiesMaint, ActivitySource, ActivitySource.noteID> { }

		#endregion

		[PXHidden] 
		public PXFilter<ActivitySource>
			Filter;

		public ActivitiesMaint()
		{
		}
	}
}
