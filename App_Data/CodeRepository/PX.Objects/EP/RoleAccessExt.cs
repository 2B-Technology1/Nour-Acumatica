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
using System.Threading.Tasks;
using PX.Data;
using PX.EP;
using PX.SM;

namespace PX.Objects.EP
{
    public class RoleAccessExt : PXGraphExtension<RoleAccess>
    {
        #region Event Handlers
        [PXDBString(256, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXDefault(typeof(Users.username))]
        [PXUIField(DisplayName = "Username")]
        [PXParent(typeof(Select<Users, Where<Users.username, Equal<Current<UsersInRoles.username>>>>))]
        [PXSelector(typeof(Search2<Users.username,
            LeftJoin<EPLoginType, 
                On<EPLoginType.loginTypeID, Equal<Users.loginTypeID>>,
            LeftJoin<EPLoginTypeAllowsRole, 
                On<EPLoginTypeAllowsRole.loginTypeID, Equal<EPLoginType.loginTypeID>>>>,
                                Where2<Where2<Where<Users.isHidden, Equal<False>>,
                                And<Where2<Where<Users.source, Equal<PXUsersSourceListAttribute.application>, Or<Users.overrideADRoles, Equal<True>>>,
                                And<Where<Current<Roles.guest>, Equal<True>, Or<Users.guest, NotEqual<True>>>>>>>,
                                And<Where<EPLoginTypeAllowsRole.rolename, Equal<Current<UsersInRoles.rolename>>, 
                                Or<Users.loginTypeID, IsNull>>>>>),
                                DescriptionField = typeof(Users.comment), DirtyRead = true)]
        protected virtual void UsersInRoles_Username_CacheAttached(PXCache sender) { }
        #endregion
    }
}
