//using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using System.Windows.Forms;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PSP05TE01
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            checkBox1.CheckedChanged += CheckBox_CheckedChanged;
            checkBox2.CheckedChanged += CheckBox_CheckedChanged;
            checkBox3.CheckedChanged += CheckBox_CheckedChanged;
        }

        /***
         * Metodo del boton Registrar
         * Comprueba que el usuario existe
         * Actualiza los datos del usuario en el programa
         * **/
        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            checkBox3.Enabled = false;

            string ficheroUsuario = "bbdd\\" + textBox1.Text + ".txt";

            if (File.Exists(ficheroUsuario))
            {
                MessageBox.Show("El usuario existe");
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;

                cargarUsuario(ficheroUsuario);
            }
            else
            {
                MessageBox.Show("El usuario no existe");
                groupBox1.Enabled = true;
            }
        }

        /****
         * Metodo del boton Aceptar
         * Comprueba que el nombe del usuario es valido
         * Crea el fichero del usuario(txt)
         * Genera las claves publica y privada
         * Manda la clave privada por email
         * **/
        private void button2_Click(object sender, EventArgs e)
        {
            string nombreUsuario = textBox1.Text;
            string patron = "^[a-z]{4,10}$";

            if (radioButton1.Checked)
            {
                if (Regex.IsMatch(nombreUsuario, patron))
                {
                    FileStream fs = File.Create("bbdd\\" + nombreUsuario + ".txt");
                    fs.Close();
                    generarClaves(nombreUsuario);
                    MessageBox.Show("Ficheros con claves creados");
                    mandaEmail("privatekeys\\" + nombreUsuario + "_private.xml");

                }
                else
                {
                    MessageBox.Show("El nombre debe cumplir estas condiciones:\r\n" +
                        "Todo en minusculas y letras exclucivamente.\r\n" +
                        "Maximo 10 caracteres\r\n" +
                        "Minimo 4 caracteres.");
                }
            }
        }

        /****
         * Metodo del boton Fichero
         * Abre una ventana para seleccionar un fichero
         * Desencripta la contrase�a y la muestra en pantalla
         * **/
        private void button3_Click(object sender, EventArgs e)
        {
            // Configurar el OpenFileDialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Seleccionar archivo";
                openFileDialog.Filter = "Todos los archivos (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                // Permitir seleccionar un solo archivo
                openFileDialog.Multiselect = false;

                // Mostrar el di�logo y obtener el resultado
                DialogResult result = openFileDialog.ShowDialog();

                // Si el usuario hizo clic en "Aceptar", obtener la ruta del archivo seleccionado
                if (result == DialogResult.OK)
                {
                    string rutaArchivo = openFileDialog.FileName;

                    label8.Text = rutaArchivo;
                }
            }

            // Buscamos la contrase�a en el fichero del usuario y devolvemos su byte[]
            byte[] contrase�aByte = buscarContrase�a(comboBox1.Text);
            // Desencriptamos la contrase�a
            byte[] contase�aDesencriptada = desencriptar(label8.Text, contrase�aByte);
            // Pasamos la contase�a desencriptada a string para imprimirla
            string contrase�aDesencriptada = Encoding.UTF8.GetString(contase�aDesencriptada);

            textBox4.Text = contrase�aDesencriptada;
        }

        /***
         * Metodo del boton Guardar
         * Comprueba que la contrase�a es valida
         * Encripta la contrase�a y la escribe en el fichero
         * **/
        private void button4_Click(object sender, EventArgs e)
        {

            /**
             * Longitud de la contrase�a etre 8 y 10 caracteres
             * Tiene que contener minimo 1 mayuscula, 1 minuscula y un numero (todos obligatorios)
             * Tiene que contener un caracter de estos: !@#&()�[{}:',?/*~$^+=<>
             * **/
            string patron = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#&()�\[{}\]:',?/*~$^+=<>]).{8,10}$";

            if (Regex.IsMatch(textBox6.Text, patron))
            {
                MessageBox.Show("Contrase�a correcta");
                string descripcion = textBox5.Text;
                string contrase�a = textBox6.Text;

                // Encriptamos la contrase�a
                byte[] contrase�aEncriptada = encriptar("publickeys\\" + textBox1.Text + "_public.xml", contrase�a);

                escribirFichero("bbdd\\" + textBox1.Text + ".txt", descripcion, contrase�aEncriptada);
            }
            else
            {
                MessageBox.Show("La contrase�a no cumple con las siguientes condiciones:\r\n" +
                    "8 - 10 digitos\r\n" +
                    "Al menos 1 mayuscula, 1 minuscula y 1 numero\r\n" +
                    "1 caracter entre estos: !@#&()�[{}:',?/*~$^+=<>");
            }
            //Actualiza datos del usuario
            cargarUsuario("bbdd\\" + textBox1.Text + ".txt");
        }

        /****
         * Metodo del boton Borrar
         * Busca la contrase�a en el fichero
         * Borra la contrase�a y descripcion del fichero
         * **/
        private void button5_Click(object sender, EventArgs e)
        {
            string nombreFichero = "bbdd\\" + textBox1.Text + ".txt";

            List<Password> listaPassword = leerFichero(nombreFichero);

            int indice = 0;

            for (int i = 0; i < listaPassword.Count; i++)
            {
                if (listaPassword[i].descripcion == comboBox2.Text)
                {
                    indice = i;
                }
            }
            listaPassword.RemoveAt(indice);

            //Actualiza fichero
            using (StreamWriter sw = new StreamWriter(nombreFichero, false))
            {
                // Escribe una cadena vac�a en el archivo
                sw.Write(string.Empty);
            }

            foreach (Password password in listaPassword)
            {
                escribirFichero(nombreFichero, password.descripcion, password.contrase�a);
            }

            // Actualiza datos del usuario
            cargarUsuario(nombreFichero);

            MessageBox.Show("Contrase�a borrada correctamente");
        }

        /***
         * Cierra la aplicacion
         * **/
        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /****
         * Metodo que genera la clave publica y privada
         * Crea un archivo por cada una de ellas y guarda la clave en el.
         * @param string nomnbreusuario con el nombre del usuario qu quiere crear las claves
         * **/
        private void generarClaves(string nombreUsuario)
        {
            string publicFich = "publickeys\\" + nombreUsuario + "_public.xml";
            string privateFich = "privatekeys\\" + nombreUsuario + "_private.xml";

            // Creamos un RSACryptoServiceProvider para usar la clave publica y privada
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // Tenemos que indicar que NO tenemos proveedor de servicios criptograficos
                rsa.PersistKeyInCsp = false;

                //Si existe un fichero con ese nombre lo borramos
                if (File.Exists(publicFich))
                {
                    File.Delete(publicFich);
                }
                if (File.Exists(privateFich))
                {
                    File.Delete(privateFich);
                }

                // Creamos la clave publica. Para que sea publica hay que pasarle false
                string publicKey = rsa.ToXmlString(false);
                //Creamos el fichero y guardamos la clave publica en el
                File.WriteAllText(publicFich, publicKey);

                // Creamos la clave privada. Para la privada hay que pasarle true
                string privateKey = rsa.ToXmlString(true);
                // Creamos el fichero y guardamos la clvae privada en el
                File.WriteAllText(privateFich, privateKey);
            }
        }

        /******
         * Metodo que envia un email
         * **/
        private void mandaEmail(string ficheroEmail)
        {
            // Generamos el email
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress("Arkaitz Gonzalez", "argobakoprueba@gmail.com"));

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("El campo email esta vacio");
            }
            else
            {
                mensaje.To.Add(new MailboxAddress("Profesor", textBox2.Text));
            }

            mensaje.Subject = "Clave Privada";

            var body = new TextPart("plain")
            {
                Text = "Clave de acceso a contrase�as en el gestor de password."
            };

            // Adjuntar archivo XML
            var attachment = new MimePart("application", "xml")
            {
                Content = new MimeContent(System.IO.File.OpenRead(ficheroEmail)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = ficheroEmail
            };

            // Crear un contenedor multipart para el cuerpo del mensaje y el archivo adjunto
            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);

            // Asignar el contenido multipart al cuerpo del mensaje
            mensaje.Body = multipart;

            // Mandamos el email
            using (var cliente = new SmtpClient())
            {
                cliente.ServerCertificateValidationCallback = (s, c, h, e) => true;

                cliente.Connect("smtp.gmail.com", 587, false);

                cliente.Authenticate("argobakoprueba@gmail.com", "pbqxmwjejhuhzrww");

                try
                {
                    cliente.Send(mensaje);
                    MessageBox.Show("Email enviado");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show("Error en el envio del email");
                }

                cliente.Disconnect(true);
            }
        }

        /***
         * Metodo que verifica si los checbox estan seleccionados
         * **/
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox == checkBox1)
            {
                groupBox2.Enabled = checkBox.Checked;
            }
            else if (checkBox == checkBox2)
            {
                groupBox3.Enabled = checkBox.Checked;
            }
            else if (checkBox == checkBox3)
            {
                groupBox4.Enabled = checkBox.Checked;
            }
        }



        /***
         * Metodo que encripta una cadena de bytes
         * (A la clave publica hay que pasarle un array de bytes)
         * @param 
         *      publicKey, fichero con la clave publica
         *      textoPlano, con array de bytes a encriptar
         * @return
         *      byte[] con la contrase�a cifrada 
         * **/
        public static byte[] encriptar(string publicFich, string contrase�a)
        {
            byte[] encriptado;
            //Pasamos la contrase�a a bytes para encriptarla
            byte[] contrase�aByte = Encoding.UTF8.GetBytes(contrase�a);

            //Se crea un objeto de tipo RSACryptoServiceProvider para poder hacer uso de sus m�todos de encriptaci�n
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                //Le indicamos el valor a false porque no queremos que est� en ning�n proveedor de servicios.
                rsa.PersistKeyInCsp = false;

                //Lee el contenido del fichero y lo guarda en un string
                string publicKey = File.ReadAllText(publicFich);

                //FromXmlString(publicKey): Inicializa un objeto RSA de la informaci�n de clave de una cadena XML.
                rsa.FromXmlString(publicKey);

                //Cifra los datos con el algoritmo RSA.
                //@textoPlano: datos que se van a cifrar
                //@Booleano: true para realizar el cifrado RSA directo mediante el relleno de OAEP (solo disponible en equipos con Windows XP o versiones posteriores como en nuestro caso); de lo contrario, false para usar el relleno PKCS#1 v1.5.
                encriptado = rsa.Encrypt(contrase�aByte, true);
            }

            //Valor que se devuelve
            return encriptado;
        }

        /****
         * Metodo que desencripta una contrase�a
         * @param
         *      privateFich, con el fichero que contiene la clave privada
         *      textoEncriptado, con la contrase�a que se quiere desencriptar
         * @return
         *      byte[] con la contrase�a desencriptada
         * **/
        public static byte[] desencriptar(string privateFich, byte[] textoEncriptado)
        {

            byte[] desencriptado;
            //Se crea un objeto de tipo RSACryptoServiceProvider para poder hacer uso de sus m�todos.
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                //Le indicamos el valor a false porque no queremos que est� en ning�n proveedor de servicios.
                rsa.PersistKeyInCsp = false;


                //Lee el contenido del fichero y lo guarda en un string
                string privateKey = File.ReadAllText(privateFich);

                //FromXmlString(false): Inicializa un objeto RSA de la informaci�n de clave de una cadena XML.
                //En este caso clave privada ya que la utilizaremos para descifrar
                rsa.FromXmlString(privateKey);

                //Descifra los datos que se cifraron anteriormente.IMPORTANTE, descifra el texto que hemos cifrado
                //@textoEncriptado: Datos que se van a descifrar.
                //@Booleano: true para realizar el cifrado RSA directo mediante el relleno de OAEP (solo disponible en equipos con Windows XP o versiones posteriores como en nuestro caso); de lo contrario, false 
                desencriptado = rsa.Decrypt(textoEncriptado, true);

            }
            return (desencriptado);

        }

        /***
         * Metodo que escribe la descripcion y la contrase�a en un fichero
         * @param
         *      fichero, con el fichero en el que se quiere escribir
         *      descripcion, con la descripcion a escribir
         *      contrase�a, con la contrase�a cifrada que se quiere escibir
         * **/
        private void escribirFichero(string fichero, string descripcion, byte[] contrase�a)
        {
            string contrase�aEncriptada = Convert.ToBase64String(contrase�a);
            // Abre el archivo para escritura en formato binario
            using (StreamWriter sw = File.AppendText(fichero))
            {
                sw.WriteLine(descripcion);
                sw.WriteLine(contrase�aEncriptada);
            }
        }

        /****
         * Lee el fichero
         * @param
         *      fichero, con el fcihero a leer
         * @return
         *      List<Password>, con una lista de contrase�as
         * **/
        private List<Password> leerFichero(string fichero)
        {
            List<Password> listaPassword = new List<Password>();

            string descripcion = "";
            byte[] contrase�a = null;

            try
            {
                using (StreamReader sr = new StreamReader(fichero))
                {
                    string linea;
                    bool esDescripcion = true;

                    // Leer el archivo l�nea por l�nea
                    while (!sr.EndOfStream)
                    {
                        linea = sr.ReadLine();
                        if (esDescripcion)
                        {
                            descripcion = linea;
                        }
                        else
                        {
                            contrase�a = Convert.FromBase64String(linea);
                            // Crear el objeto Datos solo si tenemos descripci�n y contrase�a
                            listaPassword.Add(new Password(descripcion, contrase�a));
                        }
                        // Cambiar entre descripci�n y contrase�a
                        esDescripcion = !esDescripcion;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return listaPassword;
        }


        /****
         * Busca una contrase�a dentro de un fichero
         * @param
         *      descripcion, con la descripcion de la que quiere saber la contrase�a
         * @return
         *      byte[], con la contrase�a que se buscaba
         * **/
        private byte[] buscarContrase�a(string descripcion)
        {
            List<Password> listaPassword = leerFichero("bbdd\\" + textBox1.Text + ".txt");

            foreach (Password password in listaPassword)
            {
                if (descripcion == password.descripcion)
                {
                    return password.contrase�a;
                }
            }
            return null;
        }

        /****
         * Actualiza los datos del usuario en el programa
         * **/
        private void cargarUsuario(string nombreUsuario)
        {
            string nombreFichero = nombreUsuario;
            List<Password> listaPassword = leerFichero(nombreFichero);

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            foreach (Password password in listaPassword)
            {
                comboBox1.Items.Add(password.descripcion);
                comboBox2.Items.Add(password.descripcion);
            }
        }

        /*****
         * Clase Password, para manejar mejor las contrase�as y descripciones
         * **/
        public class Password
        {
            public string descripcion { get; set; }
            public byte[] contrase�a { get; set; }

            // Constructor
            public Password(string descripcion, byte[] contrase�a)
            {
                this.descripcion = descripcion;
                this.contrase�a = contrase�a;
            }
        }
    }
}
