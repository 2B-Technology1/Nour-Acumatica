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

using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Relations
{
    public class DrawingLogsMaintRelationsExtension : DrawingLogRelationsBaseExtension<DrawingLogsMaint>
    {
        public new static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected override DrawingLog GetCurrentDrawingLog()
        {
            return Base.DrawingLogs.Current;
        }

        protected override WebDialogResult ShowConfirmationDialog(string message)
        {
            return Base.DrawingLogs.Ask(SharedMessages.Warning, message, MessageButtons.OKCancel);
        }
    }
}