using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text;
using LogicLync.Service.Infrastructure;
using DocumentFormat.OpenXml.Office2013.Excel;
using System.Drawing.Text;
//using IronBarCode;
using ZXing.QrCode;
using ZXing;

namespace LogicLync.Service
{
    public class BarcodeService : IBarcodeService
    {
        public string createBarcodeBase64(String barcode)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Bitmap bitMap = new Bitmap(barcode.Length * 24, 100))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitMap))
                        {
                        //   var fonts = new PrivateFontCollection();
                        //fonts.AddFontFile(rootPath +"/Content/Font/IDAutomationHC39M.ttf");
                            Font font = new Font("IDAutomationHC39M Free Version", 16);
                            PointF point = new PointF(2f, 2f);
                            SolidBrush whiteBrush = new SolidBrush(Color.White);
                            graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                            SolidBrush blackBrush = new SolidBrush(Color.Black);
                            graphics.DrawString("*" + barcode + "*", font, blackBrush, point);
                        }
                        bitMap.Save(memoryStream, ImageFormat.Jpeg);
                        var barcode_image_url = "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
                        return barcode_image_url;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public string createQRCodeUrl(String code, String BaseFolder, String rootPath)
        {
            try
            {
                string filename = code + DateTime.Now.ToString("_yyyy_MM_dd_hh_mm_ss") + ".png";

                QrCodeEncodingOptions qrCodeEncodingOptions = new QrCodeEncodingOptions();
                qrCodeEncodingOptions.DisableECI = true;
                qrCodeEncodingOptions.CharacterSet = "UTF-8";
                qrCodeEncodingOptions.Width = 500;
                qrCodeEncodingOptions.Height = 500;

                BarcodeWriter writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = qrCodeEncodingOptions;

                Bitmap qrCodeBitmap = writer.Write(code);
                string basepath = Path.Combine(rootPath, BaseFolder);
                qrCodeBitmap.Save(basepath + filename);
                return BaseFolder + filename;

            }
            catch (Exception e)
            {
                return "";

            }



            /* try
             {
                 GeneratedBarcode qrCode = IronBarCode.BarcodeWriter.CreateBarcode(
                       code, BarcodeEncoding.QRCode);

                 string basepath = Path.Combine(rootPath, BaseFolder);
                 string filename = code + DateTime.Now.ToString("_yyyy_MM_dd_hh_ss_mm") + ".png";
                 qrCode.SaveAsPng(basepath + filename);
                 return BaseFolder + filename;

             }
             catch (Exception e)
             {
                 return "";

             }*/
        }

        public string createBarcodeUrl(String barcode, String BaseFolder, String rootPath)
        {
            try
            {
               string filename = barcode + DateTime.Now.ToString("_yyyy_MM_dd_hh_mm_ss") + ".png";
                
                QrCodeEncodingOptions qrCodeEncodingOptions = new QrCodeEncodingOptions();
                qrCodeEncodingOptions.DisableECI = true;
                qrCodeEncodingOptions.CharacterSet = "UTF-8";
                qrCodeEncodingOptions.Width = 500;
                qrCodeEncodingOptions.Height = 500;
                
                BarcodeWriter writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.CODE_128;
                writer.Options = qrCodeEncodingOptions;
               
                Bitmap qrCodeBitmap = writer.Write(barcode);
                string basepath = Path.Combine(rootPath, BaseFolder);
                qrCodeBitmap.Save(basepath+filename);
                return BaseFolder + filename;

            }
            catch (Exception e)
            {
                return "";

            }

            /* try
             {
                 GeneratedBarcode qrCode = IronBarCode.BarcodeWriter.CreateBarcode(
                       //barcode, BarcodeEncoding.Code128).ResizeToMil(1.88977, 0.23623, 96);
                       barcode, BarcodeEncoding.Code128);
                 string basepath = Path.Combine(rootPath, BaseFolder);
                 string filename = barcode + DateTime.Now.ToString("_yyyy_MM_dd_hh_ss_mm") + ".png";
                 qrCode.SaveAsPng(basepath + filename);

                 return BaseFolder + filename;

             }
             catch(Exception e)
             {
                 return "";

             }*/
            /*
            try
            {
                using (Bitmap bitMap = new Bitmap(barcode.Length * 24, 100))
                {
                    using (Graphics graphics = Graphics.FromImage(bitMap))
                    {
                        var fonts = new PrivateFontCollection();
                        fonts.AddFontFile(rootPath +"/Content/Font/IDAutomationHC39M.ttf");
                        //Font font = new Font("IDAutomationHC39M Free Version", 16); 
                        Font font = new Font((FontFamily)fonts.Families[0], 16f);
                        PointF point = new PointF(2f, 2f);
                        SolidBrush whiteBrush = new SolidBrush(Color.White);
                        graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                        SolidBrush blackBrush = new SolidBrush(Color.Black);
                        graphics.DrawString("*" + barcode + "*", font, blackBrush, point);
                    }
                    string filename = barcode + DateTime.Now.ToString("_yyyy_MM_dd_hh_ss_mm") + ".jpg";
                    bitMap.Save(rootPath +  BaseFolder + filename, ImageFormat.Jpeg);
                    var len = BaseFolder.Length;
                    var basepath = BaseFolder.Substring(1, len-1);

                    return basepath + filename;
                }
            }
            catch (Exception ex)
            {
                return "";
            }*/
        }


        public byte[] createBarcodeImg(String barcode)
        {
            try
            {

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Bitmap bitMap = new Bitmap(barcode.Length * 24, 100))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitMap))
                        {
                            Font font = new Font("IDAutomationHC39M Free Version", 16);
                            PointF point = new PointF(2f, 2f);
                            SolidBrush whiteBrush = new SolidBrush(Color.White);
                            graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                            SolidBrush blackBrush = new SolidBrush(Color.Black);
                            graphics.DrawString("*" + barcode + "*", font, blackBrush, point);
                        }
                        bitMap.Save(memoryStream, ImageFormat.Jpeg);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
