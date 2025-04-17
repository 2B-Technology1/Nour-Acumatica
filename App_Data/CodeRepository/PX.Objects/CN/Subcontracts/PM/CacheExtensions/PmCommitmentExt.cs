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
using PX.Objects.CN.Subcontracts.PM.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.CacheExtensions
{
    public sealed class PmCommitmentExt : PXCacheExtension<PMCommitment>
    {
        [PXString]
        [PXUIField(DisplayName = Messages.PmCommitment.RelatedDocumentType, Visible = false, Enabled = false,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXStringList(new[]
        {
            Messages.PmCommitment.PurchaseOrderType,
            Messages.PmCommitment.SalesOrderType,
            Messages.PmCommitment.SubcontractType
        }, new[]
        {
            Messages.PmCommitment.PurchaseOrderLabel,
            Messages.PmCommitment.SalesOrderLabel,
            Messages.PmCommitment.SubcontractLabel
        })]
        public string RelatedDocumentType
        {
            get;
            set;
        }

        [PXRemoveBaseAttribute(typeof(PMCommitment.PXRefNoteAttribute))]
        [CommitmentRefNote]
        public Guid? RefNoteID
        {
            get;
            set;
        }

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		public abstract class relatedDocumentType : IBqlField
        {
        }
    }
}