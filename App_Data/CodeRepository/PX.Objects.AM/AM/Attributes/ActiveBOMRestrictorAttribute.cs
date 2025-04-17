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
using PX.Data.BQL.Fluent;

namespace PX.Objects.AM.Attributes
{
	/// <summary>
	/// <see cref="PXRestrictorAttribute"/> for <see cref="AMBomItem"/> to restrict by boms which contain at least one active revision
	/// </summary>
	public class ActiveBOMRestrictorAttribute : PXRestrictorAttribute
    {
        public ActiveBOMRestrictorAttribute() 
            : base(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), 
                  Messages.BomIsNotActive, 
                  typeof(AMBomItem.bOMID))
        {
        }

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall || e.NewValue == null)
			{
				return;
			}

			var activeBom = SelectFrom<AMBomItemActiveAggregate>
				.Where<AMBomItemActiveAggregate.bOMID.IsEqual<@P.AsString>>
				.View.Select(sender.Graph, e.NewValue).TopFirst;

			if(activeBom?.BOMID != null)
            {
				// bom has at least one active record
				return;
            }

			object errorValue = e.NewValue;
			sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
			PXFieldState state = errorValue as PXFieldState;
			e.NewValue = state != null ? state.Value : errorValue;

			throw new PXSetPropertyException(Messages.BomIsNotActive, e.NewValue);
		}
    }
}