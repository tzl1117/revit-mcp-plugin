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
    /// CommandSetSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CommandSetSettingsPage : Page
    {
        private ObservableCollection<CommandSet> commandSets;
        private ObservableCollection<CommandFeature> currentFeatures;

        public CommandSetSettingsPage()
        {
            InitializeComponent();

            // 初始化数据集合
            commandSets = new ObservableCollection<CommandSet>();
            currentFeatures = new ObservableCollection<CommandFeature>();

            // 设置数据绑定
            CommandSetListBox.ItemsSource = commandSets;
            FeaturesListView.ItemsSource = currentFeatures;

            // 加载命令集
            LoadCommandSets();

            // 初始状态
            NoSelectionTextBlock.Visibility = Visibility.Visible;
        }

        private void LoadCommandSets()
        {
            try
            {
                commandSets.Clear();

                // 在实际应用中，这里应该从特定路径加载真实的命令集
                // 例如：string[] commandSetPaths = Directory.GetDirectories("path/to/command/sets");

                // 模拟发现的命令集
                commandSets.Add(new CommandSet
                {
                    Id = "file-ops",
                    Name = "文件操作集",
                    Description = "包含文件和文件夹的基本操作功能",
                    Features = new List<CommandFeature>
                    {
                        new CommandFeature { Id = "file-copy", Name = "文件复制", Description = "增强的文件复制功能，支持断点续传", IsEnabled = true },
                        new CommandFeature { Id = "file-move", Name = "文件移动", Description = "文件移动和重命名功能", IsEnabled = true },
                        new CommandFeature { Id = "file-search", Name = "文件搜索", Description = "高级文件搜索和索引", IsEnabled = false }
                    }
                });

                commandSets.Add(new CommandSet
                {
                    Id = "media-tools",
                    Name = "媒体工具集",
                    Description = "音频和视频处理工具",
                    Features = new List<CommandFeature>
                    {
                        new CommandFeature { Id = "audio-convert", Name = "音频转换", Description = "支持多种音频格式间的转换", IsEnabled = false },
                        new CommandFeature { Id = "video-trim", Name = "视频裁剪", Description = "快速剪辑和拼接视频片段", IsEnabled = false },
                        new CommandFeature { Id = "image-resize", Name = "图片缩放", Description = "批量处理图片尺寸和格式", IsEnabled = true }
                    }
                });

                commandSets.Add(new CommandSet
                {
                    Id = "dev-tools",
                    Name = "开发者工具集",
                    Description = "面向开发人员的高级功能",
                    Features = new List<CommandFeature>
                    {
                        new CommandFeature { Id = "code-format", Name = "代码格式化", Description = "支持多种编程语言的代码格式化", IsEnabled = false },
                        new CommandFeature { Id = "json-tools", Name = "JSON工具", Description = "JSON验证、格式化和转换工具", IsEnabled = true },
                        new CommandFeature { Id = "regex-test", Name = "正则测试", Description = "正则表达式测试和生成工具", IsEnabled = false },
                        new CommandFeature { Id = "md5-hash", Name = "MD5计算", Description = "文件和文本的MD5哈希计算", IsEnabled = true }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载命令集时出错: {ex.Message}", "错误",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommandSetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentFeatures.Clear();

            var selectedCommandSet = CommandSetListBox.SelectedItem as CommandSet;
            if (selectedCommandSet != null)
            {
                NoSelectionTextBlock.Visibility = Visibility.Collapsed;
                FeaturesHeaderTextBlock.Text = $"{selectedCommandSet.Name} - 功能列表";

                // 加载选中命令集的功能
                foreach (var feature in selectedCommandSet.Features)
                {
                    currentFeatures.Add(feature);
                }
            }
            else
            {
                NoSelectionTextBlock.Visibility = Visibility.Visible;
                FeaturesHeaderTextBlock.Text = "功能列表";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存当前选择状态
            var selectedIndex = CommandSetListBox.SelectedIndex;

            // 重新加载命令集
            LoadCommandSets();

            // 恢复选择
            if (selectedIndex >= 0 && selectedIndex < commandSets.Count)
            {
                CommandSetListBox.SelectedIndex = selectedIndex;
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var feature in currentFeatures)
            {
                feature.IsEnabled = true;
            }
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var feature in currentFeatures)
            {
                feature.IsEnabled = false;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // 在实际应用中，这里应该保存设置并将启用的功能注册到软件中

            // 构建启用功能的报告
            string enabledFeatures = "";
            int enabledCount = 0;

            foreach (var cmdSet in commandSets)
            {
                foreach (var feature in cmdSet.Features)
                {
                    if (feature.IsEnabled)
                    {
                        enabledFeatures += $"• {cmdSet.Name}: {feature.Name}\n";
                        enabledCount++;
                    }
                }
            }

            MessageBox.Show($"已成功应用命令集设置!\n\n已启用 {enabledCount} 个功能:\n{enabledFeatures}",
                          "设置已保存", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // 数据模型
    public class CommandSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CommandFeature> Features { get; set; } = new List<CommandFeature>();
    }

    public class CommandFeature
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}
