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
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    /// <summary>Defines the default mapping of the <see cref="DocumentLine" /> mapped cache extension to a DAC.</summary>
    public class DocumentLineMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(DocumentLine);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;
        /// <summary>Creates the default mapping of the <see cref="DocumentLine" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public DocumentLineMapping(Type table) { _table = table; }
    }
}
