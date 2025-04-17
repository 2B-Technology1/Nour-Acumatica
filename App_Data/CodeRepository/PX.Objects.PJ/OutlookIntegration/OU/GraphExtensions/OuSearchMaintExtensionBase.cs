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
using System.Linq;
using PX.Objects.PJ.OutlookIntegration.OU.DAC;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.OutlookIntegration.OU.GraphExtensions
{
    public class OuSearchMaintExtensionBase : PXGraphExtension<OUSearchMaint>
    {
        public PXFilter<RequestForInformationOutlook> RequestForInformationOutlook;
        public PXFilter<ProjectIssueOutlook> ProjectIssueOutlook;

        public void CreateEntity(Action createEntity)
        {
            try
            {
                Base.Filter.Current.ErrorMessage = null;
                using (var scope = new PXTransactionScope())
                {
                    createEntity();
                    scope.Complete();
                }
                Base.Filter.Current.Operation = null;
            }
            catch (PXOuterException exception)
            {
                Base.Filter.Current.ErrorMessage = exception.InnerMessages.First();
            }
            catch (Exception exception)
            {
                Base.Filter.Current.ErrorMessage = exception.InnerException?.Message ?? exception.Message;
            }
        }
    }
}
