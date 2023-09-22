using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Shared
{
    public class CustomFilePickerTypes
    {

        public static FilePickerFileType types = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            {
                DevicePlatform.Android, new[]
                {
                    "image/jpeg", "image/png", "application/pdf", "image/gif", "audio/mpeg", "video/mp4"
                }

            },
            {
                DevicePlatform.WinUI, new[]
                {
                    "jpeg", "jpg", "png", "pdf", "gif", "mp3", "mp4"
                }
            }
        });
    }
}
