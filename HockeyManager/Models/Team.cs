using System.Collections.Generic;

public class TimeZone
{
    public string id { get; set; }
    public int offset { get; set; }
    public string tz { get; set; }

}

public class Venue
{
    public string name { get; set; }
    public string link { get; set; }
    public string city { get; set; }
    public TimeZone timeZone { get; set; }

}

public class Division
{
    public int id { get; set; }
    public string name { get; set; }
    public string nameShort { get; set; }
    public string link { get; set; }
    public string abbreviation { get; set; }

}

public class Conference
{
    public int id { get; set; }
    public string name { get; set; }
    public string link { get; set; }

}

public class Franchise
{
    public int franchiseId { get; set; }
    public string teamName { get; set; }
    public string link { get; set; }

}

public class Team
{
    public int id { get; set; }
    public string name { get; set; }
    public string link { get; set; }
    public Venue venue { get; set; }
    public string abbreviation { get; set; }
    public string teamName { get; set; }
    public string locationName { get; set; }
    public string firstYearOfPlay { get; set; }
    public Division division { get; set; }
    public Conference conference { get; set; }
    public Franchise franchise { get; set; }
    public string shortName { get; set; }
    public string officialSiteUrl { get; set; }
    public int franchiseId { get; set; }
    public bool active { get; set; }

}

public class TeamRoot
{
    public string copyright { get; set; }
    public List<Team> teams { get; set; }

}

