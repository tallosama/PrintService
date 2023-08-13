using Newtonsoft.Json.Linq;
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
using System.Windows.Forms; 

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
                string resultado = ProcessRequest(data);
         
                byte[] buffer = Encoding.UTF8.GetBytes("{\"resultado\": \""+ resultado + "\"}");
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
    

        private string ProcessRequest(string data)
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


                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += new PrintPageEventHandler(ImprimirContenido);

                PrintController printController = new StandardPrintController();
                printDocument.PrintController = printController;

                // Configura el nombre de la impresora, si lo deseas
                //printDocument.PrinterSettings.PrinterName = "NombreDeTuImpresora";

                printDocument.Print();



                //var parsedData = JObject.Parse(data);
                //if (parsedData["nombre"].ToString() == "Ejemplo")

                //{

                //}
                return "true";
            }
            catch (Exception ex)
            {
                string rutaArchivo = "C:\\printerService\\Logs\\ErrorDeImpresion " + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss", CultureInfo.InvariantCulture) + ".txt";
                using (System.IO.StreamWriter writer = new StreamWriter(rutaArchivo))
                {
                    writer.WriteLine("Exepcion: "+ex.GetType().ToString());
                    writer.WriteLine("Cadena de Exepcion: " + ex.ToString());
                    writer.WriteLine("Mensaje: " + ex.Message);
                                        
                }
                return "Ocurrió un error: "+ex.GetType().ToString();
            }

        }
        private static void ImprimirContenido(object sender, PrintPageEventArgs e)
        {
            string contenido = "¡Hola, esta es una impresión de prueba desde una consola!";

            using (Font font = new Font("Arial", 12))
            {
                e.Graphics.DrawString(contenido, font, Brushes.Black, new PointF(100, 100));
            }
        }
    }
}
