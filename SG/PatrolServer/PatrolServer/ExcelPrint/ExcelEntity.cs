using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Printing;
using System.Diagnostics;
using PatrolServer.Services.Patrol.Response.Entity;
using PatrolServer.Services.Patrol.Response;
using LogWriter;
using Model;
using Model.EntityManager;
using System.Runtime.InteropServices;

namespace PatrolServer.ExcelPrint
{
    /// <summary>
    /// Excel对象基类
    /// </summary>
    public class ExcelEntity
    {
        private object _locker = new object();//用于锁对象控制excel
        public ExcelEntity(){
        }

        public void GenerateExcel(string patrolno)
        {

            //DataTable dt = new DataTable();

            //dt.Columns.Add("Name", typeof(string));

            //dt.Columns.Add("Age", typeof(string));

            //DataRow dr = dt.NewRow();

            //dr["Name"] = "spring";

            //dr["Age"] = "20";

            //dt.Rows.Add(dr);

            //dt.AcceptChanges();

            //ExportExcel(dt);

            ResShowReport data = this.GetData(patrolno);
            string strdir = DateTime.Now.Ticks.ToString();
            this.ExportExcel(data, CommonInfo.ExcelUrl + "\\" + strdir, CommonInfo.ExcelUrl + "\\" + strdir + "\\" + strdir);

        }

        /// <summary>
        /// 打印表格数据 Demo例子可以参考修改
        /// </summary>
        /// <param name="dt"></param>
        public void ExportExcel(System.Data.DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0) return;

                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();



                if (xlApp == null)
                {

                    return;

                }

                System.Globalization.CultureInfo CurrentCI = System.Threading.Thread.CurrentThread.CurrentCulture;

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;

                Microsoft.Office.Interop.Excel.Workbook workbook = xlApp.Workbooks.Open(CommonInfo.ExcelTemplateFile);//workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);

                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];

                Microsoft.Office.Interop.Excel.Range range;

                long totalCount = dt.Rows.Count;

                long rowRead = 0;

                float percent = 0;

                for (int i = 0; i < dt.Columns.Count; i++)
                {

                    worksheet.Cells[1, i + 1] = dt.Columns[i].ColumnName;

                    range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, i + 1];

                    range.Interior.ColorIndex = 15;

                    range.Font.Bold = true;

                }

                for (int r = 0; r < dt.Rows.Count; r++)
                {

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {

                        worksheet.Cells[r + 2, i + 1] = dt.Rows[r][i].ToString();

                    }

                    rowRead++;

                    percent = ((float)(100 * rowRead)) / totalCount;

                }
                range = worksheet.get_Range("Title");
                range.FormulaR1C1 = range.Left + "---" + range.Top;
                //添加图片
                worksheet.Shapes.AddPicture(@"C:\Users\Public\Pictures\Sample Pictures\0.jpg", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 0, range.Top, 300, 300);

                //添加文字
                worksheet.Shapes.AddTextEffect(Microsoft.Office.Core.MsoPresetTextEffect.msoTextEffect1, "HelloWorld", "Red", 15, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue, 0, 0);

                worksheet.PageSetup.LeftHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                worksheet.PageSetup.CenterHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                worksheet.PageSetup.RightHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                worksheet.PageSetup.LeftHeader = "&G";
                worksheet.PageSetup.CenterHeader = "&G";
                worksheet.PageSetup.RightHeader = "&G";
                worksheet.PageSetup.CenterFooter = "&\"Arial,加粗\"&16©Hitachi Construction Machinery Co.Ltd.";
                worksheet.PageSetup.LeftMargin = xlApp.Application.InchesToPoints(0.748031496062992);
                worksheet.PageSetup.RightMargin = xlApp.Application.InchesToPoints(0.748031496062992);
                worksheet.PageSetup.TopMargin = xlApp.Application.InchesToPoints(1.22047244094488);
                worksheet.PageSetup.BottomMargin = xlApp.Application.InchesToPoints(0.984251968503937);
                worksheet.PageSetup.HeaderMargin = xlApp.Application.InchesToPoints(0.511811023622047);
                worksheet.PageSetup.FooterMargin = xlApp.Application.InchesToPoints(0.511811023622047);
                worksheet.PageSetup.PrintArea = "";
                worksheet.PageSetup.PrintQuality = 600;
                worksheet.PageSetup.PaperSize = Microsoft.Office.Interop.Excel.XlPaperSize.xlPaperA4;

                xlApp.Visible = true;
            }
            catch (Exception ex)
            {
                MyLog.Loger.Error(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Excel单元格命名对应
        /// </summary>
        public enum RangeNames
        {
            ReportNo,
            Two,
            PublishDate,
            Customer,
            CompanyName,
            CompanyTel,
            CompanyAddress,
            CustomerName,
            CustomerTel,
            CustomerAddress,
            MachineType,
            MachineName,
            MachineNo,
            PinAndVin,
            WorkedTimes,
            WorkAddress,
            WorkMan,
            WorkDate,
            Owner,
            OwnerTel,
            OperatorTypeName,
            OperatorName,
            OperatorTypeTel,
            OperatorTel,
            Decode,
            NamePlate,
            Remarks,
            StartCell,
            EndCell,
            Title,    
            PrintBottom
        }

        /// <summary>
        /// 获取特巡报告书需要的打印数据
        /// </summary>
        /// <param name="patrolno"></param>
        /// <returns></returns>
        public ResShowReport GetData(string patrolno) {
            ResShowReport response = null;
            if (patrolno != null && patrolno != String.Empty)
            {
                response = new ResShowReport();
                DataTable header = PatrolEntity.getPatrolHeader4Report(patrolno);
                DataTable detail = PatrolEntity.getPatrolDetail4Report(patrolno);
                //DataTable facadeList = PatrolEntity.getFacadeList4Report(patrolno);
                List<PatrolDetailInfo> detailList = ResShowReport.getPatrolDetailList(detail);
                response.patrol_header = ResShowReport.getPatrolHeader(header);

                response.facade_list = ResShowReport.getFacadeImageList(detailList);
                response.patrol_detail_list = detailList;
                
            }
            return response;
        }

        #region 设置默认打印机
        [DllImport("winspool.drv")]
        static extern bool SetDefaultPrinter(String Name); //调用win api将指定名称的打印机设置为默认打印机

        //初始化打印机
        static bool SetDefaultPrinterByConfig()
        {
            bool printerExists = false;
            try
            {
                string printerName = CommonInfo.DefaultPrinter;

                PrinterSettings.StringCollection printList = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
                printerName = printerName.ToLower();
                foreach (string item in printList)
                {
                    Console.WriteLine("*.已安装打印机：" + item);
                    if (item.ToLower() == printerName)
                    {
                        printerName = item;//换回区分大小写名称 
                        //调用Win32程序
                        printerExists = SetDefaultPrinter(printerName);
                        Console.WriteLine("1.成功设置打印机");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("x:设置默认打印机初始化失败");
            }
            return printerExists;
        }
        #endregion

        /// <summary>
        /// 导出Excel设置
        /// </summary>
        /// <param name="source"></param>
        public bool ExportExcel(ResShowReport source,string dirName, string excelFileName)
        {
            bool success = false;

            //找到对应打印机才转换
            if (!SetDefaultPrinterByConfig())
            {
                Console.WriteLine("打印机恢复默认设置失败");
                return false;

            }
            else
            {
                Console.WriteLine("打印机恢复默认设置成功");
            }   
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Microsoft.Office.Interop.Excel.Workbooks workbooks = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
            //单元格对象
            Microsoft.Office.Interop.Excel.Range range = null;
            //用于存储excel进程id
            int processId = 0;
            try
            {
                if (source == null || source.patrol_header == null || source.patrol_detail_list == null) return success;
                System.Diagnostics.Process[] psf = null;
                System.Diagnostics.Process[] psa = null;
                //创建excel之前的列表
                lock (this._locker)
                {
                    psf = System.Diagnostics.Process.GetProcessesByName("excel");
                    xlApp = new Microsoft.Office.Interop.Excel.Application();
                    //创建excel之后的列表
                    psa = System.Diagnostics.Process.GetProcessesByName("excel");                    
                }
                //取得进程id
                for (int i = 0; i < psa.Length; i++)
                {
                    //从旧集合中找出不同的
                    bool isexist = false;
                    for (int j = 0; j < psf.Length; j++)
                    {
                        if (psf[j].Id==psa[i].Id)
                        {
                            isexist = true;
                            break;
                        }
                    }
                    //新创建的excel进程
                    if (!isexist)
                    {
                        processId = psa[i].Id;
                        break;
                    }
                }
                if (xlApp == null)
                {
                    Console.WriteLine("Excel程序打开失败！");
                    return success;

                }

                xlApp.DisplayAlerts = false;
                xlApp.Visible = true;

                System.Globalization.CultureInfo CurrentCI = System.Threading.Thread.CurrentThread.CurrentCulture;

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                workbooks = xlApp.Workbooks;

                //Excel保存路径
                string strdir = String.Empty;
                string strurl = String.Empty;
                string strpdf = String.Empty;
                if (dirName != null && dirName != String.Empty)
                {
                    strurl = CommonInfo.ExcelUrl + "\\" + excelFileName + CommonInfo.ExcelExtend;
                    strpdf = CommonInfo.ExcelUrl + "\\" + excelFileName + CommonInfo.PDFExtend;
                    strdir = CommonInfo.ExcelUrl + "\\" + dirName;
                }
                else
                {
                    //未完成特巡报告书
                    return success;
                }
                //复制模板
                if (System.IO.File.Exists(CommonInfo.ExcelTemplateFile))
                {
                    if (!System.IO.Directory.Exists(strdir))
                    {
                        System.IO.Directory.CreateDirectory(strdir);
                    }
                    //模板文件存在,那么复制模板文件到Excel导出文件夹
                    System.IO.File.Copy(CommonInfo.ExcelTemplateFile, strurl, true);

                }
                workbook = xlApp.Workbooks.Open(strurl);//workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);

                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];

                ////必须在excel的visible为true时遍历进程 不然获取不到窗口标题名字标题,名字默认唯一 
                //System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("excel");
                //string strMainWindowTitle = "Microsoft Excel - " + workbook.Name;
                //foreach (System.Diagnostics.Process item in ps)
                //{
                //    Console.WriteLine("进程Id：" + item.MainWindowTitle);
                //    if (strMainWindowTitle == item.MainWindowTitle)
                //    {
                //        Console.WriteLine("关闭进程：" + item.Id);
                //        processId = item.Id;
                //    }

                //}


                #region 配置信息
                //计算可用宽度
                float OffSetX = 20F;
                float OffSetY = 10F;
                float ImageMargin = 60;//左右图片之间的间隔
                //开始单元格坐标和结束单元格坐标
                range = worksheet.get_Range("StartCell");
                double StartX = range.Left;
                double StartY = range.Top;
                int StartRowIndex = range.Row;
                int StartColumnIndex = range.Column;
                range = worksheet.get_Range("EndCell");
                double EndX = range.Left;
                double EndY = range.Top;
                int EndColumnIndex = range.Column;
                range = worksheet.get_Range("PrintBottom");//下一页的开始行
                double BottomX = range.Left;
                double BottomY = range.Top;
                int EndRowIndex = range.Row;
                //图片开始坐标
                double ImageStartX = OffSetX;
                double ImageStartY = StartY;
                int ImageStartRow = range.Row;
                range = worksheet.get_Range("Title");//下一页的开始行
                double HeadTitleHeight = range.Height;//标题行高度
                double ContainerWidth = Convert.ToDouble((EndX - ImageMargin - OffSetX * 2).ToString("0.00"));//容器的可用总宽度
                double ContainerHeight = Convert.ToDouble((BottomY - range.Top - OffSetY * 2 - HeadTitleHeight).ToString("0.00"));//容器的可用总高度
                //计算图片的长和宽以及间隙
                float ImageRowsPerPage = 3;//一页打印3行
                float ImageColumnsPerPage = 2;//一行打印2列
                float TitleHeight = 14;//说明文字高度
                float RemarksHeight = 36;//备注文字高度
                float ImageWidth = Convert.ToSingle(((ContainerWidth - ImageMargin) / 2).ToString("0.00"));//图片宽度
                float ImageHeight = Convert.ToSingle((ContainerHeight / ImageRowsPerPage - TitleHeight*2 - RemarksHeight - 15.0F).ToString("0.00"));//图片高度

                //调整图片为正方形,取两者中小的那个作为基准,打印会重新设置图片的宽高所以需要按比例显示
                if (ImageWidth - ImageHeight > 0.001)
                {
                    ImageWidth = ImageHeight;
                }
                else
                {
                    ImageHeight = ImageWidth;
                }
                //重新定位图片坐标 使得图片居中显示
                OffSetX = Convert.ToSingle((EndX - ImageMargin) / 2 - ImageWidth);
                double ImageLeftX = OffSetX;//左区域图片X坐标
                double ImageLeftY = ImageStartY;//左区域图片Y坐标
                double ImageRightX = Convert.ToDouble((EndX - OffSetX - ImageWidth).ToString("0.00"));//右区域图片X坐标
                double ImageRightY = ImageLeftY;//右区域图片Y坐标
                int RowsCount = EndRowIndex - StartRowIndex;//一页区域跨域的行数量
                //动态控制坐标变量
                float px = 0;
                float py = 0;
                List<Microsoft.Office.Interop.Excel.Range> listPages = new List<Microsoft.Office.Interop.Excel.Range>();
                //所有的图片
                List<Microsoft.Office.Interop.Excel.Shape> listShapes = new List<Microsoft.Office.Interop.Excel.Shape>();
                ////所有的说明文本框
                //List<Microsoft.Office.Interop.Excel.Shape> listTitleShapes = new List<Microsoft.Office.Interop.Excel.Shape>();
                ////所有的备注信息
                //List<Microsoft.Office.Interop.Excel.Shape> listRemarkShapes = new List<Microsoft.Office.Interop.Excel.Shape>();
                #endregion

                #region 第一页头部数据
                PatrolHeaderInfo header = source.patrol_header;
                //编号
                range = worksheet.get_Range(RangeNames.ReportNo.ToString());
                range.FormulaR1C1 = header.patrol_no;
                //发行方
                range = worksheet.get_Range(RangeNames.CompanyName.ToString());
                range.FormulaR1C1 = header.company_name;
                //发行日
                range = worksheet.get_Range(RangeNames.PublishDate.ToString());
                range.FormulaR1C1 = header.publish_date;
                //二维码图片位置
                range = worksheet.get_Range(RangeNames.Two.ToString());
                range.FormulaR1C1 = String.Empty;
                //添加图片
                worksheet.Shapes.AddPicture(CommonInfo.TwoFile, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, range.Left, range.Top, 64F, 64F);//Convert.ToSingle(range.Width * 2 + 5), Convert.ToSingle(range.Height * 3 + 5));
                //客户
                range = worksheet.get_Range(RangeNames.Customer.ToString());
                range.FormulaR1C1 = "尊敬的   " + header.contactor_name + "   先生/女士/公司：";
                range.get_Characters(7, header.contactor_name.Length).Font.Size = 9;
                //检查概要
                range = worksheet.get_Range(RangeNames.Remarks.ToString());
                //range.ShrinkToFit = true;//缩小字体
                range.FormulaR1C1 = header.remarks;
                //机型代码
                range = worksheet.get_Range(RangeNames.MachineType.ToString());
                range.FormulaR1C1 = header.machine_type + ":" + header.machine_name;
                //机号
                range = worksheet.get_Range(RangeNames.MachineNo.ToString());
                range.FormulaR1C1 = header.machine_no;
                //当月运转小时(HR)
                range = worksheet.get_Range(RangeNames.WorkedTimes.ToString());
                range.FormulaR1C1 = header.worked_times;
                //施工地点
                range = worksheet.get_Range(RangeNames.WorkAddress.ToString());
                range.FormulaR1C1 = header.province + "" + header.city + "" + header.address;
                //作业人
                range = worksheet.get_Range(RangeNames.WorkMan.ToString());
                range.FormulaR1C1 = header.reporter_name;
                //作业日期
                range = worksheet.get_Range(RangeNames.WorkDate.ToString());
                range.FormulaR1C1 = header.report_date.Substring(0, 4) + "/" + header.report_date.Substring(4, 2) + "/" + header.report_date.Substring(6, 2);
                //机主
                range = worksheet.get_Range(RangeNames.Owner.ToString());
                range.FormulaR1C1 = header.contactor_name;
                //机主电话
                range = worksheet.get_Range(RangeNames.OwnerTel.ToString());
                range.FormulaR1C1 = header.contactor_phone;
                //现场联系人(管理员、顾客、操作手、其他)
                range = worksheet.get_Range(RangeNames.OperatorTypeName.ToString());
                range.FormulaR1C1 = "现场联系人(" + header.operator_type_name + ")";
                //现场联系人名称
                range = worksheet.get_Range(RangeNames.OperatorName.ToString());
                range.FormulaR1C1 = header.operator_name;
                //现场联系人(管理员、顾客、操作手、其他)电话
                range = worksheet.get_Range(RangeNames.OperatorTypeTel.ToString());
                range.FormulaR1C1 = "(" + header.operator_type_name + ")电话";
                //电话内容
                range = worksheet.get_Range(RangeNames.OperatorTel.ToString());
                range.FormulaR1C1 = header.operator_phone;

                #endregion

                #region 外观和铭牌信息
                //初始化
                px = 0;
                py = 0;
                //三张图片的间隔 (20171211修改只显示整机外观一张图片)
                float startPosition = Convert.ToSingle((EndX - ImageWidth) / 3);//30f; //左图起始位置
                float imageMargin = 10F;//Convert.ToSingle((EndX - ImageWidth * 3 - startPosition * 2) / 2);
                //单独控制长宽
                float decodeWidth = Convert.ToSingle(ImageWidth + startPosition);
                float decodeHeight = Convert.ToSingle(ImageHeight + 50F);
                //外观图片单独处理
                List<Microsoft.Office.Interop.Excel.Shape> listDecode = new List<Microsoft.Office.Interop.Excel.Shape>();
                
                //电话内容
                //左边外观、右边铭牌
                foreach (PatrolDetailInfo item in source.facade_list)
                {
                    if (item.location_code == "SP0001")
                    {
                        //外观
                        //左边区域图片
                        range = worksheet.get_Range(RangeNames.Decode.ToString());
                        px = startPosition;
                        py = Convert.ToSingle(range.Top + 2.0F);

                    }
                    else if (item.location_code == "SP0002")
                    {
                        //铭牌
                        //中间区域图片
                        range = worksheet.get_Range(RangeNames.NamePlate.ToString());
                        px = Convert.ToSingle(startPosition + ImageWidth + imageMargin);
                        py = Convert.ToSingle(range.Top + 2.0F);
                    
                    }
                    else
                    {
                        //铭牌
                        //右边区域图片
                        range = worksheet.get_Range(RangeNames.NamePlate.ToString());
                        px = Convert.ToSingle(startPosition + (ImageWidth + imageMargin) * 2 );
                        py = Convert.ToSingle(range.Top + 2.0F);
                    }
                    //添加图片
                    Microsoft.Office.Interop.Excel.Shape shape = worksheet.Shapes.AddPicture(CommonInfo.ImageSaveUrl + "\\" + item.pic_url, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, px, py, decodeWidth, decodeHeight);
                    //图片固定大小和位置 不随单元格变化而变化
                    shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                    //shape.IncrementLeft(0.75F);
                    //shape.ScaleWidth(0.9999F, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);
                    shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                    shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                    shape.Line.ForeColor.TintAndShade = 0;
                    shape.Line.Transparency = 0;
                    //改成一张图片后大小不一样不加入调整控制
                    ////加入到控制集合中用于设置视图变化导致长宽不一致问题
                    //listShapes.Add(shape);
                    listDecode.Add(shape);
                    ////补齐数组
                    //listShapes.Add(shape);
                    //listShapes.Add(shape);
                    #region 整机外观不显示此信息
                    ////点检部位和具体位置                
                    //shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, py + ImageHeight, ImageWidth, TitleHeight);
                    ////图片固定大小和位置 不随单元格变化而变化
                    //shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                    ////文字居中显示
                    //shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignCenter;
                    //shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    //shape.TextFrame2.TextRange.Font.Size = 10;
                    //shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;
                    //if (item.question_level == PatrolEntity.QuestionLevel_Emergency)
                    //{
                    //    //点检状态为《需修理/更换》或者问题程度为紧急字体变红色
                    //    shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = 255;
                    //}
                    //shape.TextFrame2.TextRange.Characters.Text = item.spot_code_name;
                    //shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                    //shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                    //shape.Line.ForeColor.TintAndShade = 0;
                    //shape.Line.Transparency = 0;
                    ////加入到控制集合中用于设置视图变化导致长宽不一致问题
                    //listShapes.Add(shape);

                    ////问题程度和状态                
                    //shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, py + ImageHeight + TitleHeight, ImageWidth, TitleHeight);
                    ////图片固定大小和位置 不随单元格变化而变化
                    //shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                    ////文字居中显示
                    //shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignCenter;
                    //shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    //shape.TextFrame2.TextRange.Font.Size = 10;
                    //shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;
                    //if (item.question_level == PatrolEntity.QuestionLevel_Emergency)
                    //{
                    //    //点检状态为《需修理/更换》或者问题程度为紧急字体变红色
                    //    shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = 255;
                    //}
                    //shape.TextFrame2.TextRange.Characters.Text = item.question_level_name + ":" + item.status_name;
                    //shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                    //shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                    //shape.Line.ForeColor.TintAndShade = 0;
                    //shape.Line.Transparency = 0;
                    ////加入到控制集合中用于设置视图变化导致长宽不一致问题
                    //listShapes.Add(shape);
                    #endregion

                    //添加备注文字                
                    shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, py + decodeHeight, decodeWidth, RemarksHeight);
                    //图片固定大小和位置 不随单元格变化而变化
                    shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                    //文字居左显示
                    shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignLeft;
                    shape.TextFrame2.TextRange.ParagraphFormat.BaselineAlignment = Microsoft.Office.Core.MsoBaselineAlignment.msoBaselineAlignTop;
                    shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    shape.TextFrame2.TextRange.Font.Size = 8;
                    //shape.TextFrame2.MarginLeft = 0;
                    //shape.TextFrame2.MarginRight = 0;
                    //shape.TextFrame2.MarginBottom = 0;
                    //shape.TextFrame2.MarginTop = 0;
                    shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorTop;
                    shape.TextFrame2.TextRange.Characters.Text = item.remarks;
                    shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                    shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                    shape.Line.ForeColor.TintAndShade = 0;
                    shape.Line.Transparency = 0;
                    //加入到控制集合中用于设置视图变化导致长宽不一致问题                    
                    listDecode.Add(shape);
                }
                //设置第一个分页符位置
                range = worksheet.get_Range("StartCell");
                listPages.Add(range);
                worksheet.HPageBreaks.Add(range);
                #endregion

                #region 点检部位信息
                List<PatrolDetailInfo> detailList = source.patrol_detail_list;
                int pageIndex = 1;
                //遍历所有明细
                //for (int i = 0; i < detailList.Count; i++)
                for (int i = 0; i < detailList.Count; i++)
                {
                    //每页标题部分
                    //range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[StartRowIndex + 1, StartColumnIndex];
                    range = worksheet.Range[worksheet.Cells[StartRowIndex + 3, StartColumnIndex], worksheet.Cells[StartRowIndex + 3, StartColumnIndex + 4]];
                    range.MergeCells = true;
                    //range.Interior.Color = 65535;//背景颜色黄色
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                    range.VerticalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                    //range.FormulaR1C1 = "检查" + detailList[i].location_code_name + "明细";
                    range.FormulaR1C1 = "<各部位点检情况>";
                    range.Font.Color = -13434727;
                    range.Font.Bold = true;
                    range.RowHeight = HeadTitleHeight;
                    //每页图片左边开始区域坐标
                    range = worksheet.Range[worksheet.Cells[StartRowIndex, StartColumnIndex], worksheet.Cells[StartRowIndex + 1, EndColumnIndex - 1]];
                    float logoY = Convert.ToSingle(range.Top + 5.0F);
                    range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[StartRowIndex + 3, StartColumnIndex];
                    float logoHeight = Convert.ToSingle(range.Top - logoY - 3.0F);
                    //添加logo图片
                    worksheet.Shapes.AddPicture(CommonInfo.ExcelLogoFile, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 0, logoY, Convert.ToSingle(EndX - 1.0F), logoHeight);

                    ImageLeftX = ImageLeftX;
                    ImageLeftY = range.Top;
                    //右边区域只要同步更新Y坐标即可
                    ImageRightY = range.Top;

                    //每页输出6张图片
                    for (int j = 0; j < ImageColumnsPerPage * ImageRowsPerPage; j++)
                    {
                        if (j % 2 == 0)
                        {
                            //左边区域图片
                            px = Convert.ToSingle(OffSetX.ToString("0.00"));
                            py = Convert.ToSingle((ImageLeftY + 8.0F + HeadTitleHeight).ToString("0.00"));
                            //Y坐标定位下一个左边区域
                            ImageLeftY += Convert.ToDouble((ImageHeight + TitleHeight*2 + RemarksHeight + 10F).ToString("0.00"));
                        }
                        else
                        {
                            //右边区域图片
                            px = Convert.ToSingle(ImageRightX.ToString("0.00"));
                            py = Convert.ToSingle((ImageRightY + 8.0F + HeadTitleHeight).ToString("0.00"));
                            //Y坐标定位下一个左边区域
                            ImageRightY += Convert.ToDouble((ImageHeight + TitleHeight*2 + RemarksHeight + 10F).ToString("0.00"));
                        }

                        //添加图片
                        Microsoft.Office.Interop.Excel.Shape shape = worksheet.Shapes.AddPicture(CommonInfo.ImageSaveUrl + "\\" + detailList[i].pic_url, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, Convert.ToSingle((px).ToString("0.00")), py, ImageWidth, ImageHeight);
                        //shape.IncrementLeft(0.75F);
                        //shape.ScaleWidth(1.0F, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);
                        //shape.ScaleHeight(1.0F, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoScaleFrom.msoScaleFromTopLeft);
                        //图片固定大小和位置 不随单元格变化而变化
                        shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                        shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                        shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                        shape.Line.ForeColor.TintAndShade = 0;
                        shape.Line.Transparency = 0;
                        shape.Line.Style = Microsoft.Office.Core.MsoLineStyle.msoLineSingle;
                        //加入到控制集合中用于设置视图变化导致长宽不一致问题
                        listShapes.Add(shape);

                        //添加点检部位和具体位置                
                        shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, Convert.ToSingle((py + ImageHeight).ToString("0.00")), ImageWidth, TitleHeight + TitleHeight);
                        //图片固定大小和位置 不随单元格变化而变化
                        shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                        //文字居中显示
                        shape.Width = ImageWidth;
                        shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignLeft;
                        shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                        shape.TextFrame2.TextRange.Font.Size = 8;
                        if (detailList[i].question_level == PatrolEntity.QuestionLevel_Emergency)
                        {
                            //点检状态为《需修理/更换》或者问题程度为紧急字体变红色
                            shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = 255;
                        }
                        shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;
                        shape.TextFrame2.TextRange.Characters.Text = detailList[i].location_code_name + "-" + detailList[i].spot_code_name + Environment.NewLine + detailList[i].status_name + ":" + detailList[i].question_level_name;
                        shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                        shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                        shape.Line.ForeColor.TintAndShade = 0;
                        shape.Line.Transparency = 0;
                        //加入到控制集合中用于设置视图变化导致长宽不一致问题
                        listShapes.Add(shape);

                        ////添加问题程度和状态                
                        //shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, Convert.ToSingle((py + ImageHeight + TitleHeight).ToString("0.00")), ImageWidth, TitleHeight);
                        ////图片固定大小和位置 不随单元格变化而变化
                        //shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                        ////文字居中显示
                        //shape.Width = ImageWidth;
                        //shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignLeft;
                        //shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                        //shape.TextFrame2.TextRange.Font.Size = 8;
                        //if (detailList[i].question_level == PatrolEntity.QuestionLevel_Emergency)
                        //{
                        //    //点检状态为《需修理/更换》或者问题程度为紧急字体变红色
                        //    shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = 255;
                        //}
                        //shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorMiddle;
                        //shape.TextFrame2.TextRange.Characters.Text = detailList[i].status_name +":" + detailList[i].question_level_name;
                        //shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                        //shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                        //shape.Line.ForeColor.TintAndShade = 0;
                        //shape.Line.Transparency = 0;
                        ////加入到控制集合中用于设置视图变化导致长宽不一致问题
                        //listShapes.Add(shape);

                        //添加备注文字                
                        shape = worksheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, px, Convert.ToSingle((py + ImageHeight + TitleHeight + TitleHeight).ToString("0.00")), ImageWidth, RemarksHeight);
                        //图片固定大小和位置 不随单元格变化而变化
                        shape.Placement = Microsoft.Office.Interop.Excel.XlPlacement.xlFreeFloating;
                        //文字居左显示
                        shape.Width = ImageWidth;
                        shape.TextFrame2.TextRange.ParagraphFormat.Alignment = Microsoft.Office.Core.MsoParagraphAlignment.msoAlignLeft;
                        //shape.TextFrame2.TextRange.ParagraphFormat.BaselineAlignment = Microsoft.Office.Core.MsoBaselineAlignment.msoBaselineAlignTop;
                        shape.TextFrame2.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                        shape.TextFrame2.TextRange.Font.Size = 8;
                        //shape.TextFrame2.MarginLeft = 0;
                        //shape.TextFrame2.MarginRight = 0;
                        shape.TextFrame2.MarginBottom = 0;
                        shape.TextFrame2.MarginTop = 0;
                        shape.TextFrame2.VerticalAnchor = Microsoft.Office.Core.MsoVerticalAnchor.msoAnchorTop;
                        shape.TextFrame2.TextRange.Characters.Text = detailList[i].remarks;
                        shape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                        shape.Line.ForeColor.ObjectThemeColor = Microsoft.Office.Core.MsoThemeColorIndex.msoThemeColorText1;
                        shape.Line.ForeColor.TintAndShade = 0;
                        shape.Line.Transparency = 0;
                        //加入到控制集合中用于设置视图变化导致长宽不一致问题
                        listShapes.Add(shape);
                        //索引同步更新
                        if (i >= detailList.Count - 1 || j == ImageColumnsPerPage * ImageRowsPerPage - 1)
                        {
                            //到记录最后一条,退出 i值减一
                            break;
                        }
                        else
                        {
                            i++;                        
                        }
                    }
                    //重新定位到下一页行位置
                    //获取开始行所在位置 总行数包含当前行所以要减一
                    StartRowIndex += RowsCount;
                    range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[StartRowIndex, StartColumnIndex];
                    //设置分页符从第二个开始设置 第一个不用设置 
                    worksheet.HPageBreaks.Add(range);
                    listPages.Add(range);
                    //自增1
                    pageIndex++;
                }
                #endregion

                //打印设置 设置页眉显示图片 已放弃
                //worksheet.PageSetup.LeftHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                //worksheet.PageSetup.CenterHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                //worksheet.PageSetup.RightHeaderPicture.Filename = @"C:\Users\liuchengyun.DOMAIN\Desktop\header.bmp";
                //worksheet.PageSetup.LeftHeader = "&G";
                //worksheet.PageSetup.CenterHeader = "&G";
                //worksheet.PageSetup.RightHeader = "&G";
                ////设置页脚
                worksheet.PageSetup.LeftFooter = "";
                worksheet.PageSetup.CenterFooter = "&\"Arial,加粗\"&14©" + header.company_name + "版权所有";
                worksheet.PageSetup.RightFooter = "第 &P 页，共 &N 页";
                //worksheet.PageSetup.LeftMargin = xlApp.Application.InchesToPoints(0);
                //worksheet.PageSetup.RightMargin = xlApp.Application.InchesToPoints(0);
                //worksheet.PageSetup.TopMargin = xlApp.Application.InchesToPoints(0);
                //worksheet.PageSetup.BottomMargin = xlApp.Application.InchesToPoints(0);
                //worksheet.PageSetup.HeaderMargin = xlApp.Application.InchesToPoints(0);
                //worksheet.PageSetup.FooterMargin = xlApp.Application.InchesToPoints(0);
                worksheet.PageSetup.PrintArea = "";//"$A$1:$U$" + StartRowIndex;//重设打印区域
                //worksheet.PageSetup.PrintQuality = 600;
                //worksheet.PageSetup.CenterHorizontally = true;
                //worksheet.PageSetup.CenterVertically = true;

                //worksheet.PageSetup.PaperSize = Microsoft.Office.Interop.Excel.XlPaperSize.xlPaperA4;
                //double pp = xlApp.Application.CentimetersToPoints(21);

                //如果没有点检明细那么不要设置只有一页
                //动态设置分页符
                try
                {

                    if (source.patrol_detail_list.Count > 0)
                    {
                        xlApp.ActiveWindow.View = Microsoft.Office.Interop.Excel.XlWindowView.xlPageBreakPreview;
                        //分页符设置
                        for (int i = 1; i <= listPages.Count; i++)
                        {
                            if (i <= worksheet.HPageBreaks.Count && listPages.Count >= worksheet.HPageBreaks.Count)
                            {
                                worksheet.HPageBreaks.Item[i].Location = listPages[i - 1];
                            }
                        }
                    }
                    xlApp.ActiveWindow.View = Microsoft.Office.Interop.Excel.XlWindowView.xlPageLayoutView;
                    for (int j = 0; j < listDecode.Count; j++)
                    {
                        //外观图片备注宽度控制
                        listShapes[j].Width = decodeWidth;                       
                    }
                    //图片长宽设置
                    for (int i = 0; i < listShapes.Count; i = i + 3)
                    {
                        //图片保证正方形
                        listShapes[i].Width = ImageWidth;
                        listShapes[i].Height = ImageWidth;
                        //文本框保持宽度和图片一致高度不动
                        listShapes[i + 1].Width = ImageWidth;
                        //listShapes[i+1].Height = ImageWidth;
                        listShapes[i + 2].Width = ImageWidth;
                        //listShapes[i+2].Height = ImageWidth;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("分页错误：" + e.Message);
                    MyLog.Loger.Error("设置分页符错误:" + e.Message);
                }
                //保存文件
                xlApp.ActiveWindow.View = Microsoft.Office.Interop.Excel.XlWindowView.xlPageBreakPreview;
                //xlApp.Visible = true;
                workbook.Save();
                workbook.Saved = true;

                //所有处理都完成
                //转换成pdf文件保存
                worksheet.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF,
                    strpdf,
                    Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard);
                //worksheet.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF,
                //    strpdf,
                //    Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard,
                //    true,
                //    true,
                //    Type.Missing,
                //    Type.Missing,
                //    false,
                //    Type.Missing
                //    );
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("导出错误：" + ex.Message);
                MyLog.Loger.Error("导出Excel错误:" + ex.Message);
            }
            finally
            {
                try
                {
                    //if (range != null)
                    //{
                    //    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(range);
                    //    Console.WriteLine("Range关闭");
                    //    range = null;
                    //}
                    //if (worksheet != null)
                    //{
                    //    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(worksheet);
                    //    Console.WriteLine("Sheet关闭");
                    //    worksheet = null;
                    //}
                    //if (workbook != null)
                    //{
                    //    workbook.Close(false);
                    //    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbook);
                    //    Console.WriteLine("workbook关闭");
                    //    workbook = null;
                    //}
                    //if (workbooks != null)
                    //{
                    //    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbooks);
                    //    Console.WriteLine("workbooks关闭");
                    //    workbooks = null;
                    //}
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Excel关闭错误：" + exc.Message);
                    MyLog.Loger.Error("Excel关闭错误:" + exc.Message);
                    success = false;
                }
                finally {
                    if (xlApp != null)
                    {
                        //关闭Excel进程
                        KillSpecialExcel(xlApp,processId);                       
                        xlApp = null;
                        Console.WriteLine("Excel结束所有");
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }

            }
            return success;
        }
        
        #region 结束EXCEL.EXE进程的方法

        /// <summary>

        /// 结束EXCEL.EXE进程的方法

        /// </summary>

        /// <param name="m_objExcel">EXCEL对象</param>

        [DllImport(@"user32.dll", SetLastError = true)]

        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        private static void KillSpecialExcel(Microsoft.Office.Interop.Excel.Application m_objExcel,int processId)
        {
            try
            {

                if (m_objExcel != null)
                {
                    int lpdwProcessId;
                    int pintid = GetWindowThreadProcessId(new IntPtr(m_objExcel.Hwnd), out lpdwProcessId);
                    
                    Console.WriteLine("Excel句柄是：" + m_objExcel.Hwnd.ToString());
                    Console.WriteLine("pintid句柄是：" + pintid.ToString());
                    Console.WriteLine("正在尝试关闭Excel进程" + processId.ToString());
                    GC.Collect();
                    //System.Diagnostics.Process.GetProcessById(lpdwProcessId, System.Environment.MachineName).CloseMainWindow();
                    //System.Diagnostics.Process.GetProcessById(lpdwProcessId, System.Environment.MachineName).Close();
                    //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(lpdwProcessId);

                    System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("excel");
                    foreach (System.Diagnostics.Process item in ps)
                    {
                        Console.WriteLine("进程Id：" + item.Id.ToString());
                        if (processId == item.Id)
                        {
                            Console.WriteLine("关闭进程：" + item.Id);
                            item.Kill();
                        }

                    }
                }

            }

            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }   
        }

        #endregion
    }
}
