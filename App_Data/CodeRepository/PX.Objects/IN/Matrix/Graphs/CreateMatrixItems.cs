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
using PX.Data;
using PX.Objects.IN.Matrix.GraphExtensions;
using PX.Objects.IN.Matrix.DAC.Unbound;

namespace PX.Objects.IN.Matrix.Graphs
{
	public class CreateMatrixItems : PXGraph<CreateMatrixItems, EntryHeader>
	{
		public class CreateMatrixItemsImpl : CreateMatrixItemsExt<CreateMatrixItems, EntryHeader>
		{
		}

		public CreateMatrixItems()
		{
			Save.SetVisible(false);
			Insert.SetVisible(false);
			Delete.SetVisible(false);
			CopyPaste.SetVisible(false);
			Next.SetVisible(false);
			Previous.SetVisible(false);
			First.SetVisible(false);
			Last.SetVisible(false);
		}

		public override bool CanClipboardCopyPaste() => false;
	}
}
