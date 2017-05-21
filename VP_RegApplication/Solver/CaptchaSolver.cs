using System;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using DeathByCaptcha;

namespace VP_RegApplication.Solver
{
    /// <summary>
    /// Captcha solver. Uses DeathByCaptcha
    /// </summary>
    public class CaptchaSolver
    {
        private Client _client;

        /// <summary>
        /// Constructor
        /// </summary>
        public CaptchaSolver()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes connection. Gets user credentials.
        /// </summary>
        private void Initialize()
        {
            //logger.Info("Read Credentials");
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "Solver\\Credentials.txt")))
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] data = line.Split(':');
                        var username = data[0];
                        var password = data[1];
                        _client = (Client)new SocketClient(username, password);
                    }
                    reader.Close();
                }
            }
            catch(FileNotFoundException)
            {
                //logger.Error(e);
                MessageBox.Show("Could not read credentials from the file");
            }
        }

        /// <summary>
        /// Gets captcha solved from image. Be careful, takes time.
        /// </summary>
        /// <param name="img"></param>
        /// <returns>Solved captcha</returns>
        public Captcha GetCaptcha(Bitmap img)
        {
            using (var msCaptcha = new MemoryStream())
            {
                img.Save(msCaptcha, ImageFormat.Png);

                Captcha captcha = _client.Decode(msCaptcha, 17);
                return captcha;
            }
        }

        /// <summary>
        /// Report if captcha was solved incorrectly
        /// </summary>
        /// <param name="captcha"></param>
        public void Report(Captcha captcha)
        {
            _client.Report(captcha);
        }
    }
}
