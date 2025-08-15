using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnNhienCafe
{
    [Serializable]
    [PXCacheName("Entity Item")]
    public class EntityItem : PXBqlTable, IBqlTable
    {
        #region Key
        [PXString]
        [PXUIField(DisplayName = "Key")]
        public virtual string Key { get; set; }
        public abstract class key : PX.Data.BQL.BqlString.Field<key> { }
        #endregion

        #region Name
        [PXString]
        [PXUIField(DisplayName = "Name")]
        public virtual string Name { get; set; }
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
        #endregion

        #region Path
        [PXString]
        [PXUIField(DisplayName = "Path")]
        public virtual string Path { get; set; }
        public abstract class path : PX.Data.BQL.BqlString.Field<path> { }
        #endregion

        #region Icon
        [PXString]
        [PXUIField(DisplayName = "Icon")]
        public virtual string Icon { get; set; }
        public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }
        #endregion
    }
}
