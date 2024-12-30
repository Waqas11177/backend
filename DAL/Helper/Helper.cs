using DAL.Repositeries;
using Microsoft.EntityFrameworkCore;

public static class Helper
{
    public static int GetMaxCode(RepositeryContext dbContext)
    {
        var maxCode = dbContext.masterItems.Any()
            ? dbContext.masterItems.Max(x => x.ItemCode)
            : 0;

        var newCode = maxCode + 1;

        return newCode;
    }
}
