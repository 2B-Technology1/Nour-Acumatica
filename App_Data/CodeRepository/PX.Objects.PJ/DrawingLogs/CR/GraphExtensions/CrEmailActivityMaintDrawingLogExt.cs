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
using PX.Objects.PJ.Common.GraphExtensions;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DrawingLogs.CR.GraphExtensions
{
    public class CrEmailActivityMaintDrawingLogExt : PXGraphExtension<CrEmailActivityMaintExt, CREmailActivityMaint>
    {
        [PXOverride]
        public IEnumerable send(PXAdapter adapter, CrEmailActivityMaintExt.SendDelegate baseMethod)
        {
            if (Base1.IsDrawingLogEmail())
            {
                Base.Persist();
                var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
                Base1.EmailFileAttachService.AttachDrawingLogArchive(graph);
            }
            return baseMethod(adapter);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }
    }
}