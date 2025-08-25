using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookLibwithSub.Repo.Infrastructure
{
    public static class UtcDateTimeConverters
    {
        private static readonly ValueConverter<DateTime, DateTime> _dateTimeConverter =
            new(v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                 v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> _nullableDateTimeConverter =
            new(v => v == null ? null :
                     (v.Value.Kind == DateTimeKind.Utc ? v :
                      DateTime.SpecifyKind(v.Value.ToUniversalTime(), DateTimeKind.Utc)),
                 v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static void UseUtcDateTimes(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(_dateTimeConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(_nullableDateTimeConverter);
                }
            }
        }
    }
}
