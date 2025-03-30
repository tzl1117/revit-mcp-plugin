using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Commands.Versioning
{
    /// <summary>
    /// Revit版本适配器，处理不同版本Revit API的动态加载
    /// </summary>
    public class RevitVersionAdapter
    {
        private static RevitVersionAdapter _instance;
        private readonly string _revitVersion;
        private readonly string _revitAPIPath;
        private readonly string _revitAPIUIPath;
        private Assembly _revitAPIAssembly;
        private Assembly _revitAPIUIAssembly;

        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// 获取RevitVersionAdapter单例
        /// </summary>
        public static RevitVersionAdapter Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("RevitVersionAdapter not initialized. Call Initialize first.");
                return _instance;
            }
        }

        /// <summary>
        /// 当前Revit版本
        /// </summary>
        public string RevitVersion => _revitVersion;

        /// <summary>
        /// 初始化RevitVersionAdapter
        /// </summary>
        /// <param name="uiApplication">Revit UI应用程序对象</param>
        /// <returns>初始化后的RevitVersionAdapter实例</returns>
        public static RevitVersionAdapter Initialize(object uiApplication)
        {
            if (_instance != null)
                return _instance;

            // 通过反射获取Revit版本
            var appType = uiApplication.GetType();
            var appProperty = appType.GetProperty("Application");
            var app = appProperty.GetValue(uiApplication);
            var versionNumberProperty = app.GetType().GetProperty("VersionNumber");
            var versionNumber = versionNumberProperty.GetValue(app).ToString();

            // 使用已加载的RevitAPI.dll路径
            var revitAPIAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "RevitAPI");

            if (revitAPIAssembly == null)
                throw new InvalidOperationException("RevitAPI assembly not found in current AppDomain.");

            string revitAPIPath = revitAPIAssembly.Location;
            string revitFolder = Path.GetDirectoryName(revitAPIPath);
            string revitAPIUIPath = Path.Combine(revitFolder, "RevitAPIUI.dll");

            _instance = new RevitVersionAdapter(versionNumber, revitAPIPath, revitAPIUIPath);
            return _instance;
        }

        private RevitVersionAdapter(string revitVersion, string revitAPIPath, string revitAPIUIPath)
        {
            _revitVersion = revitVersion;
            _revitAPIPath = revitAPIPath;
            _revitAPIUIPath = revitAPIUIPath;

            //// 记录版本信息
            //Logger.GetLogger(typeof(RevitVersionAdapter))
            //    .Info($"Initialized RevitVersionAdapter for Revit {_revitVersion}");
            //Logger.GetLogger(typeof(RevitVersionAdapter))
            //    .Debug($"RevitAPI path: {_revitAPIPath}");
            //Logger.GetLogger(typeof(RevitVersionAdapter))
            //    .Debug($"RevitAPIUI path: {_revitAPIUIPath}");

            // 加载RevitAPI程序集
            _revitAPIAssembly = LoadAssembly(_revitAPIPath);
            _revitAPIUIAssembly = LoadAssembly(_revitAPIUIPath);
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="assemblyPath">程序集路径</param>
        /// <returns>加载的程序集</returns>
        private Assembly LoadAssembly(string assemblyPath)
        {
            if (_loadedAssemblies.TryGetValue(assemblyPath, out var assembly))
                return assembly;

            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
                _loadedAssemblies[assemblyPath] = assembly;
                return assembly;
            }
            catch (Exception ex)
            {
                //Logger.GetLogger(typeof(RevitVersionAdapter))
                //    .Error($"Failed to load assembly {assemblyPath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取RevitAPI中的类型
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>找到的类型</returns>
        public Type GetRevitAPIType(string typeName)
        {
            return _revitAPIAssembly.GetType(typeName, true);
        }

        /// <summary>
        /// 获取RevitAPIUI中的类型
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>找到的类型</returns>
        public Type GetRevitAPIUIType(string typeName)
        {
            return _revitAPIUIAssembly.GetType(typeName, true);
        }

        /// <summary>
        /// 检查插件是否支持当前Revit版本
        /// </summary>
        /// <param name="supportedVersions">插件支持的版本</param>
        /// <returns>是否支持</returns>
        public bool IsPluginCompatible(string[] supportedVersions)
        {
            if (supportedVersions == null || supportedVersions.Length == 0)
                return false;

            // 支持精确匹配（例如"2022"）和年份匹配（例如"R2022"）
            // 也可以扩展添加更灵活的规则，如版本范围等
            return supportedVersions.Any(v =>
                v == _revitVersion ||
                v == $"R{_revitVersion}" ||
                v == $"Revit{_revitVersion}");
        }
    }
}
