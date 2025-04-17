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

using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System.Collections;
using PX.Common;

namespace PX.Objects.CR.BackwardCompatibility
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CREmailActivityMaint_CbAPI_LinkActionExt : CRBaseActivityMaint_CbAPI_LinkActionExt<CREmailActivityMaint, CRSMEmail> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active 
	public class CRActivityMaint_CbAPI_LinkActionExt : CRBaseActivityMaint_CbAPI_LinkActionExt<CRActivityMaint, CRActivity> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class EPEventMaint_CbAPI_LinkActionExt : CRBaseActivityMaint_CbAPI_LinkActionExt<EPEventMaint, CRActivity> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRTaskMaint_CbAPI_LinkActionExt : CRBaseActivityMaint_CbAPI_LinkActionExt<CRTaskMaint, CRActivity> { }

	[PXInternalUseOnly]
	public abstract class CRBaseActivityMaint_CbAPI_LinkActionExt<TGraph, TMain> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		public PXAction<TMain> attachRefNote;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable AttachRefNote(PXAdapter adapter) => adapter.Get();
	}
}
