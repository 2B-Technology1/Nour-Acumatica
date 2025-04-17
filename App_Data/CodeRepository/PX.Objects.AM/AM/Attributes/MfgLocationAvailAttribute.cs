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
using PX.Data.BQL;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing warehouse location available attribute.
    /// Includes location restrictions related to production locations.
    /// </summary>
    public class MfgLocationAvailAttribute : LocationAvailAttribute
    {
        /// <summary>
        /// Should the location default a value.
        /// (Default is true)
        /// </summary>
        public bool DefaultLocation { get; set; }

        /// <summary>
        /// Always restricted by production allowed bin locations
        /// </summary>
        protected Type _IsProductionType = BqlCommand.Compose(typeof(Where<,>), typeof(boolTrue), typeof(Equal<boolTrue>));
        /// <summary>
        /// Filed representing a condition that excludes the restriction on mfg only locations (any location allowed)
        /// </summary>
        protected Type _IgnoreLocationRestrictionField;
        /// <summary>
        /// If the _IgnoreLocationRestrictionField comes from a different/parent DAC... set the parent type
        /// </summary>
        protected Type _IgnoreLocationRestrictionParent;

        public MfgLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, bool IsSalesType, bool IsReceiptType)
            : this(InventoryType, SubItemType, SiteIDType, IsSalesType, IsReceiptType, null, null)
        {
        }

        public MfgLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, bool IsSalesType, bool IsReceiptType, Type IgnoreLocationRestrictionField)
            : this(InventoryType, SubItemType, SiteIDType, IsSalesType, IsReceiptType, IgnoreLocationRestrictionField, null)
        {
        }

        public MfgLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, bool IsSalesType, bool IsReceiptType, Type IgnoreLocationRestrictionField, Type IgnoreLocationRestrictionParent)
            : base(InventoryType, SubItemType, typeof(CostCenter.freeStock), SiteIDType, IsSalesType, IsReceiptType, false)
        {
            _IgnoreLocationRestrictionField = IgnoreLocationRestrictionField;
            _IgnoreLocationRestrictionParent = IgnoreLocationRestrictionParent;
            ReplaceAttributes(InventoryType);
            ReplaceSelectorAttribute(InventoryType, SubItemType, SiteIDType);
            DefaultLocation = true;
        }

        public MfgLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type IsSalesType, Type IsReceiptType)
            : base(InventoryType, SubItemType, typeof(CostCenter.freeStock), SiteIDType, IsSalesType, IsReceiptType, null)
        {
            ReplaceAttributes(InventoryType);
            ReplaceSelectorAttribute(InventoryType, SubItemType, SiteIDType);
            DefaultLocation = true;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            // Fix SQL bound error when trying to sort selector by INLocationStatus field:
            sender.Graph.Caches.AddCacheMapping(typeof(INLocationStatusByCostCenter), typeof(INLocationStatusByCostCenter));
        }

        /// <summary>
        /// Replace the base selector with a modified production related selector
        /// </summary>
        protected void ReplaceSelectorAttribute(Type InventoryType, Type SubItemType, Type SiteIDType)
        {
            var search = BqlTemplate.OfCommand<
                    Search<INLocation.locationID,
                        Where2<Match<Current<AccessInfo.userName>>,
                            And<INLocation.siteID, Equal<Optional<SiteIDPhAM>>>>>>
                .Replace<SiteIDPhAM>(SiteIDType)
                .ToType();

            var lookupJoin = BqlTemplate.OfJoin<
                    LeftJoin<INLocationStatusByCostCenter,
                        On<INLocationStatusByCostCenter.locationID, Equal<INLocation.locationID>,
                            And<INLocationStatusByCostCenter.inventoryID, Equal<Optional<InventoryPhAM>>,
                            And<INLocationStatusByCostCenter.subItemID, Equal<Optional<SubItemPhAM>>,
							And<INLocationStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>>>>>
                .Replace<InventoryPhAM>(InventoryType)
                .Replace<SubItemPhAM>(SubItemType)
                .ToType();

            Type[] fieldList =
            {
                typeof(INLocation.locationCD),
                typeof(INLocationStatusByCostCenter.qtyOnHand),
                typeof(INLocationStatusByCostCenter.active),
                typeof(INLocation.primaryItemID),
                typeof(INLocation.primaryItemClassID),
                typeof(INLocation.receiptsValid),
                typeof(INLocation.salesValid),
                typeof(INLocation.productionValid),
                typeof(INLocation.projectID),
                typeof(INLocation.taskID),
                typeof(INLocationExt.aMMRPFlag)
            };
            var attr = new PXDimensionSelector.WithCachingByCompositeKeyAttribute(DimensionName, search, GetSiteIDKeyRelation(SiteIDType), lookupJoin, typeof(INLocation.locationCD), fieldList);

            attr.ValidComboRequired = true;
            attr.DirtyRead = true;
            attr.DescriptionField = typeof(INLocation.descr);
            _Attributes[_SelAttrIndex] = attr;
        }

        /// <summary>
        /// Make sure the correct attributes are in place for Manufacturing locations.
        /// Swap the LocationRestrictorAttribute with the MfgLocationRestrictorAttribute.
        /// </summary>
        protected void ReplaceAttributes(Type inventoryType)
        {
            PXEventSubscriberAttribute replaceLocationRestrictorAttribute = null;
            var foundPrimaryItemRestrcitor = false;
            var foundMfgLocRestrictor = false;
            foreach (var pxEventSubscriberAttribute in this._Attributes)
            {
                if (pxEventSubscriberAttribute is LocationRestrictorAttribute && !(pxEventSubscriberAttribute is MfgLocationRestrictorAttribute))
                {
                    replaceLocationRestrictorAttribute = pxEventSubscriberAttribute;
                    continue;
                }

                if (pxEventSubscriberAttribute is MfgLocationRestrictorAttribute)
                {
                    foundMfgLocRestrictor = true;
                    continue;
                }

                if (pxEventSubscriberAttribute is PrimaryItemRestrictorAttribute)
                {
                    foundPrimaryItemRestrcitor = true;
                    continue;
                }

                if (foundPrimaryItemRestrcitor && foundMfgLocRestrictor && replaceLocationRestrictorAttribute != null)
                {
                    break;
                }
            }

            if (replaceLocationRestrictorAttribute != null)
            {
                _Attributes.Remove(replaceLocationRestrictorAttribute);
            }

            if (!foundMfgLocRestrictor)
            {
                _Attributes.Add(new MfgLocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsProductionType, _IgnoreLocationRestrictionField, _IgnoreLocationRestrictionParent));
            }

            if (!foundPrimaryItemRestrcitor)
            {
                _Attributes.Add(new PrimaryItemRestrictorAttribute(inventoryType, _IsReceiptType, _IsSalesType, typeof(Where<True, Equal<False>>)));
            }
        }

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (DefaultLocation || !InventoryHelper.MultiWarehouseLocationFeatureEnabled)
            {
                base.FieldDefaulting(sender, e);
                return;
            }

            e.Cancel = true;
        }

        [PXHidden]
        protected class InventoryPhAM : BqlPlaceholderBase
        {
        }

        [PXHidden]
        protected class SubItemPhAM : BqlPlaceholderBase
        {
        }

        [PXHidden]
        protected class SiteIDPhAM : BqlPlaceholderBase
        {
        }
    }
}
