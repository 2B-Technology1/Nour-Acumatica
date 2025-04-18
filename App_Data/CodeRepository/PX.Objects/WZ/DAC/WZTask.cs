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
using System.Text;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.WZ
{
    [Serializable]
	[PXHidden]
	public partial class WZTask : IBqlTable
    {
        #region ScenraioID
        public abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        protected Guid? _ScenarioID;

        [PXDBGuid()]
        [PXSelector(typeof(WZScenario.scenarioID), SubstituteKey = typeof(WZScenario.name))]
        [PXParent(typeof(Select<WZScenario, Where<WZScenario.scenarioID, Equal<Current<WZTask.scenarioID>>>>))]
        public virtual Guid? ScenarioID
        {
            get
            {
                return this._ScenarioID;
            }
            set
            {
                this._ScenarioID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }

        protected Guid? _TaskID;

        [PXDBGuid(IsKey = true)]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region ParentTaskID
        public abstract class parentTaskID : PX.Data.BQL.BqlGuid.Field<parentTaskID> { }

        protected Guid? _ParentTaskID;
        [PXDBGuid]
        [PXUIField(DisplayName = "Parent Task", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? ParentTaskID
        {
            get
            {
                return this._ParentTaskID;
            }
            set
            {
                this._ParentTaskID = value;
            }
        }
        #endregion
        #region Name
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

        protected String _Name;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }

        }
        #endregion
        #region Position
        public abstract class position : PX.Data.BQL.BqlInt.Field<position> { }

        protected Int32? _Position;
        [PXDefault]
        [PXDBInt]
        public virtual Int32? Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                this._Position = value;
            }
        }
        #endregion
        #region Order
        public abstract class order : PX.Data.BQL.BqlInt.Field<order> { }

        protected Int32? _Order;
        
        [PXInt]
        public virtual Int32? Order
        {
            get
            {
                return this._Order;
            }
            set
            {
                this._Order = value;
            }
        }
        #endregion
        #region Offset
        public abstract class offset : PX.Data.BQL.BqlInt.Field<offset> { }

        protected Int32? _Offset;

        [PXInt]
        public virtual Int32? Offset
        {
            get
            {
                return this._Offset;
            }
            set
            {
                this._Offset = value;
            }
        }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        protected String _Type;

        [PXDBString(2, IsFixed = true)]
        [WizardTaskTypes]
        [PXDefault(WizardTaskTypesAttribute._ARTICLE)]
        [PXUIField(DisplayName = "Type")]
        public virtual String Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        protected String _Status;
        [PXDBString(2, IsFixed = true)]
        [WizardTaskStatuses]
        [PXDefault(WizardTaskStatusesAttribute._PENDING)]
        [PXUIField(DisplayName = "Status")]
        public virtual String Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region IsOptional
        public abstract class isOptional : PX.Data.BQL.BqlBool.Field<isOptional> { }

        protected bool? _IsOptional;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Optional", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? IsOptional
        {
            get
            {
                return this._IsOptional;
            }
            set
            {
                this._IsOptional = value;
            }
        }
        #endregion
       
        #region Details
        public abstract class details : PX.Data.BQL.BqlString.Field<details> { }

        protected string _Details;
        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = "Details")]
        public virtual string Details
        {
            get
            {
                return this._Details;
            }
            set
            {
                this._Details = value;
            }
        }
        #endregion
        #region ScreenID
        public abstract class screenID : PX.Data.BQL.BqlString.Field<screenID> { }

        protected String _ScreenID;
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName="Screen Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<SiteMap.screenID, Where<SiteMap.screenID, IsNotNull>>), 
                new []{typeof(SiteMap.nodeID), typeof(SiteMap.screenID) },
				DescriptionField = typeof(SiteMap.title))]
        public virtual String ScreenID
        {
            get
            {
                return this._ScreenID;
            }
            set
            {
                this._ScreenID = value;
            }
        }
        #endregion
        #region ImportScenraioID
        public abstract class importScenarioID : PX.Data.BQL.BqlGuid.Field<importScenarioID> { }

        protected Guid? _ImportScenarioID;
        [PXDBGuid]
        [PXUIField(DisplayName = "Import Scenario")]
        [PXSelector(typeof(Search<SYMapping.mappingID, Where<SYMapping.screenID, Equal<Current<WZTask.screenID>>, 
            And<SYMapping.mappingType, Equal<SYMapping.mappingType.typeImport>,
            And<SYMapping.isActive, Equal<True>>>>>), SubstituteKey = typeof(SYMapping.name))]
        public Guid? ImportScenarioID
        {
            get
            {
                return this._ImportScenarioID;
            }
            set
            {
                this._ImportScenarioID = value;
            }
        }
        #endregion
        #region AssignedTo
        public abstract class assignedTo : PX.Data.BQL.BqlInt.Field<assignedTo> { }

        protected int? _AssignedTo;

        [PXDBInt]
        [PXUIField(DisplayName = "Assigned To")]
        [PXSelector(typeof(Search2<Contact.contactID,
                                LeftJoin<EPEmployee, On<EPEmployee.defContactID, Equal<Contact.contactID>>,
                                LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
                                Where<Users.isHidden, Equal<False>, 
                                And<Users.isApproved, Equal<True>,
                                And<Users.guest, NotEqual<True>>>>>),
                                new Type[] {
                                    typeof(Users.username),
                                    typeof(Contact.displayName),
                                    typeof(Users.fullName),
                                    typeof(Users.state),
                                    typeof(EPEmployee.acctCD),
                                    typeof(EPEmployee.acctName)
                                }
                                ,DescriptionField = typeof(Contact.displayName))]
        public virtual int? AssignedTo
        {
            get
            {
                return this._AssignedTo;
            }
            set
            {
                this._AssignedTo = value;
            }
        }
        #endregion
        #region AssignedDate
        public abstract class assignedDate : PX.Data.BQL.BqlDateTime.Field<assignedDate> { }

        protected DateTime? _AssignedDate;
        
        [PXDBDateAndTime]
        public virtual DateTime? AssignedDate
        {
            get
            {
                return this._AssignedDate;
            }
            set
            {
                this._AssignedDate = value;
            }
        }
        #endregion
        #region StartedDate
        public abstract class startedDate : PX.Data.BQL.BqlDateTime.Field<startedDate> { }

        protected DateTime? _StartedDate;

        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Started", Enabled = false)]
        public virtual DateTime? StartedDate
        {
            get
            {
                return this._StartedDate;
            }
            set
            {
                this._StartedDate = value;
            }
        }
        #endregion
        #region CompletedBy
        public abstract class completedBy : PX.Data.BQL.BqlInt.Field<completedBy> { }

        protected int? _CompletedBy;

        [PXDBInt]
        [PXUIField(DisplayName = "Completed By", Enabled = false)]
		[PXSelector(typeof(Search2<Contact.contactID,
								LeftJoin<EPEmployee, On<EPEmployee.defContactID, Equal<Contact.contactID>>,
								LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
								Where<Users.isHidden, Equal<False>,
								And<Users.isApproved, Equal<True>,
								And<Users.guest, NotEqual<True>>>>>),
								new Type[] {
									typeof(Users.username),
									typeof(Contact.displayName),
									typeof(Users.fullName),
									typeof(Users.state),
									typeof(EPEmployee.acctCD),
									typeof(EPEmployee.acctName)
								}
								, DescriptionField = typeof(Contact.displayName))]
		public virtual int? CompletedBy
        {
            get
            {
                return this._CompletedBy;
            }
            set
            {
                this._CompletedBy = value;
            }
        }
        #endregion
        #region CompletedDate
        public abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }

        protected DateTime? _CompletedDate;

        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual DateTime? CompletedDate
        {
            get
            {
                return this._CompletedDate;
            }
            set
            {
                this._CompletedDate = value;
            }
        }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote()]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
    }
}
