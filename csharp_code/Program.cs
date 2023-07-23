using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDL2.SDL;

namespace desktopHack
{
    class Program
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

        //particle setup
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
            public Vector2 MaxSize { get; set; } = new(-1,-1);
            public List<bool> UseRandomForColors { get; set; } = new List<bool>(){false, false, false};  
            public Vector2[] RandomValuesForColors { get; set; } = { new(0, 255), new(0, 255), new(0, 255) };
            public int ParticleCount { get; set; } = 200;
        }
        static int CastToInt(float number)
        {
            return (int)number;
        }

        //v2 drawing
        static void SDL_SetDrawingColor(IntPtr renderer, Color color)
        {
            SDL_SetRenderDrawColor(renderer, color.R, color.G, color.B, color.A);
        }
        static void SDL_SetDrawingColor(IntPtr renderer, byte R, byte G, byte B, byte A)
        {
            SDL_SetRenderDrawColor(renderer, R, G, B, A);
        }
        static IntPtr? SDL_LoadImage(IntPtr renderer, string path)
        {
            if(path.Contains(".png")) {
                IntPtr img = SDL_image.IMG_Load(path);
                if (img == IntPtr.Zero)
                {
                    throw new Exception($"Image at '{path}' failed loading: {SDL_image.IMG_GetError()}");
                }
                IntPtr surface = SDL_CreateTextureFromSurface(renderer, img);
                if (surface == IntPtr.Zero)
                {
                    throw new Exception($"Can't create surface from given image: {SDL_GetError()}");
                }
                return surface;
            } else
            {
                return null;
            }
            
        }
        static List<IntPtr> LoadImagesFromAssets(IntPtr renderer)
        {
            List<IntPtr> images = new List<IntPtr>();
#if DEBUG
            foreach (String filepath in Directory.GetFiles("../../../assets"))
            {
                var img = SDL_LoadImage(renderer, filepath);
                if(img != null)
                {
                    images.Add((IntPtr)img);
                }
            }
#else
            foreach (String filepath in Directory.GetFiles("../assets"))
            {
                var img = SDL_LoadImage(renderer, filepath);
                if(img != null)
                {
                    images.Add((IntPtr)img);
                }
            }
#endif
            return images;
        }
        static void DoSDLRendering(IntPtr window, ParticleSettings pSettings)
        {
            if(SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error on init: {SDL_GetError()}");
                throw new Exception("init error");
            }
            var renderer = SDL_CreateRenderer(SDL_CreateWindowFrom(window), -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error when creating renderer: {SDL_GetError()}");
                throw new ApplicationException("Renderer error");
            }
            if(SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
            {
                throw new ApplicationException($"Error on SDL2_img: {SDL_image.IMG_GetError()}");
            }

            bool running = true;

            // loading images
            List<IntPtr> images = LoadImagesFromAssets(renderer);

            // particle generation
            Vector2 screenSize = new(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            List<Particle> particles = new();
            Random rnd = new();
            
            for (int i = 0; i < pSettings.ParticleCount; i++) {
                Particle newp = new(
                    pos: new((float)rnd.NextDouble() * screenSize.X, (float)rnd.NextDouble() * screenSize.Y),
                    size: rnd.Next((int)pSettings.LimitSizeMultiplyer.X, (int)pSettings.LimitSizeMultiplyer.Y)/100f,
                    angle: (float)rnd.NextDouble() * 360
                ) {
                    Velocity = new(
                        x: rnd.Next((int)pSettings.LimitVelocityX.X, (int)pSettings.LimitVelocityX.Y),
                        y: rnd.Next((int)pSettings.LimitVelocityY.X, (int)pSettings.LimitVelocityY.Y)
                    ),
                    AngularVelocity = rnd.Next((int)pSettings.LimitAngularVelocity.X, (int)pSettings.LimitAngularVelocity.Y),
                    Surface = images[rnd.Next(images.Count)],
                };
                // color for particle according to settings
                byte[] particleColor = {0,0,0};
                // Red
                if (pSettings.UseRandomForColors[0])
                {
                    particleColor[0] = (byte)rnd.Next((int)pSettings.RandomValuesForColors[0].X, (int)pSettings.RandomValuesForColors[0].Y);
                } else
                {
                    particleColor[0] = pSettings.BaseColor.R;
                }
                // Green
                if (pSettings.UseRandomForColors[1]) {
                    particleColor[1] = (byte)rnd.Next((int)pSettings.RandomValuesForColors[1].X, (int)pSettings.RandomValuesForColors[1].Y);
                } else
                {
                    particleColor[1] = pSettings.BaseColor.G;
                }
                // Blue
                if (pSettings.UseRandomForColors[2]) {
                    particleColor[2] = (byte)rnd.Next((int)pSettings.RandomValuesForColors[2].X, (int)pSettings.RandomValuesForColors[2].Y);
                } else
                {
                    particleColor[2] = pSettings.BaseColor.B;
                }
                newp.Color = Color.FromArgb(255, particleColor[0], particleColor[1], particleColor[2]);
                particles.Add(newp);
            }

            while (running)
            {
                while (SDL_PollEvent(out SDL.SDL_Event ev) == 1)
                {
                    switch (ev.type)
                    {
                        case SDL_EventType.SDL_QUIT: running = false; break;
                    }
                }
                SDL_SetDrawingColor(renderer, pSettings.BackgroundColor);
                SDL_RenderClear(renderer);

                // render cycle
                foreach(Particle particle in particles) {
                    SDL_QueryTexture(particle.Surface, out _, out _, out int img_width, out int img_height);
                    Vector2 imgSize = new(img_width, img_height);
                    SDL_SetDrawingColor(renderer, particle.Color);
                    SDL_Rect endPos = new()
                    {
                        x = CastToInt(particle.Position.X),
                        y = CastToInt(particle.Position.Y),
                        w = CastToInt(imgSize.X * particle.Size),
                        h = CastToInt(imgSize.Y * particle.Size)
                    };
                    if(pSettings.MaxSize.X > 0 || pSettings.MaxSize.Y > 0)
                    {
                        endPos.w = CastToInt(pSettings.MaxSize.X * particle.Size);
                        endPos.h = CastToInt(pSettings.MaxSize.Y * particle.Size);
                    }

                    if(particle.Surface == IntPtr.Zero)
                    {
                        throw new Exception("Image destroyed...");
                    }
                    if (SDL_SetTextureColorMod(particle.Surface, particle.Color.R, particle.Color.G, particle.Color.B) < 0)
                    {
                        throw new Exception($"color change didn't work: {SDL_GetError()}");
                    };
                    SDL_RenderCopyEx(renderer, particle.Surface, IntPtr.Zero, ref endPos, particle.Angle, IntPtr.Zero, 0);
                    
                    particle.Position = new(
                        x: particle.Position.X + particle.Velocity.X,
                        y: particle.Position.Y + particle.Velocity.Y
                    );
                    if(particle.Angle + particle.AngularVelocity > 360)
                    {
                        particle.Angle = particle.AngularVelocity;
                    } else
                    {
                        particle.Angle += particle.AngularVelocity;
                    }

                    if(particle.Position.X > screenSize.X+imgSize.X+2)
                    {
                        particle.Position = new(-imgSize.X, particle.Position.Y);
                    } else if(particle.Position.X < -imgSize.X-2) {
                        particle.Position = new(screenSize.X+imgSize.X, particle.Position.Y);
                    }
                    if(particle.Position.Y > screenSize.Y+imgSize.Y+2)
                    {
                        particle.Position = new(particle.Position.X, -64);
                    } else if(particle.Position.Y < -imgSize.Y-2)
                    {
                        particle.Position = new(particle.Position.X, screenSize.Y + imgSize.Y);
                    }
                }

                // display changes
                SDL_RenderPresent(renderer);
            }
        }

        // config parsing
        static Vector2 ParseArgsAsVec2(List<string> args)
        {
            bool parsedX = float.TryParse(args[0], out float X);
            bool parsedY = float.TryParse(args[1], out float Y);
            if(!parsedX || !parsedY)
            {
                throw new Exception($"Vec2 parse error: X: {parsedX} Y: {parsedY}");
            }
            return new(X, Y);
        }
        static Vector3 ParseArgsAsVec3(List<string> args) {
            Vector2 res = ParseArgsAsVec2(args);
            bool parsedZ = float.TryParse(args[2], out float Z);
            if(!parsedZ)
            {
                throw new Exception($"Vec3 parse error: Z not parsed");
            }
            return new(res.X, res.Y, Z);
        }
        static Color ParseArgsAsColor(List<string> args)
        {
            Vector3 rawColor = ParseArgsAsVec3(args);
            return Color.FromArgb(255, (int)rawColor.X, (int)rawColor.Y, (int)rawColor.Z);
        }
        static List<bool> ParseArgsAsBools(List<string> args)
        {
            List<bool> bools = new();
            foreach(string arg in args)
            {
                if(arg == "1")
                {
                    bools.Add(true);
                } else
                {
                    bools.Add(false);
                }
            }
            return bools;
        }
        static ParticleSettings LoadParticleSettings()
        {
            var settings = new ParticleSettings();
#if DEBUG
            StreamReader stream = new("./../../../assets/config.txt");
#else
            StreamReader stream = new("../assets/config.txt");
#endif
            string line = stream.ReadLine();
            while(line != null)
            {
                List<string> args = line.Split(' ').ToList();
                if(args.Count > 0)
                {
                    string command = args[0];
                    args.Remove(command);
                    switch(command)
                    {
                        case "xvel":
                            settings.LimitVelocityX = ParseArgsAsVec2(args);
                            break;
                        case "yvel":
                            settings.LimitVelocityY = ParseArgsAsVec2(args);
                            break;
                        case "angvel":
                            settings.LimitAngularVelocity = ParseArgsAsVec2(args);
                            break;
                        case "sizemult":
                            settings.LimitSizeMultiplyer = ParseArgsAsVec2(args);
                            break;
                        case "bgcolor":
                            settings.BackgroundColor = ParseArgsAsColor(args);
                            break;
                        case "defaultcolor":
                            settings.BaseColor = ParseArgsAsColor(args);
                            break;
                        case "userandom":
                            settings.UseRandomForColors = ParseArgsAsBools(args);
                            break;
                        case "rrandom":
                            settings.RandomValuesForColors[0] = ParseArgsAsVec2(args);
                            break;
                        case "grandom":
                            settings.RandomValuesForColors[1] = ParseArgsAsVec2(args);
                            break;
                        case "brandom":
                            settings.RandomValuesForColors[2] = ParseArgsAsVec2(args);
                            break;
                        case "pcount":
                            settings.ParticleCount = int.Parse(args[0]);
                            break;
                        case "maxrectres":
                            settings.MaxSize = ParseArgsAsVec2(args);
                            break;
                        default: break;
                    }
                }
                line = stream.ReadLine();
            }
            stream.Dispose();
            return settings;
        }
        static void Main()
        {
            // setup desktop graphics
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);
            IntPtr workerw = IntPtr.Zero;
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", "");
                if (p != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", "");
                }
                return true;
            }), IntPtr.Zero);
            IntPtr dc = GetDCEx(workerw, IntPtr.Zero, (DeviceContextValues)0x403);

            // do animations
            if (dc != IntPtr.Zero)
            {
                ParticleSettings ps = LoadParticleSettings();
                Console.WriteLine(ps.ToString());
                DoSDLRendering(workerw, ps);
                ReleaseDC(workerw, dc);
            }
        }
    }
}
