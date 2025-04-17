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

using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using PX.Common;

namespace PX.Objects.GDPR
{
	public class GDPREraseProcess : GDPRPseudonymizeProcess
	{
		#region ctor

		public GDPREraseProcess()
		{
			GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.notPseudonymized);
			SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.Erased;

			SelectedItems.SetProcessDelegate(delegate (List<ObfuscateEntity> entries)
			{
				var graph = PXGraph.CreateInstance<GDPREraseProcess>();

				Process(entries, graph);
			});

			SelectedItems.SetProcessCaption(Messages.Erase);
			SelectedItems.SetProcessAllCaption(Messages.EraseAll);
		}

		#endregion

		#region Implementation

		protected override void TopLevelProcessor(string combinedKey, Guid? topParentNoteID, string info)
		{
			DeleteSearchIndex(topParentNoteID);
		}

		protected override void ChildLevelProcessor(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			PseudonymizeChilds(processingGraph, childTable, fields, childs);
			
			WipeAudit(processingGraph, childTable, fields, childs);
		}
		
		#endregion
	}
}
