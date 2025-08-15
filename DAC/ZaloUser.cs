using PX.Data;
using System;

namespace AnNhienCafe
{
    [PXCacheName("Zalo User")]
    public class ZaloUser : PXBqlTable, IBqlTable
    {
        #region UserID
        [PXDBIdentity(IsKey = true)]
        public virtual int? UserID { get; set; }
        public abstract class userID : PX.Data.BQL.BqlInt.Field<userID> { }
        #endregion

        #region ZaloUserID
        [PXDBString(50, IsUnicode = false)]
        [PXUIField(DisplayName = "Zalo User ID")]
        [PXDefault]
        public virtual string ZaloUserID { get; set; }
        public abstract class zaloUserID : PX.Data.BQL.BqlString.Field<zaloUserID> { }
        #endregion

        #region Name
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Name")]
        public virtual string Name { get; set; }
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
        #endregion

        #region Role
        [PXDBString(3)]
        [PXUIField(DisplayName = "Role")]
        [PXStringList(
            new string[] { RoleList.Employee, RoleList.Owner },
            new string[] { "Employee", "Owner" }
        )]
        public virtual string Role { get; set; }
        public abstract class role : PX.Data.BQL.BqlString.Field<role> { }
        #endregion

        #region CreatedDate
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Created Date", Enabled = false)]
        public virtual DateTime? CreatedDate { get; set; }
        public abstract class createdDate : PX.Data.BQL.BqlDateTime.Field<createdDate> { }
        #endregion

        #region IsActive
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }

    public class RoleList
    {
        public const string Employee = "EMP";
        public const string Owner = "OWN";

        public class employee : PX.Data.BQL.BqlString.Constant<employee>
        {
            public employee() : base(Employee) { }
        }

        public class owner : PX.Data.BQL.BqlString.Constant<owner>
        {
            public owner() : base(Owner) { }
        }
    }

}
