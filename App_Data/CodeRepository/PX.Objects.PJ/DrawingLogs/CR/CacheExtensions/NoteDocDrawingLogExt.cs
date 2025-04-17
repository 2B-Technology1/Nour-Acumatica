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

using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.CR.CacheExtensions
{
    public sealed class NoteDocDrawingLogExt : PXCacheExtension<NoteDocExt, NoteDoc>
    {
        [PXString]
        [PXUIField(DisplayName = DrawingLogLabels.DrawingLogId, Enabled = false)]
        public string DrawingLogCd
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Drawing Number", Enabled = false)]
        public string Number
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Revision", Enabled = false)]
        public string Revision
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class drawingLogCd : IBqlField
        {
        }

        public abstract class number : IBqlField
        {
        }

        public abstract class revision : IBqlField
        {
        }
    }
}