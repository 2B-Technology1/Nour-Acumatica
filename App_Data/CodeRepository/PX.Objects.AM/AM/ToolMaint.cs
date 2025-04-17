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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.GraphExtensions;
using System.Collections;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Tool Maintenance
    /// </summary>
    public class ToolMaint : PXGraph<ToolMaint, AMToolMst>
    {
        public PXSelect<AMToolMst> Tools;
        public PXSelect<AMToolMst, Where<AMToolMst.toolID, Equal<Current<AMToolMst.toolID>>>> ToolSelected;
		public SelectFrom<AMToolMstCurySettings>.Where<AMToolMstCurySettings.toolID.IsEqual<AMToolMst.toolID.FromCurrent>
			.And<AMToolMstCurySettings.curyID.IsEqual<AccessInfo.baseCuryID.AsOptional>>>.View ToolCurySelected;

		public class CurySettings : CurySettingsExtension<ToolMaint, AMToolMst, AMToolMstCurySettings>
		{
			public static bool IsActive() => true;
		}

		protected virtual void AMToolMst_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var toolRec = (AMToolMst) e.Row;
            if (toolRec == null)
            {
                return;
            }

            AMBomTool ambomtool = PXSelect<AMBomTool, 
                Where<AMBomTool.toolID, Equal<Required<AMBomTool.toolID>>>
                >.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (ambomtool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                     Messages.GetLocal(Messages.Tool_NotDeleted_OnBOM,
                            ambomtool.BOMID));
                e.Cancel = true;
                return;
            }

            AMProdTool amprodtool = PXSelect<AMProdTool, 
                Where<AMProdTool.toolID, Equal<Required<AMProdTool.toolID>>>
                >.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (amprodtool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                    Messages.GetLocal(Messages.Tool_NotDeleted_OnProd,
                        amprodtool.OrderType.TrimIfNotNullEmpty(),
                        amprodtool.ProdOrdID.TrimIfNotNullEmpty()));
                e.Cancel = true;
                return;
            }

            AMEstimateTool amestimatetool = PXSelect<AMEstimateTool, 
                Where<AMEstimateTool.toolID, Equal<Required<AMEstimateTool.toolID>>
                    >>.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (amestimatetool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                    Messages.GetLocal(Messages.Tool_NotDeleted_OnEstimate,
                        amestimatetool.EstimateID, amestimatetool.RevisionID));
                e.Cancel = true;
                return;
            }
        }

        protected virtual void RaiseToolIDExceptionHandling(PXCache cache, AMToolMst toolMst, string message)
        {
            cache.RaiseExceptionHandling<AMToolMst.toolID>(toolMst, toolMst.ToolID, new PXSetPropertyException(message,PXErrorLevel.Error));
        }
    }
}
