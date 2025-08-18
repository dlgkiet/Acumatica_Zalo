using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnNhienCafe
{
    [PXCacheName("Entity Item")]
    public class EntityItem : PXBqlTable, IBqlTable
    {
        #region ID
        [PXString(512, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Key")]
        public virtual string Key { get; set; }
        public abstract class key : PX.Data.BQL.BqlString.Field<key> { }
        #endregion

        #region Name
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Name")]
        public virtual string Name { get; set; }
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
        #endregion

        #region Path
        [PXString(512, IsUnicode = true)]
        [PXUIField(DisplayName = "Path")]
        public virtual string Path { get; set; }
        public abstract class path : PX.Data.BQL.BqlString.Field<path> { }
        #endregion

        #region Icon
        [PXString(128)]
        [PXUIField(DisplayName = "Icon")]
        public virtual string Icon { get; set; }
        public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }
        #endregion

        #region ParentKey
        [PXString(512, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent Key")]
        public virtual string ParentKey { get; set; }
        public abstract class parentKey : PX.Data.BQL.BqlString.Field<parentKey> { }
        #endregion
        // Dùng để render tree; không map DB

        // Acuminator disable once PX1032 MethodInvocationInDac [Justification]
        public List<EntityItem> Children { get; set; } = new List<EntityItem>();
    }
}
