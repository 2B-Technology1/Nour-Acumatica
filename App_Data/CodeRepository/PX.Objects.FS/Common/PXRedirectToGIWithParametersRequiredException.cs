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

using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PX.Objects.FS
{
    public class PXRedirectToGIWithParametersRequiredException : PXRedirectToUrlException
    {
        public PXRedirectToGIWithParametersRequiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }

        public PXRedirectToGIWithParametersRequiredException(string gIName, Dictionary<string, string> parameters, WindowMode windowMode = WindowMode.Same, bool supressFrameset = false)
            : base(BuildUrl(gIName, parameters), windowMode, supressFrameset, String.Empty)
        {
        }

        public PXRedirectToGIWithParametersRequiredException(Guid designId, Dictionary<string, string> parameters, WindowMode windowMode = WindowMode.Same, bool supressFrameset = false)
            : base(BuildUrl(designId, parameters), windowMode, supressFrameset, String.Empty)
        {
        }

        private static StringBuilder GetBaseUrl()
        {
            return new StringBuilder(PXGenericInqGrph.INQUIRY_URL).Append('?');
        }

        private static void AppendParameters(Dictionary<string, string> parameters, ref StringBuilder url)
        {
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                url.Append("&");
                url.Append(parameter.Key);
                url.Append("=");
                url.Append(parameter.Value.Trim());
            }
        }

        public static string BuildUrl(Guid designId, Dictionary<string, string> parameters)
        {
            StringBuilder url = new StringBuilder(GetBaseUrl().Append("id=").Append(designId.ToString()).ToString());

            if (parameters != null)
            {
                AppendParameters(parameters, ref url);
            }

            return url.ToString();
        }

        private static string BuildUrl(string gIName, Dictionary<string, string> parameters)
        {
            StringBuilder url = new StringBuilder(GetBaseUrl().Append("name=").Append(gIName.Replace(" ", "+")).ToString());

            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    AppendParameters(parameters, ref url);
                }
            }

            return url.ToString();
        }
    }
}
