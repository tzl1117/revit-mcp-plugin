using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.API.Versioning
{
    /// <summary>
    /// 版本比较帮助类
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// 解析版本号字符串为 Version 对象
        /// </summary>
        public static Version ParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return null;

            if (Version.TryParse(versionString, out Version version))
                return version;

            // 处理只有年份的情况，如 "2022"
            if (int.TryParse(versionString, out int year))
                return new Version(year, 0, 0, 0);

            return null;
        }

        /// <summary>
        /// 比较两个版本号
        /// </summary>
        public static int CompareVersions(string version1, string version2)
        {
            var v1 = ParseVersion(version1);
            var v2 = ParseVersion(version2);

            if (v1 == null && v2 == null)
                return 0;

            if (v1 == null)
                return -1;

            if (v2 == null)
                return 1;

            return v1.CompareTo(v2);
        }
    }
}
