using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace revit_mcp_plugin.UI
{
    /// <summary>
    /// CommandSetSettingsControl.xaml 的交互逻辑
    /// </summary>
    public partial class CommandSetSettingsControl : UserControl
    {
        private ObservableCollection<CommandSetItem> commandSets;

        public CommandSetSettingsControl()
        {
            InitializeComponent();

            // 初始化命令集集合
            commandSets = new ObservableCollection<CommandSetItem>();
            LoadCommandSets();

            // 绑定数据源
            CommandSetsListView.ItemsSource = commandSets;
        }

        private void LoadCommandSets()
        {
            // 这里模拟从系统加载可用的命令集
            commandSets.Clear();
            commandSets.Add(new CommandSetItem
            {
                IsEnabled = true,
                Name = "基础命令集",
                Description = "包含文件操作、编辑和查看等基本命令"
            });
            commandSets.Add(new CommandSetItem
            {
                IsEnabled = false,
                Name = "媒体命令集",
                Description = "包含音频和视频处理相关命令"
            });
            commandSets.Add(new CommandSetItem
            {
                IsEnabled = true,
                Name = "文档工具集",
                Description = "包含文档格式转换和批处理功能"
            });
            commandSets.Add(new CommandSetItem
            {
                IsEnabled = false,
                Name = "开发者工具集",
                Description = "面向开发人员的高级命令和API功能"
            });
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCommandSets();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // 在这里实现保存设置并注册命令集的逻辑

            // 遍历所有命令集并注册启用的部分
            foreach (var cmdSet in commandSets)
            {
                if (cmdSet.IsEnabled)
                {
                    // 注册此命令集到应用程序
                    // RegisterCommandSet(cmdSet);
                }
                else
                {
                    // 取消注册此命令集
                    // UnregisterCommandSet(cmdSet);
                }
            }

            MessageBox.Show("命令集设置已应用，将在下次启动时生效。", "设置已保存",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // 命令集数据模型
    public class CommandSetItem
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
