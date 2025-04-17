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
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCINCategoryMaintExt : PXGraphExtension<INCategoryMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		[PXBreakInheritance]
		[PXHidden]
		public class SelectedINCategory : INCategory
		{
			public abstract new class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			[PXNote]
			public override Guid? NoteID { get; set; }

		}
		public PXSelectReadonly<SelectedINCategory> SelectedCategory;

		public override void Initialize()
		{
			base.Initialize();

			if (SelectedCategory.Current != null)
			{
				Base.Folders.Cache.ActiveRow = SelectedCategory.Current;
				SelectedCategory.Current = null;
			}
		}

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void INCategory_LastModifiedDateTime_CacheAttached(PXCache sender) { }
	}
}