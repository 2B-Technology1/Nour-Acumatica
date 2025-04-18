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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table with the product configurator rule results. The data is based on the configuration maintenance data from the <see cref="AMConfigurationRule"/> class with the entered or calculated results during the configuration entry. New data is stored for each new configuration result.
	/// The <see cref="AMConfigResultsRule.RuleSource"/> class indicates that the rule is for a feature or an attribute.
	/// Parent: <see cref = "AMConfigurationResults"/>
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("[{ConfigResultsID}][{ConfigurationID}:{Revision}]{RuleTarget}:{TargetLineNbr}:{TargetSubLineNbr}:{RuleSource}:{RuleSourceLineNbr}:{RuleLineNbr}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationResultRule)]
    public class AMConfigResultsRule : IBqlTable, IRuleValid, IProductConfigResult
    {
		#region Keys

        public class PK : PrimaryKeyOf<AMConfigResultsRule>.By<configResultsID, ruleTarget, targetLineNbr, targetSubLineNbr, ruleSource, ruleSourceLineNbr, ruleLineNbr>
        {
            public static AMConfigResultsRule Find(PXGraph graph, int? configResultsID, string ruleTarget, int? targetLineNbr, int? targetSubLineNbr, string ruleSource, int? ruleSourceLineNbr, int? ruleLineNbr, PKFindOptions options = PKFindOptions.None) 
                => FindBy(graph, configResultsID, ruleTarget, targetLineNbr, targetSubLineNbr, ruleSource, ruleSourceLineNbr, ruleLineNbr, options);

            public static AMConfigResultsRule FindDirty(PXGraph graph, int? configResultsID, string ruleTarget, int? targetLineNbr, int? targetSubLineNbr, string ruleSource, int? ruleSourceLineNbr, int? ruleLineNbr)
                => PXSelect<AMConfigResultsRule,
                        Where<configResultsID, Equal<Required<configResultsID>>,
                            And<ruleTarget, Equal<Required<ruleTarget>>,
                                And<targetLineNbr, Equal<Required<targetLineNbr>>,
                                    And<targetSubLineNbr, Equal<Required<targetSubLineNbr>>,
                                        And<ruleSource, Equal<Required<ruleSource>>,
                                            And<ruleSourceLineNbr, Equal<Required<ruleSourceLineNbr>>,
                                                And<ruleLineNbr, Equal<Required<ruleLineNbr>>>>>>>>>>
                    .SelectWindowed(graph, 0, 1, configResultsID, ruleTarget, targetLineNbr, targetSubLineNbr, ruleSource, ruleSourceLineNbr, ruleLineNbr);
        }

        public static class FK
        {
            public class ConfiguraitonResult : AMConfigurationResults.PK.ForeignKeyOf<AMConfigResultsRule>.By<configResultsID> { }
            public class Configuraiton : AMConfiguration.PK.ForeignKeyOf<AMConfigResultsRule>.By<configurationID, revision> { }
        }

        #endregion

		#region ConfigResultsID (key)
		public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }


        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(AMConfigurationResults.configResultsID))]
        [PXParent(typeof(Select<AMConfigurationResults, Where<AMConfigurationResults.configResultsID, Equal<Current<configResultsID>>>>))]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
        public virtual int? ConfigResultsID { get; set; }
		#endregion
		#region ConfigurationID
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		[PXDBString(15, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
        public virtual string ConfigurationID { get; set; }
        #endregion
        #region Revision
        public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

		[PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.revision))]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual string Revision { get; set; }
        #endregion
        #region RuleTarget (key)
        public abstract class ruleTarget : PX.Data.BQL.BqlString.Field<ruleTarget> { }

        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Rule Target", Visible = false, Enabled = false)]
        [RuleTargetSource.TargetList]
        public virtual string RuleTarget { get; set; }
        #endregion
        #region TargetLineNbr (key)
        public abstract class targetLineNbr : PX.Data.BQL.BqlInt.Field<targetLineNbr> { }


        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Target Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? TargetLineNbr { get; set; }
        #endregion

        #region TargetSubLineNbr (key)
        public abstract class targetSubLineNbr : PX.Data.BQL.BqlInt.Field<targetSubLineNbr> { }


        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Target Sub-Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? TargetSubLineNbr { get; set; }
        #endregion
        #region RuleSource (key)
        public abstract class ruleSource : PX.Data.BQL.BqlString.Field<ruleSource> { }


        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Rule Source", Visible = false, Enabled = false)]
        [RuleTargetSource.SourceList]
        public virtual string RuleSource { get; set; }
        #endregion
        #region RuleSourceLineNbr (key)
        public abstract class ruleSourceLineNbr : PX.Data.BQL.BqlInt.Field<ruleSourceLineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Rule Source Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? RuleSourceLineNbr { get; set; }
        #endregion
        #region RuleLineNbr (key)
        public abstract class ruleLineNbr : PX.Data.BQL.BqlInt.Field<ruleLineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Rule Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? RuleLineNbr { get; set; }
        #endregion

        #region RuleType
        public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }


        [PXDBString(1, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Rule Type")]
        [RuleTypes.List]
        public virtual string RuleType { get; set; }
        #endregion

        #region IsSoftRule
        public abstract class isSoftRule : PX.Data.BQL.BqlBool.Field<isSoftRule> { }
        [PXDBBool]
        [PXDefault]
        public bool? IsSoftRule { get; set; }
        #endregion

        #region RuleValid
        public abstract class ruleValid : PX.Data.BQL.BqlBool.Field<ruleValid> { }

        [PXDBBool]
        [PXDefault(true)]
        public virtual bool? RuleValid { get; set; }
        #endregion

        #region Condition
        public abstract class condition : PX.Data.BQL.BqlString.Field<condition> { }
        [PXDBString(2, InputMask = "")]
        [PXUIField(DisplayName = "Condition")]
        [RuleFormulaConditions.List]
        [PXDefault(RuleFormulaConditions.Equal)]
        public string Condition { get; set; }
        #endregion

        #region CalcValue
        public abstract class calcValue : PX.Data.BQL.BqlString.Field<calcValue> { }
        [PXDBString]
        [PXUIField(DisplayName = "Value")]
        public string CalcValue { get; set; }
        #endregion

        #region CalcValue1
        public abstract class calcValue1 : PX.Data.BQL.BqlString.Field<calcValue1> { }
        [PXDBString]
        [PXUIField(DisplayName = "Value 1")]
        public string CalcValue1 { get; set; }
        #endregion

        #region CalcValue2
        public abstract class calcValue2 : PX.Data.BQL.BqlString.Field<calcValue2> { }
        [PXDBString]
        [PXUIField(DisplayName = "Value 2")]
        public string CalcValue2 { get; set; }
        #endregion
        #region System Fields
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #endregion
    }

    public interface IRuleValid
    {
        bool? RuleValid { get; set; }
    }
}
