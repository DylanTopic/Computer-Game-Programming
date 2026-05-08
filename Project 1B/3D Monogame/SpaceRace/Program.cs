try
{
    using var game = new SpaceRace.Game1();
    game.Run();
}
catch (System.Exception ex)
{
    System.IO.File.WriteAllText("crash.log", ex.ToString());
    System.Console.Error.WriteLine(ex.ToString());
    throw;
}