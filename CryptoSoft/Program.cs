namespace CryptoSoft;

public static class Program
{
    private static Mutex? mutex = null;

    public static void Main(string[] args)
    {
        mutex = new Mutex(true, out bool createdNew);
        if (!createdNew)
        {
            Environment.Exit(-99);
        }

        try
        {

            var fileManager = new FileManager(args[0], args[1]);
            int ElapsedTime = fileManager.TransformFile();
            Environment.Exit(ElapsedTime);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(-99);
        }
        finally
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();
        }
    }
}