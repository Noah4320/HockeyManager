using System.Collections.Generic;

public class Person
{
    public int id { get; set; }
    public string fullName { get; set; }
    public string link { get; set; }

}

public class Position
{
    public string code { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string abbreviation { get; set; }

}

public class Roster
{
    public Person person { get; set; }
    public string jerseyNumber { get; set; }
    public Position position { get; set; }

}

public class PersonRoot
{
    public string copyright { get; set; }
    public List<Roster> roster { get; set; }
    public string link { get; set; }

}

