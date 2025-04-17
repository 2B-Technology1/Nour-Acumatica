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
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CM
{
    [TableAndChartDashboardType]
    public class TranslationRelease : PXGraph<TranslationRelease>
    {
        [PXFilterable]
        public PXAction<TranslationHistory> cancel;
        [PX.SM.PXViewDetailsButton(typeof(TranslationHistory.referenceNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
        public PXProcessing<TranslationHistory, Where<TranslationHistory.released, Equal<False>>> TranslationReleaseList;
        public PXSetup<CMSetup> CMSetup;

        #region Implementation 
        public TranslationRelease()
        {
            CMSetup setup = CMSetup.Current;
            TranslationReleaseList.SetProcessDelegate(
                delegate(TranslationHistory transl)
                {
					var graph = CreateInstance<TranslationHistoryMaint>();
					graph.CreateBatch(transl, false);
                });

            TranslationReleaseList.SetProcessCaption(Messages.Release);
            TranslationReleaseList.SetProcessAllVisible(false);
        }
        #endregion

        #region Buttons
        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            TranslationReleaseList.Cache.Clear();
            TimeStamp = null;
            PXLongOperation.ClearStatus(this.UID);
            return adapter.Get();
        }
        #endregion
    }
}