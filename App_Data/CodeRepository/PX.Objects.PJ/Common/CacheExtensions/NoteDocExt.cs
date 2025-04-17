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
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.CacheExtensions
{
    [Serializable]
    public sealed class NoteDocExt : PXCacheExtension<NoteDoc>
    {
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Add")]
        public bool? IsAttached
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "File Name", Enabled = false)]
        public string FileName
        {
            get;
            set;
        }

        [PXBool]
        [PXUIField(DisplayName = "Current", Enabled = false)]
        public bool? IsDrawingLogCurrentFile
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isAttached : IBqlField
        {
        }

        public abstract class fileName : IBqlField
        {
        }

        public abstract class isDrawingLogCurrentFile : IBqlField
        {
        }
    }
}