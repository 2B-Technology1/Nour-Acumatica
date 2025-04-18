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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Vendor WIP Types
    /// </summary>
    public class AMShipType
    {
        /// <summary>
        /// Shipment
        /// </summary>
        public const string Shipment = "S";
        /// <summary>
        /// Return
        /// </summary>
        public const string Return = "R";       

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// Shipment
            /// </summary>
            public static string Shipment => Messages.GetLocal(Messages.Shipment);

            /// <summary>
            /// Return
            /// </summary>
            public static string Return => Messages.GetLocal(Messages.Return);
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[]
            {
                Shipment,
                Return
            }, new string[]
            {
                Messages.Shipment,
                Messages.Return
            })
                {
                }
        }

        public class shipment : PX.Data.BQL.BqlString.Constant<shipment>
        {
            public shipment() : base(Shipment) { }
        }

        public class typereturn : PX.Data.BQL.BqlString.Constant<typereturn>
        {
            public typereturn() : base(Return) { }
        }


        public static string GetShipTypeDesc(string docType)
        {
            if (string.IsNullOrWhiteSpace(docType))
            {
                return string.Empty;
            }

            switch (docType)
            {
                case Shipment:
                    return Desc.Shipment;
                case Return:
                    return Desc.Return;               
                default:
                    return string.Empty;
            }
        }  
    }

    public class AMShipLineType
    {
        /// <summary>
        /// WIP
        /// </summary>
        public const string WIP = "W";
        /// <summary>
        /// Material
        /// </summary>
        public const string Material = "M";

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// WIP
            /// </summary>
            public static string WIP => Messages.GetLocal(Messages.WIP);

            /// <summary>
            /// Material
            /// </summary>
            public static string Material => Messages.GetLocal(Messages.Material);
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[]
            {
                WIP,
                Material
            }, new string[]
            {
                Messages.WIP,
                Messages.Material
            })
            {
            }
        }

        public class wIP : PX.Data.BQL.BqlString.Constant<wIP>
        {
            public wIP() : base(WIP) { }
        }

        public class material : PX.Data.BQL.BqlString.Constant<material>
        {
            public material() : base(Material) { }
        }
        
        public static string GetShipTypeDesc(string docType)
        {
            if (string.IsNullOrWhiteSpace(docType))
            {
                return string.Empty;
            }

            switch (docType)
            {
                case WIP:
                    return Desc.WIP;
                case Material:
                    return Desc.Material;
                default:
                    return string.Empty;
            }
        }
    }
}