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
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.FS
{
    public class PXRedirectToBoardRequiredException : PXRedirectToUrlException
    {
        private static string BuildUrl(string baseBoardUrl, KeyValuePair<string, string>[] args)
        {
            StringBuilder boardUrl = new StringBuilder(@"~\");
            boardUrl.Append(baseBoardUrl);

            if (args != null && args.Length > 0)
            {
                boardUrl.Append("?");

                KeyValuePair<string, string> kvp;

                for (int i = 0; i < args.Length; i++)
                {
                    kvp = args[i];
                    boardUrl.Append(kvp.Key);
                    boardUrl.Append("=");
                    boardUrl.Append(kvp.Value);

                    if (i != args.Length - 1)
                    {
                        boardUrl.Append("&");
                    }
                }
            }

            return boardUrl.ToString();
        }

        public PXRedirectToBoardRequiredException(string baseBoardUrl, KeyValuePair<string, string>[] parameters, WindowMode windowMode = WindowMode.NewWindow, bool supressFrameset = true)
            : base(BuildUrl(baseBoardUrl, parameters), windowMode, supressFrameset, null)
        {
        }
    }
}
