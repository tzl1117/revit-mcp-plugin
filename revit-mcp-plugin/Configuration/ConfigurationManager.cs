using Newtonsoft.Json;
using revit_mcp_plugin.API.Interfaces;
using revit_mcp_plugin.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace revit_mcp_plugin.Configuration
{
    public class ConfigurationManager
    {
        private readonly ILogger _logger;
        private readonly string _configPath;

        public FrameworkConfig Config { get; private set; }

        public ConfigurationManager(ILogger logger)
        {
            _logger = logger;

            // 配置文件路径
            _configPath = PathManager.GetCommandRegistryFilePath();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    Config = JsonConvert.DeserializeObject<FrameworkConfig>(json);
                    _logger.Info("已加载配置文件: {0}", _configPath);
                }
                else
                {
                    // 创建默认配置
                    Config = CreateDefaultConfig();
                    SaveConfiguration();
                    _logger.Info("已创建默认配置文件: {0}", _configPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("加载配置文件失败: {0}", ex.Message);
                Config = CreateDefaultConfig();
            }

            // 记录加载时间
            _lastConfigLoadTime = DateTime.Now;
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void RefreshConfiguration()
        {
            LoadConfiguration();
            _logger.Info("配置已重新加载");
        }

        public bool HasConfigChanged()
        {
            if (!File.Exists(_configPath))
                return false;

            DateTime lastWrite = File.GetLastWriteTime(_configPath);
            return lastWrite > _lastConfigLoadTime;
        }

        private DateTime _lastConfigLoadTime;

        private FrameworkConfig CreateDefaultConfig()
        {
            var config = new FrameworkConfig
            {
                Settings = new ServiceSettings
                {
                    LogLevel = "Info",
                    Port = 8080,
                },
                Commands = new List<CommandConfig>
                {
                    // 添加默认内置命令
                    new CommandConfig
                    {
                        CommandName = "delete_element",
                        AssemblyPath = "BuiltIn",
                        Enabled = true,
                        SupportedRevitVersions = new string[] { "2019", "2020", "2021", "2022", "2023" },
                        Developer = new DeveloperInfo
                        {
                            Organization = "revit-mcp",
                            Email = ""        
                        },
                        Description = "删除Revit中的元素"
                    },
                    new CommandConfig
                    {
                        CommandName = "get_current_view_info",
                        AssemblyPath = "BuiltIn",
                        Enabled = true,
                        SupportedRevitVersions = new string[] { "2019", "2020", "2021", "2022", "2023" },
                        Developer = new DeveloperInfo
                        {
                            Organization = "revit-mcp",
                            Email = ""
                        },
                        Description = "获取当前视图信息"
                    },

                }
            };

            return config;
        }

        public void SaveConfiguration()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                _logger.Info("配置已保存到: {0}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.Error("保存配置失败: {0}", ex.Message);
                throw new ConfigurationException($"保存配置文件失败: {ex.Message}", ex);
            }
        }
    }
}
