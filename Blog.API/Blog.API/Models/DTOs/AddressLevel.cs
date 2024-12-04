namespace Blog.API.Models.DTOs
{
    public static class AddressLevel
{
    public static Dictionary<GarAddressLevel, string> LevelsDictionary = new Dictionary<GarAddressLevel, string>
    {
        { GarAddressLevel.Region, "\u0421\u0443\u0431\u044a\u0435\u043a\u0442 \u0420\u0424" }, // ������� ��
        { GarAddressLevel.AdministrativeArea, "\u0410\u0434\u043c\u0438\u043d\u0438\u0441\u0442\u0440\u0430\u0442\u0438\u0432\u043d\u044b\u0439 \u0440\u0430\u0439\u043e\u043d" }, // ���������������� �����
        { GarAddressLevel.MunicipalArea, "\u041c\u0443\u043d\u0438\u0446\u0438\u043f\u0430\u043b\u044c\u043d\u044b\u0439 \u0440\u0430\u0439\u043e\u043d" }, // ������������� �����
        { GarAddressLevel.RuralUrbanSettlement, "\u0421\u0435\u043b\u044c\u0441\u043a\u043e\u0435 \u041f\u043e\u0441\u0435\u043b\u0435\u043d\u0438\u0435" }, // �������� ���������
        { GarAddressLevel.City, "\u0413\u043e\u0440\u043e\u0434" }, // �����
        { GarAddressLevel.Locality, "\u041d\u0430\u0441\u0435\u043b\u0435\u043d\u043d\u044b\u0439 \u043f\u0443\u043d\u043a\u0442" }, // ���������� �����
        { GarAddressLevel.ElementOfPlanningStructure, "\u042d\u043b\u0435\u043c\u0435\u043d\u0442 \u043f\u043b\u0430\u043d\u0438\u0440\u043e\u0432\u043e\u0447\u043d\u043e\u0439 \u0441\u0442\u0440\u0443\u043a\u0442\u0443\u0440\u044b" }, // ������� ������������� ���������
        { GarAddressLevel.ElementOfRoadNetwork, "\u042d\u043b\u0435\u043c\u0435\u043d\u0442 \u0443\u043b\u0438\u0447\u043d\u043e-\u0434\u043e\u0440\u043e\u0436\u043d\u043e\u0439 \u0441\u0435\u0442\u0438" }, // ������� ������-�������� ����
        { GarAddressLevel.Land, "\u0417\u0435\u043c\u0435\u043b\u044c\u043d\u044b\u0439 \u0443\u0447\u0430\u0441\u0442\u043e\u043a" }, // ��������� �������
        { GarAddressLevel.Building, "\u0417\u0434\u0430\u043d\u0438\u0435(\u0421\u0442\u0440\u043e\u0435\u043d\u0438\u0435)" }, // ������(��������)
        { GarAddressLevel.Room, "\u041a\u043e\u0440\u043f\u0443\u0441" }, // ������
        { GarAddressLevel.RoomInRooms, "\u041a\u0432\u0430\u0440\u0442\u0438\u0440\u0430" }, // ��������
        { GarAddressLevel.AutonomousRegionLevel, "\u0410\u0432\u0442\u043e\u043d\u043e\u043c\u043d\u044b\u0439 \u043e\u043a\u0440\u0443\u0433" }, // ���������� �����
        { GarAddressLevel.IntracityLevel, "\u0412 \u0447\u0435\u0440\u0442\u0435 \u0433\u043e\u0440\u043e\u0434\u0430" }, // � ����� ������
        { GarAddressLevel.AdditionalTerritoriesLevel, "\u041f\u0440\u0438\u043b\u0435\u0433\u0430\u044e\u0449\u0438\u0435 \u0442\u0435\u0440\u0440\u0438\u0442\u043e\u0440\u0438\u0438" }, // ����������� ����������
        { GarAddressLevel.LevelOfObjectsInAdditionalTerritories, "\u042d\u043b\u0435\u043c\u0435\u043d\u0442 \u043f\u043b\u0430\u043d\u0438\u0440\u043e\u0432\u043e\u0447\u043d\u043e\u0439 \u0441\u0442\u0440\u0443\u043a\u0442\u0443\u0440\u044b" }, // ������� ������������� ���������
        { GarAddressLevel.CarPlace, "\u041f\u0430\u0440\u043a\u043e\u0432\u043e\u0447\u043d\u043e\u0435 \u043c\u0435\u0441\u0442\u043e" } // ����������� �����
    };
}
    
}