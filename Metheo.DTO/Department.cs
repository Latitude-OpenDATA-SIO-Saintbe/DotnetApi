namespace Metheo.DTO;

public class Department
{
    // Primary key property - EF Core will use this by default
    public string Id { get; set; }
    public string Name { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}