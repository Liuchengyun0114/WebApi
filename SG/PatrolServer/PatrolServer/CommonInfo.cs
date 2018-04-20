using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using LogWriter;

namespace PatrolServer
{
    public class CommonInfo
    {
        #region 日志打印控制
        public static void Log(string msg)
        {
            MyLog.Loger.Info("================日志开始================");
            MyLog.Loger.Info(msg);
            MyLog.Loger.Info("================日志结束================");
        }
        public static void Error(string msg)
        {
            MyLog.Loger.Error("================日志开始================");
            MyLog.Loger.Error(msg);
            MyLog.Loger.Error("================日志结束================");
        }
        #endregion

        /// <summary>
        /// 图片裁剪正方形的边长
        /// </summary>
        private static int _ClipImageSize = 0;

        public static int ClipImageSize
        {
            get
            {
                if (CommonInfo._ClipImageSize == 0)
                {
                    int width = 0;
                    try
                    {
                        String size = System.Configuration.ConfigurationManager.AppSettings["clipImageSize"].ToString();
                        //读取数据成功
                        if (size != null && size != String.Empty)
                        {
                            width = Int32.Parse(size);
                        }
                        else
                        {
                            //转换失败
                            width = 400;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("裁剪图片配置错误,采用默认设置");
                        LogWriter.MyLog.Log("裁剪图片配置错误,采用默认设置");
                        width = 400;
                    }
                    finally {
                        CommonInfo._ClipImageSize = width;
                    }

                }
                return CommonInfo._ClipImageSize;
            }
        }

        /// <summary>
        /// Excel打印模板路径
        /// </summary>
        private static string _ExcelTemplateFile = String.Empty;

        public static string ExcelTemplateFile
        {
            get
            {
                if (CommonInfo._ExcelTemplateFile == String.Empty)
                {
                    CommonInfo._ExcelTemplateFile = AppDomain.CurrentDomain.BaseDirectory + "" + System.Configuration.ConfigurationManager.AppSettings["excelTemplateFile"].ToString();
                }
                return CommonInfo._ExcelTemplateFile;
            }
        }
        
        /// <summary>
        /// 默认打印机控制
        /// </summary>
        private static string _DefaultPrinter = String.Empty;

        public static string DefaultPrinter
        {
            get
            {
                if (CommonInfo._DefaultPrinter == String.Empty)
                {
                    CommonInfo._DefaultPrinter = System.Configuration.ConfigurationManager.AppSettings["defaultPrinter"].ToString();
                }
                return CommonInfo._DefaultPrinter;            
            }
        }

        /// <summary>
        /// PDF后缀名称
        /// </summary>
        private static string _PDFExtend = String.Empty;

        public static string PDFExtend
        {
            get
            {
                if (CommonInfo._PDFExtend == String.Empty)
                {
                    CommonInfo._PDFExtend = System.Configuration.ConfigurationManager.AppSettings["pdfExtend"].ToString();
                }
                return CommonInfo._PDFExtend;
            }
        }

        /// <summary>
        /// Excel后缀名称
        /// </summary>
        private static string _ExcelExtend = String.Empty;

        public static string ExcelExtend
        {
            get
            {
                if (CommonInfo._ExcelExtend == String.Empty)
                {
                    CommonInfo._ExcelExtend = System.Configuration.ConfigurationManager.AppSettings["excelExtend"].ToString();
                }
                return CommonInfo._ExcelExtend;
            }
        }

        /// <summary>
        /// Excel打印Logo路径
        /// </summary>
        private static string _ExcelLogoFile = String.Empty;

        public static string ExcelLogoFile
        {
            get
            {
                if (CommonInfo._ExcelLogoFile == String.Empty)
                {
                    CommonInfo._ExcelLogoFile = AppDomain.CurrentDomain.BaseDirectory + "" + System.Configuration.ConfigurationManager.AppSettings["excelLogoFile"].ToString();
                }
                return CommonInfo._ExcelLogoFile;
            }
        }

        /// <summary>
        /// 二维码图片地址路径
        /// </summary>
        private static string _TwoFile = String.Empty;

        public static string TwoFile
        {
            get
            {
                if (CommonInfo._TwoFile == String.Empty)
                {
                    CommonInfo._TwoFile = AppDomain.CurrentDomain.BaseDirectory + "" + System.Configuration.ConfigurationManager.AppSettings["twoFile"].ToString();
                }
                return CommonInfo._TwoFile;
            }
        }

        /// <summary>
        /// 密码重置默认
        /// </summary>
        private static string _DefaultPassword = String.Empty;

        public static string DefaultPassword
        {
            get
            {
                if (CommonInfo._DefaultPassword == String.Empty)
                {
                    CommonInfo._DefaultPassword = System.Configuration.ConfigurationManager.AppSettings["defaultPassword"].ToString();
                }
                return CommonInfo._DefaultPassword;
            }
        }

        /// <summary>
        /// Excel报表打印存储文件夹
        /// </summary>
        private static string _ExcelUrl = String.Empty;

        public static string ExcelUrl
        {
            get
            {
                if (CommonInfo._ExcelUrl == String.Empty)
                {
                    CommonInfo._ExcelUrl = System.Configuration.ConfigurationManager.AppSettings["excelUrl"].ToString();
                }
                return CommonInfo._ExcelUrl;
            }
        }

        /// <summary>
        /// 上传图片临时存储文件夹
        /// </summary>
        private static string _ImageTempUrl = String.Empty;

        public static string ImageTempUrl
        {
            get
            {
                if (CommonInfo._ImageTempUrl == String.Empty)
                {
                    CommonInfo._ImageTempUrl = System.Configuration.ConfigurationManager.AppSettings["imageTempUrl"].ToString();
                }
                return CommonInfo._ImageTempUrl;
            }
        }

        /// <summary>
        /// 上传特巡成功后将临时文件夹移动到此存储文件夹
        /// </summary>
        private static string _ImageSaveUrl = String.Empty;

        public static string ImageSaveUrl
        {
            get
            {
                if (CommonInfo._ImageSaveUrl == String.Empty)
                {
                    CommonInfo._ImageSaveUrl = System.Configuration.ConfigurationManager.AppSettings["imageSaveUrl"].ToString();
                }
                return CommonInfo._ImageSaveUrl;
            }
        }
        /// <summary>
        /// 启动服务前的环境初始化,包括上传图片临时文件夹、存储文件夹的创建
        /// </summary>
        /// <returns>true=创建成功,false=创建失败</returns>
        public static bool Init()
        {
            bool success = false;
            try
            {
                if (!System.IO.Directory.Exists(ImageTempUrl))
                {
                    Directory.CreateDirectory(ImageTempUrl);
                }
                if (!System.IO.Directory.Exists(ImageSaveUrl))
                {
                    Directory.CreateDirectory(ImageSaveUrl);
                }
                if (!System.IO.Directory.Exists(ExcelUrl))
                {
                    Directory.CreateDirectory(ExcelUrl);
                }
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return success;
        }

        //文件夹所有jpg图片裁剪处理
        public static void ClipImage(string dirname)
        {
            try
            {
                //取得文件夹下所有jpg图片
                if (System.IO.Directory.Exists(dirname))
                {
                    int intWidth = 400;//裁剪宽度
                    int intHeight = 400;//裁剪高度
                    string[] imageList = System.IO.Directory.GetFiles(dirname, "*.jpg", SearchOption.TopDirectoryOnly);
                    Image templateImage = null;
                    Graphics g = null;
                    Image img = null;
                    if (imageList == null || imageList.Length < 0)
                    {
                        Console.WriteLine("文件夹为空没有图片需要处理");
                        return;
                    }
                    for (int i = 0; i < imageList.Length; i++)
                    {
                        string filename = imageList[i];
                        ImageProcess(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //单图片处理
        public static void ImageProcess(string filename)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    System.Drawing.Image img = Image.FromStream(fs, true);
                    fs.Close();
                    if (img != null)
                    {
                        //裁剪图片大小
                        Size rect = new Size(CommonInfo.ClipImageSize,CommonInfo.ClipImageSize);                        
                        //创建截图
                        Image templateImage = new Bitmap(rect.Width, rect.Height);
                        //绘图对象
                        Graphics g = Graphics.FromImage(templateImage);
                        //质量控制
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        //原图长宽不等 那么以中心点位置截图放大至模板大小
                        if (img.Width != img.Height)
                        {

                            if (img.Width > img.Height)
                            {
                                g.DrawImage(img, new Rectangle(0, 0, rect.Width, rect.Height), new Rectangle((img.Width - img.Height) / 2, 0, img.Height, img.Height), GraphicsUnit.Pixel);
                            }
                            else
                            {
                                g.DrawImage(img, new Rectangle(0, 0, rect.Width, rect.Height), new Rectangle(0, (img.Height - img.Width) / 2, img.Width, img.Width), GraphicsUnit.Pixel);
                            }
                        }
                        else
                        {
                            //原图等比例缩小
                            g.DrawImage(img, new Rectangle(0, 0, rect.Width, rect.Height), new Rectangle(0, 0, img.Width, img.Width), GraphicsUnit.Pixel);
                        }
                        img = (System.Drawing.Image)templateImage.Clone();
                        //filename = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.Ticks.ToString() + ".jpg";                        
                        img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        templateImage.Dispose();
                        templateImage = null;
                        img.Dispose();
                        img = null;
                        g.Dispose();
                        g = null;
                        //日志输出
                        LogWriter.MyLog.Log("裁剪图片成功,文件路径:" + filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("裁剪图片错误" + ex.Message);
                //日志输出
                LogWriter.MyLog.Log("裁剪图片错误" + ex.Message);
            }

        }
    }
}
