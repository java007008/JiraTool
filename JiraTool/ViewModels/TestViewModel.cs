using JiraTool.ViewModels.Base;
using System.Collections.ObjectModel;

namespace JiraTool.ViewModels
{
    /// <summary>
    /// 测试视图模型
    /// </summary>
    public class TestViewModel : ViewModelBase
    {
        private ObservableCollection<TestItem> _testItems;

        /// <summary>
        /// 测试数据项
        /// </summary>
        public ObservableCollection<TestItem> TestItems
        {
            get => _testItems;
            set => SetProperty(ref _testItems, value);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TestViewModel()
        {
            // 创建一些测试数据
            TestItems = new ObservableCollection<TestItem>();
            for (int i = 1; i <= 10; i++)
            {
                TestItems.Add(new TestItem
                {
                    Id = i,
                    Name = $"测试项 {i}",
                    Description = $"这是测试项 {i} 的描述"
                });
            }
        }
    }

    /// <summary>
    /// 测试数据项
    /// </summary>
    public class TestItem
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
