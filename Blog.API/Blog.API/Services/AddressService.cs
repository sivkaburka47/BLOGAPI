using Blog.API.Context;
using Blog.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services;

public interface IAddressService
{
    Task<List<SearchAddressModel>> GetAddressChain(Guid objectGuid);
    Task<List<SearchAddressModel>> SearchAddress(Int64 parentObjectId, string query);
}

public class AddressService : IAddressService
{
    private readonly MyDbContext _context;

    public AddressService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchAddressModel>> SearchAddress(Int64 parentObjectId, string query)
    {
        var addressList = new List<SearchAddressModel>();

        var hierarchyRecords = await _context.AsAdmHierarchies
            .Where(h => h.Parentobjid == parentObjectId)
            .ToListAsync();

        var objectIds = hierarchyRecords.Select(h => h.Objectid).ToList();

        var sortedAddrObjs = await _context.AsAddrObjs
            .Where(a => objectIds.Contains(a.Objectid))
            .OrderBy(a => a.Typename)
            .ThenBy(a => a.Name)
            .ToListAsync();

        foreach (var addrObj in sortedAddrObjs)
        {
            if (!string.IsNullOrEmpty(query) && !addrObj.Name.Contains(query))
            {
                continue;
            }

            var level = Enum.Parse<GarAddressLevel>(addrObj.Level) - 1;
            addressList.Add(new SearchAddressModel
            {
                objectId = (int)addrObj.Objectid,
                objectGuid = addrObj.Objectguid,
                text = addrObj.Typename + " " + addrObj.Name,
                objectLevel = level,
                objectLevelText = AddressLevel.LevelsDictionary[level]
            });
        }

        return addressList.Take(10).ToList();
    }

    public async Task<List<SearchAddressModel>> GetAddressChain(Guid objectGuid)
    {
        var addressChain = new List<SearchAddressModel>();

        var addrObj = await _context.AsAddrObjs
            .FirstOrDefaultAsync(a => a.Objectguid == objectGuid);

        long objectId;

        if (addrObj != null)
        {
            objectId = addrObj.Objectid;
        }
        else
        {
            var houseObj = await _context.AsHouses
                .FirstOrDefaultAsync(h => h.Objectguid == objectGuid);

            if (houseObj == null)
            {
                throw new KeyNotFoundException($"Object with id={objectGuid} not found");
            }

            objectId = houseObj.Objectid;

            addressChain.Add(new SearchAddressModel
            {
                objectId = (int)houseObj.Objectid,
                objectGuid = houseObj.Objectguid,
                text = houseObj.Housenum,
                objectLevel = GarAddressLevel.Building,
                objectLevelText = AddressLevel.LevelsDictionary[GarAddressLevel.Building]
            });
        }

        var hierarchy = await _context.AsAdmHierarchies
            .FirstOrDefaultAsync(h => h.Objectid == objectId);

        if (hierarchy == null)
        {
            return new List<SearchAddressModel>();
        }

        var pathObjectIds = hierarchy.Path.Split('.').Select(long.Parse).ToList();

        if (addressChain.Count != 0)
        {
            pathObjectIds.RemoveAt(pathObjectIds.Count - 1);
        }

        var addrObjs = await _context.AsAddrObjs
            .Where(a => pathObjectIds.Contains(a.Objectid))
            .ToListAsync();

        addressChain.InsertRange(0, pathObjectIds.Select(id =>
        {
            var addr = addrObjs.FirstOrDefault(a => a.Objectid == id);
            if (addr != null)
            {
                var level = Enum.Parse<GarAddressLevel>(addr.Level) - 1;
                return new SearchAddressModel
                {
                    objectId = (int)addr.Objectid,
                    objectGuid = addr.Objectguid,
                    text = addr.Typename + " " + addr.Name,
                    objectLevel = level,
                    objectLevelText = AddressLevel.LevelsDictionary[level]
                };
            }

            return null;
        }).Where(a => a != null));
        return addressChain;
    }
}