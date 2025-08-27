namespace JobService.Domain.Jobs;
public sealed class Location
{
    public string City { get; private set; }
    public string District { get; private set; }
    public double? Lat { get; private set; }
    public double? Lng { get; private set; }

    private Location() { }
    public Location(string city, string district, double? lat = null, double? lng = null)
    {
        City = city.Trim(); District = district.Trim(); Lat = lat; Lng = lng;
    }
}
