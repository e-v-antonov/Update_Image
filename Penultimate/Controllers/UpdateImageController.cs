using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing;
using UpdateImage.AdditionalClass;
using System.IO;
using UpdateImage.Controllers;
using Microsoft.AspNetCore.Hosting;

namespace UpdateImage.Views.Home
{
    public class UpdateImageController : Controller
    {
        private readonly IHostingEnvironment he;
        private static Bitmap image;
        private static UInt32[,] pixel;
        private static byte [] bitmapBytes;
        private bool updateRGB = false;

        public UpdateImageController(IHostingEnvironment e)
        {
            he = e;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UpdateImage(int brightness, int contrast)  //изменение яркости и контрастности
        {
            UInt32 p;
            image = new Bitmap(ActionPictureController.pathImage);
            // UInt32[,] pixel;

            //получение матрицы с пикселями
            pixel = new UInt32[image.Height, image.Width];
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());

            //яркость
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    p = BrightnessContrast.Brightness(pixel[i, j], brightness, 10); //получение значения цвета пискеля
                    FromOnePixelToBitmap(i, j, p);  //присвоение полученного цвета пикселю на картинке
                    pixel[i, j] = (UInt32)(image.GetPixel(j, i).ToArgb());  //занесение нового цвета пикселя в матрицу, т. е. сохранение
                }

            //контрастность
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    p = BrightnessContrast.Contrast(pixel[i, j], contrast, 10);
                    FromOnePixelToBitmap(i, j, p);
                    pixel[i, j] = (UInt32)(image.GetPixel(j, i).ToArgb());
                }

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateColorImage(int blueRed, int purpleGreen, int yellowDarkBlue) //изменение цветового баланса
        {
            UInt32 p;
            image = new Bitmap(ActionPictureController.pathImage);


            //получение матрицы с пикселями
            pixel = new UInt32[image.Height, image.Width];
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());

            //цветовой баланс R
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    p = ColorBalance.ColorBalance_R(pixel[i, j], blueRed, 10);
                    FromOnePixelToBitmap(i, j, p);
                    pixel[i, j] = (UInt32)(image.GetPixel(j, i).ToArgb());
                }

            //цветовой баланс G
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    p = ColorBalance.ColorBalance_G(pixel[i, j], purpleGreen, 10);
                    FromOnePixelToBitmap(i, j, p);
                    pixel[i, j] = (UInt32)(image.GetPixel(j, i).ToArgb());
                }

            //цветовой баланс B
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    p = ColorBalance.ColorBalance_B(pixel[i, j], yellowDarkBlue, 10);
                    FromOnePixelToBitmap(i, j, p);
                    pixel[i, j] = (UInt32)(image.GetPixel(j, i).ToArgb());
                }

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }


        [HttpGet]
        public IActionResult UpdateSharpnessImage() //повышение резкости
        {
            image = new Bitmap(ActionPictureController.pathImage);

            pixel = new UInt32[image.Height, image.Width];
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());

            pixel = Filter.matrix_filtration(image.Width, image.Height, pixel, Filter.N1, Filter.sharpness);
            FromPixelToBitmap();

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateBlurImage()  //размытие
        {
            image = new Bitmap(ActionPictureController.pathImage);

            pixel = new UInt32[image.Height, image.Width];
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());

            pixel = Filter.matrix_filtration(image.Width, image.Height, pixel, Filter.N2, Filter.blur);
            FromPixelToBitmap();

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        private static byte[] BitmapToBytes(Bitmap img) //из Bitmap в байты
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }

        public static void FromPixelToBitmap()  //преобразование из писелей в Bitmap
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb((int)pixel[y, x]));
        }

        public static void FromOnePixelToBitmap(int x, int y, UInt32 pixel) //преобразование из UINT32 to Bitmap по одному пикселю
        {
            image.SetPixel(y, x, Color.FromArgb((int)pixel));
        }

        [HttpGet]
        public IActionResult UpdateBlackWhiteImage()    //черно-белое изображение
        {
            image = new Bitmap(ActionPictureController.pathImage);

            // перебираем в циклах все пиксели исходного изображения
            for (int j = 0; j < image.Height; j++)
                for (int i = 0; i < image.Width; i++)
                {
                    // получаем (i, j) пиксель
                    UInt32 pixel = (UInt32)(image.GetPixel(i, j).ToArgb());
                    // получаем компоненты цветов пикселя
                    float R = (float)((pixel & 0x00FF0000) >> 16); // красный
                    float G = (float)((pixel & 0x0000FF00) >> 8); // зеленый
                    float B = (float)(pixel & 0x000000FF); // синий
                                                           // делаем цвет черно-белым (оттенки серого) - находим среднее арифметическое
                    R = G = B = (R + G + B) / 3.0f;
                    // собираем новый пиксель по частям (по каналам)
                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);
                    // добавляем его в Bitmap нового изображения
                    image.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateInvertImage()    //инверсия изображения
        {
            image = new Bitmap(ActionPictureController.pathImage);

            BitmapData bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - image.Width * 4;
                int nWidth = image.Width;

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        p[0] = (byte)(255 - p[0]); //red
                        p[1] = (byte)(255 - p[1]); //green
                        p[2] = (byte)(255 - p[2]); //blue
                        //p[3] is alpha, don't want to invert alpha

                        p += 4;
                    }
                    p += nOffset;
                }
            }
            image.UnlockBits(bmData);

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateRotate90Image()  //поворот на 90
        {
            image = new Bitmap(ActionPictureController.pathImage);

            image.RotateFlip(RotateFlipType.Rotate90FlipNone);

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateRotate180Image() //поворот на 180
        {
            image = new Bitmap(ActionPictureController.pathImage);

            image.RotateFlip(RotateFlipType.Rotate180FlipNone);

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateRotate270Image() //поворот на 270
        {
            image = new Bitmap(ActionPictureController.pathImage);

            image.RotateFlip(RotateFlipType.Rotate270FlipNone);

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateFlipXImage() //отражание по горизонтали
        {
            image = new Bitmap(ActionPictureController.pathImage);

            image.RotateFlip(RotateFlipType.RotateNoneFlipX);

            bitmapBytes = BitmapToBytes(image);
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult UpdateFlipYImage() //отражение по вертикали
        {
            image = new Bitmap(ActionPictureController.pathImage);

            image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bitmapBytes = BitmapToBytes(image);            
            image.Dispose();

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }

        [HttpGet]
        public IActionResult SaveImage()    //сохранение измененного изображения
        {
            string pathFile = Path.Combine(he.WebRootPath, "update_" + ActionPictureController.NameImageUpdate);
            System.IO.File.WriteAllBytes(pathFile, bitmapBytes);
            System.IO.File.Delete(ActionPictureController.pathImage);
            System.IO.File.Move(pathFile, ActionPictureController.pathImage);

            string imreBase64Data = Convert.ToBase64String(bitmapBytes);
            string imgDataURL = string.Format("data:image/jpeg;base64,{0}", imreBase64Data);
            ViewBag.ImageData = imgDataURL;
            return View("~/Views/Home/UpdateImage.cshtml");
        }
    }
}