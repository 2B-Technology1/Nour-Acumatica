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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
    public class ValidateRequiredRelatedItems: ValidateRequiredRelatedItems<SOShipmentEntry, SOOrder, SOLine>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.relatedItems>();

        /// <summary>
        /// Overrides <see cref="SOShipmentEntry.ValidateLineBeforeShipment"/>
        /// </summary>
        [PXOverride]
        public virtual bool ValidateLineBeforeShipment(SOLine line, Func<SOLine, bool> baseImpl)
        {
            if (!Validate(line))
                return false;

            return baseImpl(line);
        }

        public override void ThrowError() 
        {
            if (IsMassProcessing)
                throw new PXException(IN.RelatedItems.Messages.ShipmentCannotBeCreatedOnProcessingScreen);
            throw new PXException(IN.RelatedItems.Messages.ShipmentCannotBeCreated);
        }
    }
}
