using System;
using System.Collections;
using PX.Common;
using PX.Data;

namespace AnNhienCafe
{
  public class ZaloUserMaint : PXGraph<ZaloUserMaint>
  {
        public PXSave<ZaloUser> Save;
        public PXCancel<ZaloUser> Cancel;

        public PXSelect<ZaloUser> ZaloUsers;

        public PXAction<ZaloUser> Insert;

        [PXUIField(DisplayName = "Insert New User", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        protected virtual IEnumerable insert(PXAdapter adapter)
        {
            var newUser = ZaloUsers.Insert(); // tạo dòng mới

            // Gán các giá trị mặc định nếu cần
            newUser.IsActive = true;
            newUser.CreatedDate = PXTimeZoneInfo.Now;

            return adapter.Get();
        }

    }
}