using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PastingMaui.Platforms.Android.CustomPerms
{
    internal class BluetoothPerms : BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string permission, bool isRuntime)>
        {
            ("android.permission.BLUETOOTH", true),
            ("android.permission.BLUETOOTH_ADMIN", true)
        }.ToArray();
    }
}
