using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

public class UdpFileServer
{
    // Информация о файле (требуется для получателя)
    public class FileDetails
    {
        public string fileType = "";
        public long filSize = 0;
    }

    private static FileDetails fileInfo = new FileDetails();

    // Поля, связанные с UdpClient
    private static IPAddress IPAddres;
    private const int portNum = 5002;
    private static UdpClient sender = new UdpClient();
    private static IPEndPoint IPEndPoint = null;

    // Filestream object
    private static FileStream fstream;

    static void Main(string[] args)
    {
        try
        {
            // Получаем удаленный IP-адрес и создаем IPEndPoint
            Console.WriteLine("Enter IP-address: ");
            IPAddres = IPAddress.Parse(Console.ReadLine().ToString());//"127.0.0.1");
            IPEndPoint = new IPEndPoint(IPAddres, portNum);

            // Получаем путь файла и его размер (должен быть меньше 8kb)
            Console.WriteLine("Enter path to file and it`s name: ");
            fstream = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

            if (fstream.Length > 8192)
            {
                Console.Write("File size must be less the 8kB");
                sender.Close();
                fstream.Close();
                return;
            }

            // Отправляем информацию о файле
            SendFileInfo();

            // Ждем 2 секунды
            Thread.Sleep(2000);

            // Отправляем сам файл
            SendFile();

            Console.ReadLine();

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void SendFileInfo()
    {

        // Получаем тип и расширение файла
        fileInfo.fileType = fstream.Name.Substring((int)fstream.Name.Length - 3, 3);

        // Получаем длину файла
        fileInfo.filSize = fstream.Length;

        XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
        MemoryStream stream = new MemoryStream();

        // Сериализуем объект
        fileSerializer.Serialize(stream, fileInfo);

        // Считываем поток в байты
        stream.Position = 0;
        Byte[] bytes = new Byte[stream.Length];
        stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

        Console.WriteLine("Sending file information...");

        // Отправляем информацию о файле
        sender.Send(bytes, bytes.Length, IPEndPoint);
        stream.Close();

    }
    private static void SendFile()
    {
        // Создаем файловый поток и переводим его в байты
        Byte[] bytes = new Byte[fstream.Length];
        fstream.Read(bytes, 0, bytes.Length);

        Console.WriteLine("Sending file with size: " + fstream.Length + " bytes");
        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, IPEndPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            // Закрываем соединение и очищаем поток
            fstream.Close();
            sender.Close();
        }
        Console.WriteLine("File succesfully sending.");
        Console.Read();
    }
}