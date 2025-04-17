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

namespace PX.Objects.Localizations.GB.HMRC
{
    // Token: 0x02000011 RID: 17
    [Serializable]
	[PXHidden]
	public class HMRCHeaderData : IBqlTable
    {
        #region Gov Headers

        #region GovClientConnectionMethod
        [PXDBString(255)]
        public virtual string GovClientConnectionMethod { get; set; }
        public abstract class govClientConnectionMethod : PX.Data.BQL.BqlString.Field<govClientConnectionMethod> { }
        #endregion
        #region GovClientPublicIP
        [PXDBString(255)]
        public virtual string GovClientPublicIP { get; set; }
        public abstract class govClientPublicIP : PX.Data.BQL.BqlString.Field<govClientPublicIP> { }
        #endregion
        #region GovClientPublicIPTimestamp
        [PXDBString(255)]
        public virtual string GovClientPublicIPTimestamp { get; set; }
        public abstract class govClientPublicIPTimestamp : PX.Data.BQL.BqlString.Field<govClientPublicIPTimestamp> { }
        #endregion
        #region GovClientPublicPort
        [PXDBString(255)]
        public virtual string GovClientPublicPort { get; set; }
        public abstract class govClientPublicPort : PX.Data.BQL.BqlString.Field<govClientPublicPort> { }
        #endregion
        #region GovClientDeviceID
        [PXDBString(255)]
        public virtual string GovClientDeviceID { get; set; }
        public abstract class govClientDeviceID : PX.Data.BQL.BqlString.Field<govClientDeviceID> { }
        #endregion
        #region GovClientUserIDs
        [PXDBString(255)]
        public virtual string GovClientUserIDs { get; set; }
        public abstract class govClientUserIDs : PX.Data.BQL.BqlString.Field<govClientUserIDs> { }
        #endregion
        #region GovClientTimezone
        [PXDBString(255)]
        public virtual string GovClientTimezone { get; set; }
        public abstract class govClientTimezone : PX.Data.BQL.BqlString.Field<govClientTimezone> { }
        #endregion
        #region GovClientLocalIPs
        [PXDBString(255)]
        public virtual string GovClientLocalIPs { get; set; }
        public abstract class govClientLocalIPs : PX.Data.BQL.BqlString.Field<govClientLocalIPs> { }
        #endregion
        #region GovClientLocalIPsTimestamp
        [PXDBString(255)]
        public virtual string GovClientLocalIPsTimestamp { get; set; }
        public abstract class govClientLocalIPsTimestamp : PX.Data.BQL.BqlString.Field<govClientLocalIPsTimestamp> { }
        #endregion
        #region GovClientScreens
        [PXDBString(255)]
        public virtual string GovClientScreens { get; set; }
        public abstract class govClientScreens : PX.Data.BQL.BqlString.Field<govClientScreens> { }
        #endregion
        #region GovClientWindowSize
        [PXDBString(255)]
        public virtual string GovClientWindowSize { get; set; }
        public abstract class govClientWindowSize : PX.Data.BQL.BqlString.Field<govClientWindowSize> { }
        #endregion
        #region GovClientBrowserPlugins
        [PXDBString(255)]
        public virtual string GovClientBrowserPlugins { get; set; }
        public abstract class govClientBrowserPlugins : PX.Data.BQL.BqlString.Field<govClientBrowserPlugins> { }
        #endregion
        #region GovClientBrowserJSUserAgent
        [PXDBString(255)]
        public virtual string GovClientBrowserJSUserAgent { get; set; }
        public abstract class govClientBrowserJSUserAgent : PX.Data.BQL.BqlString.Field<govClientBrowserJSUserAgent> { }
        #endregion
        #region GovClientBrowserDoNotTrack
        [PXDBString(255)]
        public virtual string GovClientBrowserDoNotTrack { get; set; }
        public abstract class govClientBrowserDoNotTrack : PX.Data.BQL.BqlString.Field<govClientBrowserDoNotTrack> { }
        #endregion
        #region GovClientMultiFactor
        [PXDBString(255)]
        public virtual string GovClientMultiFactor { get; set; }
        public abstract class govClientMultiFactor : PX.Data.BQL.BqlString.Field<govClientMultiFactor> { }
        #endregion
        #region GovVendorProductName
        [PXDBString(255)]
        public virtual string GovVendorProductName { get; set; }
        public abstract class govVendorProductName : PX.Data.BQL.BqlString.Field<govVendorProductName> { }
        #endregion
        #region GovVendorVersion
        [PXDBString(255)]
        public virtual string GovVendorVersion { get; set; }
        public abstract class govVendorVersion : PX.Data.BQL.BqlString.Field<govVendorVersion> { }
        #endregion
        #region GovVendorLicenseIDs
        [PXDBString(255)]
        public virtual string GovVendorLicenseIDs { get; set; }
        public abstract class govVendorLicenseIDs : PX.Data.BQL.BqlString.Field<govVendorLicenseIDs> { }
        #endregion
        #region GovVendorPublicIP
        [PXDBString(255)]
        public virtual string GovVendorPublicIP { get; set; }
        public abstract class govVendorPublicIP : PX.Data.BQL.BqlString.Field<govVendorPublicIP> { }
        #endregion
        #region GovVendorForwarded
        [PXDBString(255)]
        public virtual string GovVendorForwarded { get; set; }
        public abstract class govVendorForwarded : PX.Data.BQL.BqlString.Field<govVendorForwarded> { }
        #endregion

        #endregion
    }
}
