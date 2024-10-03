﻿
class LevelData
{
    private List<LevelElement> _elements = new List<LevelElement>();
    public List<LevelElement> Elements { get { return _elements; } }
    public Hero Hero { get; set; }

    public void Load(string levelFileName)
    {
        string pathToLevel = Directory.GetCurrentDirectory().Replace("\\bin\\Debug\\net8.0", $"\\Levels\\{levelFileName}");

        using (StreamReader reader = new StreamReader(pathToLevel))
        { 
            Position readerPosition = new Position(0, 3);
            while (!reader.EndOfStream)
            {
                char currentChar = (char)reader.Read();
                if (currentChar == '#')
                {
                    _elements.Add(new Wall(new Position(readerPosition.X, readerPosition.Y)));
                }
                if (currentChar == '@')
                {
                    _elements.Add(new Hero(new Position(readerPosition.X, readerPosition.Y)));
                    Hero = (Hero)Elements[Elements.Count - 1];
                }
                else if (currentChar == 'r')
                {
                    _elements.Add(new Rat(new Position(readerPosition.X, readerPosition.Y)));
                }
                else if (currentChar == 's')
                {
                    _elements.Add(new Snake(new Position(readerPosition.X, readerPosition.Y)));
                }
                else if (currentChar == '\n')
                {
                    readerPosition.X = 0;
                    readerPosition.Y++;
                    continue;
                }
                readerPosition.X++;
            }
        }

    }
    public void PrintStatusBar()
    {
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = Hero.SpriteColor;
        Console.WriteLine($"Name: {Hero.Name}  -  Health: {$"{Hero.HP}/100".PadLeft(7)}  -  Turn: {$"{Hero.Turn}".PadLeft(5)}");
    }
    public void DrawLevel()
    {
        foreach (var element in Elements)
        {
            element.Draw(element.IsInsideVisionRange);
        }
    }
    public void UpdateVision()
    {
        foreach (var element in Elements)
        {
            element.UpdateIsInsideVisionRange(Hero);
        }
    }
    public void MoveCharacters()
    {
        Hero.Move(Elements);
        foreach (var element in Elements)
        {
            if(element is Enemy)
            {
                (element as Rat)?.Move(Elements);
                (element as Snake)?.Move(Elements, Hero);
            }
        }
    }
}