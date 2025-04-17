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
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.EP
{
    public class EPApprovalSettings<SetupApproval, DocType> : IPrefetchable
        where SetupApproval : IBqlTable
		where DocType : IBqlField

    {
        public static Type IsApprovalDisabled<DocType, DocTypes>(params string[] availableDocTypes)
            where DocType : IBqlField => Slot.IsApprovalDisableCommand<DocType, DocTypes>(availableDocTypes);

        public static Type IsApprovalDisabled<DocType, DocTypes, StateCondition>(params string[] availableDocTypes)
            where DocType : IBqlField 
            where StateCondition : IBqlUnary => 
            ComposeAnd<StateCondition>(Slot.IsApprovalDisableCommand<DocType, DocTypes>(availableDocTypes));
        
        public static Type ComposeAnd<AddCondition>(Type baseCondition)
            where AddCondition : IBqlUnary =>
                BqlCommand.Compose(typeof(Where2<,>),
                baseCondition,
                typeof(And<>),
                typeof(AddCondition));
        
        public static List<string> ApprovedDocTypes => Slot.DocTypes;
        
        private static EPApprovalSettings<SetupApproval, DocType> Slot => PXDatabase
            .GetSlot<EPApprovalSettings<SetupApproval, DocType>>(typeof(SetupApproval).FullName, typeof(SetupApproval));
        private List<string> DocTypes { get; set; }

        void IPrefetchable.Prefetch()
        {
            DocTypes = new List<string>();
            if (!PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>()) return;
            HashSet<string> hashDocTypes = new HashSet<string>();
            foreach (PXDataRecord rec in PXDatabase.SelectMulti<SetupApproval>(
                new PXDataField<DocType>(),
                new PXDataFieldValue<AP.APSetupApproval.isActive>(true)))
            {
                hashDocTypes.Add(rec.GetString(0).Trim());
            }
            DocTypes = hashDocTypes.ToList();
        }

        private Type IsApprovalDisableCommand<DocType, DocTypeList>(params string[] availableDocTypes)
            where DocType : IBqlField
        {
            Type command = null;
            
            Type type = typeof(DocTypeList);
            var constans = new Dictionary<string, Type>();
            foreach (var constant in
                type
                    .GetNestedTypes()
                    .Where(t => typeof(IConstant).IsAssignableFrom(t)))
            {
                if (Activator.CreateInstance(constant) is IConstant c)
                {
                    var key = c.Value.ToString();
                    if(!constans.ContainsKey(key))
                        constans.Add(key, constant);
                }
            }
            
            foreach (string docType in DocTypes
                .Where(e => availableDocTypes.Length == 0 || availableDocTypes.Contains(e)))
            {
                if (constans.TryGetValue(docType, out var constType))
                {
                    command = (command == null)
                        ? BqlCommand.Compose(typeof(Where<,>), typeof(DocType), typeof(Equal<>), constType)
                        : BqlCommand.Compose(typeof(Where<,,>), typeof(DocType), typeof(Equal<>), constType,
                            typeof(Or<>),
                            command);
                }
            }
            
            return command == null
                ? typeof(Where<True, Equal<True>>)
                : BqlCommand.Compose(typeof(Not<>), command);
        }
    }

	public class EPApprovalSettings<SetupApproval> : EPApprovalSettings<SetupApproval, AP.APSetupApproval.docType>
	where SetupApproval : IBqlTable
	{ }
}
