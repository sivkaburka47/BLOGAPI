namespace Blog.API.Models.DTOs
{
    public static class AddressLevel
{
    public static Dictionary<GarAddressLevel, string> LevelsDictionary = new Dictionary<GarAddressLevel, string>
    {
        { GarAddressLevel.Region, "Субъект РФ" },
        { GarAddressLevel.AdministrativeArea, "Административный район" },
        { GarAddressLevel.MunicipalArea, "Муниципальный район" },
        { GarAddressLevel.RuralUrbanSettlement, "Сельское Поселение" },
        { GarAddressLevel.City, "Город" },
        { GarAddressLevel.Locality, "Населенный пункт" },
        { GarAddressLevel.ElementOfPlanningStructure, "Элемент планировочной структуры" },
        { GarAddressLevel.ElementOfRoadNetwork, "Элемент улично-дорожной сети" },
        { GarAddressLevel.Land, "Земельный участок" },
        { GarAddressLevel.Building, "Здание(Строение)" },
        { GarAddressLevel.Room, "Корпус" },
        { GarAddressLevel.RoomInRooms, "Квартира" },
        { GarAddressLevel.AutonomousRegionLevel, "Автономный округ" },
        { GarAddressLevel.IntracityLevel, "В черте города" },
        { GarAddressLevel.AdditionalTerritoriesLevel, "Прилегающие территории" },
        { GarAddressLevel.LevelOfObjectsInAdditionalTerritories, "Элемент планировочной структуры" },
        { GarAddressLevel.CarPlace, "Парковочное место" }
    };
}
    
}