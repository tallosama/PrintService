
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading; 

namespace PrintService
{
    [RunInstaller(true)]

    public partial class Service1 : ServiceBase
    {
        private HttpListener listener;
        private string apiLocal = "http://127.0.0.1:51199/impresion/";
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(apiLocal);
            listener.Start();
            listener.BeginGetContext(ProcessRequest, listener);
        }

        protected override void OnStop()
        {
            listener?.Stop();
        }
        private void ProcessRequest(IAsyncResult result)
        {
            if (!listener.IsListening) return;

            HttpListenerContext context = listener.EndGetContext(result);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var data = ReadDataFromRequest(context.Request);
                string resultado = printService(data);

                byte[] buffer = Encoding.UTF8.GetBytes("{\"resultado\": \"" + resultado + "\"}");
                context.Response.ContentType = "application/json";
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                listener.BeginGetContext(ProcessRequest, listener);
            });
        }
        private string ReadDataFromRequest(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }


        private string printService(string json)
        {
            try
            {
                //////CON ERROR////string rutaArchivo = "C:\\Users\\" + Environment.UserName + "\\Desktop\\printer" + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
                //string rutaArchivo = "C:\\printerService\\ImpresionDone " + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
                //using (System.IO.StreamWriter writer = new StreamWriter(rutaArchivo))
                //{
                //    writer.WriteLine("Impresión done!: ");
                //    writer.WriteLine("xD ");
                //    writer.WriteLine("Mensaje: " + Environment.UserName + " " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);

                //}





                ////////////////ver
                //PrintDocument printDocument = new PrintDocument();
                //printDocument.PrintPage += new PrintPageEventHandler(ImprimirContenido);

                //PrintDialog printDialog = new PrintDialog();
                //printDialog.Document = printDocument;

                //if (printDialog.ShowDialog() == DialogResult.OK)
                //{
                //    printDocument.Print();
                //}

                ////////¨PENDIENTE//////////
                //PrintDocument printDocument = new PrintDocument();
                //printDocument.PrintPage += new PrintPageEventHandler(ImprimirContenido);
                //PrintController printController = new StandardPrintController();
                //printDocument.PrintController = printController;
                // Configura el nombre de la impresora, si lo deseas
                //printDocument.PrinterSettings.PrinterName = "NombreDeTuImpresora";
                // printDocument.Print();

                //    var parsedData = Newtonsoft.Json.Linq.JObject.Parse( json);
                dynamic parsedData = JsonConvert.DeserializeObject<dynamic>(json);
                
                if (parsedData != null)
                {
                    
                    PrintDocument printDocument = new PrintDocument();
                 
                    printDocument.PrintPage += new PrintPageEventHandler(ImprimirContenido);

                    // Configuración
                    printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 300, 1000);
                    printDocument.DefaultPageSettings.PaperSource = printDocument.PrinterSettings.PaperSources[0];
                    printDocument.DefaultPageSettings.Landscape = false;

                    PrintController printController = new StandardPrintController();
                    printDocument.PrintController = printController;
                  
                    printDocument.PrinterSettings.PrinterName = parsedData.nombreImpresora;
                 
                    printDocument.Print();
                }
                 
                //////////////TEST DE DOCUMENTO
                //var parsedData = JObject.Parse(data);
                //if (parsedData["JsonData"]!=null)
                //{

                //    string rutaArchivo = "C:\\printerService\\ImpresionDone " + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
                //    using (System.IO.StreamWriter writer = new StreamWriter(rutaArchivo))
                //    {
                //        writer.WriteLine("Impresión done!: ");
                //        writer.WriteLine("xDDDDDDD "+ parsedData["valorFront"]);
                //        writer.WriteLine("Mensaje: " + parsedData["JsonData"]);

                //    }

                //}
                return "true";
            }
            catch (Exception ex)
            {
                string rutaArchivo = "C:\\printerService\\Logs\\ErrorDeImpresion " + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
                using (System.IO.StreamWriter writer = new StreamWriter(rutaArchivo))
                {
                    writer.WriteLine("Exepcion: " + ex.GetType().ToString());
                    writer.WriteLine("Cadena de Exepcion: " + ex.ToString());
                    writer.WriteLine("Mensaje: " + ex.Message);

                }
                return "Ocurrió un error: " + ex.GetType() != null ? ex.GetType().ToString() : ex.Message;
            }

        }
        private static void toLog(object param, string num)
        {
            string rutaArchivo = "C:\\printerService\\Feed\\Log " + num + " " + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
            using (System.IO.StreamWriter writer = new StreamWriter(rutaArchivo))
            {

                writer.WriteLine(  param);

            }
        }
        private static void ImprimirContenido(object sender, PrintPageEventArgs e)
        {
            string contenido = "¡TEST DE IMPRESION!"; 
            using (Font font = new Font("Arial", 12))
            { 
                e.Graphics.DrawString(contenido, font, Brushes.Black, new PointF(100, 100));
            } 
        }
    }
}

