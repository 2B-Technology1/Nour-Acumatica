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
using PX.Data.Process;
using PX.Objects.CR;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PX.Objects.GDPR
{
	public class GDPRToolsAuditMaint : PXGraph<GDPRToolsAuditMaint>
	{
		public PXSelectOrderBy<SMPersonalDataLog,
			OrderBy<Desc<SMPersonalDataLog.createdDateTime>>> Log;

		public PXCancel<SMPersonalDataLog> Cancel;

		public PXAction<SMPersonalDataLog> OpenContact;
		[PXButton]
		[PXUIField(DisplayName = Messages.OpenContact, Visible = false)]
		public virtual IEnumerable openContact(PXAdapter adapter)
		{
			var entity = this.Caches[typeof(SMPersonalDataLog)].Current as SMPersonalDataLog;
			if (entity == null)
				return adapter.Get();

			new EntityHelper(this).NavigateToRow(entity.TableName, entity.CombinedKey.Split(PXAuditHelper.SEPARATOR), PXRedirectHelper.WindowMode.New);


			return adapter.Get();
		}

		[PXIntList(
		new[] { 0, 1, 3 },
		new[] { "Restored",
				"Pseudonymized",
				"Erased" })]
		[PXUIField(DisplayName = "Status", Visible = true)]
		[PXMergeAttributes]
		protected virtual void _(Events.CacheAttached<SMPersonalDataLog.pseudonymizationStatus> e) { }
	}
}
