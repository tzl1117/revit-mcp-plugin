using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;

namespace revit_mcp_plugin
{
    public class CreateWallEventHandler : IExternalEventHandler
    {
        // 创建墙的参数
        private double _startX;
        private double _startY;
        private double _endX;
        private double _endY;
        private double _height;
        private double _thickness;

        // 创建的墙体信息
        private Wall _createdWall;
        public WallInfo CreatedWallInfo { get; private set; }

        // 标记操作是否完成
        public bool TaskCompleted { get; private set; }

        // 事件等待对象，用于同步
        private readonly System.Threading.ManualResetEvent _resetEvent = new System.Threading.ManualResetEvent(false);

        // 设置创建墙的参数
        public void SetWallParameters(double startX, double startY, double endX, double endY, double height, double thickness)
        {
            _startX = startX;
            _startY = startY;
            _endX = endX;
            _endY = endY;
            _height = height;
            _thickness = thickness;

            TaskCompleted = false;
            _resetEvent.Reset();
        }

        // 等待墙创建完成
        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                Document doc = app.ActiveUIDocument.Document;

                using (Transaction trans = new Transaction(doc, "创建墙体"))
                {
                    trans.Start();

                    // 创建墙的起点和终点
                    XYZ startPoint = new XYZ(_startX, _startY, 0);
                    XYZ endPoint = new XYZ(_endX, _endY, 0);

                    // 创建墙的曲线
                    Line curve = Line.CreateBound(startPoint, endPoint);

                    // 获取当前文档中的墙类型
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    collector.OfClass(typeof(WallType));
                    WallType wallType = collector.FirstElement() as WallType;

                    // 创建墙
                    _createdWall = Wall.Create(
                        doc,
                        curve,
                        wallType.Id,
                        doc.ActiveView.GenLevel.Id,
                        _height,
                        0.0,  // 墙基点偏移
                        false,  // 不翻转
                        false); // 不是结构墙

                    trans.Commit();

                    // 获取墙的详细信息
                    CreatedWallInfo = new WallInfo
                    {
                        ElementId = _createdWall.Id.IntegerValue,
                        StartPoint = new Point { X = startPoint.X, Y = startPoint.Y, Z = 0 },
                        EndPoint = new Point { X = endPoint.X, Y = endPoint.Y, Z = 0 },
                        Height = _height,
                        Thickness = _thickness,
                    };
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"创建墙体时出错: {ex.Message}");
                CreatedWallInfo = new WallInfo { ErrorMessage = ex.Message };
            }
            finally
            {
                TaskCompleted = true;
                _resetEvent.Set(); // 通知等待线程操作已完成
            }
        }

        public string GetName()
        {
            return "创建墙体";
        }
    }

    // 墙体信息结构，用于返回创建的墙的详细信息
    public class WallInfo
    {
        [JsonProperty("elementId")]
        public int ElementId { get; set; }

        [JsonProperty("startPoint")]
        public Point StartPoint { get; set; } = new Point();

        [JsonProperty("endPoint")]
        public Point EndPoint { get; set; } = new Point();

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("thickness")]
        public double Thickness { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class Point
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
}
