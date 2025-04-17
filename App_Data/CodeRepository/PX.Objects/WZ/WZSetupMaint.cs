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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.WZ
{
    public class WZSetupMaint : PXGraph<WZSetupMaint>
    {
        public PXSelect<WZSetup> Setup;

        protected virtual IEnumerable setup()
        {
            PXCache cache = Setup.Cache;
            PXResultset<WZSetup> ret = PXSelect<WZSetup>.SelectSingleBound(this, null);

            if (ret.Count == 0)
            {
                WZSetup setup = (WZSetup)cache.Insert(new WZSetup());
                cache.IsDirty = false;
                ret.Add(new PXResult<WZSetup>(setup));
            }
            else if (cache.Current == null)
            {
                cache.SetStatus((WZSetup)ret, PXEntryStatus.Notchanged);
            }

            return ret;
        }

        public PXAction<WZSetup> enableWizards;
        public PXAction<WZSetup> disableWizards;

        public virtual IEnumerable EnableWizards(PXAdapter adapter)
        {
            WZSetup setup = this.Setup.Select();
            setup.WizardsStatus = true;
            this.Setup.Update(setup);
            this.Actions.PressSave();

            //Activate main scenario
            PXSiteMap.Provider.Clear();
            WZTaskEntry wzGraph = PXGraph.CreateInstance<WZTaskEntry>();
            foreach (WZScenario activeScenario in PXSelect<WZScenario, Where<WZScenario.nodeID, Equal<Required<WZScenario.nodeID>>>>.Select(this, Guid.Empty))
            {
                wzGraph.Scenario.Current = activeScenario;
                wzGraph.activateScenarioWithoutRefresh.Press();
            }
            return adapter.Get();
        }

        public virtual IEnumerable DisableWizards(PXAdapter adapter)
        {
            WZSetup setup = this.Setup.Select();
            setup.WizardsStatus = false;
            this.Setup.Update(setup);
            this.Actions.PressSave();

            //Deactivate all scenarios
            PXSiteMap.Provider.Clear();

            WZTaskEntry wzGraph = PXGraph.CreateInstance<WZTaskEntry>();
            foreach (WZScenario activeScenario in PXSelect<WZScenario, Where<WZScenario.status, Equal<Required<WZScenario.status>>>>.Select(this, WizardScenarioStatusesAttribute._ACTIVE))
            {
                wzGraph.Scenario.Current = activeScenario;
                wzGraph.completeScenarioWithoutRefresh.Press();
            }
            return adapter.Get();
        }
    }
}
