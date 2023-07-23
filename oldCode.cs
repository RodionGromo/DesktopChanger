using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDL2.SDL;

namespace desktopHack
{
    internal class oldCode
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);
        [Flags()]
        private enum DeviceContextValues : uint
        {
            /// <summary>DCX_WINDOW: Returns a DC that corresponds to the window rectangle rather
            /// than the client rectangle.</summary>
            Window = 0x00000001,
            /// <summary>DCX_CACHE: Returns a DC from the cache, rather than the OWNDC or CLASSDC
            /// window. Essentially overrides CS_OWNDC and CS_CLASSDC.</summary>
            Cache = 0x00000002,
            /// <summary>DCX_NORESETATTRS: Does not reset the attributes of this DC to the
            /// default attributes when this DC is released.</summary>
            NoResetAttrs = 0x00000004,
            /// <summary>DCX_CLIPCHILDREN: Excludes the visible regions of all child windows
            /// below the window identified by hWnd.</summary>
            ClipChildren = 0x00000008,
            /// <summary>DCX_CLIPSIBLINGS: Excludes the visible regions of all sibling windows
            /// above the window identified by hWnd.</summary>
            ClipSiblings = 0x00000010,
            /// <summary>DCX_PARENTCLIP: Uses the visible region of the parent window. The
            /// parent's WS_CLIPCHILDREN and CS_PARENTDC style bits are ignored. The origin is
            /// set to the upper-left corner of the window identified by hWnd.</summary>
            ParentClip = 0x00000020,
            /// <summary>DCX_EXCLUDERGN: The clipping region identified by hrgnClip is excluded
            /// from the visible region of the returned DC.</summary>
            ExcludeRgn = 0x00000040,
            /// <summary>DCX_INTERSECTRGN: The clipping region identified by hrgnClip is
            /// intersected with the visible region of the returned DC.</summary>
            IntersectRgn = 0x00000080,
            /// <summary>DCX_EXCLUDEUPDATE: Unknown...Undocumented</summary>
            ExcludeUpdate = 0x00000100,
            /// <summary>DCX_INTERSECTUPDATE: Unknown...Undocumented</summary>
            IntersectUpdate = 0x00000200,
            /// <summary>DCX_LOCKWINDOWUPDATE: Allows drawing even if there is a LockWindowUpdate
            /// call in effect that would otherwise exclude this window. Used for drawing during
            /// tracking.</summary>
            LockWindowUpdate = 0x00000400,
            /// <summary>DCX_USESTYLE: Undocumented, something related to WM_NCPAINT message.</summary>
            UseStyle = 0x00010000,
            /// <summary>DCX_VALIDATE When specified with DCX_INTERSECTUPDATE, causes the DC to
            /// be completely validated. Using this function with both DCX_INTERSECTUPDATE and
            /// DCX_VALIDATE is identical to using the BeginPaint function.</summary>
            Validate = 0x00200000,
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromDC(IntPtr hDC);

        class Particle
        {
            public Vector2 Position { get; set; } = new Vector2(0, 0);
            public float Size { get; set; } = 0.0f;
            public Vector2 Velocity { get; set; } = new Vector2(0.0f, 0.0f);
            public float Angle { get; set; } = 0.0f;
            public float AngularVelocity { get; set; } = 0f;
            public Color Color { get; set; } = new();
            public Image Image { get; set; } = null;
            public IntPtr Surface { get; set; } = default;
            public Particle(Vector2 pos, float size, float angle)
            {
                this.Position = pos;
                this.Size = size;
                this.Angle = angle;
                Random rnd = new();
                this.Color = Color.FromArgb(255, 255, rnd.Next(0, 255), 0);
            }
        }
        class ParticleSettings
        {
            public Vector2 LimitVelocityX { get; set; } = new(-5, 5);
            public Vector2 LimitVelocityY { get; set; } = new(1, 5);
            public Vector2 LimitAngularVelocity { get; set; } = new(.1f, 5);
            public Vector2 LimitSizeMultiplyer { get; set; } = new(1, 2);
            public Color BackgroundColor { get; set; } = Color.Gray;
            public Color BaseColor { get; set; } = Color.White;
            public bool[] UseRandomForColors { get; set; } = { false, false, false };
            public Vector2[] RandomValuesForColors { get; set; } = { new(0, 255), new(0, 255), new(0, 255) };
            public int ParticleCount { get; set; } = 200;
        }
        static int CastToInt(float number)
        {
            return (int)number;
        }
        //v1 drawing
        static Image RotateImage(Image img, float angle)
        {
            Bitmap bmp = new(img.Width, img.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            gfx.RotateTransform(angle);
            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gfx.DrawImage(img, new Point(0, 0));
            gfx.Dispose();
            return bmp;
        }

        static Image RecolorImage(Image img, Color color)
        {
            Bitmap bmp = new(img);
            int height = bmp.Height;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (bmp.GetPixel(x, y).A != 0)
                    {
                        bmp.SetPixel(x, y, color);
                    }
                }
            }
            return bmp;
        }

        static void DoDrawingLeafs(Graphics desktop, int particleCount = 200)
        {
            // settings
            int maxFrames = 1500;
            int baseSize = 15;
            // leafs
            Image leaf1 = Image.FromFile("C:/Users/RodionGromo/Documents/pyWhat/graphicalEdit/particleDroppers/base spriteDropper/leafs/3.png");
            // code
            List<Particle> particles = new();
            Random rnd = new();
            Vector2 screenSize = new(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 newPos = new(rnd.Next(0, (int)screenSize.X), rnd.Next(-50, (int)screenSize.Y));
                Particle prtcl = new(newPos, (float)(rnd.NextDouble() * 1.5), (float)(rnd.NextDouble() * 360))
                {
                    Velocity = new Vector2(rnd.Next(-2, 2), rnd.Next(1, 10))
                };
                prtcl.Image = RecolorImage(RotateImage(leaf1, prtcl.Angle), prtcl.Color);
                particles.Add(prtcl);
            }

            Brush bgBrush = new SolidBrush(Color.Gray);
            Image newDesktop = (Image)new Bitmap((int)desktop.VisibleClipBounds.Width, (int)desktop.VisibleClipBounds.Height);
            Graphics ndgfx = Graphics.FromImage(newDesktop);
            for (int i = 0; i < maxFrames; i++)
            {
                ndgfx.FillRectangle(bgBrush, new(0, 0, (int)screenSize.X, (int)screenSize.Y));
                for (int i2 = 0; i2 < particles.Count; i2++)
                {
                    Particle prtcl = particles[i2];
                    prtcl.Position = new(prtcl.Position.X + prtcl.Velocity.X, prtcl.Position.Y + prtcl.Velocity.Y);
                    ndgfx.DrawImage(prtcl.Image, new Point((int)prtcl.Position.X, (int)prtcl.Position.Y));

                    if (prtcl.Position.X > screenSize.X)
                    {
                        prtcl.Position = new(0, prtcl.Position.Y);
                    }
                    else if (prtcl.Position.X < 0)
                    {
                        prtcl.Position = new(screenSize.X, prtcl.Position.Y);
                    }

                    if (prtcl.Position.Y > screenSize.Y)
                    {
                        prtcl.Position = new(prtcl.Position.X, -baseSize);
                    }
                }
                desktop.DrawImageUnscaled(newDesktop, 0, 0);
            }
        }
        //v2 drawing
        static void SDL_SetDrawingColor(IntPtr renderer, Color color)
        {
            SDL.SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
        }
        static void SDL_SetDrawingColor(IntPtr renderer, byte R, byte G, byte B, byte A)
        {
            SDL.SDL_SetRenderDrawColor(renderer, R, G, B, A);
        }
        static IntPtr SDL_LoadImage(IntPtr renderer, string path)
        {
            IntPtr img = SDL_image.IMG_Load(path);
            if (img == IntPtr.Zero)
            {
                throw new Exception($"Image at '{path}' failed loading: {SDL_image.IMG_GetError()}");
            }
            IntPtr surface = SDL_CreateTextureFromSurface(renderer, img);
            if (surface == IntPtr.Zero)
            {
                throw new Exception($"Can't create surface from given image: {SDL.SDL_GetError()}");
            }
            return surface;
        }
        static void DoSDLRendering(IntPtr window, int particleCount = 1000)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error on init: {SDL.SDL_GetError()}");
                throw new Exception("init error");
            }
            var renderer = SDL.SDL_CreateRenderer(SDL.SDL_CreateWindowFrom(window), -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error when creating renderer: {SDL.SDL_GetError()}");
                throw new ApplicationException("Renderer error");
            }
            if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
            {
                throw new ApplicationException($"Error on SDL2_img: {SDL_image.IMG_GetError()}");
            }

            bool running = true;
            // loading image
            List<IntPtr> images = new();
            foreach (String filepath in Directory.GetFiles($"./assets"))
            {
                images.Add(SDL_LoadImage(renderer, filepath));
            }
            SDL.SDL_QueryTexture(images[0], out _, out _, out int img_width, out int img_height);
            Vector2 imgSize = new(img_width, img_height);
            // particle generation
            Vector2 screenSize = new(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            List<Particle> particles = new();
            Random rnd = new();

            for (int i = 0; i < particleCount; i++)
            {
                Particle newp = new(new((float)rnd.NextDouble() * screenSize.X, (float)rnd.NextDouble() * screenSize.Y), (float)rnd.NextDouble() + .5f, (float)rnd.NextDouble() * 360)
                {
                    Velocity = new((float)rnd.Next(-2, 2), (float)rnd.NextDouble() * 5 + .1f),
                    AngularVelocity = (float)rnd.NextDouble(),
                    Surface = images[rnd.Next(images.Count)]
                };
                particles.Add(newp);
            }
            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event ev) == 1)
                {
                    switch (ev.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT: running = false; break;
                    }
                }
                SDL_SetDrawingColor(renderer, Color.DarkOliveGreen);
                SDL.SDL_RenderClear(renderer);
                // render cycle
                foreach (Particle particle in particles)
                {
                    SDL_SetDrawingColor(renderer, particle.Color);
                    SDL_Rect endPos = new()
                    {
                        x = CastToInt(particle.Position.X),
                        y = CastToInt(particle.Position.Y),
                        w = CastToInt(imgSize.X * particle.Size),
                        h = CastToInt(imgSize.Y * particle.Size)
                    };

                    if (particle.Surface == IntPtr.Zero)
                    {
                        throw new Exception("Image destroyed...");
                    }
                    if (SDL.SDL_SetTextureColorMod(particle.Surface, particle.Color.R, particle.Color.G, particle.Color.B) < 0)
                    {
                        throw new Exception($"color change didn't work: {SDL.SDL_GetError()}");
                    };
                    SDL.SDL_RenderCopyEx(renderer, particle.Surface, IntPtr.Zero, ref endPos, particle.Angle, IntPtr.Zero, 0);

                    particle.Position = new(
                        x: particle.Position.X + particle.Velocity.X,
                        y: particle.Position.Y + particle.Velocity.Y
                    );
                    if (particle.Angle + particle.AngularVelocity > 360)
                    {
                        particle.Angle = particle.AngularVelocity;
                    }
                    else
                    {
                        particle.Angle += particle.AngularVelocity;
                    }

                    if (particle.Position.X > screenSize.X + 65)
                    {
                        particle.Position = new(-64, particle.Position.Y);
                    }
                    else if (particle.Position.X < -65)
                    {
                        particle.Position = new(screenSize.X + 64, particle.Position.Y);
                    }
                    if (particle.Position.Y > screenSize.Y)
                    {
                        particle.Position = new(particle.Position.X, -64);
                    }
                }
                // display changes
                SDL.SDL_RenderPresent(renderer);
            }
        }
        static readonly int version = 2;
        //static void Main()
        //{
        //    // setup desktop graphics
        //    IntPtr progman = FindWindow("Progman", null);
        //    IntPtr result = IntPtr.Zero;
        //    SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);
        //    IntPtr workerw = IntPtr.Zero;
        //    EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
        //    {
        //        IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", "");
        //        if (p != IntPtr.Zero)
        //        {
        //            workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", "");
        //        }
        //        return true;
        //    }), IntPtr.Zero);
        //    IntPtr dc = GetDCEx(workerw, IntPtr.Zero, (DeviceContextValues)0x403);

        //    // do animations
        //    if (dc != IntPtr.Zero)
        //    {
        //        if (version == 1)
        //        {
        //            using (Graphics g = Graphics.FromHdc(dc))
        //            {
        //                DoDrawingLeafs(g);
        //            }
        //        }
        //        else if (version == 2)
        //        {
        //            DoSDLRendering(workerw);
        //        }
        //        ReleaseDC(workerw, dc);
        //    }
        //}
    }
}
