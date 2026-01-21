namespace TTMapEditor
{

    public static class Program
    {
        static void Main()
        {
            using (var game = (TTMapEditor)TTMapEditor.Instance())
            {
                game.Run();
            }
        }
    }
}
