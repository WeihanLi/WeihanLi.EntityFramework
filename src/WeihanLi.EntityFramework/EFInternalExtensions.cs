using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WeihanLi.EntityFramework
{
    public static class EFInternalExtensions
    {
        public static string GetColumnName(this PropertyEntry propertyEntry)
        {
            var storeObjectId =
                StoreObjectIdentifier.Create(propertyEntry.Metadata.DeclaringEntityType, StoreObjectType.Table);
            return propertyEntry.Metadata.GetColumnName(storeObjectId.GetValueOrDefault()) ?? propertyEntry.Metadata.Name;
        }
    }
}
