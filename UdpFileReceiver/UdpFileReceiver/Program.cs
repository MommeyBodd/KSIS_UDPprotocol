using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

public class UdpFileClient
{
    // Детали файла
    public class FileDetails
    {
        public string fileType = "";
        public long fileSize = 0;
    }

    private static FileDetails fileInfo;

    // Поля, связанные с UdpClient
    private static int portNum = 5002;
    private static UdpClient Client = new UdpClient(portNum);
    private static IPEndPoint IPEndPoint = null;

    private static FileStream fstream;
    private static Byte[] receivingBuffer = new Byte[0];

    static void Main(string[] args)
    {
        // Получаем информацию о файле
        GetFileDetails();

        // Получаем файл
        ReceiveFile();
    }
    private static void GetFileDetails()
    {
        try
        {
            Console.WriteLine("Waiting for file information...");

            // Получаем информацию о файле
            receivingBuffer = Client.Receive(ref IPEndPoint);
            Console.WriteLine("File information was taking...");

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // Считываем информацию о файле
            stream.Write(receivingBuffer, 0, receivingBuffer.Length);
            stream.Position = 0;

            // Вызываем метод Deserialize
            fileInfo = (FileDetails)fileSerializer.Deserialize(stream);
            Console.WriteLine("Take file with type ." + fileInfo.fileType +
                " with size: " + fileInfo.fileSize.ToString() + " bytes");
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void ReceiveFile()
    {
        try
        {
            Console.WriteLine("Waiting for file...");

            // Получаем файл
            receivingBuffer = Client.Receive(ref IPEndPoint);

            // Преобразуем и отображаем данные
            Console.WriteLine("File was taking, saving...");

            // Создаем временный файл с полученным расширением
            fstream = new FileStream("temp." + fileInfo.fileType, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fstream.Write(receivingBuffer, 0, receivingBuffer.Length);

            Console.WriteLine("File saved.");

            Console.WriteLine("opening file...");

            // Открываем файл связанной с ним программой
            Process.Start(fstream.Name);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            fstream.Close();
            Client.Close();
            Console.Read();
        }
    }
}