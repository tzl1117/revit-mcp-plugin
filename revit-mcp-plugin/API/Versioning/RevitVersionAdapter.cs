using Autodesk.Revit.ApplicationServices;
using System;

namespace revit_mcp_plugin.API.Versioning
{
    /// <summary>
    /// Revit 版本适配器，用于处理不同版本的兼容性问题
    /// </summary>
    public class RevitVersionAdapter
    {
        private readonly Application _app;

        public RevitVersionAdapter(Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        /// <summary>
        /// 获取当前 Revit 版本号
        /// </summary>
        public string GetRevitVersion()
        {
            // 获取主版本号，例如 "2022"
            return _app.VersionNumber;
        }

        /// <summary>
        /// 检查当前 Revit 版本是否支持指定的命令
        /// </summary>
        /// <param name="supportedVersions">命令支持的版本列表</param>
        /// <returns>当前版本是否被支持</returns>
        public bool IsVersionSupported(System.Collections.Generic.IEnumerable<string> supportedVersions)
        {
            if (supportedVersions == null || !supportedVersions.GetEnumerator().MoveNext())
                return true; // 如果未指定支持版本，默认支持所有版本

            string currentVersion = GetRevitVersion();

            foreach (string version in supportedVersions)
            {
                if (currentVersion == version)
                    return true;
            }

            return false;
        }
    }
}
